using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;

namespace TheDarkZone.Structure
{
    public class Property
    {
        public Vector3 propEntrance { get; set; }
        public Vector3 propEntranceSpawn { get; set; }
        public Vector3 propEntranceSpawnRot { get; set; }
        public Vector3 propExit { get; set; }
        public Vector3 propExitSpawn { get; set; }
        public string IPL { get; set; }
        public int propPrice { get; set; }

        public Property(string IPL, Vector3 propEntrance, Vector3 propEntranceSpawn, Vector3 propEntranceSpawnRot, Vector3 propExit, Vector3 propExitSpawn, int propPrice)
        {
            this.IPL = IPL;
            this.propEntrance = propEntrance;
            this.propEntranceSpawn = propEntranceSpawn;
            this.propEntranceSpawnRot = propEntranceSpawnRot;
            this.propExit = propExit;
            this.propExitSpawn = propExitSpawn;
            this.propPrice = propPrice; 
        }

    }
}
