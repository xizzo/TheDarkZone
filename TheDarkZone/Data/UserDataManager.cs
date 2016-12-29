using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using System.IO;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

namespace TheDarkZone.Data
{
    public class UserDataManager
    {

        #region "Variables"

        private string connectionString = "";
        private string strSQL = "";
        private TheDarkZone apiScript;

        #endregion 

        #region "Initialize"

        public UserDataManager(TheDarkZone apiScript)
        {
            this.apiScript = apiScript;
            connectionString = GetMysqlConnectionString();
        }

        #endregion

        #region "Public functions"

        public bool UserNameExists(string username)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "SELECT id FROM USER_Accounts WHERE name=@name";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@name", username);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows) return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                apiScript.LogToConsole("Error checking if username exists: " + ex.Message);
            }
            return false;
        }
       
        public int UserAccountExists(string username, string password)
        {
            int id = 0;
            try
            {
                var pSalt = GetPlayerSalt(username);
                var encodedPassword = Encoding.Unicode.GetBytes(password);
                var hashedPassword = GeneratePasswordHash(encodedPassword, pSalt, 10, 20);

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "SELECT id FROM USER_Accounts WHERE name=@name and hashedpassword=@hashedpassword";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@name", username);
                        MySqlParameter paramHashedPassword = cmd.Parameters.Add("@hashedpassword", MySqlDbType.VarBinary);
                        paramHashedPassword.Value = hashedPassword;
                   
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    id = (int)rdr["ID"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                apiScript.LogToConsole("Error checking if user account exists: " + ex.Message);
            }
            return id;
        }

        public int CreateUserAccount(string username, string password)
        {
            try
            {
                var newSalt = GenerateSalt(10);
                var encodedPassword = Encoding.Unicode.GetBytes(password);
                var hashedPassword = GeneratePasswordHash(encodedPassword, newSalt, 10, 20);

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "INSERT INTO USER_Accounts (name, salt, hashedpassword) VALUES (@name, @salt, @hashedpassword)";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@name", username);

                        MySqlParameter paramSalt;
                        paramSalt = cmd.Parameters.Add("@salt", MySqlDbType.VarBinary);
                        paramSalt.Value = newSalt;

                        MySqlParameter paramPassword = cmd.Parameters.Add("@hashedpassword", MySqlDbType.VarBinary);
                        paramPassword.Value = hashedPassword;

                        cmd.ExecuteNonQuery();
                        apiScript.LogToConsole("Created new user account: " + username);
                        return GetUserID(username);
                    }
                }
            }
            catch (Exception ex)
            {
                apiScript.LogToConsole("Error creating new user account: " + ex.Message);

            }
            return 0;
        }

        public int GetUserID(string username)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "SELECT id FROM USER_Accounts WHERE name=@name";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@name", username);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    return (int)rdr["id"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                apiScript.LogToConsole("Error getting user ID: " + ex.Message);
            }
            return 0;
        } 

        #endregion 

        #region "Private functions"

        private string GetMysqlConnectionString()
        {
            string conStr = "";
            try
            {
                using (StreamReader rdr = new StreamReader(@"C:\constr.txt"))
                {
                    conStr = rdr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                apiScript.LogToConsole("Failed to load MySql connectionstring: " + ex.Message);
            }
            return conStr;
        }

        private byte[] GeneratePasswordHash(byte[] password, byte[] salt, int iterations, int lenght)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return deriveBytes.GetBytes(lenght);
            }
        }

        private byte[] GenerateSalt(int lenght)
        {
            var bytes = new byte[lenght];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        private byte[] GetPlayerSalt(string username)
        {
            byte[] bytes = new Byte[10];
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "SELECT salt FROM USER_Accounts WHERE name=@name";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@name", username);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    bytes = (byte[])rdr["salt"]; 
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                apiScript.LogToConsole("Error checking if user account exists: " + ex.Message);
            }
            return bytes;
        }

        #endregion 

    }
}
