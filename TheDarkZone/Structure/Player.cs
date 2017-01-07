using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDarkZone.Data;

namespace TheDarkZone.Structure
{
    public class Player
    {

        #region "Variables"

        public Client client { get; set; }
        public NetHandle vehicle { get; set; }
        public bool hasVehicle { get; set; }
        public int roleLevel { get; set; }
        public int money { get; set; }
        public int userID { get; set; }
        public string ownedAppartment { get; set; }

        private KeyManager keys;
        private UserDataManager udm;
        
        #endregion"

        #region "Initialize"

        public Player(Client client, UserDataManager udm, KeyManager keys)
        {
            this.userID = 0;
            this.hasVehicle = false;
            this.udm = udm;
            this.keys = keys;
            this.client = client;
            this.ownedAppartment = "";
        }

        public Player()
        {

        }

        #endregion

        #region "Public functions"

        public void LoadPlayerData()
        {
            if (userID != 0)
            {
                udm.RetrievePlayerDataFromDB(this);
                API.shared.consoleOutput("Loaded player data for user: " + client.name);

                API.shared.setEntityData(client.handle, keys.KEY_USER_ADMIN_LEVEL, roleLevel);
                API.shared.setEntityData(client.handle, keys.KEY_USER_APARTMENT, ownedAppartment);

                API.shared.setEntitySyncedData(client.handle, keys.KEY_USER_MONEY, money);
            }
            else
            {
                API.shared.consoleOutput("Failed to load player data because userid = 0!");
            }
        }

        public void SavePlayerData()
        {
            if (!API.shared.getEntityData(client, keys.KEY_USER_AUTHENTICATED)) return;
            this.money = API.shared.getEntitySyncedData(this.client, keys.KEY_USER_MONEY);
            udm.SavePlayerData(this);
        }

        #endregion 
    }
}
