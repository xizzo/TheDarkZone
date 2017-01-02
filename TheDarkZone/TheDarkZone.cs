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
using TheDarkZone.Missions;

namespace TheDarkZone
{
    public class TheDarkZone : Script
    {

        #region "Variables"

        public static List<Player> Players = new List<Player>();
        public static List<Veh> Vehicles = new List<Veh>();
        public static List<Item> Items = new List<Item>();
        public double currUpdateTick = 0;
        public KeyManager keys = new KeyManager();

        private UserDataManager userDM;
        private int ClearVehicleAfterTicks = 10;
        private Vector3 lobbySpawnPoint = new Vector3(-75.33064f, -819.012f, 326.175f);
        private Vector3 lobbyRotation = new Vector3(0, 0, -89.63917);

        #endregion

        #region "Missions"

        mission1 miss1;

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
            API.onChatMessage += onChatMessage;
            API.onPlayerDeath += onPlayerDeath;
            API.onClientEventTrigger += onClientEventTrigger;
            LoadMissions();
        }

        private void LoadMissions()
        {
            miss1 = new mission1(this);
        }

        #endregion

        #region "Events"

        public void onPlayerConnected(Client sender)
        {
            PutPlayerInLobby(sender);
            Players.Add(new Player(sender, userDM, keys));
            SetPlayerCleanEntityData(sender);
            API.setPlayerSkin(sender, (PedHash)(788443093));
            API.sendChatMessageToAll("~c~[" + DateTime.Now.ToString("HH:mm") + "] " + sender.name + " joined the server!");
            API.sendChatMessageToPlayer(sender, "Please ~g~/register [username] [password]");
            API.sendChatMessageToPlayer(sender, "or ~g~/login [username] [password]");
            API.sendChatMessageToPlayer(sender, "if you already have an account.");
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
            if (API.getEntityData(sender.handle, keys.KEY_USER_AUTHENTICATED))
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

                    API.sendChatMessageToAll("~c~" + sender.name + " left the server! (" + reason + ")");
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
            userDM = new UserDataManager();
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

        public void onChatMessage(Client sender, string message, CancelEventArgs e)
        {
            if(API.getEntityData(sender.handle, keys.KEY_USER_ADMIN_LEVEL) == 3)
            {
                API.sendChatMessageToAll("~c~[" + DateTime.Now.ToString("HH:mm") + "]~w~[~b~ADMIN~w~] ~y~" + sender.name + "~w~: " + message);
            }
            else
            {
                API.sendChatMessageToAll("~c~[" + DateTime.Now.ToString("HH:mm") + "] ~y~" + sender.name + "~w~: " + message);
            }
            e.Cancel = true;
        }

        public void onPlayerDeath(Client sender, NetHandle reason, int weapon)
        {
            if (!reason.IsNull)
            {
                foreach (Player p in Players)
                {
                    if (p.client.handle == reason)
                    {
                        if (p.client.name == sender.name) break;
                        API.sendChatMessageToAll(GetChatTimeStamp() + " " + sender.name + " was killed by " + p.client.name+ ".");
                        API.sendNotificationToAll(sender.name + " was killed by " + p.client.name);
                        return;
                    }
                }
            }
            API.sendNotificationToAll(sender.name + " died");
        }

        public void onClientEventTrigger(Client sender, string name, object[] args)
        {
            switch (name)
            {
                case "test":
                    // do something
                    break;
            }
        }

        public void onEntityEnterColShape(ColShape shape, NetHandle entity){
            Client sender = API.getPlayerFromHandle(entity);
            if (sender == null) return;
            if (shape.getData(keys.KEY_MARKER_CURR_PLAYERID) != PlayerUserID(sender.handle)) return;
            switch((int)shape.getData(keys.KEY_MARKER_CURR_MISSION))
            {
                case 1:
                    miss1.NextStep(sender, shape);
                    break;
            }
            API.triggerClientEvent(sender, "DestroyLastMissionMarker");
            API.deleteColShape(shape);
        }

        #endregion 

        #region "Commands"

        #region "Player commands"

        [Command("skin")]
        public void ChangeSkinCommand(Client sender, PedHash model)
        {
            API.setPlayerSkin(sender, model);
            API.sendNativeToPlayer(sender, 0x45EEE61580806D63, sender.handle);
        }

        [Command("mission")]
        public void Mission(Client sender)
        {
            API.setEntityData(sender, keys.KEY_MISSION_CURR_STEP, 0);
            miss1.NextStep(sender);
        }

        #endregion

        #region "Admin commands"

        [Command("testpos")]
        public void testpos(Client sender)
        {
            API.setEntityPosition(sender, lobbySpawnPoint);
        }

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
            Player p = GetPlayerObjFromClient(sender);
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

        [Command("notifyall", GreedyArg=true)]
        public void NotifyAll(Client sender, string txt)
        {
            if (PlayerAdminLevel(sender.handle) == 3)
            {
                API.sendNotificationToAll("[ADMIN] " + sender.name + ": " + txt);
            }
        }

        [Command("nuke")]
        public void Nuke(Client sender, Client target)
        {
            if (PlayerAdminLevel(sender.handle) == 3)
            {
                var t = Task.Run(async delegate
                {
                    API.sendChatMessageToPlayer(sender, "~g~Target " + target.name + " is beeing nuked!");
                    API.sendNotificationToAll("A rocket strike was ordered on " + target.name);
                    for (int i = 0; i < 20; i++)
                    {
                        if (API.getPlayerHealth(target) == 0) break;
                        if (!API.isPlayerConnected(target)) break;
                        double x = target.position.X + GetRandomDoubleBetween((int)(-25), (int)(25));
                        double y = target.position.Y + GetRandomDoubleBetween((int)(-25), (int)(25));
                        double z = target.position.Z + 20;
                        Vector3 projStart = new Vector3(x, y, z);
                        Vector3 projTarget = new Vector3(x, y, (z - 999999));

                        API.createProjectile((WeaponHash)(-1312131151), projStart, projTarget, 0, GetRandomFloatBetween(7, 15));
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    }
                });
            }
        }

        [Command("tp")]
        public void TeleportTo(Client sender, Client target)
        {
            if (PlayerAdminLevel(sender.handle) != 4 && PlayerLoggedIn(sender.handle))
            {
                Vector3 pos = sender.position;
                Vector3 targetPos = new Vector3(target.position.X + 4, target.position.Y + 4, target.position.Z);
                API.createParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", pos, new Vector3(), 1f);
                API.createParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", targetPos, new Vector3(), 1f);
                API.setEntityPosition(sender.handle, targetPos);
                API.sendNotificationToPlayer(target, "You have teleported to " + target.name);
            }
        }

        [Command("tptm")]
        public void TeleportToMe(Client sender, Client target)
        {
            if (PlayerAdminLevel(sender.handle) == 3)
            {
                Vector3 pos = target.position;
                Vector3 targetPos = new Vector3(sender.position.X + 4, sender.position.Y + 4, sender.position.Z);
                API.createParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", pos, new Vector3(), 1f);
                API.createParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", targetPos, new Vector3(), 1f);
                API.setEntityPosition(target, targetPos);

                API.sendNotificationToPlayer(sender, target.name + " has been teleported to you");
                API.sendNotificationToPlayer(target, "You have been teleported to " + sender.name);
            }
        }

        [Command("serverinfo")]
        public void ServerInfo(Client sender)
        {
            if (PlayerAdminLevel(sender.handle) == 3)
            {
                API.sendChatMessageToAll("~g~This server will have a gamemode based on the darkzone of The Divison.");
                API.sendChatMessageToAll("~g~Players will be able to progress their characters by doing missions, survive and pvp.");
                API.sendChatMessageToAll("~g~There will be randomized events like loot drops in the world for which players have to fight for.");
            }
        }

        #endregion

        #region "Account commands"

        [Command(SensitiveInfo = true)]
        public void Login(Client sender, string username, string password)
        {
            if (!PlayerLoggedIn(sender.handle))
            {
                int uID = userDM.UserAccountExists(username, password);
                if (uID != 0)
                {
                    PlayerLoginSuccessfull(sender, uID);
                }
                else
                {
                    API.sendChatMessageToPlayer(sender, "~r~No account exists with those credentials!");
                    API.sendNotificationToPlayer(sender, "Failed to login!");
                }
            }
        }

        [Command(SensitiveInfo = true)]
        public void Register(Client sender, string username, string password)
        {
            if (!PlayerLoggedIn(sender.handle))
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
        }

        #endregion

        #endregion

        #region "Functions"

        #region "Player Get Data functions"

        public int PlayerUserID(NetHandle nethandle)
        {
            return API.getEntityData(nethandle, keys.KEY_USER_ID);
        }

        public int PlayerAdminLevel(NetHandle netHandle)
        {
            return API.getEntityData(netHandle, keys.KEY_USER_ADMIN_LEVEL);
        }

        public bool PlayerLoggedIn(NetHandle nethandle)
        {
            return API.getEntityData(nethandle, keys.KEY_USER_AUTHENTICATED);
        }

        #endregion

        #region "Player Functions"

        private void PlayerLoginSuccessfull(Client sender, int id)
        {
            foreach (Player p in Players)
            {
                if (sender == p.client)
                {
                    p.userID = id;
                    p.LoadPlayerData();
                    break;
                }
            }
            
            API.sendChatMessageToPlayer(sender, "~g~You are now logged in.");
            API.sendNotificationToPlayer(sender, "You have successfully logged in!", false);
            API.setEntityData(sender.handle, keys.KEY_USER_AUTHENTICATED, true);
            API.setEntityData(sender.handle, keys.KEY_USER_ID, id);
            API.setEntityDimension(sender, 0);
            API.getPlayerFromHandle(sender.handle).invincible = false;
            PlayerFreshSpawn(sender);
            API.sendChatMessageToPlayer(sender, "~r~This server is currently under FULL DEVELOPMENT!");
            API.sendChatMessageToPlayer(sender, "~r~For now you are able to spawn vehicles with ~y~/v [vehiclename]");
            API.sendChatMessageToPlayer(sender, "~r~We are working on this server on a daily basis so");
            API.sendChatMessageToPlayer(sender, "~r~make sure to check us out often!");
            API.sendChatMessageToPlayer(sender, "~g~Our goal for this server: character progression, missions");
            API.sendChatMessageToPlayer(sender, "~g~survival elements, pvp, leaderbords and alot more!");
        }

        private void SetPlayerCleanEntityData(Client sender)
        {
            API.setEntityData(sender.handle, keys.KEY_USER_AUTHENTICATED, false);
            API.setEntityData(sender.handle, keys.KEY_USER_ID, 0);
            API.setEntityData(sender.handle, keys.KEY_USER_ADMIN_LEVEL, 0);

            API.setEntityData(sender.handle, keys.KEY_MISSION_CURR_COLL, null);
            API.setEntityData(sender.handle, keys.KEY_MISSION_CURR_STEP, 0);
        }

        private void PutPlayerInLobby(Client sender)
        {
            API.setEntityPosition(sender, lobbySpawnPoint);
            API.setEntityRotation(sender, lobbyRotation);
            API.freezePlayer(sender, true);
            API.setEntityDimension(sender, 1);
            API.getPlayerFromHandle(sender.handle).invincible = true;
        }

        private void PlayerFreshSpawn(Client sender)
        {
            API.freezePlayer(sender, false);
            API.setEntityPosition(sender.handle, new Vector3(-276.4822, -891.2561, 1066.544));
            API.setEntityRotation(sender.handle, new Vector3(-5.371634, 7.375679, 19.1878));
            API.givePlayerWeapon(sender, (WeaponHash)(-72657034), 1, true, true);
            API.givePlayerWeapon(sender, (WeaponHash)(-619010992), 9999, false, false);
            API.givePlayerWeapon(sender, (WeaponHash)(-1074790547), 9999, false, false);
            API.givePlayerWeapon(sender, (WeaponHash)(2132975508), 9999, false, false);
            API.givePlayerWeapon(sender, (WeaponHash)(100416529), 9999, false, false);
            API.givePlayerWeapon(sender, (WeaponHash)(-1312131151), 9999, false, false);
            API.givePlayerWeapon(sender, (WeaponHash)(-619010992), 9999, false, false);

        }

        private Player GetPlayerClientObjByHandle(NetHandle handle)
        {
            foreach (Player p in Players)
            {
                if (p.client.handle == handle)
                {
                    return p;
                }
            }
            return new Player();
        }

        private Player GetPlayerObjFromClient(Client sender)
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

        #region "Random functions"

        private string GetChatTimeStamp()
        {
            return "~c~[" + DateTime.Now.ToString("HH:mm") + "]";
        }

        private double GetRandomDoubleBetween(int min, int max)
        {
            Random r = new Random();
            return double.Parse((r.Next(min, max).ToString()));
        }

        private double GetRandomIntBetween(int min, int max)
        {
            Random r = new Random();
            return r.Next(min, max);
        }

        private float GetRandomFloatBetween(int min, int max)
        {
            Random r = new Random();
            return float.Parse((r.Next(min, max)).ToString());
        }
        #endregion

        #endregion
    }
}
