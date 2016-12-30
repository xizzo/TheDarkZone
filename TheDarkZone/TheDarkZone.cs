using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using TheDarkZone.Structure;
using TheDarkZone.Data;

namespace TheDarkZone
{
    public class TheDarkZone : Script
    {

        #region "Variables"

        public static List<Player> Players = new List<Player>();
        public static List<Veh> Vehicles = new List<Veh>();
        public static List<Item> Items = new List<Item>();
        public double currUpdateTick = 0;

        private UserDataManager userDM;
        private int ClearVehicleAfterTicks = 10;
        private Vector3 lobbySpawnPoint = new Vector3(-75.33064f, -819.012f, 326.175f);
        private Vector3 lobbyRotation = new Vector3(0, 0, -89.63917);

        #region "Data Keys"

        private string KEY_USER_AUTHENTICATED = "USER_AUTHENTICATED";
        private string KEY_USER_ID = "USER_ID";

        #endregion

        #endregion

        #region "Initialize"

        public TheDarkZone()
        {
            API.onResourceStart += onResourceStart;
            API.onPlayerConnected += onPlayerConnected;
            API.onUpdate += onUpdate;
            API.onPlayerRespawn += onPlayerRespawn;
            API.onPlayerDisconnected += onPlayerDisconnected;
            API.onVehicleDeath += onVehicleDeath;
            API.onPlayerEnterVehicle += onPlayerEnterVehicle;
            API.onPlayerPickup += onPlayerPickup;
        }

        #endregion

        #region "Events"

        public void onPlayerConnected(Client sender)
        {
            PutPlayerInLobby(sender);
            Players.Add(new Player(sender, sender.handle));
            SetPlayerCleanEntityData(sender);
            API.setPlayerSkin(sender, (PedHash)(788443093));
            API.sendChatMessageToAll(sender.name + " connected to the server!");
            API.sendChatMessageToPlayer(sender, "Please ~g~/register [username] [password]");
            API.sendChatMessageToPlayer(sender, "or ~g~/login [username] [password] ~w~if you already have an account.");
            API.sendNotificationToPlayer(sender, "Welcome! Please register or login to continue!", true);
        }

        public void onUpdate()
        {
            currUpdateTick++;
            if (currUpdateTick == 2000)
            {
                DoVehicleCleanup();
                currUpdateTick = 0;
            }
        }

        public void onPlayerRespawn(Client sender)
        {
            if (API.getEntityData(sender.handle, KEY_USER_AUTHENTICATED))
            {
                PlayerFreshSpawn(sender);
            }
            else
            {
                PutPlayerInLobby(sender);
            }
        }

        public void onPlayerDisconnected(Client sender, string reason)
        {
            foreach (Player p in Players)
            {
                if (p.client == sender)
                {

                    if (p.hasVehicle)
                    {
                        API.deleteEntity(p.vehicle);
                    }

                    API.sendChatMessageToAll(sender.name + " left the server! (" + reason + ")");
                    Players.Remove(p);
                    break;
                }
            }
        }

        public void onResourceStart()
        {
            API.consoleOutput("The Dark Zone script loaded");
            API.setWeather(11);
            API.setTime(5, 0);
            userDM = new UserDataManager(this);
        }

        public void onVehicleDeath(NetHandle vehicle)
        {
            if (API.getEntityData(vehicle, "RESPAWNABLE") == true)
            {
                API.delay(8000, true, () =>
                {
                    var model = API.getEntityModel(vehicle);
                    var spawnPos = API.getEntityData(vehicle, "SPAWN_POS");
                    var spawnRot = API.getEntityData(vehicle, "SPAWN_ROT");

                    API.deleteEntity(vehicle);

                    var veh = API.createVehicle((VehicleHash)model, spawnPos, spawnRot, 0, 0);

                    API.setEntityData(veh, "RESPAWNABLE", true);
                    API.setEntityData(veh, "SPAWN_POS", spawnPos);
                    API.setEntityData(veh, "SPAWN_ROT", spawnRot);
                    API.setEntityData(veh, "HasBeenUsed", false);
                    API.setEntityData(veh, "CleanupTicks", 0);
                });
            }
        }

        public void onPlayerEnterVehicle(Client sender, NetHandle vehicle)
        {
            API.givePlayerWeapon(sender, (WeaponHash)(-72657034), 1, true, true);
            API.setEntityData(vehicle, "HasBeenUsed", true);
            API.setEntityData(vehicle, "CleanupTicks", 0);
        }

        public void onPlayerPickup(Client sender, NetHandle pickup)
        {
            API.sendChatMessageToPlayer(sender, "pickup");
        }

        #endregion 

        #region "Commands"

        #region "Admin commands"

        [Command("savepos")]
        public void SavePlayerPos(Client sender)
        {

            var rot = API.getEntityRotation(sender.handle);
            var pos = sender.position;

            API.consoleOutput(rot + "  :  " + pos);

            using (StreamWriter writer = new StreamWriter(@"C:\pos.txt", true))
            {
                writer.WriteLine(rot + "  :  " + pos);
            }
        }

        [Command("v")]
        public void SpawnCarCommand(Client sender, VehicleHash model)
        {
            Player p = GetPlayerClientObj(sender);
            if (p.hasVehicle)
            {
                API.deleteEntity(p.vehicle);
            }
            else
            {
                p.hasVehicle = true;
            }

            var rot = API.getEntityRotation(p.client.handle);
            p.vehicle = API.createVehicle(model, p.client.position, new Vector3(0, 0, rot.Z), 0, 0);
            API.setEntityData(p.vehicle, "RESPAWNABLE", false);
            API.setPlayerIntoVehicle(p.client, p.vehicle, -1);

        }

        #endregion

        #region "Account commands"

        [Command(SensitiveInfo = true)]
        public void Login(Client sender, string username, string password)
        {
            int uID = userDM.UserAccountExists(username, password);
            if (uID != 0)
            {
                PlayerLoginSuccessfull(sender, uID);
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "~r~No Account exists with those credentials!");
                API.sendNotificationToPlayer(sender, "Failed to login!");
            }
        }

        [Command(SensitiveInfo = true)]
        public void Register(Client sender, string username, string password)
        {
            if (userDM.UserNameExists(username))
            {
                API.sendChatMessageToPlayer(sender, "~r~Failed to register, because username exists already!");
                API.sendNotificationToPlayer(sender, "Registration failed!", true);
                return;
            }
            int uID = userDM.CreateUserAccount(username, password);
            if (uID != 0)
            {
                API.sendNotificationToPlayer(sender, "Registration complete!", true);
                API.sendChatMessageToPlayer(sender, "Thank you for registering!");
                PlayerLoginSuccessfull(sender, uID);
            }
            else
            {
                API.sendNotificationToPlayer(sender, "Failed to register!", true);
                API.sendChatMessageToPlayer(sender, "~r~Failed to register!");
            }
        }

        #endregion

        #endregion

        #region "Functions"

        #region "Player Functions"

        private void PlayerLoginSuccessfull(Client sender, int id)
        {
            API.sendChatMessageToPlayer(sender, "~g~You are now logged in.");
            API.sendNotificationToPlayer(sender, "You have successfully logged in!", false);
            API.setEntityData(sender.handle, KEY_USER_AUTHENTICATED, true);
            API.setEntityData(sender.handle, KEY_USER_ID, id);
            PlayerFreshSpawn(sender);
        }

        private void SetPlayerCleanEntityData(Client sender)
        {
            API.setEntityData(sender.handle, KEY_USER_AUTHENTICATED, false);
            API.setEntityData(sender.handle, KEY_USER_ID, 0);
        }

        private void PutPlayerInLobby(Client sender)
        {
            API.setEntityPosition(sender, lobbySpawnPoint);
            API.setEntityRotation(sender, lobbyRotation);
            API.freezePlayer(sender, true);
        }

        private void PlayerFreshSpawn(Client sender)
        {
            API.freezePlayer(sender, false);
            API.setEntityPosition(sender.handle, new Vector3(-276.4822, -891.2561, 1066.544));
            API.setEntityRotation(sender.handle, new Vector3(-5.371634, 7.375679, 19.1878));
            API.givePlayerWeapon(sender, (WeaponHash)(-72657034), 1, true, true);
        }

        private Player GetPlayerClientObjByHandle(NetHandle handle)
        {
            foreach (Player p in Players)
            {
                if (p.netHandle == handle)
                {
                    return p;
                }
            }
            return new Player();
        }

        private Player GetPlayerClientObj(Client sender)
        {
            foreach (Player p in Players)
            {
                if (p.client == sender)
                {
                    return p;
                }
            }
            return new Player();
        }

        #endregion

        #region "Public functions"


        #endregion 

        #region "Vehicle functions"

        private void GetAllVehicleData()
        {
            using (StreamReader rdr = new StreamReader(@"resources\airportstrip\vehicles.txt"))
            {
                while (!rdr.EndOfStream)
                {
                    string s = rdr.ReadLine();
                    string[] vehData = s.Split(';');

                    Vector3 pos = new Vector3(double.Parse(vehData[1]), double.Parse(vehData[2]), double.Parse(vehData[3]));
                    Vector3 rot = new Vector3(double.Parse(vehData[4]), double.Parse(vehData[5]), double.Parse(vehData[6]));
                    VehicleHash modelVeh = API.vehicleNameToModel(vehData[0]);

                    Veh veh = new Veh(pos, rot, modelVeh);
                    Vehicles.Add(veh);

                }
            }
        }

        private void SpawnAllVehicles()
        {
            API.consoleOutput("Spawning all vehicles");
            foreach (NetHandle h in API.getAllVehicles())
            {
                API.deleteEntity(h);
            }

            foreach (Veh veh in Vehicles)
            {
                var car = API.createVehicle(veh.Hash, veh.position, veh.rotation, 0, 0);
                API.setEntityData(car, "RESPAWNABLE", true);
                API.setEntityData(car, "SPAWN_POS", veh.position);
                API.setEntityData(car, "SPAWN_ROT", veh.rotation);
                API.setEntityData(car, "HasBeenUsed", false);
                API.setEntityData(car, "CleanupTicks", 0);
            }

        }

        private void DoVehicleCleanup()
        {
            try
            {
                foreach (NetHandle veh in API.getAllVehicles())
                {
                    if (API.getEntityData(veh, "RESPAWNABLE"))
                    {
                        if (API.getEntityData(veh, "HasBeenUsed"))
                        {
                            bool vehInUse = false;
                            foreach (Player p in Players)
                            {
                                if (API.isPlayerInAnyVehicle(p.client))
                                {
                                    if (API.getPlayerVehicle(p.client) == veh)
                                    {
                                        vehInUse = true;
                                        break;
                                    }
                                }
                            }
                            if (!vehInUse)
                            {
                                if (API.getEntityData(veh, "CleanupTicks") != ClearVehicleAfterTicks)
                                {
                                    API.setEntityData(veh, "CleanupTicks", API.getEntityData(veh, "CleanupTicks") + 1);
                                    continue;
                                }
                                var model = API.getEntityModel(veh);
                                var spawnPos = API.getEntityData(veh, "SPAWN_POS");
                                var spawnRot = API.getEntityData(veh, "SPAWN_ROT");

                                API.deleteEntity(veh);

                                var vehicle = API.createVehicle((VehicleHash)model, spawnPos, spawnRot, 0, 0);

                                API.setEntityData(vehicle, "RESPAWNABLE", true);
                                API.setEntityData(vehicle, "SPAWN_POS", spawnPos);
                                API.setEntityData(vehicle, "SPAWN_ROT", spawnRot);
                                API.setEntityData(vehicle, "HasBeenUsed", false);
                                API.setEntityData(vehicle, "CleanupTicks", 0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                API.consoleOutput("caught error: " + ex.Message);
            }
        }

        #endregion

        #region "Pickup functions"

        private void GetAllPickupData()
        {
            using (StreamReader rdr = new StreamReader(@"resources\airportstrip\pickups.txt"))
            {
                while (!rdr.EndOfStream)
                {
                    string s = rdr.ReadLine();
                    string[] pickupData = s.Split(';');
                    Vector3 pos = new Vector3(double.Parse(pickupData[1]), double.Parse(pickupData[2]), double.Parse(pickupData[3]));
                    Vector3 rot = new Vector3(double.Parse(pickupData[4]), double.Parse(pickupData[5]), double.Parse(pickupData[6]));
                    PickupHash modelPickup = (PickupHash)API.getHashKey(pickupData[0]);

                    Item item = new Item(modelPickup, pos, rot, 5, pickupData[0]);
                    Items.Add(item);
                }
            }
        }

        private void SpawnAllPickupItems()
        {
            API.consoleOutput("Spawning all items");
            foreach (NetHandle p in API.getAllPickups())
            {
                API.deleteEntity(p);
            }

            foreach (Item item in Items)
            {
                item.netHandle = API.createPickup(item.model, item.pos, item.rot, 1, item.respawnTime);
            }
        }

        #endregion

        #endregion
    }
}
