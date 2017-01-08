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
    public class AmmoShopManager
    {
        public TheDarkZone mainScript { get; set; }
        public List<AmmoShop> ammoShops = new List<AmmoShop>();
        

        public AmmoShopManager(TheDarkZone mainScript)
        {
            this.mainScript = mainScript;
            LoadAmmoShops();
            API.shared.consoleOutput("Ammo shops loaded: " + ammoShops.Count());
            CreateAmmoShops();
        }

        private void CreateAmmoShops()
        {
            foreach (AmmoShop ammoShop in ammoShops)
            {
                NetHandle shopBlip = API.shared.createBlip(ammoShop.entrance, 100f, 0);
                API.shared.setBlipSprite(shopBlip, 110);

                API.shared.createMarker(1, new Vector3(ammoShop.entrance.X, ammoShop.entrance.Y, ammoShop.entrance.Z - 1), new Vector3(), new Vector3(), new Vector3(0.8, 0.8, 0.5), 241, 247, 57, 180);
                API.shared.createMarker(1, new Vector3(ammoShop.exit.X, ammoShop.exit.Y, ammoShop.exit.Z - 1), new Vector3(), new Vector3(), new Vector3(0.8, 0.8, 0.5), 241, 247, 57, 180);
                API.shared.createMarker(1, new Vector3(ammoShop.buyArea.X, ammoShop.buyArea.Y, ammoShop.buyArea.Z - 1), new Vector3(), new Vector3(), new Vector3(0.8, 0.8, 0.5), 241, 247, 57, 180);

                CylinderColShape colEntrance = API.shared.createCylinderColShape(ammoShop.entrance, 0.8f, 0.5f);
                colEntrance.setData(mainScript.keys.AMMOSHOP_TELEPORT_TO, ammoShop.entranceSpawn);
                colEntrance.setData(mainScript.keys.AMMOSHOP_TELEPORT_IN_OUT, true);
                colEntrance.onEntityEnterColShape += onPlayerEnterDoorMarker;

                CylinderColShape colExit = API.shared.createCylinderColShape(ammoShop.exit, 0.8f, 0.5f);
                colExit.setData(mainScript.keys.AMMOSHOP_TELEPORT_TO, ammoShop.exitSpawn);
                colExit.setData(mainScript.keys.AMMOSHOP_TELEPORT_IN_OUT, false);
                colExit.onEntityEnterColShape += onPlayerEnterDoorMarker;

                CylinderColShape colBuyArea = API.shared.createCylinderColShape(ammoShop.buyArea, 0.8f, 0.5f);
                colBuyArea.onEntityEnterColShape += onPlayerEnterBuyArea;
                colBuyArea.onEntityExitColShape += onPlayerExitBuyArea;

                API.shared.createPed((PedHash)(ammoShop.pedHash), ammoShop.npcSpawn, ammoShop.npcSpawnRot, 0);
            }
        }

        private void onPlayerEnterBuyArea(ColShape shape, NetHandle entity)
        {
            API.shared.triggerClientEvent(API.shared.getPlayerFromHandle(entity), "ShowBuyWeaponMenu");
        }

        private void onPlayerExitBuyArea(ColShape shape, NetHandle entity)
        {
            API.shared.triggerClientEvent(API.shared.getPlayerFromHandle(entity), "CloseBuyWeaponmenu");
        }

        private void onPlayerEnterDoorMarker(ColShape shape, NetHandle entity)
        {
            if (shape.getData(mainScript.keys.AMMOSHOP_TELEPORT_IN_OUT))
            {
                API.shared.setEntityDimension(entity, API.shared.getEntityData(entity, mainScript.keys.KEY_USER_ID));
            }
            else
            {
                API.shared.setEntityDimension(entity, 0);
            }
            API.shared.setEntityPosition(entity, shape.getData(mainScript.keys.AMMOSHOP_TELEPORT_TO));
        }

        private void LoadAmmoShops()
        {
            XDocument xDoc;
            using (StreamReader rdr = new StreamReader(@"resources/TheDarkZone/Managers/Data/ammoshops.xml"))
            {
                xDoc = XDocument.Parse(rdr.ReadToEnd());
            }

            foreach (XElement xAmmoShops in xDoc.Root.Elements())
            {
                if(xAmmoShops.Name == "shop")
                {
                    Vector3 entrance = new Vector3();
                    Vector3 entranceSpawn= new Vector3();
                    Vector3 exit = new Vector3();
                    Vector3 exitSpawn = new Vector3();
                    Vector3 buyArea = new Vector3();
                    Vector3 npcSpawn  = new Vector3();
                    float npcSpawnRot = 0f;
                    PedHash npcHash = new PedHash();

                    foreach (XElement xShop in xAmmoShops.Elements())
                    {
                        string currValue = xShop.Value;
                        if (mainScript.inDebug) currValue = currValue.Replace('.', ',');
                        switch (xShop.Name.ToString().ToUpper())
                        {
                            case "ENTRANCE":
                                string[] pEntrance = currValue.Split(';');
                                entrance = new Vector3(float.Parse(pEntrance[0]), float.Parse(pEntrance[1]), float.Parse(pEntrance[2]));
                                break;
                            case "ENTRANCESPAWN":
                                string[] pEntranceSpawn = currValue.Split(';');
                                entranceSpawn = new Vector3(float.Parse(pEntranceSpawn[0]), float.Parse(pEntranceSpawn[1]), float.Parse(pEntranceSpawn[2]));
                                break;
                            case "EXIT":
                                string[] pExit = currValue.Split(';');
                                exit = new Vector3(float.Parse(pExit[0]), float.Parse(pExit[1]), float.Parse(pExit[2]));
                                break;
                            case "EXITSPAWN":
                                string[] pExitSpawn = currValue.Split(';');
                                exitSpawn = new Vector3(float.Parse(pExitSpawn[0]), float.Parse(pExitSpawn[1]), float.Parse(pExitSpawn[2]));
                                break;
                            case "BUYAREA":
                                string[] pBuyArea = currValue.Split(';');
                                buyArea = new Vector3(float.Parse(pBuyArea[0]), float.Parse(pBuyArea[1]), float.Parse(pBuyArea[2]));
                                break;
                            case "NPCSPAWN":
                                string[] pNpcSpawn = currValue.Split(';');
                                npcSpawn = new Vector3(float.Parse(pNpcSpawn[0]), float.Parse(pNpcSpawn[1]), float.Parse(pNpcSpawn[2]));
                                break;
                            case "NPCSPAWNROT":
                                npcSpawnRot = float.Parse(currValue);
                                break;
                            case "NPCHASH":
                                npcHash = (PedHash)(int.Parse(currValue));
                                break;
                        }
                    }
                    ammoShops.Add(new AmmoShop(entrance, entranceSpawn, exit, exitSpawn, buyArea, npcSpawn, npcSpawnRot, npcHash));
                }
            }
        }
    }
}
