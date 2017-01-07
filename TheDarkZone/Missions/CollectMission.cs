using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using TheDarkZone.Data;
using System.Xml.Linq;
using System.IO;

namespace TheDarkZone.Missions
{
    public class CollectMission
    {
        TheDarkZone mainScript { get; set; }
        List<CollectMissionInfo> missionInfos = new List<CollectMissionInfo>();

        public CollectMission(TheDarkZone mainScript)
        {
            this.mainScript = mainScript;
            LoadMissionInfo();
        }

        public void NextStep(Client client, ColShape col = null)
        {
            Vector3 markerPos = new Vector3();
            int currStep = (int)(API.shared.getEntityData(client, mainScript.keys.KEY_MISSION_CURR_STEP));
            switch(currStep)
            {
                case 0 :
                    int randomMission = mainScript.GetRandomIntBetween(0, missionInfos.Count);
                    API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLLECTION_ID, randomMission);
                    SendMissionNotification(client, "Agent, we need you to pickup some DNA samples and delivery them to us asap!");
                    API.shared.sendNotificationToPlayer(client, "~c~TIP: You can see your mission objectives (M) on the map.");

                    markerPos = missionInfos[randomMission].collectPickupPos;
                    CreateNewMarkerForPlayer(client, markerPos);
                    break;

                case 1:
                    SendMissionNotification(client, "Good job agent, now deliver the sample to us.");
                    markerPos = new Vector3(-356.6516f, -113.0946f, 38.69668f - 2);
                    CreateNewMarkerForPlayer(client, markerPos);
                    break;
                case 2:
                    ResetAndDeleteCollShapes(client);
                    SendMissionNotification(client, "You did a good job out there agent. We'll transfer some money over to you!");
                    CompleteMission(client);
                    break;

            }
            API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_STEP, currStep + 1);
        }

        private void CompleteMission(Client client)
        {
            mainScript.AddPlayerMoney(client, missionInfos[API.shared.getEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLLECTION_ID)].moneyReward);
            API.shared.sendNotificationToAll(client.name + " ~g~completed ~w~a ~g~mission");
            API.shared.setEntityData(client, mainScript.keys.KEY_USER_HAS_ACTIVE_MISSION, false);
        }

        private void CreateNewMarkerForPlayer(Client sender, Vector3 pos)
        {
            ResetAndDeleteCollShapes(sender);

            API.shared.triggerClientEvent(sender, "CreateMissionMarker", 1, pos, true);
            ColShape colShape = API.shared.createCylinderColShape(pos, 2.0f, 3.0f);
            colShape.setData(mainScript.keys.KEY_MARKER_CURR_PLAYERID, mainScript.PlayerUserID(sender));
            colShape.setData(mainScript.keys.KEY_MARKER_CURR_MISSION, 1);
            colShape.onEntityEnterColShape += mainScript.onEntityEnterColShape;
            API.shared.setEntityData(sender, mainScript.keys.KEY_MISSION_CURR_COLL, colShape);
        }   

        private void ResetAndDeleteCollShapes(Client client)
        {
            if(API.shared.getEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL) != null)
            {
                API.shared.deleteColShape(API.shared.getEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL));
                API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL, null);
            }
            API.shared.triggerClientEvent(client, "DestroyLastMissionMarker");
        }

        private void DeleteCurrColShape(Client client, ColShape colShape)
        {
            API.shared.deleteColShape(colShape);
            API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL, null);
        }

        private void SendMissionNotification(Client sender, string txt)
        {
            API.shared.sendNotificationToPlayer(sender, "~o~[~r~MISSION~o~]: " + txt);
        }

        private void LoadMissionInfo()
        {
            XDocument xDoc;
            using(StreamReader rdr = new StreamReader(@"resources/TheDarkZone/Missions/MissionsData/collectmissiondata.xml"))
            {
                xDoc = XDocument.Parse(rdr.ReadToEnd());
            }
            foreach(XElement elemMission in xDoc.Root.Elements())
            {
                if (elemMission.Name == "mission")
                {
                    CollectMissionInfo missionInfo = new CollectMissionInfo();
                    missionInfo.collectPickupPos = new Vector3();
                    foreach (XElement elemInfo in elemMission.Elements())
                    {
                        string currValue = elemInfo.Value;
                        if (mainScript.inDebug) currValue = currValue.Replace('.', ',');
                        switch (elemInfo.Name.ToString())
                        {
                            case "xpos":
                                missionInfo.collectPickupPos.X = float.Parse(currValue);
                                break;
                            case "ypos":
                                missionInfo.collectPickupPos.Y = float.Parse(currValue);
                                break;
                            case "zpos":
                                missionInfo.collectPickupPos.Z = float.Parse(currValue);
                                break;
                            case "reward":
                                missionInfo.moneyReward = int.Parse(currValue);
                                break;
                        }
                    }
                    missionInfos.Add(missionInfo);
                }
            }
        }

    }
}
