﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using System.IO;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using TheDarkZone.Structure;

namespace TheDarkZone.Data
{
    public class UserDataManager
    {

        #region "Variables"

        private string connectionString = "";
        private string strSQL = "";
        private TheDarkZone mainScript { get; set; }

        #endregion 

        #region "Initialize"

        public UserDataManager(TheDarkZone mainScript)
        {
            this.mainScript = mainScript;
            connectionString = GetMysqlConnectionString();
        }

        #endregion

        #region "Public functions"

        public void SavePlayerClothing(Client sender)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "UPDATE USER_Clothing SET ";
                    strSQL += "hat=@hat, face=@face, mask=@mask, hair=@hair, hair_color=@hair_color, legs=@legs, bags=@bags, feet=@feet, accessories=@accessories, top=@top, torso=@torso, undershirt=@undershirt ";
                    strSQL += "WHERE userid=@userid";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@hat", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAT));
                        cmd.Parameters.AddWithValue("@face", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_FACE));
                        cmd.Parameters.AddWithValue("@mask", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_MASK));
                        cmd.Parameters.AddWithValue("@hair", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR));
                        cmd.Parameters.AddWithValue("@hair_color", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR_COLOR));
                        cmd.Parameters.AddWithValue("@legs", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_LEGS));
                        cmd.Parameters.AddWithValue("@bags", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_BAGS));
                        cmd.Parameters.AddWithValue("@feet", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_FEET));
                        cmd.Parameters.AddWithValue("@accessories", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_ACCESSORIES));
                        cmd.Parameters.AddWithValue("@top", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_TOP));
                        cmd.Parameters.AddWithValue("@userid", API.shared.getEntityData(sender, mainScript.keys.KEY_USER_ID));
                        cmd.Parameters.AddWithValue("@torso", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_TORSO));
                        cmd.Parameters.AddWithValue("@undershirt", API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_UNDERSHIRT));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                API.shared.consoleOutput("Error saving player clothing " + ex.Message);
            }
        }

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
                API.shared.consoleOutput("Error checking if username exists: " + ex.Message);
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
                API.shared.consoleOutput("Error checking if user account exists: " + ex.Message);
            }
            return id;
        }

        public int CreateUserAccount(string username, string password)
        {
            int userID = 0;
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
                        API.shared.consoleOutput("Created new user account: " + username);
                        userID = GetUserID(username);
                    }
                }
                CreatePleayerInventoryRow(userID);
                CreatePlayerClothingRow(userID);
            }
            catch (Exception ex)
            {
                API.shared.consoleOutput("Error creating new user account: " + ex.Message);

            }

            return userID;
        }

        private void CreatePleayerInventoryRow(int userID)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                strSQL = "INSERT INTO USER_Inventory (userid) VALUES (@userid)";
                using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                {
                    cmd.Parameters.AddWithValue("@userid", userID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CreatePlayerClothingRow(int userID)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                strSQL = "INSERT INTO USER_Clothing (userid) VALUES (@userid)";
                using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                {
                    cmd.Parameters.AddWithValue("@userid", userID);
                    cmd.ExecuteNonQuery();
                }
            }
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
                API.shared.consoleOutput("Error getting user ID: " + ex.Message);
            }
            return 0;
        }

        public Boolean RetrievePlayerDataFromDB(Player player)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "SELECT T1.role, T2.money, T2.weapons, T3.propertyname, T4.* FROM USER_Accounts T1 ";
                    strSQL += "INNER JOIN USER_Inventory T2 on T1.id = T2.userid ";
                    strSQL += "LEFT JOIN USER_Appartment T3 on T1.ID = T3.userid ";
                    strSQL += "LEFT JOIN USER_Clothing T4 on T1.id = T4.userid ";
                    strSQL+= " WHERE T1.id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@id", player.userID);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    player.roleLevel = (int)rdr["role"];
                                    player.money = (int)rdr["money"];
                                    if (rdr["weapons"] != DBNull.Value)
                                    {
                                        player.ownedWeapons = (string)rdr["weapons"];
                                    }
                                    else
                                    {
                                        player.ownedWeapons = "";
                                    }
                                    if(rdr["propertyname"] != DBNull.Value) 
                                    {
                                        player.ownedAppartment = (string)rdr["propertyname"];
                                    }
                                    else
                                    {
                                        player.ownedAppartment = "";
                                    }
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_HAT, (int)rdr["hat"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_FACE, (int)rdr["face"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_MASK, (int)rdr["mask"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_HAIR, (int)rdr["hair"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_HAIR_COLOR, (int)rdr["hair_color"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_LEGS, (int)rdr["legs"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_BAGS, (int)rdr["bags"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_FEET, (int)rdr["feet"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_ACCESSORIES, (int)rdr["accessories"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_TOP, (int)rdr["top"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_TORSO, (int)rdr["torso"]);
                                    API.shared.setEntityData(player.client, mainScript.keys.USER_CLOTHING_UNDERSHIRT, (int)rdr["undershirt"]);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                API.shared.consoleOutput("Failed to retrieve player data: " + ex.Message);
            }
            return false;
        }

        public void SavePlayerWeapons(Client sender)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "UPDATE USER_Inventory SET weapons = @weapons WHERE userid=@userid";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@weapons", API.shared.getEntityData(sender, mainScript.keys.KEY_USER_WEAPONS));
                        cmd.Parameters.AddWithValue("@userid", API.shared.getEntityData(sender, mainScript.keys.KEY_USER_ID));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                API.shared.consoleOutput("Failed to save player owned weapons: " + ex.Message);
            }
        }

        public void SavePlayerData(Player player)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "UPDATE USER_Inventory SET money = @money WHERE userid = @userid";
                    using(MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@money", API.shared.getEntitySyncedData(player.client, mainScript.keys.KEY_USER_MONEY));
                        cmd.Parameters.AddWithValue("@userid", player.userID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                API.shared.consoleOutput("Failed to save player data: " + ex.Message);
            }
        }

        public void SavePlayerMoney(Client sender)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "UPDATE USER_Inventory SET money = @money WHERE userid=@userid";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@money", API.shared.getEntitySyncedData(sender, mainScript.keys.KEY_USER_MONEY));
                        cmd.Parameters.AddWithValue("@userid", API.shared.getEntityData(sender, mainScript.keys.KEY_USER_ID));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                API.shared.consoleOutput("Failed to delete player owned appartment: " + ex.Message);
            }
        }

        public bool DeletePlayerAppartment(Client sender)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "DELETE FROM USER_Appartment WHERE userid = @userid";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", API.shared.getEntityData(sender, mainScript.keys.KEY_USER_ID));
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                API.shared.consoleOutput("Failed to delete player owned appartment: " + ex.Message);
            }
            return false;
        }

        public bool SavePlayerAppartment(Client sender)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    strSQL = "INSERT INTO USER_Appartment(userid, propertyname) VALUES (@userid, @propertyname)";
                    using (MySqlCommand cmd = new MySqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", API.shared.getEntityData(sender, mainScript.keys.KEY_USER_ID));
                        cmd.Parameters.AddWithValue("@propertyname", API.shared.getEntityData(sender, mainScript.keys.KEY_USER_APARTMENT));
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                API.shared.consoleOutput("Failed to create player owned appartment: " + ex.Message);
            }
            return false;
        }

        #endregion 

        #region "Private functions"

        private string GetMysqlConnectionString()
        {
            string conStr = "";
            try
            {
                using (StreamReader rdr = new StreamReader("constr.txt"))
                {
                    conStr = rdr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                API.shared.consoleOutput("Failed to load MySql connectionstring: " + ex.Message);
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
                API.shared.consoleOutput("Error checking if user account exists: " + ex.Message);
            }
            return bytes;
        }

        #endregion 

    }
}
