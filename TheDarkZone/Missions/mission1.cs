using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using TheDarkZone.Data;

namespace TheDarkZone.Missions
{
    public class mission1
    {
        TheDarkZone mainScript { get; set; }

        public mission1(TheDarkZone mainScript)
        {
            this.mainScript = mainScript;
        }

        public void NextStep(Client client, ColShape col = null)
        {
            int currStep = (int)(API.shared.getEntityData(client, mainScript.keys.KEY_MISSION_CURR_STEP));
            switch(currStep)
            {
                case 0:
                    ResetAndDeleteCollShapes(client);
                    Vector3 markerPos = new Vector3(client.position.X + 5, client.position.Y + 5, client.position.Z - 2);
                    
                    API.shared.triggerClientEvent(client, "CreateMissionMarker", 1, markerPos);
                    
                    ColShape colShape = API.shared.createCylinderColShape(markerPos, 2.0f, 3.0f);
                    colShape.setData(mainScript.keys.KEY_MARKER_CURR_PLAYERID, mainScript.PlayerUserID(client));
                    colShape.setData(mainScript.keys.KEY_MARKER_CURR_MISSION, 1);
                    colShape.onEntityEnterColShape += mainScript.onEntityEnterColShape;

                    API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL, colShape);
                    break;
                case 1:
                    DeleteCurrColShape(client, col);
                    API.shared.triggerClientEvent(client, "DestroyLastMissionMarker");
                    break;
            }
            API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_STEP, currStep + 1);
        }


        private void ResetAndDeleteCollShapes(Client client)
        {
            if(API.shared.getEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL) != null)
            {
                API.shared.deleteColShape(API.shared.getEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL));
                API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL, null);
            }
        }

        private void DeleteCurrColShape(Client client, ColShape colShape)
        {
            API.shared.deleteColShape(colShape);
            API.shared.setEntityData(client, mainScript.keys.KEY_MISSION_CURR_COLL, null);
        }

    }
}
