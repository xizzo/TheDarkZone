using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using System.Xml.Linq;
using System.IO;
using TheDarkZone.Structure;

namespace TheDarkZone.Data
{
    public class WeaponData
    {
        public List<Weapon> weapons = new List<Weapon>();

        public WeaponData()
        {
            LoadWeapons();
            API.shared.consoleOutput("Loaded weapons: " + weapons.Count());
        }

        private void LoadWeapons()
        {
            XDocument xDoc;
            using (StreamReader rdr = new StreamReader(@"resources/TheDarkZone/Data/Data/weapons.xml"))
            {
                xDoc = XDocument.Parse(rdr.ReadToEnd());
            }

            foreach (XElement xWeapons in xDoc.Root.Elements())
            {
                if (xWeapons.Name == "weapon")
                {
                    string weaponName = "";
                    WeaponHash weaponHash = new WeaponHash();
                    int weaponPrice = 0;
                    foreach (XElement xWeapon in xWeapons.Elements())
                    {
                        switch (xWeapon.Name.ToString().ToUpper())
                        {
                            case "WEAPONNAME":
                                weaponName = xWeapon.Value.ToString();
                                break;
                            case "WEAPONHASH":
                                weaponHash = (WeaponHash)int.Parse(xWeapon.Value.ToString());
                                break;
                            case "PRICE":
                                weaponPrice = int.Parse(xWeapon.Value.ToString());
                                break;
                        }
                    }
                    weapons.Add(new Weapon(weaponName, weaponHash, weaponPrice));
                }
            }
        }

        public int GetWeaponIdByName(string weaponName)
        {
            int ret = 0;
            foreach (Weapon wep in weapons)
            {
                if (wep.weaponName == weaponName)
                {
                    break;
                }
                ret++;
            }
            return ret;
        }

    }
}
