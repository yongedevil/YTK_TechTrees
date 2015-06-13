using System;
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

        public int totalCost;
        public int numNodes;
        public int numNodes_level1;
        public int numNodes_level2;
        public int numNodes_level3;

        public YT_TreeDeclaration(string title, string url, string description)
        {
            this.title = title;
            this.url = url;
            this.desctription = description;

            totalCost = 0;
            numNodes = 0;
            numNodes_level1 = 0;
            numNodes_level2 = 0;
            numNodes_level3 = 0;
        }
    }


    /*======================================================*\
     * YT_TechTreesScenario class                           *
     * ScenarioModule to handle seting up the tech tree for *
     * each game.                                           *
    \*======================================================*/
    [KSPScenario(ScenarioCreationOptions.AddToNewScienceSandboxGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.SPACECENTER)]
    public class YT_TechTreesScenario : ScenarioModule
    {
        //Stock Tree information
        private string stockTree_url;
        private string stockTree_title;
        private string stockTree_description;

        //Custom TechTree fields
        //gives details on tech trees available
        //title is the displayed name for the tree
        //description should describe the tree to the player
        private const string TECHTREE_FIELD_TITLE = "title";
        private const string TECHTREE_FIELD_DESCRIPTION = "description";
        private const string TECHTREE_FIELD_BUYSTARTPARTS = "unlockAllStartParts";

        //Custom Unlocks node added to RDNode node in TechTree node
        //RDNode_startID is the id of the starting node
        //UnlocksNode lists parts unlocked by that RDNode
        private const string RDNode_UNLOCKSNODE_NAME = "Unlocks";
        private const string RDNode_UNLOCKSNODE_FIELD_PART = "part";

        //YT_TechTreeScenario node
        //saves if the tech tree has been selected for this game
        private const string YT_SCENARIONODE_FIELD_TREESELECTED = "treeSelected";

        private int RDNodeMaxCost1;
        private int RDNodeMaxCost2;


        private bool m_treeSelected;
        private List<YT_TreeDeclaration> m_treeDeclarationsList;
        private YT_TechTreesSelectionWindow m_selectionWindow;


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * OnAwake function                                                     *
         *                                                                      *
        \************************************************************************/
        public override void OnAwake()
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.OnAwake()");
#endif
            RDNodeMaxCost1 = 100;
            RDNodeMaxCost1 = 500;

            m_treeSelected = false;
            m_treeDeclarationsList = new List<YT_TreeDeclaration>();
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
            Debug.Log("YT_TechTreesScenario.OnLoad()");
#endif
            base.OnLoad(node);

            //This loads the field names for reading data from the ConfigNodes, so it has to be first
            ReadConfigFile();

            //loads data on the tree delcarations
            LoadTechTreeData();

            //Read data from ConfigNode
            if (node.HasValue(YT_SCENARIONODE_FIELD_TREESELECTED))
                m_treeSelected = ("TRUE" == (node.GetValue(YT_SCENARIONODE_FIELD_TREESELECTED)).ToUpper());
            else
                m_treeSelected = false;
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * OnSave function                                                      *
         *                                                                      *
        \************************************************************************/
        public override void OnSave(ConfigNode node)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.OnSave()");
#endif
            base.OnSave(node);

            //Save data to ConfigNode
            node.AddValue(YT_SCENARIONODE_FIELD_TREESELECTED, m_treeSelected);
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * Start function                                                       *
         *                                                                      *
        \************************************************************************/
        public void Start()
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.Start()");
#endif
            //Check that the game mode is Career or Science
            if (Game.Modes.CAREER == HighLogic.CurrentGame.Mode || Game.Modes.SCIENCE_SANDBOX == HighLogic.CurrentGame.Mode)
            {
                if (!m_treeSelected)
                {
                    //Create tree selection window and pass it the list of tree declarations
                    m_selectionWindow = new YT_TechTreesSelectionWindow();
                    m_selectionWindow.TechTrees = m_treeDeclarationsList.ToArray();
                }
                else
                {
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
         * ReadConfigFile function                                              *
         *                                                                      *
         * Reads settings in from the mod's configuration file.                 *
        \************************************************************************/
        private void ReadConfigFile()
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.ReadConfigFile()");
#endif
            //Read Mod Configuration File
            KSP.IO.PluginConfiguration configFile = KSP.IO.PluginConfiguration.CreateForType<YT_TechTreesScenario>();
            configFile.load();

            stockTree_url = configFile.GetValue<string>("stockTree_url");
            stockTree_title = configFile.GetValue<string>("stockTree_title");
            stockTree_description = configFile.GetValue<string>("stockTree_description");

            RDNodeMaxCost1 = configFile.GetValue<int>("RDNode_maxCost_level1");
            RDNodeMaxCost2 = configFile.GetValue<int>("RDNode_maxCost_level2");
#if DEBUG
            string values = "";
            values += "stockTree_url = " + stockTree_url + "\n";
            values += "stockTree_title = " + stockTree_title + "\n";
            values += "stockTree_description = " + stockTree_description + "\n";
            values += "m_RDNodeMaxCost1 = " + RDNodeMaxCost1 + "\n";
            values += "m_RDNodeMaxCost2 = " + RDNodeMaxCost2 + "\n";
            Debug.Log("YT_TechTreesScenario.ReadConfigFile(): values\n" + values);
#endif
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * LoadTechTreeData function                                            *
         *                                                                      *
         * Finds TechTree ConfigNodes in the GameDatabase and sets up           *
         * m_treeDeclarationsList with infor on each TechTree.                  *
        \************************************************************************/
        private void LoadTechTreeData()
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.LoadModData()");
#endif
            ConfigNode node;
            string title;
            string url;
            string description;

            //Traverse through game configs looking for TechTree configs
            foreach (UrlDir.UrlConfig config in GameDatabase.Instance.root.AllConfigs)
            {
                if ("TechTree" == config.name)
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.LoadModData(): found TechTree Config url = GameData/" + config.parent.url + "." + config.parent.fileExtension);
#endif
                    node = config.config;
                    url = "GameData/" + config.parent.url + "." + config.parent.fileExtension;

                    //Check if this is the stock tree
                    //use stock tree title and description loaded from mod config file if it is
                    if (url == stockTree_url)
                    {
                        title = stockTree_title;
                        description = stockTree_description;
                    }

                    //If not stock tree attempt to read title and description from the TechTree ConfigNode
                    //if they can't be read default values are used instead
                    else
                    {
                        if (node.HasValue(TECHTREE_FIELD_TITLE))
                            title = node.GetValue(TECHTREE_FIELD_TITLE);
                        else
                            title = "(" + config.parent.url + ")";

                        if (node.HasValue(TECHTREE_FIELD_DESCRIPTION))
                            description = node.GetValue(TECHTREE_FIELD_DESCRIPTION);
                        else
                            description = "No description available.";
                    }

                    //create YT_TreeDeclaration and add stats
                    YT_TreeDeclaration treeData = new YT_TreeDeclaration(title, url, description);
                    CalculateTechTreeStats(node, treeData);

                    //Add information to treeDeclarationList
                    m_treeDeclarationsList.Add(treeData);
                }
            }
        }

        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * CalculateTechTreeStats function                                      *
         *                                                                      *
         * Calculates the stats for the given tech tree.                        *
        \************************************************************************/
        private void CalculateTechTreeStats(ConfigNode treeNode, YT_TreeDeclaration treeData)
        {
            ConfigNode UnlocksNode;
            bool isHidden;
            int nodeCost = 0;

            foreach (ConfigNode RDNode in treeNode.nodes)
            {
                if ("RDNode" != RDNode.name)
                    continue;

                /****************************************************************************************\
                 * Check here if the RDNode is set to hide if Empty and is empty                        *
                 * If hideEmpty is True                                                                 *
                 *   Check all unlock parts against the PartLoader to see if they exist.                *
                 *   If at least one part exists the node is not hidden and is included in the totals.  *
                \****************************************************************************************/
                isHidden = false;
                if (RDNode.HasValue("hideEmpty") && "TRUE" == RDNode.GetValue("hideEmpty").ToUpper() )
                {
                    isHidden = true;

                    if (null != (UnlocksNode = RDNode.GetNode(RDNode_UNLOCKSNODE_NAME)))
                    {
                        //Check all parts in Unlocks against the PartLoader to see if they exist
                        foreach (string partName in UnlocksNode.GetValues(RDNode_UNLOCKSNODE_FIELD_PART))
                        {
                            if(null != PartLoader.getPartInfoByName(partName))
                            {
                                isHidden = false;
                                break;
                            }
                        }
                    }
                }


                //If the node is not hidden
                //Update treeData with totals
                if (!isHidden)
                {
                    treeData.numNodes++;

                    if (ConfigNodeParseHelper.getAsInt(RDNode, "cost", out nodeCost, 0))
                    {
                        treeData.totalCost += nodeCost;

                        if (nodeCost > RDNodeMaxCost2)
                            treeData.numNodes_level3++;
                        else if (nodeCost > RDNodeMaxCost1)
                            treeData.numNodes_level2++;
                        else
                            treeData.numNodes_level1++;
                    }
                }
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
            Debug.Log("YT_TechTreesScenario.OnGUI()");
#endif
            if (!m_treeSelected && null != m_selectionWindow)
            {
                //Draw the tree selection window
                m_selectionWindow.windowRect = GUI.Window(this.GetHashCode(), m_selectionWindow.windowRect, m_selectionWindow.DrawWindow, m_selectionWindow.WindowTitle, m_selectionWindow.WindowStyle);

                //If a tree has been picked
                if (m_treeSelected = m_selectionWindow.Done)
                {
                    ChangeTree(m_selectionWindow.TechTreeURL);
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.OnGUI(): changing tech tree to " + m_selectionWindow.TechTreeURL);
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
            Debug.Log("YT_TechTreesScenario.ChangeTree(" + treeURL + ")");
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
                        Debug.Log("YT_TechTreesScenario.ChangeTree(): ERROR unable to load ResearchAndDevelopment ScenarioModule");
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
                Debug.Log("YT_TechTreesScenario.ChangeTree(): ERROR TechTree node not loaded from url " + treeURL);
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
            Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges()");
#endif
            int nodeCost = 0;
            string techID = null;
            List<string> partNamesList = new List<string>();

            /************************************\
             * Check passed arguments are valid *
            \************************************/
            if(null == techtreeNode)
            {
                Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges(): ERROR techtreeNode node is null");
                return;
            }
            if(null == RDScenario)
            {
                Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges(): ERROR RDScenario scenario is null");
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
                    Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges(): ERROR techID not found for RDNode. Node:\n" + RDNode.ToString());
                    continue;
                }
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges(): working on node " + techID);
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
                    Debug.Log("YT_TechTreesScenario.ApplyTechTreeChanges(): ERROR techID not found for RDNode. Node:\n" + RDNode.ToString());
                    continue;
                }

                //Clean up RDScenario so any parts listed as unlocked in a tech that do not require that tech are removed
                CleanUpRDScenario(RDScenario, techID);

                //If this node has a cost of zero it is a start node, 
                //If this tree has BuyStartParts set to True
                //purchase all parts in this node.
                if (ConfigNodeParseHelper.getAsInt(RDNode, "cost", out nodeCost, -1) && 0 == nodeCost)
                {
                    if (techtreeNode.HasValue(TECHTREE_FIELD_BUYSTARTPARTS) && "TRUE" == techtreeNode.GetValue(TECHTREE_FIELD_BUYSTARTPARTS).ToUpper())
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
        \************************************************************************/
        private void ResetTechRequired()
        {
            ConfigNode node;
            string techID = null;

            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ResetTechRequired(): looking at: " + part.name);
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
                    Debug.Log("YT_TechTreesScenario.ResetTechRequired(): part.partUrlConfig.parent.fullPath is null for " + part.name);
#endif
                    continue;
                }
                if(null == (node = ConfigNode.Load(part.partUrlConfig.parent.fullPath)))
                {
                    Debug.Log("YT_TechTreesScenario.ResetTechRequired(): ERROR unable to load ConfigNode for " + part.name + ".  from file: " + part.partUrlConfig.parent.fullPath);
                    continue;
                }
                if(null == (node = node.GetNode("PART")) || null == (techID = node.GetValue("TechRequired")) )
                {
                    Debug.Log("YT_TechTreesScenario.ResetTechRequired(): ERROR can't find TechRequired in ConfigNode for " + part.name + ". Node:\n" + node.ToString());
                    continue;
                }

                //Assign the TechRequired from the origonal config file to the part
                part.TechRequired = techID;
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
            Debug.Log("YT_TechTreesScenario.GeneratePartNamesList()");
#endif
            List<string> partNamesList = new List<string>();
            ConfigNode partsNode = null;

            if (null != (partsNode = RDNode.GetNode(RDNode_UNLOCKSNODE_NAME)))
            {
                foreach (string partName in partsNode.GetValues(RDNode_UNLOCKSNODE_FIELD_PART))
                {
                    //replace _ with . to match the internal format of the game
                    partNamesList.Add(partName.Replace("_", "."));
                }
            }
#if DEBUG
            string partNames = "";
            foreach (string partName in partNamesList)
                partNames += partName + "\n";
            Debug.Log("YT_TechTreesScenario.GeneratePartNamesList(): generated partNamesList for " + RDNode.GetValue("id") + ":\n" + partNames);
#endif

            return partNamesList;
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
            Debug.Log("YT_TechTreesScenario.CleanUpRDScenario()");
#endif
            ProtoTechNode tech = null;

            //Get tech info for techID from the ResearchAndDevelopment Scenario
            if (null == (tech = RDScenario.GetTechState(techID)))
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.CleanUpRDScenario(): no data found for tech " + techID + " in the ResearchAndDevelopment Scenario");
#endif
                return;
            }
            
            //Traverse through list of parts purchased and remove any that don't require this tech
            for (int i = 0; i < tech.partsPurchased.Count; )
            {
                if (tech.partsPurchased[i].TechRequired != techID)
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.CleanUpRDScenario(): Removing value for " + tech.partsPurchased[i].title + " from ResearchAndDevelopment Scenario for " + techID);
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
            Debug.Log("YT_TechTreesScenario.BuyAllParts()");
#endif
            ProtoTechNode tech = null;
            AvailablePart avalablePart = null;


            //Get tech info for techID from the ResearchAndDevelopment Scenario
            if (null == (tech = RDScenario.GetTechState(techID)))
            {
#if DEBUG
                Debug.Log("YT_TechTreesScenario.BuyAllParts(): no data found for tech " + techID + " in the ResearchAndDevelopment Scenario");
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
                    Debug.Log("YT_TechTreesScenario.BuyAllParts(): WARNING part " + partName + " not found in PartLoader.");
                    continue;
                }

                //if the part is not in the purchased list, add them
                if(!tech.partsPurchased.Contains(avalablePart))
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.BuyAllParts(): techID does not have " + avalablePart.title + " adding it");
#endif
                    tech.partsPurchased.Add(avalablePart);
                }
            }
        }


        /************************************************************************\
         * YT_TechTreesScenario class                                           *
         * ChangeTechRequired function                                          *
         *                                                                      *
         * Edits the TechRequired for the parts in partNamesList to be techID.  *
        \************************************************************************/
        private void ChangeTechRequired(List<string> partNamesList, string techID)
        {
#if DEBUG
            Debug.Log("YT_TechTreesScenario.ChangeTechRequired()");
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
                    Debug.Log("YT_TechTreesScenario.ChangeTechRequired(): WARNING part " + partName + " not found in PartLoader.");
                    continue;
                }
#if DEBUG
                Debug.Log("YT_TechTreesScenario.ChangeTechRequired(): edditing " + partName + " to require " + techID);
#endif
                avalablePart.TechRequired = techID;
            }
        }


    }
}
