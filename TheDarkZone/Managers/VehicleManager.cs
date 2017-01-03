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

namespace TheDarkZone.Managers
{
    public class VehicleManager
    {
        private TheDarkZone mainScript {get; set;}

        private VehicleHash[] normalVehicleHashes = new VehicleHash[] {
            (VehicleHash)(-344943009),      //Blista
            (VehicleHash)(1039032026),      //BLista2
            (VehicleHash)(-591651781),      //Blista3
            (VehicleHash)(1549126457),      //Brioso
            (VehicleHash)(-1130810103),     //Dilettante
            (VehicleHash)(1682114128),      //Dilettante2
            (VehicleHash)(-1177863319),     //Issi2
            (VehicleHash)( -1450650718),    //Prairie
            (VehicleHash)(841808271),       //Rhapsody
            (VehicleHash)(330661258),       //CogCabrio
            (VehicleHash)(-5153954),        //Examplar
            (VehicleHash)(591610296),       //F620
            (VehicleHash)(-391594584),      //Felon
            (VehicleHash)(-89291282),       //Felon2
            (VehicleHash)(-624529134),      //Jackal
            (VehicleHash)(1348744438),      //Oracle
            (VehicleHash)(-511601230),      //Oracle2
            (VehicleHash)(1349725314),      //Sentinel
            (VehicleHash)(873639469),       //Sentinel2
            (VehicleHash)(1581459400),      //Windsor
            (VehicleHash)(-1930048799),     //Windsor2
            (VehicleHash)(-1122289213),     //Zion
            (VehicleHash)(-1193103848),     //Zion2
            (VehicleHash)(-1216765807),     //Adder
            (VehicleHash)(-1696146015),     //Bullet
            (VehicleHash)(-1311154784),     //Cheetah
            (VehicleHash)( -1291952903),    //EntityXF
            (VehicleHash)(1426219628),      //FMJ
            (VehicleHash)(418536135),       //Inferrnus
            (VehicleHash)(2067820283),      //Tyrus
            (VehicleHash)(338562499),       //Vacca
            (VehicleHash)(-1403128555),     //Zentorno
            (VehicleHash)(819197656),       //Sheava
            (VehicleHash)(2072156101),      //Bison2
            (VehicleHash)(1069929536),      //BobcatXL
            (VehicleHash)(1475773103),      //Rumpo3
            (VehicleHash)(-808457413),      //Patriot
            (VehicleHash)(2136773105),      //Rocoto
        };

        public VehicleManager(TheDarkZone mainScript)
        {
            this.mainScript = mainScript;
            LoadNormalVehicleData();
            API.shared.consoleOutput("loaded vehicles: " + mainScript.Vehicles.Count);
            SpawnVehicles();
        }

        private void LoadNormalVehicleData()
        {
            XDocument xDoc;
            using (StreamReader rdr = new StreamReader(@"resources/TheDarkZone/Managers/Data/vehicles.xml")) 
            {
                xDoc = XDocument.Parse(rdr.ReadToEnd());
            }
            foreach (XElement eSpawn in xDoc.Root.Elements())
            {
                if (eSpawn.Name == "spawn")
                {
                    Veh newVehicle = new Veh();
                    newVehicle.position = new Vector3();
                    newVehicle.rotation = new Vector3();
                    foreach (XElement eVal in eSpawn.Elements())
                    {
                        string currValue = eVal.Value;
                        if (mainScript.inDebug) currValue = currValue.Replace('.', ',');
                        switch(eVal.Name.ToString())
                        {
                            case "xpos":
                                newVehicle.position.X = float.Parse(currValue);
                                break;
                            case "ypos":
                                newVehicle.position.Y = float.Parse(currValue);
                                break;
                            case "zpos":
                                newVehicle.position.Z = float.Parse(currValue);
                                break;
                            case "xrot":
                                newVehicle.rotation.X = float.Parse(currValue);
                                break;
                            case "yrot":
                                newVehicle.rotation.Y = float.Parse(currValue);
                                break;
                            case "zrot":
                                newVehicle.rotation.Z = float.Parse(currValue);
                                break;
                        }
                    }
                    mainScript.Vehicles.Add(newVehicle);
                }
            }
        }

        private void SpawnVehicles()
        {
            List<NetHandle> lstVehicles = API.shared.getAllVehicles();
            foreach (NetHandle vehicleHandle in lstVehicles)
            {
                API.shared.deleteEntity(vehicleHandle);
            }

            foreach (Veh spawnVeh in mainScript.Vehicles)
            {
                VehicleHash randomHash = (VehicleHash)normalVehicleHashes[mainScript.GetRandomIntBetween(0, normalVehicleHashes.Count())];
                mainScript.CreateRespawnableVehicle(randomHash, spawnVeh.position, spawnVeh.rotation);
            }
        }

    }
}
