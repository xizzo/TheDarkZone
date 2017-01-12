using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using System.Xml.Linq;
using System.IO;

namespace TheDarkZone.Managers
{
    public class ClothingManager
    {

        private TheDarkZone mainScript { get; set; }

        private int[] maleHats = new int[102];
        private int[] maleFaces = new int[] {0,1,4,5,6,7,10,11,12,13,16,17,18};
        private int[] masks = new int[102];
        private int[] maleHairStyles = new int[24];
        private int[] hairColor = new int[5];
        private int[] malePants = new int[85];
        private int[] invalidMalePants = new int[] {8,10, 11, 33,40,44,57,59,72,74};
        private int[] allowedBags = new int[] {7,18,28,37,40,41,44,45,61};
        private int[] shoes = new int[58];
        private int[] invalidShoes = new int[] {33,13};
        private int[] allowedAccessories = new int[] { 11, 22, 31, 32, 35, 36, 112, 114};
        private List<TopClothing> allowedTops = new List<TopClothing>();   

        public ClothingManager(TheDarkZone mainScript)
        {
            this.mainScript = mainScript;
            LoadTopsData();
            API.shared.consoleOutput("tops loaded: " + allowedTops.Count().ToString());
        }
    
        private void LoadClothingData()
        {
            for (int i = 0; i < 102; i++)
            {
                maleHats[i] = i;
            }
            for (int i = 0; i < 58; i++)
            {
                shoes[i] = i;
            }
            for (int i = 0; i < 102; i++)
            {
                masks[i] = i;
            }
            for (int i = 0; i < 24; i++)
            {
                maleHairStyles[i] = i;
            }
            for (int i = 0; i < 5; i++)
            {
                hairColor[i] = i;
            }
            for (int i = 0; i < 85; i++)
            {
                malePants[i] = i;
            }
        }

        public void ChangePlayerGender(Client sender)
        {
            string currGender = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_GENDER);
            if (currGender == "M") {
                API.shared.setPlayerSkin(sender, (PedHash)(-1667301416));
                API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_GENDER, "F");
                API.shared.setPlayerClothes(sender, 0, 21, 0);
                API.shared.setPlayerClothes(sender, 3, 3, 0);
                API.shared.setPlayerClothes(sender, 8, 2, 0);
                API.shared.setPlayerClothes(sender, 11, 3, 0);
            }
            else{
                API.shared.setPlayerSkin(sender, (PedHash)(1885233650));
                API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_GENDER, "M");
                API.shared.setPlayerClothes(sender, 0, 0, 0);
                API.shared.setPlayerClothes(sender, 3, 1, 0);
                API.shared.setPlayerClothes(sender, 11, 3, 0);
            }
        }

        public void ChangePlayerHairColor(Client sender)
        {
            int currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR_COLOR);
            if (currItem == (hairColor.Count() - 1))
            {
                currItem = 0;
            }
            else
            {
                currItem++;
            }
            API.shared.setPlayerClothes(sender, 2, API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR), currItem);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR_COLOR, currItem);
        }

        public void ChangePlayerHat(Client sender)
        {
            int currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAT);
            if (currItem == (maleHats.Count() - 1))
            {
                currItem = 0;
            }
            else
            {
                currItem++;
            }
            API.shared.setPlayerClothes(sender, 1, 0, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_MASK, 0);
            API.shared.setPlayerAccessory(sender, 0, currItem, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_HAT, currItem);
        }

        public void SetPlayerNextPieceOfClothing(Client sender, int slot)
        {
            int currItem = 0;
            switch(slot){
                case 0:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_FACE_CURR_INDEX);
                    if (currItem == (maleFaces.Count() - 1))
                    {
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    API.shared.setPlayerClothes(sender, 0, maleFaces[currItem], 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_FACE, maleFaces[currItem]);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_FACE_CURR_INDEX, currItem);
                    break;
                case 1:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_MASK);
                    if (currItem == (masks.Count() - 1))
                    {
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    API.shared.setPlayerAccessory(sender, 0, 8, 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_HAT, 8);
                    API.shared.setPlayerClothes(sender, 1, currItem, 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_MASK, currItem);
                    break;
                case 2:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR);
                    if (currItem == (maleHairStyles.Count() - 1))
                    {
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    API.shared.setPlayerClothes(sender, 2, currItem, (int)API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR_COLOR));
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR, currItem);
                    break;
                case 4:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_LEGS);
                    if (currItem == (malePants.Count() - 1)){
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    while (invalidMalePants.Contains(currItem))
                    {
                        currItem++;
                    }
                    API.shared.setPlayerClothes(sender, 4, currItem, 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_LEGS, currItem);
                    break;
                case 5:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_BAGS_CURR_INDEX);
                    if (currItem == (allowedBags.Count() - 1))
                    {
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    API.shared.setPlayerClothes(sender, 5, allowedBags[currItem], 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_BAGS, allowedBags[currItem]);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_BAGS_CURR_INDEX, currItem);
                    break;
                case 6:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_FEET);
                    if (currItem == (shoes.Count() - 1))
                    {
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    while (invalidShoes.Contains(currItem))
                    {
                        currItem++;
                    }
                    API.shared.setPlayerClothes(sender, 6, currItem, 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_FEET, currItem);
                    break;
                case 7:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_ACCESSORIES_CURR_INDEX);
                    if (currItem == (allowedAccessories.Count() - 1))
                    {
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    API.shared.setPlayerClothes(sender, 7, allowedAccessories[currItem], 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_ACCESSORIES, allowedAccessories[currItem]);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_ACCESSORIES_CURR_INDEX, currItem);
                    break;
                case 11:
                    currItem = API.shared.getEntityData(sender, mainScript.keys.USER_CLOTHING_TOP_CURR_INDEX);
                    if (currItem == (allowedTops.Count() - 1))
                    {
                        currItem = 0;
                    }
                    else
                    {
                        currItem++;
                    }
                    API.shared.setPlayerClothes(sender, 11, allowedTops[currItem].itemID, 0);
                    API.shared.setPlayerClothes(sender, 3, allowedTops[currItem].torso, 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_TORSO, allowedTops[currItem].torso);
                    API.shared.setPlayerClothes(sender, 8, allowedTops[currItem].undershirt, 0);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_UNDERSHIRT, allowedTops[currItem].undershirt);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_TOP, allowedTops[currItem].itemID);
                    API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_TOP_CURR_INDEX, currItem);
                    break;
            }
        }

        public void ResetPlayerClothing(Client sender)
        {
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_HAT, 8);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_GENDER, "M");
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_FACE, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_MASK, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_HAIR_COLOR, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_LEGS, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_BAGS, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_FEET, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_ACCESSORIES, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_TOP, 3);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_TORSO, 1);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_UNDERSHIRT, 0);

            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_ACCESSORIES_CURR_INDEX, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_BAGS_CURR_INDEX, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_TOP_CURR_INDEX, 0);
            API.shared.setEntityData(sender, mainScript.keys.USER_CLOTHING_FACE_CURR_INDEX, 0);

            API.shared.setPlayerSkin(sender, (PedHash)(1885233650));
            API.shared.setPlayerAccessory(sender, 0, 8, 0);
            API.shared.setPlayerClothes(sender, 0, 0, 0);
            API.shared.setPlayerClothes(sender, 1, 0, 0);
            API.shared.setPlayerClothes(sender, 2, 0, 0);
            API.shared.setPlayerClothes(sender, 3, 1, 0);
            API.shared.setPlayerClothes(sender, 4, 0, 0);
            API.shared.setPlayerClothes(sender, 5, 0, 0);
            API.shared.setPlayerClothes(sender, 6, 0, 0);
            API.shared.setPlayerClothes(sender, 7, 0, 0);
            API.shared.setPlayerClothes(sender, 9, 0, 0);
            API.shared.setPlayerClothes(sender, 11, 3, 0);
        }

        private void LoadTopsData()
        {
            XDocument xDoc;
            using (StreamReader rdr = new StreamReader(@"resources/TheDarkZone/Managers/Data/tops.xml"))
            {
                xDoc = XDocument.Parse(rdr.ReadToEnd());
            }

            foreach (XElement xTop in xDoc.Root.Elements())
            {
                if (xTop.Name == "top")
                {
                    TopClothing topClothing = new TopClothing();
                    foreach (XElement xTopInfo in xTop.Elements())
                    {
                        int currValue = int.Parse(xTopInfo.Value);
                        switch (xTopInfo.Name.ToString().ToUpper())
                        {
                            case "ID":
                                topClothing.itemID = currValue;
                                break;
                            case "UNDERSHIRT":
                                topClothing.undershirt = currValue;
                                break;
                            case "BODY":
                                topClothing.torso = currValue;
                                break;
                        }
                    }
                    allowedTops.Add(topClothing);
                }
            }
        }

    }
}

public class TopClothing
{
    public int itemID { get; set; }
    public int undershirt { get; set; }
    public int torso { get; set; }
}
