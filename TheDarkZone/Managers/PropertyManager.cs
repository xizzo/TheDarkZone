using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using TheDarkZone.Structure;
using System.Xml.Linq;
using System.IO;

namespace TheDarkZone.Managers
{
    public class PropertyManager
    {
        private TheDarkZone mainScript { get; set; }
        public List<Property> properties = new List<Property>();

        public PropertyManager(TheDarkZone mainScript)
        {
            this.mainScript = mainScript;
            LoadProperties();
            CreateProperties();
            API.shared.consoleOutput("properties loaded: " + properties.Count);
        }

        private void CreateProperties()
        {
            foreach(Property prop in properties)
            {
                NetHandle propBlip = API.shared.createBlip(prop.propEntrance, 100f, 0);
                API.shared.setBlipSprite(propBlip, 350);
                API.shared.setBlipColor(propBlip, 4);
                API.shared.requestIpl(prop.IPL);
              
                API.shared.createMarker(1, new Vector3(prop.propEntrance.X, prop.propEntrance.Y, prop.propEntrance.Z -1), new Vector3(), new Vector3(), new Vector3(1.5, 1.5, 1), 241, 247, 57, 180);
                API.shared.createMarker(1, new Vector3(prop.propExit.X, prop.propExit.Y, prop.propExit.Z - 1), new Vector3(), new Vector3(), new Vector3(1.5, 1.5, 1), 241, 247, 57, 180);

                CylinderColShape colEntrance =  API.shared.createCylinderColShape(prop.propEntrance, 1.5f, 1.5f);
                colEntrance.setData(mainScript.keys.PROPERTY_NAME, prop.IPL);
                colEntrance.setData(mainScript.keys.PROPERTY_ENTRANCE_SPAWN, prop.propEntranceSpawn);
                colEntrance.setData(mainScript.keys.PROPERTY_ENTRANCE_SPAWN_ROT, prop.propEntranceSpawnRot);
                colEntrance.setData(mainScript.keys.PROPERTY_IN_OUT, true);
                colEntrance.onEntityEnterColShape += onPlayerEnterPropColShape;

                CylinderColShape colExit = API.shared.createCylinderColShape(prop.propExit, 1.5f, 1.5f);
                colExit.setData(mainScript.keys.PROPERTY_NAME, prop.IPL);
                colExit.setData(mainScript.keys.PROPERTY_EXIT_SPAWN, prop.propExitSpawn);
                colExit.setData(mainScript.keys.PROPERTY_IN_OUT, false);
                colExit.onEntityEnterColShape += onPlayerEnterPropColShape;

            }
        }

        public void onPlayerEnterPropColShape(ColShape shape, NetHandle entity)
        {
            if (!API.shared.hasEntityData(entity, mainScript.keys.KEY_USER_ID)) return;
            if (API.shared.isPlayerInAnyVehicle(API.shared.getPlayerFromHandle(entity))) return;

            if (shape.getData(mainScript.keys.PROPERTY_IN_OUT))
            {
                if (API.shared.getEntityData(entity, mainScript.keys.KEY_USER_APARTMENT) != shape.getData(mainScript.keys.PROPERTY_NAME))
                {
                    API.shared.sendNotificationToPlayer(API.shared.getPlayerFromHandle(entity), "~y~You can purchase this appartment from the property menu (F3)!");
                    return;
                }
            }
            if (shape.getData(mainScript.keys.PROPERTY_IN_OUT))
            {
                API.shared.setEntityDimension(entity, API.shared.getEntityData(entity, mainScript.keys.KEY_USER_ID));
                API.shared.setEntityPosition(entity, shape.getData(mainScript.keys.PROPERTY_ENTRANCE_SPAWN));
                API.shared.setEntityRotation(entity, shape.getData(mainScript.keys.PROPERTY_ENTRANCE_SPAWN_ROT));
            }
            else
            {
                API.shared.setEntityDimension(entity, 0);
                API.shared.setEntityPosition(entity, shape.getData(mainScript.keys.PROPERTY_EXIT_SPAWN));
            }
        }

        private void LoadProperties()
        {
            XDocument xDoc;
            using (StreamReader rdr = new StreamReader(@"resources/TheDarkZone/Managers/Data/properties.xml"))
            {
                xDoc = XDocument.Parse(rdr.ReadToEnd());
            }

            foreach (XElement xProperty in xDoc.Root.Elements())
            {
                if (xProperty.Name == "property")
                {
                    Vector3 propEntrance = new Vector3();
                    Vector3 propEntranceSpawn = new Vector3();
                    Vector3 propEntranceSpawnRot = new Vector3();
                    Vector3 propExit = new Vector3();
                    Vector3 propExitSpawn = new Vector3();
                    string IPL = "";
                    int propPrice = 0;
                    foreach (XElement xProp in xProperty.Elements())
                    {
                        string currValue = xProp.Value;
                        if (mainScript.inDebug) currValue = currValue.Replace('.', ',');
                        switch (xProp.Name.ToString().ToUpper())
                        {
                            case "ENTRANCE":
                                string[] pEntrance = currValue.Split(';');
                                propEntrance = new Vector3(float.Parse(pEntrance[0]), float.Parse(pEntrance[1]), float.Parse(pEntrance[2]));
                                break;
                            case "ENTRANCESPAWN":
                                string[] pEntranceSpawn = currValue.Split(';');
                                propEntranceSpawn = new Vector3(float.Parse(pEntranceSpawn[0]), float.Parse(pEntranceSpawn[1]), float.Parse(pEntranceSpawn[2]));
                                break;
                            case "ENTRANCESPAWNROT":
                                string[] pEntranceSpawnRot = currValue.Split(';');
                                propEntranceSpawnRot = new Vector3(float.Parse(pEntranceSpawnRot[0]), float.Parse(pEntranceSpawnRot[1]), float.Parse(pEntranceSpawnRot[2]));
                                break;
                            case "IPL":
                                IPL = currValue;
                                break;
                            case "EXIT":
                                string[] pExit = currValue.Split(';');
                                propExit = new Vector3(float.Parse(pExit[0]), float.Parse(pExit[1]), float.Parse(pExit[2]));
                                break;
                            case "EXITSPAWN":
                                string[] pExitSpawn = currValue.Split(';');
                                propExitSpawn = new Vector3(float.Parse(pExitSpawn[0]), float.Parse(pExitSpawn[1]), float.Parse(pExitSpawn[2]));
                                break;
                            case "PRICE":
                                propPrice = int.Parse(currValue);
                                break;
                        }
                    }
                    properties.Add(new Property(IPL, propEntrance, propEntranceSpawn, propEntranceSpawnRot, propExit, propExitSpawn, propPrice));
                }
            }

        }
            
        public Vector3 GetPropertyPosition(string IPL)
        {
            Vector3 pos = new Vector3();
            foreach (Property prop in properties)
            {
                if (prop.IPL == IPL)
                {
                    pos = prop.propEntrance;
                    break;
                }
            }
            return pos;
        }

        public int GetPropertyPrice(string IPL)
        {
            int price = 0;
            foreach (Property prop in properties)
            {
                if (prop.IPL == IPL)
                {
                    price = prop.propPrice;
                    break;
                }
            }
            return price;
        }
        
    }
}
