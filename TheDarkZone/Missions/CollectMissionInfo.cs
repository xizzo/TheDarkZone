using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;

namespace TheDarkZone.Missions
{
    public class CollectMissionInfo
    {
        public int ID {get; set;}
        public Vector3 collectPickupPos { get; set; }
        public int moneyReward { get; set; }
    }
}
