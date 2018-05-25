using System;
using System.Collections.Generic;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{
    /*======================================================*\
     * YT_TreeDeclaration class                             *
     * Class to store info on a tech tree.                  *
    \*======================================================*/
    public class YT_TreeDeclaration
    {
        public string title;
        public string url;
        public string description;

        public int totalCost;
        public int numNodes;
        public int numNodes_level1;
        public int numNodes_level2;
        public int numNodes_level3;

        public YT_TreeDeclaration(string title, string url, string description)
        {
            this.title = title;
            this.url = url;
            this.description = description;

            totalCost = 0;
            numNodes = 0;
            numNodes_level1 = 0;
            numNodes_level2 = 0;
            numNodes_level3 = 0;
        }
    }


    /*======================================================*\
     * YT_TechRequiredDatabase class                        *
     * Singleton class to keep track of the origonal        *
     * RequiredTech for all parts.                          *
    \*======================================================*/
    public class YT_TechRequiredDatabase
    {
        //Singleton
        private static YT_TechRequiredDatabase instance = null;
        public static YT_TechRequiredDatabase Instance
        {
            get
            {
                if (null == instance)
                    instance = new YT_TechRequiredDatabase();

                return instance;
            }
        }

        //dictionaries of origonal techRequired
        //first string is part or upgrade name
        //second string is TechRequired
        private Dictionary<string, string> m_part_origonalTechRequired;
        private Dictionary<string, string> m_upgrade_origonalTechRequired;

        public Dictionary<string, string> Part_OrigonalTechRequired
        {
            get { return m_part_origonalTechRequired; }
        }
        public Dictionary<string, string> Upgrade_OrigonalTechRequired
        {
            get { return m_upgrade_origonalTechRequired; }
        }


        /************************************************************************\
         * YT_TechRequiredDatabase class                                        *
         * Constructor                                                          *
        \************************************************************************/
        private YT_TechRequiredDatabase()
        {
            m_part_origonalTechRequired = new Dictionary<string, string>();
            m_upgrade_origonalTechRequired = new Dictionary<string, string>();
            //LoadTechRequiredData();
        }

        /************************************************************************\
         * YT_TechRequiredDatabase class                                        *
         * CheckAndAddPart function                                             *
         *                                                                      *
         * If partName is not already in the database it is added.              *
        \************************************************************************/
        public void CheckAndAddPart(string partName, string techRequired)
        {
#if DEBUG
            Log.Info("YT_TechRequiredDatabase.CheckAndAddPart");
#endif
            if (! m_part_origonalTechRequired.ContainsKey(partName))
            {
                try
                {
                    m_part_origonalTechRequired.Add(partName, techRequired);
                }
                catch (ArgumentException)
                {
                    Log.Info("YT_TechRequiredDatabase.CreateDatabase: ERROR part with the same name already exisits " + partName);
                }
            }
        }
        /************************************************************************\
         * YT_TechRequiredDatabase class                                        *
         * CheckAndAddUpgrade function                                          *
         *                                                                      *
         * If upgradName is not already in the database it is added.            *
        \************************************************************************/
        public void CheckAndAddUpgrade(string upgradName, string techRequired)
        {
#if DEBUG
            Log.Info("YT_TechRequiredDatabase.CheckAndAddUpgrade");
#endif
            if (! m_upgrade_origonalTechRequired.ContainsKey(upgradName))
            {
                try
                {
                    m_upgrade_origonalTechRequired.Add(upgradName, techRequired);
                }
                catch (ArgumentException)
                {
                    Log.Info("YT_TechRequiredDatabase.CheckAndAddUpgrade: ERROR part upgrade with the same name already exisits " + upgradName);
                }
            }
        }

    } //END of YT_TechRequiredDatabase
    

    /*======================================================*\
     * YT_TechTreeDatabase class                            *
     * Singleton class to keep track of the TechTree        *
     * ConfigNodes.                                         *
    \*======================================================*/
    public class YT_TechTreeDatabase
    {
        //Singleton
        private static YT_TechTreeDatabase instance = null;
        public static YT_TechTreeDatabase Instance
        {
            get
            {
                if (null == instance)
                    instance = new YT_TechTreeDatabase();

                return instance;
            }
        }

        private List<YT_TreeDeclaration> m_techTrees;
        public List<YT_TreeDeclaration> TechTrees { get { return m_techTrees; } }


        /************************************************************************\
         * YT_TechTreeDatabase class                                            *
         * Constructor                                                          *
        \************************************************************************/
        private YT_TechTreeDatabase()
        {
            m_techTrees = new List<YT_TreeDeclaration>();
            LoadTechTreeData();
        }


        /************************************************************************\
         * YT_TechTreeDatabase class                                            *
         * LoadTechTreeData function                                            *
         *                                                                      *
         * Finds TechTree ConfigNodes in the GameDatabase and sets up           *
         * m_treeDeclarationsList with infor on each TechTree.                  *
        \************************************************************************/
        private void LoadTechTreeData()
        {
#if DEBUG
            Log.Info("YT_TechTreesScenario.LoadModData");
#endif
            bool isStock = false;
            ConfigNode node;
            string title;
            string url;
            string description;

            //Traverse through game configs looking for TechTree configs
            foreach (UrlDir.UrlConfig config in GameDatabase.Instance.root.GetConfigs("TechTree"))
            {
                isStock = false;
#if DEBUG
                    Log.Info("YT_TechTreesScenario.LoadTechTreeData: found TechTree Config url = GameData/" + config.parent.url + "." + config.parent.fileExtension);
#endif
                node = config.config;
                url = "GameData/" + config.parent.url + "." + config.parent.fileExtension;

                //Check if this is the stock tree
                //use stock tree title and description loaded from mod config file if it is
                if (url == YT_TechTreesSettings.Instance.StockTree_url)
                {
                    isStock = true;
                    title = YT_TechTreesSettings.Instance.StockTree_title;
                    description = YT_TechTreesSettings.Instance.StockTree_description;
                }

                //If not stock tree attempt to read title and description from the TechTree ConfigNode
                //if they can't be read default values are used instead
                else
                {
                    if (node.HasValue(YT_TechTreesSettings.TECHTREE_FIELD_TITLE))
                        title = node.GetValue(YT_TechTreesSettings.TECHTREE_FIELD_TITLE);
                    else
                        title = "(" + config.parent.url + ")";

                    if (node.HasValue(YT_TechTreesSettings.TECHTREE_FIELD_DESCRIPTION))
                        description = node.GetValue(YT_TechTreesSettings.TECHTREE_FIELD_DESCRIPTION);
                    else
                        description = "No description available.";
                }

                //create YT_TreeDeclaration and add stats
                YT_TreeDeclaration treeData = new YT_TreeDeclaration(title, url, description);
                CalculateTechTreeStats(node, treeData);

                //Add information to treeDeclarationList
                if (isStock)
                    m_techTrees.Insert(0, treeData);
                else
                    m_techTrees.Add(treeData);
            }
        }


        /************************************************************************\
         * YT_TechTreeDatabase class                                            *
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
                if (RDNode.HasValue("hideEmpty") && "TRUE" == RDNode.GetValue("hideEmpty").ToUpper())
                {
                    isHidden = true;

                    if (null != (UnlocksNode = RDNode.GetNode(YT_TechTreesSettings.RDNode_UNLOCKSNODE_NAME)))
                    {
                        //Check all parts in Unlocks against the PartLoader to see if they exist
                        foreach (string partName in UnlocksNode.GetValues(YT_TechTreesSettings.RDNode_UNLOCKSNODE_FIELD_PART))
                        {
                            if (null != PartLoader.getPartInfoByName(partName))
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

                        if (nodeCost > YT_TechTreesSettings.Instance.RDNode_maxCost2)
                            treeData.numNodes_level3++;
                        else if (nodeCost > YT_TechTreesSettings.Instance.RDNode_maxCost1)
                            treeData.numNodes_level2++;
                        else
                            treeData.numNodes_level1++;
                    }
                }
            }
        }
    } //END of YT_TechTreeDatabase

}
