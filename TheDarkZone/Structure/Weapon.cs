using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;

namespace TheDarkZone.Structure
{
    public class Weapon
    {
        public string weaponName { get; set; }
        public WeaponHash weaponHash { get; set; }
        public int weaponPrice { get; set; }

        public Weapon(string weaponName, WeaponHash weaponHash, int weaponPrice)
        {
            this.weaponName = weaponName;
            this.weaponHash = weaponHash;
            this.weaponPrice = weaponPrice;
        }
    }
}
