using CommunalSystem.Data;
using CommunalSystem.Models;
using MySqlConnector;
using System.Data;

namespace CommunalSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DbConnection _dbConnection;

        public UserRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private User CreateUserFromReader(MySqlDataReader reader)
        {
            int id = reader.GetInt32("id");
            string username = reader.GetString("username");
            string password = reader.GetString("password");
            string role = reader.GetString("role");
            string firstName = reader.GetString("first_name");
            string lastName = reader.GetString("last_name");
            int? communityId = reader.IsDBNull("community_id") ? null : reader.GetInt32("community_id");

            return role switch
            {
                "admin" => new Admin(id, username, password, role, firstName, lastName, communityId),
                "manager" => new Manager(id, username, password, role, firstName, lastName, communityId),
                "resident" => new Resident(id, username, password, role, firstName, lastName, communityId),
                _ => null
            };
        }

        public User FindByUsername(string username)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT id, username, password, role, first_name, last_name, community_id FROM users WHERE username = @username",
                conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return CreateUserFromReader(reader);

            return null;
        }

        public int Save(string firstName, string lastName, string role, int? communityId = null)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand(
                    "INSERT INTO users (username, password, role, first_name, last_name, community_id) " +
                    "VALUES (@username, @password, @role, @firstName, @lastName, @communityId)",
                    conn, tx);

                cmd.Parameters.AddWithValue("@username", firstName);
                cmd.Parameters.AddWithValue("@password", lastName);
                cmd.Parameters.AddWithValue("@role", role);
                cmd.Parameters.AddWithValue("@firstName", firstName);
                cmd.Parameters.AddWithValue("@lastName", lastName);
                cmd.Parameters.AddWithValue("@communityId", communityId ?? (object)DBNull.Value);

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

        public void Delete(int userId)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand("DELETE FROM users WHERE id = @id", conn, tx);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<User> GetAll()
        {
            var users = new List<User>();
            using var conn = _dbConnection.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT id, username, password, role, first_name, last_name, community_id FROM users",
                conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var user = CreateUserFromReader(reader);
                if (user != null) users.Add(user);
            }
            return users;
        }
    }
}
