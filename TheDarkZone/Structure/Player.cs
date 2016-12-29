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
        public NetHandle netHandle { get; set; }
        public NetHandle vehicle { get; set; }
        public bool hasVehicle { get; set; }
        public UserData userData { get; set; }
        
        #endregion"

        #region "Initialize"

        public Player()
        {
            this.hasVehicle = false;
        }

        public Player(Client client, NetHandle netHandle)
        {
            this.client = client;
            this.netHandle = netHandle;
            this.hasVehicle = false;
        }

        #endregion

    }
}
