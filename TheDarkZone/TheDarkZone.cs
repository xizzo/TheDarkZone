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
using TheDarkZone.Managers;

namespace TheDarkZone
{
    public class TheDarkZone : Script
    {

        #region "Variables"

        public bool inDebug = System.IO.Directory.Exists(@"C:\");

        public List<Player> Players = new List<Player>();
        public List<Veh> Vehicles = new List<Veh>();
        public List<Item> Items = new List<Item>();
        public double currUpdateTick = 0;

        private int ClearVehicleAfterTicks = 10;
        private Vector3 lobbySpawnPoint = new Vector3(-75.33064f, -819.012f, 326.175f);
        private Vector3 lobbyRotation = new Vector3(0, 0, -89.63917);

        public KeyManager keys = new KeyManager();
        private VehicleManager vehManager;
        private PropertyManager propManager;
        private UserDataManager userDM;
        private AmmoShopManager ammoSM;
        private WeaponData weaponData;

        static private Random rnd = new Random();

        #endregion

        #region "Missions"

        CollectMission collectMisson;

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
            if (inDebug) API.consoleOutput("!!!!! DEBUG MODE !!!!");
        }

        private void LoadMissions()
        {
            collectMisson = new CollectMission(this);
        }

        #endregion

        #region "Events"

        public void onPlayerConnected(Client sender)
        {
            PutPlayerInLobby(sender);
            Players.Add(new Player(sender, userDM, keys));
            SetPlayerCleanEntityData(sender);
            API.setPlayerSkin(sender, (PedHash)(788443093));
            API.sendChatMessageToAll(GetChatTimeStamp() + " " + sender.name + " joined the server!");
            API.sendChatMessageToPlayer(sender, "Please ~g~/register [username] [password]");
            API.sendChatMessageToPlayer(sender, "or ~g~/login [username] [password]");
            API.sendChatMessageToPlayer(sender, "if you already have an account.");
            API.sendNotificationToPlayer(sender, "Welcome! Please ~g~register ~w~or ~g~login ~w~to continue!", true);
        }

        public void onUpdate()
        {
            //currUpdateTick++;
            //if (currUpdateTick == 2000)
            //{
            //    DoVehicleCleanup();
            //    currUpdateTick = 0;
            //}
        }

        public void onPlayerRespawn(Client sender)
        {
            if (API.getEntityData(sender.handle, keys.KEY_USER_AUTHENTICATED))
            {
                PlayerFreshSpawn(sender);
                GivePlayerOwnedWeapons(sender);
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
                    if ((bool)API.getEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION))
                    {
                        ResetAndDeleteCollShapes(sender);
                    }
                    p.SavePlayerData();
                    API.sendChatMessageToAll("~c~" + GetChatTimeStamp() + " " + sender.name + " left the server! (" + reason + ")");
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
            propManager = new PropertyManager(this);
            vehManager = new VehicleManager(this);
            ammoSM = new AmmoShopManager(this);
            weaponData = new WeaponData();
        }

        public void onVehicleDeath(NetHandle vehicle)
        {
            if(API.hasEntityData(vehicle, keys.KEY_VEHICLE_RESPAWNABLE))
            {
                API.delay(8000, true, () =>
                {
                    VehicleHash vModel = API.getEntityData(vehicle, keys.KEY_VEHICLE_MODEL);
                    Vector3 spawnPos = API.getEntityData(vehicle, keys.KEY_VEHICLE_RESPAWN_POS);
                    Vector3 spawnRot = API.getEntityData(vehicle, keys.KEY_VEHICLE_RESPAWN_ROT);
                    CreateRespawnableVehicle(vModel, spawnPos, spawnRot);

                    API.deleteEntity(vehicle);
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

            if ((bool)API.getEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION))
            {
                API.setEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION, false);
                ResetAndDeleteCollShapes(sender);
                API.sendNotificationToPlayer(sender, "~o~[~r~MISSION~o~] Mission failed!");
            }


            if (!reason.IsNull)
            {
                foreach (Player p in Players)
                {
                    if (p.client.handle == reason)
                    {
                        if (p.client.name == sender.name) break;
                        API.sendNotificationToAll("~g~" + sender.name + " ~w~was killed by ~r~" + p.client.name);
                        return;
                    }
                }
            }
            API.sendNotificationToAll(sender.name + " ~r~died");
        }

        public void onClientEventTrigger(Client sender, string name, object[] args)
        {
            if (!PlayerLoggedIn(sender))
            {
                API.sendNotificationToPlayer(sender, "~r~You need to be logged in first!");
                return;
            }
            switch (name)
            {
                case "RequestNewMission":
                    if ((bool)API.getEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION))
                    {
                        API.sendNotificationToPlayer(sender, "~r~You already have an active mission!");
                        break;
                    }
                    API.sendNotificationToAll("~g~" + sender.name + " ~w~started a ~o~mission");
                    API.setEntityData(sender, keys.KEY_MISSION_CURR_STEP, 0);
                    API.setEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION, true);
                    collectMisson.NextStep(sender);
                    break;
                case "CancelMission" :
                    if (!(bool)API.getEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION))
                    {
                        API.sendNotificationToPlayer(sender, "~r~You don't have an active mission!");
                        break;
                    }
                    API.setEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION, false);
                    ResetAndDeleteCollShapes(sender);
                    API.sendNotificationToPlayer(sender, "~r~Mission has been cancelled!");
                    break;
                case "Suicide":
                    API.setPlayerHealth(sender, -1);
                    break;
                case "BuyApartment":
                    string appartment = args[0].ToString();
                    string appDisplayName = args[1].ToString();
                    if (API.getEntityData(sender, keys.KEY_USER_APARTMENT) == "")
                    {
                        int propPrice = propManager.GetPropertyPrice(appartment);
                        if (GetPlayerMoney(sender) < propPrice)
                        {
                            API.sendNotificationToPlayer(sender, "~r~You do not have enough money to buy this property!");
                            break;
                        }
                        API.setEntityData(sender, keys.KEY_USER_APARTMENT, appartment);
                        if (userDM.SavePlayerAppartment(sender))
                        {
                            RemovePlayerMoney(sender, propPrice);
                            API.triggerClientEvent(sender, "CreateApartmentBlip", propManager.GetPropertyPosition(appartment));
                            API.sendNotificationToAll(sender.name + " ~g~bought ~w~a ~y~" + appDisplayName);
                        }
                        else
                        {
                            API.sendNotificationToPlayer(sender, "~r~Something went wrong, please try again later!");
                            API.setEntityData(sender, keys.KEY_USER_APARTMENT, "");
                        }
                    }
                    else
                    {
                        API.sendNotificationToPlayer(sender,"~r~You already own an appartment!");
                    }
                    break;
                case "SellApartment":
                    if (API.getEntityData(sender, keys.KEY_USER_APARTMENT) != "")
                    {
                        int repay = (int)propManager.GetPropertyPrice(API.getEntityData(sender, keys.KEY_USER_APARTMENT)) / 2;
                        if(userDM.DeletePlayerAppartment(sender))
                        {
                            AddPlayerMoney(sender, repay);
                            API.sendNotificationToPlayer(sender, "You have ~g~sold ~w~your appartment!");
                            API.setEntityData(sender, keys.KEY_USER_APARTMENT, "");
                            API.triggerClientEvent(sender, "DestroyApartmentBlip");
                        }
                        else
                        {
                            API.sendNotificationToPlayer(sender, "~r~Something went wrong, please try again later!");
                        }
                    }
                    else
                    {
                        API.sendNotificationToPlayer(sender, "~r~You don't own an appartment!");
                    }
                    break;
                case "BuyWeapon":
                    string weapon = args[0].ToString();
                    int wepID = weaponData.GetWeaponIdByName(weapon);
                    if (GetPlayerMoney(sender) < weaponData.weapons[wepID].weaponPrice)
                    {
                        API.sendNotificationToPlayer(sender, "~r~You don't have enough money to buy this weapon!");
                        break;
                    }
                    else
                    {
                        string ownedWeps = API.getEntityData(sender, keys.KEY_USER_WEAPONS);
                        if (ownedWeps.Contains(weaponData.weapons[wepID].weaponHash.ToString()))
                        {
                            API.sendNotificationToPlayer(sender, "~r~You already own this weapon!");
                            break;
                        }
                        RemovePlayerMoney(sender, weaponData.weapons[wepID].weaponPrice);
                        API.sendNotificationToPlayer(sender, "You ~g~bought ~w~ a ~y~weapon~w~!");
                        API.givePlayerWeapon(sender, weaponData.weapons[wepID].weaponHash, 99999, false, false);
                        if (ownedWeps != "")
                        {
                            ownedWeps += ";" + weaponData.weapons[wepID].weaponHash.ToString();
                        }
                        else
                        {
                            ownedWeps = weaponData.weapons[wepID].weaponHash.ToString();
                        }
                        API.setEntityData(sender, keys.KEY_USER_WEAPONS, ownedWeps);
                        userDM.SavePlayerWeapons(sender);
                    }
                    break;
            }
        }

        public void onEntityEnterColShape(ColShape shape, NetHandle entity){
            Client sender = API.getPlayerFromHandle(entity);
            if (sender == null) return;
            if (shape.getData(keys.KEY_MARKER_CURR_PLAYERID) != PlayerUserID(sender.handle)) return;
            API.shared.triggerClientEvent(sender, "PlayEnterCheckPointSound");
            switch((int)shape.getData(keys.KEY_MARKER_CURR_MISSION))
            {
                case 1:
                    collectMisson.NextStep(sender, shape);
                    break;
            }

        }

        #endregion 

        #region "Commands"

        #region "Player commands"

        [Command("mod")]
        public void ModVehicle(Client sender, int modType, int mod)
        {
            if (API.isPlayerInAnyVehicle(sender))
            {
                NetHandle vehToMod = API.getPlayerVehicle(sender);
                API.setVehicleMod(vehToMod, modType, mod);
            }
        }

        [Command("skin")]
        public void ChangeSkinCommand(Client sender, PedHash model)
        {
            API.setPlayerSkin(sender, model);
            API.sendNativeToPlayer(sender, 0x45EEE61580806D63, sender.handle);
        }

        #endregion

        #region "Admin commands"

        [Command("givemoney")]
        public void GiveMoney(Client sender, Client target, int amount)
        {
            if (PlayerAdminLevel(sender) == 3)
            {
                AddPlayerMoney(target, amount);
            }
        }

        [Command("app")]
        public void app(Client sender)
        {
            if (PlayerAdminLevel(sender) == 3)
            {
                API.setEntityPosition(sender, new Vector3(-1579.756, -565.0661, 108.523));
            }
        }

        [Command("move")]
        public void MovePlayer(Client sender, float x, float y, float z)
        {
            if (PlayerAdminLevel(sender) == 3)
            {
                Vector3 pos = API.getEntityPosition(sender);
                pos.X += x;
                pos.Y += y;
                pos.Z += z;
                API.setEntityPosition(sender, pos);
            }
        }

        [Command("testpos")]
        public void testpos(Client sender)
        {
            API.setEntityPosition(sender, lobbySpawnPoint);
        }

        [Command("sv")]
        public void SaveVehiclePos(Client sender)
        {
            if (PlayerAdminLevel(sender.handle) == 3)
            {
                SaveVehiclePosInXmlFormat(API.getPlayerFromHandle(sender.handle).position, API.getPlayerFromHandle(sender.handle).rotation);
            }
        }

        [Command("savepos")]
        public void SavePlayerPos(Client sender)
        {
            if (PlayerAdminLevel(sender.handle) == 3)
            {
                var rot = API.getEntityRotation(sender.handle);
                var pos = sender.position;

                API.consoleOutput(rot + "  :  " + pos);

                using (StreamWriter writer = new StreamWriter(@"C:\pos.txt", true))
                {
                    writer.WriteLine(rot + "  :  " + pos);
                }
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
                    API.sendNotificationToAll("A ~y~rocket strike ~w~was ordered on ~r~" + target.name);
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
                API.setEntityDimension(sender, 0);
                API.createParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", pos, new Vector3(), 1f);
                API.createParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", targetPos, new Vector3(), 1f);
                API.setEntityPosition(sender.handle, targetPos);
                API.sendNotificationToPlayer(sender , "You have teleported to " + target.name);
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

        [Command("logout")]
        public void logout(Client sender)
        {
            if(PlayerLoggedIn(sender))
            {
                LogoutPlayer(sender, true);
                API.sendNotificationToPlayer(sender, "You have been ~r~logged out!");
            }
        }

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

        #region "Mission functions"

        private void ResetAndDeleteCollShapes(Client client)
        {
            if (API.getEntityData(client, keys.KEY_MISSION_CURR_COLL) != null)
            {
                API.deleteColShape(API.getEntityData(client, keys.KEY_MISSION_CURR_COLL));
                API.setEntityData(client, keys.KEY_MISSION_CURR_COLL, null);
            }
            API.triggerClientEvent(client, "DestroyLastMissionMarker");
        }

        #endregion

        #region "Player Get Set Data functions"

        public int GetPlayerMoney(Client client)
        {
            return (int)API.getEntitySyncedData(client, keys.KEY_USER_MONEY);
        }

        public void RemovePlayerMoney(Client client, int money)
        {
            int currMoney = (int)API.getEntitySyncedData(client, keys.KEY_USER_MONEY);
            currMoney -= money;
            API.setEntitySyncedData(client, keys.KEY_USER_MONEY, currMoney);
            API.sendNotificationToPlayer(client, "~g~$" + money + " ~w~was ~r~removed ~w~from your account");
            userDM.SavePlayerMoney(client);
        }

        public void AddPlayerMoney(Client client, int money)
        {
            int currMoney = (int)API.getEntitySyncedData(client, keys.KEY_USER_MONEY);
            currMoney += money;
            API.setEntitySyncedData(client, keys.KEY_USER_MONEY, currMoney);
            API.sendNotificationToPlayer(client, "You have received ~g~$" + money);
            userDM.SavePlayerMoney(client);
        }

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

        private void LogoutPlayer(Client sender, bool stayInServer = false)
        {
            foreach (Player p in Players)
            {
                if (p.client == sender)
                {

                    if (p.hasVehicle)
                    {
                        API.deleteEntity(p.vehicle);
                    }
                    if ((bool)API.getEntityData(sender, keys.KEY_USER_HAS_ACTIVE_MISSION))
                    {
                        ResetAndDeleteCollShapes(sender);
                    }
                    p.SavePlayerData();
                    break;
                }
            }
            if (stayInServer)
            {
                SetPlayerCleanEntityData(sender);
                PutPlayerInLobby(sender);
            }
        }

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
            API.setEntityDimension(sender, 0);
            API.sendNotificationToPlayer(sender, "You have successfully ~g~logged in!", false);
            API.setEntityData(sender.handle, keys.KEY_USER_AUTHENTICATED, true);
            API.setEntityData(sender.handle, keys.KEY_USER_ID, id);
            API.getPlayerFromHandle(sender.handle).invincible = false;
            if(API.shared.getEntityData(sender.handle, keys.KEY_USER_APARTMENT) != ""){
                API.triggerClientEvent(sender, "CreateApartmentBlip", propManager.GetPropertyPosition(API.shared.getEntityData(sender.handle, keys.KEY_USER_APARTMENT)));
            }
               
            PlayerFreshSpawn(sender);
            GivePlayerOwnedWeapons(sender);
            API.sendChatMessageToPlayer(sender, "~r~This server is currently under FULL DEVELOPMENT!");
            API.sendChatMessageToPlayer(sender, "~r~For now you are able to spawn vehicles with ~y~/v [vehiclename]");
            API.sendChatMessageToPlayer(sender, "~r~We are working on this server on a daily basis so");
            API.sendChatMessageToPlayer(sender, "~r~make sure to check us out often!");
            API.sendChatMessageToPlayer(sender, "~g~Our goal for this server: character progression, missions");
            API.sendChatMessageToPlayer(sender, "~g~survival elements, pvp, leaderbords and alot more!");
            API.sendChatMessageToPlayer(sender, "~y~ F1 = player menu | F2 = mission menu | F3 = property menu");
        }

        private void GivePlayerOwnedWeapons(Client sender)
        {
            string weapons = (string)API.getEntityData(sender, keys.KEY_USER_WEAPONS);
            if (weapons != "")
            {
                string[] weaponsList = weapons.Split(';');
                foreach (string wep in weaponsList)
                {
                    API.givePlayerWeapon(sender, API.weaponNameToModel(wep), 99999, false, false);
                }
            }
        }

        private void SetPlayerCleanEntityData(Client sender)
        {
            API.setEntityData(sender.handle, keys.KEY_USER_AUTHENTICATED, false);
            API.setEntityData(sender.handle, keys.KEY_USER_ID, 0);
            API.setEntityData(sender.handle, keys.KEY_USER_ADMIN_LEVEL, 0);
            API.setEntityData(sender.handle, keys.KEY_USER_HAS_ACTIVE_MISSION, false);
            API.setEntityData(sender.handle, keys.KEY_USER_APARTMENT, "");

            API.setEntityData(sender.handle, keys.KEY_MISSION_CURR_COLL, null);
            API.setEntityData(sender.handle, keys.KEY_MISSION_CURR_STEP, 0);
            API.setEntityData(sender.handle, keys.KEY_MISSION_CURR_COLLECTION_ID, 0);

            API.setEntitySyncedData(sender, keys.KEY_USER_MONEY, 0);
            API.triggerClientEvent(sender, "DestroyLastMissionMarker");
            API.triggerClientEvent(sender, "DestroyApartmentBlip");
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
            //API.givePlayerWeapon(sender, (WeaponHash)(-619010992), 9999, false, false);
            //API.givePlayerWeapon(sender, (WeaponHash)(-1074790547), 9999, false, false);
            //API.givePlayerWeapon(sender, (WeaponHash)(2132975508), 9999, false, false);
            //API.givePlayerWeapon(sender, (WeaponHash)(100416529), 9999, false, false);
            //API.givePlayerWeapon(sender, (WeaponHash)(-1312131151), 9999, false, false);
            //API.givePlayerWeapon(sender, (WeaponHash)(-619010992), 9999, false, false);

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

        public void CreateRespawnableVehicle(VehicleHash vehicleHash, Vector3 spawnPos, Vector3 spawnRot)
        {
            Vehicle newVeh = API.shared.createVehicle(vehicleHash, spawnPos, spawnRot, GetRandomIntBetween(0, 157), GetRandomIntBetween(0, 157));

            API.shared.setEntityData(newVeh.handle, keys.KEY_VEHICLE_RESPAWNABLE, true);
            API.shared.setEntityData(newVeh.handle, keys.KEY_VEHICLE_RESPAWN_POS, spawnPos);
            API.shared.setEntityData(newVeh.handle, keys.KEY_VEHICLE_RESPAWN_ROT, spawnRot);
            API.shared.setEntityData(newVeh.handle, keys.KEY_VEHICLE_MODEL, vehicleHash);
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

        public double GetRandomDoubleBetween(int min, int max)
        {
            return double.Parse((rnd.Next(min, max).ToString()));
        }

        public int GetRandomIntBetween(int min, int max)
        {
            return rnd.Next(min, max);
        }

        public float GetRandomFloatBetween(int min, int max)
        {
            return float.Parse((rnd.Next(min, max)).ToString());
        }

        private void SaveVehiclePosInXmlFormat(Vector3 vPos, Vector3 rot)
        {
            using (StreamWriter writer = new StreamWriter(@"C:\vehicles.txt", true))
            {
                writer.WriteLine("<spawn>");
                writer.WriteLine("\t<xpos>" + vPos.X.ToString().Replace(',', '.') + "</xpos>");
                writer.WriteLine("\t<ypos>" + vPos.Y.ToString().Replace(',', '.') + "</ypos>");
                writer.WriteLine("\t<zpos>" + vPos.Z.ToString().Replace(',', '.') + "</zpos>");
                writer.WriteLine("\t<xrot>" + rot.X.ToString().Replace(',', '.') + "</xrot>");
                writer.WriteLine("\t<yrot>" + rot.Y.ToString().Replace(',', '.') + "</yrot>");
                writer.WriteLine("\t<zrot>" + rot.Z.ToString().Replace(',', '.') + "</zrot>");
                writer.WriteLine("</spawn>");
            }
        }

        #endregion

        #endregion
    }
}
