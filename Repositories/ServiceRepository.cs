using CommunalSystem.Data;
using CommunalSystem.Models;
using MySqlConnector;

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
            using var cmd = new MySqlCommand("INSERT INTO services (name) VALUES (@name)", conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.ExecuteNonQuery();
            return (int)cmd.LastInsertedId;
        }

        public void Update(int serviceId, string name)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE services SET name = @name WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@id", serviceId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int serviceId)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM services WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", serviceId);
            cmd.ExecuteNonQuery();
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
