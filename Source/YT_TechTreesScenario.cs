﻿using System;
using System.Collections.Generic;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{

    public class YT_TreeDeclaration
    {
        public string title;
        public string url;
        public string desctription;

        public YT_TreeDeclaration(string title, string url, string description)
        {
            this.title = title;
            this.url = url;
            this.desctription = description;
        }
    }


    /*======================================================*\
     * YT_TechTreeScenario class                            *
     * ScenarioModule to handle seting up the tech tree for *
     * each game.                                           *
    \*======================================================*/
    [KSPScenario(ScenarioCreationOptions.AddToNewScienceSandboxGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.SPACECENTER)]
    public class YT_TechTreesScenario : ScenarioModule
    {
        //Custom YT_TreeDeclaration node
        //gives details on tech trees available
        //title is the displayed name for the tree
        //url is the location of the config file with the TechTree node
        string YT_treeDeclarationNode_nodeType;
        string YT_treeDeclarationNode_titleField;
        string YT_treeDeclarationNode_urlField;
        string YT_treeDeclarationNode_descriptionField;

        //Custom Unlocks node added to RDNode node in TechTree node
        //lists parts unlocked by that RDNode
        string RDNode_startID;
        string RDNode_UnlocksNode_nodeType;
        string RDNode_UnlocksNode_partField;

        //YT_TechTreeScenario node
        //saves if the tech tree has been selected for this game
        string YT_TechTreeScenarioNode_treeSelectedField;

        bool m_setting_buyStartParts;

        
        bool treeSelected;
        List<YT_TreeDeclaration> treeDeclarationsList;

        YT_TechTreesSelectionWindow m_selectionWindow;


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * OnAwake function                                                     *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.Awake()");
#endif
            YT_treeDeclarationNode_nodeType = null;
            YT_treeDeclarationNode_titleField = null;
            YT_treeDeclarationNode_urlField = null;
            YT_treeDeclarationNode_descriptionField = null;
            RDNode_UnlocksNode_nodeType = null;
            RDNode_UnlocksNode_partField = null;

            m_setting_buyStartParts = true;
            treeSelected = false;
            treeDeclarationsList = new List<YT_TreeDeclaration>();
            m_selectionWindow = null;
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * OnLoad function                                                      *
         *                                                                      *
        \************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.OnLoad()");
#endif
            base.OnLoad(node);

            //This loads the field names for reading data from the ConfigNodes, so it has to be first
            ReadConfigFile();

            //loads data on the tree delcarations
            LoadModData();

            //Read data from ConfigNode
            if (node.HasValue(YT_TechTreeScenarioNode_treeSelectedField))
                treeSelected = ("TRUE" == (node.GetValue(YT_TechTreeScenarioNode_treeSelectedField)).ToUpper());
            else
                treeSelected = false;


        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * OnSave function                                                      *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.OnSave()");
#endif
            base.OnSave(node);

            //Save data to ConfigNode
            node.AddValue(YT_TechTreeScenarioNode_treeSelectedField, treeSelected);
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * Start function                                                       *
         *                                                                      *
        \************************************************************************/
        public void Start()
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.Start()");
#endif
            //Check that the game mode is Career or Science
            if (Game.Modes.CAREER == HighLogic.CurrentGame.Mode || Game.Modes.SCIENCE_SANDBOX == HighLogic.CurrentGame.Mode)
            {
                if (!treeSelected)
                {
                    //Create tree selection window and pass it the list of tree declarations
                    m_selectionWindow = new YT_TechTreesSelectionWindow();
                    m_selectionWindow.TechTrees = treeDeclarationsList.ToArray();
                }
                else
                {
                    //Change to the tech tree saved for this game
                    ChangeTree(GetCurrentGameTechTreeUrl());
                }
            }
            else
            {
                treeSelected = true;
            }

        }

        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * ReadConfigFile function                                              *
         *                                                                      *
         * Reads all AST_TechTree Decleartion nodes.                            *
        \************************************************************************/
        private void ReadConfigFile()
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.ReadConfigFile()");
#endif
            //Read Mod Configuration File
            KSP.IO.PluginConfiguration configFile = KSP.IO.PluginConfiguration.CreateForType<YT_TechTreesScenario>();
            configFile.load();

            m_setting_buyStartParts = configFile.GetValue<bool>("buyStartParts");
            YT_TechTreeScenarioNode_treeSelectedField = configFile.GetValue<string>("YT_techTreeScenarioNode_treeSelectedField");

            YT_treeDeclarationNode_nodeType = configFile.GetValue<string>("YT_treeDeclarationNode_nodeType");
            YT_treeDeclarationNode_titleField = configFile.GetValue<string>("YT_treeDeclarationNode_titleField");
            YT_treeDeclarationNode_urlField = configFile.GetValue<string>("YT_treeDeclarationNode_urlField");
            YT_treeDeclarationNode_descriptionField = configFile.GetValue<string>("YT_treeDeclarationNode_descriptionField");

            RDNode_startID = configFile.GetValue<string>("RDNode_startID");
            RDNode_UnlocksNode_nodeType = configFile.GetValue<string>("RDNode_unlocksNode_nodeType");
            RDNode_UnlocksNode_partField = configFile.GetValue<string>("RDNode_unlocksNode_partField");
#if DEBUG
            string values = "";
            values += "m_setting_buyStartParts = " + m_setting_buyStartParts + "\n";
            values += "YT_TechTreeScenarioNode_treeSelectedField = " + YT_TechTreeScenarioNode_treeSelectedField + "\n";
            values += "YT_treeDeclarationNode_nodeType = " + YT_treeDeclarationNode_nodeType + "\n";
            values += "YT_treeDeclarationNode_titleField = " + YT_treeDeclarationNode_titleField + "\n";
            values += "YT_treeDeclarationNode_urlField = " + YT_treeDeclarationNode_urlField + "\n";
            values += "YT_treeDeclarationNode_descriptionField = " + YT_treeDeclarationNode_descriptionField + "\n";
            values += "RDNode_startID = " + RDNode_startID + "\n";
            values += "RDNode_UnlocksNode_nodeType = " + RDNode_UnlocksNode_nodeType + "\n";
            values += "RDNode_UnlocksNode_partField = " + RDNode_UnlocksNode_partField + "\n";
            Debug.Log("YT_TechTreeScenario.ReadConfigFile(): values\n" + values);
#endif
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * LoadModData function                                                 *
         *                                                                      *
         * Reads all AST_TechTree Decleartion nodes.                            *
        \************************************************************************/
        private void LoadModData()
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.LoadModData()");
#endif
            //Read in Tech Tree Declaration nodes from GameDatabase
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes(YT_treeDeclarationNode_nodeType))
            {
#if DEBUG
                Debug.Log("YT_TechTreeScenario.LoadModData() found " + YT_treeDeclarationNode_nodeType + " node\n" + node.ToString());
#endif
                if (node.HasValue(YT_treeDeclarationNode_titleField) && node.HasValue(YT_treeDeclarationNode_urlField))
                {
                    treeDeclarationsList.Add(new YT_TreeDeclaration(node.GetValue(YT_treeDeclarationNode_titleField), node.GetValue(YT_treeDeclarationNode_urlField), node.GetValue(YT_treeDeclarationNode_descriptionField)));
                }
            }
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * OnGUI function                                                       *
         *                                                                      *
        \************************************************************************/
        public void OnGUI()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreeScenario.OnGUI()");
#endif
            if (!treeSelected && null != m_selectionWindow)
            {
                //Draw the tree selection window
                m_selectionWindow.windowRect = GUI.Window(this.GetHashCode(), m_selectionWindow.windowRect, m_selectionWindow.DrawWindow, "Select Tech Tree", m_selectionWindow.WindowStyle);

                //If a tree has been picked
                if (treeSelected = m_selectionWindow.Done)
                {
                    ChangeTree(m_selectionWindow.TechTreeURL);
#if DEBUG
                    Debug.Log("YT_TechTreeScenario.OnGUI(): changing tech tree to " + m_selectionWindow.TechTreeURL);
#endif
                    //No longer need the selection window
                    m_selectionWindow = null;
                }
            }
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * ChangeTree function                                                  *
         *                                                                      *
         * Handles changing the active techTree to the one at treeURL.          *
        \************************************************************************/
        private void ChangeTree(string treeURL)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.ChangeTree(" + treeURL + ")");
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
                        Debug.Log("YT_TechTreeScenario.ChangeTree(): ERROR unable to load ResearchAndDevelopment ScenarioModule");
                    }
                    break;
                }
            }

            /********************************************\
             * Get the TechTree node from the tree URL  *
             * Check successful                         *
            \********************************************/
            if (null == (techtreeNode = ConfigNode.Load(treeURL).GetNode("TechTree")))
            {
                Debug.Log("YT_TechTreeScenario.ChangeTree(): ERROR TechTree node not loaded from url " + treeURL);
                return;
            }

            //Apply changes and update treeURL
            ApplyTechTreeChanges(techtreeNode, RDScenario);
            SetCurrentGameTechTreeUrl(treeURL);
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * ApplyTechTreeChanges function                                        *
         *                                                                      *
         * Applies changes to the tech tree.                                    *
        \************************************************************************/
        private void ApplyTechTreeChanges(ConfigNode techtreeNode, ResearchAndDevelopment RDScenario)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.ApplyTechTreeChanges()");
#endif
            string techID = null;
            List<string> partNamesList = new List<string>();

            /************************************\
             * Check passed arguments are valid *
            \************************************/
            if(null == techtreeNode)
            {
                Debug.Log("YT_TechTreeScenario.ApplyTechTreeChanges(): ERROR techtreeNode node is null");
                return;
            }
            if(null == RDScenario)
            {
                Debug.Log("YT_TechTreeScenario.ApplyTechTreeChanges(): ERROR RDScenario scenario is null");
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
                    Debug.Log("YT_TechTreeScenario.ApplyTechTreeChanges(): ERROR techID not found for RDNode. Node:\n" + RDNode.ToString());
                    continue;
                }
#if DEBUG
                Debug.Log("YT_TechTreeScenario.ApplyTechTreeChanges(): working on node " + techID);
#endif

                //Create list of parts unlocked by this technode (the Parts subnode is a custom addition to the RDNode)
                //Eddit parts so their TechRequired matches this node
                partNamesList = GeneratePartNamesList(RDNode);
                ChangeTechRequired(partNamesList, techID);

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
                    Debug.Log("YT_TechTreeScenario.ApplyTechTreeChanges(): ERROR techID not found for RDNode. Node:\n" + RDNode.ToString());
                    continue;
                }

                //Clean up RDScenario so any parts listed as unlocked in a tech that do not require that tech are removed
                CleanUpRDScenario(RDScenario, techID);

                //if this is the start node, buy all the parts
                if (m_setting_buyStartParts && techID == RDNode_startID)
                {
                    partNamesList = GeneratePartNamesList(RDNode);
                    BuyAllParts(partNamesList, RDScenario, techID);
                }
            }
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * GetCurrentGameTechTreeUrl function                                   *
         *                                                                      *
         * Gets the TechTreeUrl from the Current Game.                          *
        \************************************************************************/
        private string GetCurrentGameTechTreeUrl()
        {
            return HighLogic.CurrentGame.Parameters.Career.TechTreeUrl;
        }

        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * SetCurrentGameTechTreeUrl function                                   *
         *                                                                      *
         * Sets the TechTreeUrl from the Current Game.                          *
        \************************************************************************/
        private void SetCurrentGameTechTreeUrl(string url)
        {
            HighLogic.CurrentGame.Parameters.Career.TechTreeUrl = url;
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * ResetTechRequired function                                           *
         *                                                                      *
         * Resets TechRequired for all parts to their defaults.                 *
        \************************************************************************/
        private void ResetTechRequired()
        {
            ConfigNode node;
            string techID = null;

            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
#if DEBUG
                Debug.Log("YT_TechTreeScenario.ResetTechRequired(): looking at: " + part.name);
#endif
                /************************************************************\
                 * Get the full path to the origonal config file            * 
                 * Load the ConfigNode from the config file                 *
                 * Get TechRequired property from the origonal Config file  *
                 * Check successful at each stage                           *
                \************************************************************/
                if (null == part || null == part.partUrlConfig || null == part.partUrlConfig.parent)
                {
#if DEBUG
                    //some parts such as kerbalEVA have no config file, so this is not necessarily an error.
                    Debug.Log("YT_TechTreeScenario.ResetTechRequired(): part.partUrlConfig.parent.fullPath is null for " + part.name);
#endif
                    continue;
                }
                if(null == (node = ConfigNode.Load(part.partUrlConfig.parent.fullPath)))
                {
                    Debug.Log("YT_TechTreeScenario.ResetTechRequired(): ERROR unable to load ConfigNode for " + part.name + ".  from file: " + part.partUrlConfig.parent.fullPath);
                    continue;
                }
                if(null == (node = node.GetNode("PART")) || null == (techID = node.GetValue("TechRequired")) )
                {
                    Debug.Log("YT_TechTreeScenario.ResetTechRequired(): ERROR can't find TechRequired in ConfigNode for " + part.name + ". Node:\n" + node.ToString());
                    continue;
                }

                //Assign the TechRequired from the origonal config file to the part
                part.TechRequired = techID;
            }
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * GeneratePartNamesList function                                       *
         *                                                                      *
         * Returns a list of part names for parts unlocked by the given RDNode. *
        \************************************************************************/
        private List<string> GeneratePartNamesList(ConfigNode RDNode)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.GeneratePartNamesList()");
#endif
            List<string> partNamesList = new List<string>();
            ConfigNode partsNode = null;

            if (null != (partsNode = RDNode.GetNode(RDNode_UnlocksNode_nodeType)))
            {
                foreach (string partName in partsNode.GetValues(RDNode_UnlocksNode_partField))
                {
                    partNamesList.Add(partName);
                }
            }
#if DEBUG
            string partNames = "";
            foreach (string partName in partNamesList)
                partNames += partName + "\n";
            Debug.Log("YT_TechTreeScenario.GeneratePartNamesList(): generated partNamesList for " + RDNode.GetValue("id") + ":\n" + partNames);
#endif

            return partNamesList;
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * CleanUpRDScenario function                                           *
         *                                                                      *
         * Removes the parts in partNamesList from Tech nodes in RDScenarioNode *
         * that are not techID.                                                 *
        \************************************************************************/
        private void CleanUpRDScenario(ResearchAndDevelopment RDScenario, string techID)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.CleanUpRDScenario()");
#endif
            ProtoTechNode tech = null;

            //Get tech info for techID from the ResearchAndDevelopment Scenario
            if (null == (tech = RDScenario.GetTechState(techID)))
            {
#if DEBUG
                Debug.Log("YT_TechTreeScenario.CleanUpRDScenario(): no data found for tech " + techID + " in the ResearchAndDevelopment Scenario");
#endif
                return;
            }
            
            //Traverse through list of parts purchased and remove any that don't require this tech
            for (int i = 0; i < tech.partsPurchased.Count; )
            {
                if (tech.partsPurchased[i].TechRequired != techID)
                {
#if DEBUG
                    Debug.Log("YT_TechTreeScenario.CleanUpRDScenario(): Removing value for " + tech.partsPurchased[i].title + " from ResearchAndDevelopment Scenario for " + techID);
#endif
                    tech.partsPurchased.Remove(tech.partsPurchased[i]);
                }
                else
                    ++i;
            }
        }
            

        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * BuyAllParts function                                                 *
         *                                                                      *
         * Adds all parts in techID to the purchased list in RDScenario if the  *
         * tech has been researched.                                            *
        \************************************************************************/
        private void BuyAllParts(List<string> partNamesList, ResearchAndDevelopment RDScenario, string techID)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.BuyAllParts()");
#endif
            ProtoTechNode tech = null;
            AvailablePart avalablePart = null;


            //Get tech info for techID from the ResearchAndDevelopment Scenario
            if (null == (tech = RDScenario.GetTechState(techID)))
            {
#if DEBUG
                Debug.Log("YT_TechTreeScenario.BuyAllParts(): no data found for tech " + techID + " in the ResearchAndDevelopment Scenario");
#endif
                return;
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
                    Debug.Log("YT_TechTreeScenario.BuyAllParts(): ERROR part " + partName + " not found in PartLoader.");
                    continue;
                }

                //if the part is not in the purchased list, add them
                if(!tech.partsPurchased.Contains(avalablePart))
                {
#if DEBUG
                    Debug.Log("YT_TechTreeScenario.BuyAllParts(): techID does not have " + avalablePart.title + " adding it");
#endif
                    tech.partsPurchased.Add(avalablePart);
                }
            }
        }


        /************************************************************************\
         * YT_TechTreeScenario class                                            *
         * ChangeTechRequired function                                          *
         *                                                                      *
         * Edits the TechRequired for the parts in partNamesList to be techID.  *
        \************************************************************************/
        private void ChangeTechRequired(List<string> partNamesList, string techID)
        {
#if DEBUG
            Debug.Log("YT_TechTreeScenario.ChangeTechRequired()");
#endif
            AvailablePart avalablePart = null;

            foreach (string partName in partNamesList)
            {
                /********************************\
                 * Get part from PartLoader     *
                 * Check successful             *
                \********************************/
                if (null == (avalablePart = PartLoader.getPartInfoByName(partName)))
                {
                    Debug.Log("YT_TechTreeScenario.ChangeTechRequired(): ERROR part " + partName + " not found in PartLoader.");
                    continue;
                }
#if DEBUG
                Debug.Log("YT_TechTreeScenario.ChangeTechRequired(): edditing " + partName + " to require " + techID);
#endif
                avalablePart.TechRequired = techID;
            }
        }


    }
}