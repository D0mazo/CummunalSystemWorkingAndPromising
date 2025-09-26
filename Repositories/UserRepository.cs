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

        public User FindByUsername(string username)
        {
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT id, username, password, role, first_name, last_name, community_id FROM users WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int id = reader.GetInt32("id");
                string pw = reader.GetString("password");
                string role = reader.GetString("role");
                string fn = reader.GetString("first_name");
                string ln = reader.GetString("last_name");
                int? cid = reader.IsDBNull("community_id") ? null : reader.GetInt32("community_id");
                return role switch
                {
                    "admin" => new Admin(id, username, pw, role, fn, ln, cid),
                    "manager" => new Manager(id, username, pw, role, fn, ln, cid),
                    "resident" => new Resident(id, username, pw, role, fn, ln, cid),
                    _ => null
                };
            }
            return null;
        }

        public int Save(string firstName, string lastName, string role, int? communityId = null)
        {
            string username = firstName;
            string password = lastName;
            using var conn = _dbConnection.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand("INSERT INTO users (username, password, role, first_name, last_name, community_id) VALUES (@username, @password, @role, @firstName, @lastName, @communityId)", conn, tx);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
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
            using var cmd = new MySqlCommand("SELECT id, username, password, role, first_name, last_name, community_id FROM users", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32("id");
                string un = reader.GetString("username");
                string pw = reader.GetString("password");
                string role = reader.GetString("role");
                string fn = reader.GetString("first_name");
                string ln = reader.GetString("last_name");
                int? cid = reader.IsDBNull("community_id") ? null : reader.GetInt32("community_id");
                User user = role switch
                {
                    "admin" => new Admin(id, un, pw, role, fn, ln, cid),
                    "manager" => new Manager(id, un, pw, role, fn, ln, cid),
                    "resident" => new Resident(id, un, pw, role, fn, ln, cid),
                    _ => null
                };
                if (user != null) users.Add(user);
            }
            return users;
        }
    }
}