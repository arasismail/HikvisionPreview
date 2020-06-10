using DataAccess.Abstract;
using Entities.Concrete;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataAccess.Concrete
{
    public class UserDal:IUserDal
    {
        NpgsqlConnection _connection = new NpgsqlConnection(@"User ID=postgres;Password=1234;Server=localhost;Port=5432;Database=PIXSelectDb;Integrated Security=true;Pooling=true;");

        private void ConnectionControl()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        public List<User> GetAll()
        {
            ConnectionControl();

            NpgsqlCommand command = new NpgsqlCommand(@"select*from models", _connection);
            NpgsqlDataReader reader = command.ExecuteReader();

            List<User> users = new List<User>();

            while (reader.Read())
            {
                User user = new User()
                {
                    Id = Convert.ToInt32(reader["modelid"]),
                    UserName = reader["model_username"].ToString(),
                    Password = reader["model_password"].ToString(),
                    IPAddress = reader["model_address"].ToString(),
                    PortNumber = Convert.ToInt32(reader["model_port"])
                };
                users.Add(user);
            }
            reader.Close();
            _connection.Close();
            return users;
        }

        public void Add(User user)
        {
            ConnectionControl();
            NpgsqlCommand command = new NpgsqlCommand(@"insert into models( model_username, model_password, model_address, model_port) values (@UserName, @Password, @IPAddress, @PortNumber)", _connection);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@IPAddress", user.IPAddress);
            command.Parameters.AddWithValue("@PortNumber", user.PortNumber);
            command.ExecuteNonQuery();

            _connection.Close();
        }
        public void Update(User user)
        {
            ConnectionControl();
            NpgsqlCommand command = new NpgsqlCommand("update models set model_username= @UserName, model_password=@Password1, model_address=@IPAddress, model_port=@PortNumber  where modelid=@Id", _connection);
            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@Password1", user.Password);
            command.Parameters.AddWithValue("@IPAddress", user.IPAddress);
            command.Parameters.AddWithValue("@PortNumber", user.PortNumber);
            command.ExecuteNonQuery();

            _connection.Close();
        }
        public void Delete(int id)
        {
            ConnectionControl();
            NpgsqlCommand command = new NpgsqlCommand("delete from models where modelid=@Id", _connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();

            _connection.Close();
        }
    }
}
