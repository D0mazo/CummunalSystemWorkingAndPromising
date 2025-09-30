using CommunalSystem.Data;
using CommunalSystem.Models;
using MySqlConnector;
using System.Collections.Generic;

namespace CommunalSystem.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly DbConnection _dbConnection;

        public CommunityRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public int Save(string name)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("INSERT INTO communities (name) VALUES (@name)", conn);
            cmd.Parameters.AddWithValue("@name", name);

            cmd.ExecuteNonQuery();
            return (int)cmd.LastInsertedId;
        }

        public void Update(int communityId, string name)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("UPDATE communities SET name = @name WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@id", communityId);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int communityId)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("DELETE FROM communities WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", communityId);

            cmd.ExecuteNonQuery();
        }

        public List<Community> GetAll()
        {
            var communities = new List<Community>();
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("SELECT id, name FROM communities", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                communities.Add(new Community(reader.GetInt32("id"), reader.GetString("name")));
            }
            return communities;
        }

        public Community FindById(int communityId)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("SELECT id, name FROM communities WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", communityId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Community(reader.GetInt32("id"), reader.GetString("name"));
            }
            return null;
        }
    }
}
