using CommunalSystem.Data;
using CommunalSystem.Models;
using MySqlConnector;
using System.Data;

namespace CommunalSystem.Repositories
{
    public class CommunityServiceRepository : ICommunityServiceRepository
    {
        private readonly DbConnection _dbConnection;

        public CommunityServiceRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public int AssignAndSetPrice(int communityId, int serviceId, decimal price)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                // Check if exists
                using var checkCmd = new MySqlCommand("SELECT id FROM community_services WHERE community_id = @cid AND service_id = @sid", conn, tx);
                checkCmd.Parameters.AddWithValue("@cid", communityId);
                checkCmd.Parameters.AddWithValue("@sid", serviceId);
                if (checkCmd.ExecuteScalar() != null)
                {
                    throw new InvalidOperationException("Service already assigned");
                }

                using var cmd = new MySqlCommand("INSERT INTO community_services (community_id, service_id, price) VALUES (@cid, @sid, @price)", conn, tx);
                cmd.Parameters.AddWithValue("@cid", communityId);
                cmd.Parameters.AddWithValue("@sid", serviceId);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.ExecuteNonQuery();
                tx.Commit();
                return (int)cmd.LastInsertedId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void UpdatePrice(int communityId, int serviceId, decimal price)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand("UPDATE community_services SET price = @price WHERE community_id = @cid AND service_id = @sid", conn, tx);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@cid", communityId);
                cmd.Parameters.AddWithValue("@sid", serviceId);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException("No such assignment");
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<Price> GetForCommunity(int communityId, string searchTerm = null)
        {
            var prices = new List<Price>();
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            string sql = @"
                SELECT cs.id, cs.community_id, cs.service_id, cs.price 
                FROM community_services cs 
                JOIN services s ON cs.service_id = s.id 
                WHERE cs.community_id = @cid";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sql += " AND s.name LIKE @search";
            }
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cid", communityId);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            }
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                prices.Add(new Price(
                    reader.GetInt32("id"),
                    reader.GetInt32("community_id"),
                    reader.GetInt32("service_id"),
                    reader.GetDecimal("price")
                ));
            }
            return prices;
        }
    }
}