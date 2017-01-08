using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;

namespace TheDarkZone.Structure
{
    public class AmmoShop
    {
        public Vector3 entrance { get; set; }
        public Vector3 entranceSpawn { get; set; }
        public Vector3 exit { get; set; }
        public Vector3 exitSpawn { get; set; }
        public Vector3 buyArea { get; set; }
        public Vector3 npcSpawn { get; set; }
        public float npcSpawnRot { get; set; }
        public PedHash pedHash { get; set; }

        public AmmoShop(Vector3 entrance, Vector3 entranceSpawn, Vector3 exit, Vector3 exitSpawn, Vector3 buyArea, Vector3 npcSpawn, float npcSpawnRot, PedHash pedHash)
        {
            this.entrance = entrance;
            this.entranceSpawn = entranceSpawn;
            this.exit = exit;
            this.exitSpawn = exitSpawn;
            this.buyArea = buyArea;
            this.npcSpawn = npcSpawn;
            this.npcSpawnRot = npcSpawnRot;
            this.pedHash = pedHash;
        }

    }
}
