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

        //dictionary of starting techRequired
        private Dictionary<string, string> m_origonalTechRequired;


        /************************************************************************\
         * YT_TechRequiredDatabase class                                        *
         * Constructor                                                          *
        \************************************************************************/
        private YT_TechRequiredDatabase()
        {
            m_origonalTechRequired = new Dictionary<string, string>();
            LoadTechRequiredData();
        }


        /************************************************************************\
         * YT_TechRequiredDatabase class                                        *
         * CreateDatabase function                                              *
         *                                                                      *
         * Reads and stores the origonal TechRequired from all parts.           *
        \************************************************************************/
        private void LoadTechRequiredData()
        {
#if DEBUG
            Debug.Log("YT_TechRequiredDatabase.CreateDatabase()");
#endif
            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
                //Store the TechRequired from the origonal config file with the name of the part
                try
                {
                    m_origonalTechRequired.Add(part.name, part.TechRequired);
                }
                catch(ArgumentException)
                {
                    Debug.Log("YT_TechRequiredDatabase.CreateDatabase(): ERROR part with the same name already exisits " + part.name);
                }
            }
        }


        /************************************************************************\
         * YT_TechRequiredDatabase class                                        *
         * GetOrigonalTechID function                                           *
         *                                                                      *
         * Attempt to get the origonal TechRequired for the specified part.     *
         * Returns:                                                             *
         *   techID of the origonal TechRequired for the part.                  *
         *   null if partName not found in the database.                        *
        \************************************************************************/
        public string GetOrigonalTechID(string partName)
        {
#if DEBUG
            Debug.Log("YT_TechRequiredDatabase.GetOrigonalTechID(" + partName + ")");
#endif
            string techID = null;

            if (!m_origonalTechRequired.TryGetValue(partName, out techID))
            {
                techID = null;
                Debug.Log("YT_TechRequiredDatabase.GetOrigonalTechID(): WARNING " + partName + " not found in database");
            }

            return techID;
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
            Debug.Log("YT_TechTreesScenario.LoadModData()");
#endif
            bool isStock = false;
            ConfigNode node;
            string title;
            string url;
            string description;

            //Traverse through game configs looking for TechTree configs
            foreach (UrlDir.UrlConfig config in GameDatabase.Instance.root.AllConfigs)
            {
                isStock = false; 

                if ("TechTree" == config.name)
                {
#if DEBUG
                    Debug.Log("YT_TechTreesScenario.LoadModData(): found TechTree Config url = GameData/" + config.parent.url + "." + config.parent.fileExtension);
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
