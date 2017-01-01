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
        public int userID { get; set; }
        public UserDataManager udm { get; set; }

        private KeyManager keys;
        
        #endregion"

        #region "Initialize"

        public Player(Client client, UserDataManager udm, KeyManager keys)
        {
            this.userID = 0;
            this.hasVehicle = false;
            this.udm = udm;
            this.keys = keys;
            this.client = client;
        }

        public Player()
        {

        }

        public void LoadPlayerData()
        {
            if (userID != 0)
            {
                udm.RetrievePlayerDataFromDB(this);
                API.shared.consoleOutput("Loaded player data for user: " + client.name);

                API.shared.setEntityData(client.handle, keys.KEY_USER_ADMIN_LEVEL, roleLevel);
            }
            else
            {
                API.shared.consoleOutput("Failed to load player data because userid = 0!");
            }

        }

        #endregion

    }
}
