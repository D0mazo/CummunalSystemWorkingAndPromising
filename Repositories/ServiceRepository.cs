using CommunalSystem.Data;
using CommunalSystem.Models;
using MySqlConnector;
using System.Data;

namespace CommunalSystem.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly DbConnection _dbConnection;

        public ServiceRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public int Save(string name)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand("INSERT INTO services (name) VALUES (@name)", conn, tx);
                cmd.Parameters.AddWithValue("@name", name);
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

        public void Update(int serviceId, string name)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand("UPDATE services SET name = @name WHERE id = @id", conn, tx);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@id", serviceId);
                cmd.ExecuteNonQuery();
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void Delete(int serviceId)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand("DELETE FROM services WHERE id = @id", conn, tx);
                cmd.Parameters.AddWithValue("@id", serviceId);
                cmd.ExecuteNonQuery();
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<Service> GetAll()
        {
            var services = new List<Service>();
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT id, name FROM services", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                services.Add(new Service(reader.GetInt32("id"), reader.GetString("name")));
            }
            return services;
        }

        public Service FindById(int serviceId)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT id, name FROM services WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", serviceId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Service(reader.GetInt32("id"), reader.GetString("name"));
            }
            return null;
        }
    }
}