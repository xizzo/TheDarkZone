using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDarkZone.Structure
{
    public class Veh
    {

        #region "Variables"

        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public VehicleHash Hash { get; set; }

        #endregion

        #region "Initialize"

        public Veh()
        {

        }

        public Veh(Vector3 position, Vector3 rotation, VehicleHash hash)
        {
            this.position = position;
            this.rotation = rotation;
            this.Hash = hash;
        }

        #endregion

    }
}
