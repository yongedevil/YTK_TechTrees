using System;
using System.Collections.Generic;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{
    /*======================================================*\
     * YT_TechTreesScenario class                           *
     * ScenarioModule to handle seting up the tech tree for *
     * each game.                                           *
    \*======================================================*/
    [KSPScenario(ScenarioCreationOptions.AddToNewScienceSandboxGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.SPACECENTER)]
    public class YT_TechTreesScenario : ScenarioModule
    {
        //YT_TechTreeScenario node
        //saves if the tech tree has been selected for this game
        private const string YT_SCENARIONODE_FIELD_TREESELECTED = "treeSelected";
        private const string YT_SCENARIONODE_FIELD_TREEURL = "TechTreeUrl";
        

        private string m_treeURL;
        private bool m_treeSelected;
        private YT_TechTreesSelectionWindow m_selectionWindow;


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * OnAwake function                                                     *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.OnAwake");
#endif
            base.OnAwake();

            m_treeURL = null;
            m_treeSelected = false;
            m_selectionWindow = null;
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * OnLoad function                                                      *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.OnLoad");
#endif
            base.OnLoad(node);

            //Read data from ConfigNode
            if (node.HasValue(YT_SCENARIONODE_FIELD_TREESELECTED))
                m_treeSelected = ("TRUE" == (node.GetValue(YT_SCENARIONODE_FIELD_TREESELECTED)).ToUpper());
            else
                m_treeSelected = false;

            if (node.HasValue(YT_SCENARIONODE_FIELD_TREEURL))
                m_treeURL = node.GetValue(YT_SCENARIONODE_FIELD_TREEURL);
            else
                m_treeURL = null;
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * OnSave function                                                      *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.OnSave");
#endif
            base.OnSave(node);

            //Save data to ConfigNode
            node.AddValue(YT_SCENARIONODE_FIELD_TREESELECTED, m_treeSelected);

            if (null != m_treeURL)
                node.AddValue(YT_SCENARIONODE_FIELD_TREEURL, m_treeURL);
            else
                node.AddValue(YT_SCENARIONODE_FIELD_TREEURL, GetCurrentGameTechTreeUrl());
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * Start function                                                       *
         *                                                                      *
        \************************************************************************/
        public void Start()
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.Start");
#endif
            //Check that the game mode is Career or Science
            if (Game.Modes.CAREER == HighLogic.CurrentGame.Mode || Game.Modes.SCIENCE_SANDBOX == HighLogic.CurrentGame.Mode)
            {
                if (!m_treeSelected && YT_TechTreesSettings.Instance.AllowTreeSelection)
                {
                    //Create tree selection window and pass it the list of tree declarations
                    m_selectionWindow = new YT_TechTreesSelectionWindow();
                }
                else
                {
                    if (null != m_treeURL && YT_TechTreesSettings.Instance.AllowTreeSelection)
                    {
                        //use the tree url save with this scenario (this is to override the changes ModuleManager makes)
                        ChangeTree(m_treeURL);
                    }
                    else
                        //Change to the tech tree saved for this game
                        ChangeTree(GetCurrentGameTechTreeUrl());
                }
            }
            else
            {
                m_treeSelected = true;
            }
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * OnGUI function                                                       *
         *                                                                      *
        \************************************************************************/
        public void OnGUI()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreesScenario.OnGUI");
#endif
            if (!m_treeSelected && null != m_selectionWindow)
            {
                //Draw the tree selection window
                m_selectionWindow.windowRect = GUI.Window(m_selectionWindow.GetHashCode(), m_selectionWindow.windowRect, m_selectionWindow.DrawWindow, m_selectionWindow.WindowTitle, m_selectionWindow.WindowStyle);

                //If a tree has been picked
                if (m_selectionWindow.Done)
                {
                    m_treeSelected = true;
                    ChangeTree(m_selectionWindow.TechTreeURL);
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.OnGUI: changing tech tree to " + m_selectionWindow.TechTreeURL);
#endif
                    //No longer need the selection window
                    m_selectionWindow = null;
                }
            }
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * ChangeTree function                                                  *
         *                                                                      *
         * Handles changing the active techTree to the one at treeURL.          *
        \************************************************************************/
        private void ChangeTree(string treeURL)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.ChangeTree");
#endif
            ConfigNode techtreeNode = null;
            ConfigNode RDScenarioNode = null;
            ResearchAndDevelopment RDScenario = null;

            //Get R&D scenario module for current game
            foreach (ProtoScenarioModule scenarioModule in HighLogic.CurrentGame.scenarios)
            {
                if ("ResearchAndDevelopment" == scenarioModule.moduleName)
                {
                    RDScenarioNode = scenarioModule.GetData();
                    /****************************************************\
                     * Get the ResearchAndDevelopment ScenarioModule    *
                     * Check successful                                 *
                    \****************************************************/
                    if (null == (RDScenario = scenarioModule.moduleRef as ResearchAndDevelopment))
                    {
                        Debug.Log("YT_TechTreesScenario.ChangeTree: ERROR unable to load ResearchAndDevelopment ScenarioModule");
                    }
                    break;
                }
            }

            /********************************************\
             * Get the TechTree node from the tree URL  *
             * Check successful                         *
            \********************************************/
            try
            {
                //The commented out line loads the tech tree before it is modified by Module Manager.
                //The loop ensures the Module Manager changes to the unlocks are loaded.

                //techtreeNode = ConfigNode.Load(treeURL).GetNode("TechTree");
                foreach (UrlDir.UrlConfig config in GameDatabase.Instance.root.GetConfigs("TechTree"))
                {
                    if("GameData/" + config.parent.url + "." + config.parent.fileExtension == treeURL)
                    {
                        techtreeNode = config.config;
                        break;
                    }
                }
            }
            catch(NullReferenceException)
            {
                Debug.Log("YT_TechTreesScenario.ChangeTree: ERROR TechTree node not loaded from url " + treeURL);
                return;
            }

            //Apply changes and update treeURL
            ApplyTechTreeChanges(techtreeNode, RDScenario);
            SetCurrentGameTechTreeUrl(treeURL);
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * ApplyTechTreeChanges function                                        *
         *                                                                      *
         * Applies changes to the tech tree.                                    *
        \************************************************************************/
        private void ApplyTechTreeChanges(ConfigNode techtreeNode, ResearchAndDevelopment RDScenario)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges");
#endif
            int nodeCost = 0;
            string techID = null;
            List<string> partNamesList = new List<string>();
            List<string> upgradeNamesList = new List<string>();

            /************************************\
             * Check passed arguments are valid *
            \************************************/
            if (null == techtreeNode)
            {
                Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges: ERROR techtreeNode node is null");
                return;
            }
            if(null == RDScenario)
            {
                Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges: ERROR RDScenario scenario is null");
                return;
            }


            //Reset all parts to default before begining
            ResetTechRequired();

            //Loop through all RDNode configNodes in the techtree
            foreach (ConfigNode RDNode in techtreeNode.GetNodes("RDNode"))
            {
                /****************************\
                 * Get the id from RDNode   *
                 * Check successful         *
                \****************************/
                if (null == (techID = RDNode.GetValue("id")))
                {
                    Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges: ERROR techID not found for RDNode. Node:\n" + RDNode.ToString());
                    continue;
                }
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges: working on node " + techID);
#endif

                //Create list of parts unlocked by this technode (the Parts subnode is a custom addition to the RDNode)
                //Edit parts so their TechRequired matches this node
                partNamesList = GeneratePartNamesList(RDNode);
                upgradeNamesList = GenerateUpgradeNamesList(RDNode);
                ChangeTechRequired(partNamesList, upgradeNamesList, techID);

            }
            
            //Loop through all RDNode configNodes in the techtree
            foreach (ConfigNode RDNode in techtreeNode.GetNodes("RDNode"))
            {
                /****************************\
                 * Get the id from RDNode   *
                 * Check successful         *
                \****************************/
                if (null == (techID = RDNode.GetValue("id")))
                {
                    Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges: ERROR techID not found for RDNode. Node:\n" + RDNode.ToString());
                    continue;
                }

                //Clean up RDScenario so any parts listed as unlocked in a tech that do not require that tech are removed
                CleanUpRDScenario(RDScenario, techID);

                //If this node has a cost of zero it is a start node, 
                //If this tree has BuyStartParts set to True
                //purchase all parts in this node.
                if (ConfigNodeParseHelper.getAsInt(RDNode, "cost", out nodeCost, -1) && 0 == nodeCost)
                {
                    if (techtreeNode.HasValue(YT_TechTreesSettings.TECHTREE_FIELD_BUYSTARTPARTS) && "TRUE" == techtreeNode.GetValue(YT_TechTreesSettings.TECHTREE_FIELD_BUYSTARTPARTS).ToUpper())
                    {
                        partNamesList = GeneratePartNamesList(RDNode);
                        BuyAllParts(partNamesList, RDScenario, techID);
                    }
                }
            }
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * GetCurrentGameTechTreeUrl function                                   *
         *                                                                      *
         * Gets the TechTreeUrl from the Current Game.                          *
        \************************************************************************/
        private string GetCurrentGameTechTreeUrl()
        {
            return HighLogic.CurrentGame.Parameters.Career.TechTreeUrl;
        }

        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * SetCurrentGameTechTreeUrl function                                   *
         *                                                                      *
         * Sets the TechTreeUrl from the Current Game.                          *
        \************************************************************************/
        private void SetCurrentGameTechTreeUrl(string url)
        {
            HighLogic.CurrentGame.Parameters.Career.TechTreeUrl = url;
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * ResetTechRequired function                                           *
         *                                                                      *
         * Resets TechRequired for all parts to their defaults.                 *
         * Resets TechRequired for all part upgrades to their defaults.         *
        \************************************************************************/
        private void ResetTechRequired()
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.ResetTechRequired");
#endif
            foreach (KeyValuePair<string, string> partData in YT_TechRequiredDatabase.Instance.Part_OrigonalTechRequired)
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ResetTechRequired: looking at: " + partData.Key);
#endif
                PartLoader.getPartInfoByName(partData.Key).TechRequired = partData.Value;
            }

            foreach(KeyValuePair<string, string> upgradeData in YT_TechRequiredDatabase.Instance.Upgrade_OrigonalTechRequired)
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ResetTechRequired: looking at: " + upgradeData.Key);
#endif
                PartUpgradeHandler.Upgrade upgrade = PartUpgradeManager.Handler.GetUpgrade(upgradeData.Key);
        
                PartUpgradeManager.Handler.RemoveUpgrade(upgradeData.Key);
                upgrade.techRequired = upgradeData.Value;
                PartUpgradeManager.Handler.AddUpgrade(upgrade);
            }
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * GeneratePartNamesList function                                       *
         *                                                                      *
         * Returns a list of part names for parts unlocked by the given RDNode. *
        \************************************************************************/
        private List<string> GeneratePartNamesList(ConfigNode RDNode)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.GeneratePartNamesList");
#endif
            List<string> partNamesList = new List<string>();
            ConfigNode unlocksNode = null;

            if (null != (unlocksNode = RDNode.GetNode(YT_TechTreesSettings.RDNode_UNLOCKSNODE_NAME)))
            {
                foreach (string partName in unlocksNode.GetValues(YT_TechTreesSettings.RDNode_UNLOCKSNODE_FIELD_PART))
                {
                    //replace _ with . to match the internal format of the game
                    partNamesList.Add(partName.Replace("_", "."));
                }
            }
#if DEBUG
            string partNames = "";
            foreach (string partName in partNamesList)
                partNames += partName + "\n";
            Debug.Log("YT_TechTreesScenario.GeneratePartNamesList: generated partNamesList for " + RDNode.GetValue("id") + ":\n" + partNames);
#endif

            return partNamesList;
        }

        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * GenerateUpgradeNamesList function                                    *
         *                                                                      *
         * Returns a list of part upgrade names for parts unlocked by the given *
         * RDNode.                                                              *
        \************************************************************************/
        private List<string> GenerateUpgradeNamesList(ConfigNode RDNode)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.GenerateUpgradeNamesList");
#endif
            List<string> upgradeNamesList = new List<string>();
            ConfigNode unlocksNode = null;

            if (null != (unlocksNode = RDNode.GetNode(YT_TechTreesSettings.RDNode_UNLOCKSNODE_NAME)))
            {
                foreach (string upgradeName in unlocksNode.GetValues(YT_TechTreesSettings.RDNode_UNLOCKSNODE_FIELD_PARTUPGRADE))
                {
                    //replace _ with . to match the internal format of the game
                    upgradeNamesList.Add(upgradeName.Replace("_", "."));
                }
            }
#if DEBUG
            string upgradeNames = "";
            foreach (string upgradeName in upgradeNamesList)
                upgradeNames += upgradeName + "\n";
            Debug.Log("YT_TechTreesScenario.GenerateUpgradeNamesList: generated upgradeNamesList for " + RDNode.GetValue("id") + ":\n" + upgradeNames);
#endif

            return upgradeNamesList;
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * CleanUpRDScenario function                                           *
         *                                                                      *
         * Removes the parts in partNamesList from Tech nodes in RDScenarioNode *
         * that are not techID.                                                 *
        \************************************************************************/
        private void CleanUpRDScenario(ResearchAndDevelopment RDScenario, string techID)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.CleanUpRDScenario");
#endif
            ProtoTechNode tech = null;

            //Get tech info for techID from the ResearchAndDevelopment Scenario
            if (null == (tech = RDScenario.GetTechState(techID)))
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.CleanUpRDScenario: no data found for tech " + techID + " in the ResearchAndDevelopment Scenario");
#endif
                return;
            }
            
            //Traverse through list of parts purchased and remove any that don't require this tech
            for (int i = 0; i < tech.partsPurchased.Count; )
            {
                if (tech.partsPurchased[i].TechRequired != techID)
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.CleanUpRDScenario: Removing value for " + tech.partsPurchased[i].title + " from ResearchAndDevelopment Scenario for " + techID);
#endif
                    tech.partsPurchased.Remove(tech.partsPurchased[i]);
                }
                else
                    ++i;
            }
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * BuyAllParts function                                                 *
         *                                                                      *
         * Adds all parts in techID to the purchased list in RDScenario if the  *
         * tech has been researched.                                            *
        \************************************************************************/
        private void BuyAllParts(List<string> partNamesList, ResearchAndDevelopment RDScenario, string techID)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.BuyAllParts");
#endif
            ProtoTechNode tech = null;
            AvailablePart avalablePart = null;


            //Get tech info for techID from the ResearchAndDevelopment Scenario
            if (null == (tech = RDScenario.GetTechState(techID)))
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.BuyAllParts: no data found for tech " + techID + " in the ResearchAndDevelopment Scenario");
#endif
                tech = new ProtoTechNode();
                tech.techID = techID;
                tech.state = RDTech.State.Available;
                tech.partsPurchased = new List<AvailablePart>();
                RDScenario.SetTechState(techID, tech);
            }

            //Traverse through list of parts
            foreach(string partName in partNamesList)
            {
                /********************************\
                 * Get part from PartLoader     *
                 * Check successful             *
                \********************************/
                if (null == (avalablePart = PartLoader.getPartInfoByName(partName)))
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.BuyAllParts: WARNING part " + partName + " not found in PartLoader.");
#endif
                    continue;
                }

                //if the part is not in the purchased list, add them
                if(!tech.partsPurchased.Contains(avalablePart))
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.BuyAllParts: techID does not have " + avalablePart.title + " adding it");
#endif
                    tech.partsPurchased.Add(avalablePart);
                }
            }
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * ChangeTechRequired function                                          *
         *                                                                      *
         * Passes the current TechRequired for the parts and upgrades to the    *
         * YT_TechRequiredDatabase (if this is the first time editing the       *
         * part/upgrade then the TechRequired will be saved).                   *
         *                                                                      *
         * Edits the TechRequired for the parts in partNamesList to be techID.  *
         * Edits the TechRequired for the part upgrades in upgradeNamesList to  *
         * be techID.                                                           *
        \************************************************************************/
        private void ChangeTechRequired(List<string> partNamesList, List<string> upgradeNamesList, string techID)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.ChangeTechRequired");
#endif
            foreach (string partName in partNamesList)
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ChangeTechRequired: editing " + partName + " to require " + techID);
#endif
                try
                {
                    AvailablePart apart = PartLoader.getPartInfoByName(partName);

                    //Save the origonal TechRequired to the YT_TechRequiredDatabase
                    YT_TechRequiredDatabase.Instance.CheckAndAddPart(partName, apart.TechRequired);
                    //Update TechRequired to the new techID
                    apart.TechRequired = techID;
                }
                catch (NullReferenceException)
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.ChangeTechRequired: WARNING part " + partName + " not found in PartLoader.");
#endif
                }
            }

            foreach(string upgradeName in upgradeNamesList)
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ChangeTechRequired: editing " + upgradeName + " to require " + techID);
#endif
                //Save the origonal TechRequired to the YT_TechRequiredDatabase
                YT_TechRequiredDatabase.Instance.CheckAndAddUpgrade(upgradeName, PartUpgradeManager.Handler.GetUpgrade(upgradeName).techRequired);

                //Just updating the techRequired field on the upgrade does not apper to work
                //So save the upgrade and remove then re-add it to the manager.
                PartUpgradeHandler.Upgrade upgrade = PartUpgradeManager.Handler.GetUpgrade(upgradeName);
                PartUpgradeManager.Handler.RemoveUpgrade(upgradeName);
                upgrade.techRequired = techID;
                PartUpgradeManager.Handler.AddUpgrade(upgrade);
            }
        }


    }
}
