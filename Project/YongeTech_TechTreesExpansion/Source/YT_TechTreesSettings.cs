using System;
using System.Collections.Generic;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{
    
    /*======================================================*\
     * YT_RequiredTechDatabase class                        *
     * KSPAddon to force creation and initilization of      *
     * setting classes.                                     *
    \*======================================================*/
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class YT_TechTreesSettingsLoader : MonoBehaviour
    {
        public void Start()
        {
            YT_TechTreesSettings setting = YT_TechTreesSettings.Instance;
            YT_TechRequiredDatabase database = YT_TechRequiredDatabase.Instance;
        }
    }

    /*======================================================*\
     * YT_TechTreesSettings class                           *
     * Singleton class to handle reading and storing data   *
     * from the config file.                                *
    \*======================================================*/
    public class YT_TechTreesSettings
    {
        //Singleton
        private static YT_TechTreesSettings instance = null;
        public static YT_TechTreesSettings Instance
        {
            get
            {
                if(null == instance)
                    instance = new YT_TechTreesSettings();

                return instance;
            }
        }

        //TechTreesScenario settings
        private bool m_allowTreeSelection;
        public bool AllowTreeSelection { get { return m_allowTreeSelection; } }

        private string m_stockTree_url;
        private string m_stockTree_title;
        private string m_stockTree_description;
        public string StockTree_url { get { return m_stockTree_url; } }
        public string StockTree_title { get { return m_stockTree_title; } }
        public string StockTree_description { get { return m_stockTree_description; } }

        private int m_RDNode_maxCost1;
        private int m_RDNode_maxCost2;
        public int RDNode_maxCost1 { get { return m_RDNode_maxCost1; } }
        public int RDNode_maxCost2 { get { return m_RDNode_maxCost2; } }

        //TechTreesSelectionWindow settings
        private Rect m_windowRect;
        public Rect WindowRect { get { return m_windowRect; } }
        private int m_dropdownMaxSize;
        public int DropdownMaxSize { get { return m_dropdownMaxSize; } }

        private string m_windowTitle;
        private string m_introText;
        private string m_confermButtonText;
        public string WindowTitle { get { return m_windowTitle; } }
        public string IntroText { get { return m_introText; } }
        public string ConfermButtonText { get { return m_confermButtonText; } }

        private string m_portraitURL;
        private string m_portraitName;
        public string PortraitURL { get { return m_portraitURL; } }
        public string PortraitName { get { return m_portraitName; } }


        /************************************************************************\
         * YT_TechTreesSettings class                                           *
         * Constructor                                                          *
        \************************************************************************/
        YT_TechTreesSettings()
        {
            m_windowRect = new Rect();

            ReadConfigFile();
        }

        /************************************************************************\
         * YT_TechTreesSettings class                                           *
         * ReadConfigFile function                                              *
         *                                                                      *
         * Helper function for the constructor.                                 *
         * Loads data from the mod's config file.                               *
        \************************************************************************/
        private void ReadConfigFile()
        {
#if DEBUG
            Debug.Log("YT_TechTreesSettings.ReadConfigFile()");
#endif
            //Read Mod Configuration File

            KSP.IO.PluginConfiguration configFile = KSP.IO.PluginConfiguration.CreateForType<YT_TechTreesSettings>();
            configFile.load();

            //TechTreesScenario settings
            m_stockTree_url = configFile.GetValue<string>("stockTree_url");
            m_stockTree_title = configFile.GetValue<string>("stockTree_title");
            m_stockTree_description = configFile.GetValue<string>("stockTree_description");

            m_RDNode_maxCost1 = configFile.GetValue<int>("RDNodeMaxCost_level1");
            m_RDNode_maxCost2 = configFile.GetValue<int>("RDNodeMaxCost_level2");

            m_allowTreeSelection = configFile.GetValue<bool>("allowTreeSelection");

            //TechTreesSelectionWindow settings
            m_windowRect.x = configFile.GetValue<int>("window_x");
            m_windowRect.y = configFile.GetValue<int>("window_y");
            m_windowRect.width = configFile.GetValue<int>("window_width");
            m_windowRect.height = configFile.GetValue<int>("window_height");
            m_dropdownMaxSize = configFile.GetValue<int>("dropdown_maxSize");

            m_windowTitle = configFile.GetValue<string>("window_title");
            m_introText = configFile.GetValue<string>("intro_text");
            m_confermButtonText = configFile.GetValue<string>("conferm_text");

            m_portraitURL = configFile.GetValue<string>("portrait_textureUrl");
            m_portraitName = configFile.GetValue<string>("portrait_name");

#if DEBUG
            string values = "";
            values += "m_stockTree_url = " + m_stockTree_url + "\n";
            values += "m_stockTree_title = " + m_stockTree_title + "\n";
            values += "m_stockTree_description = " + m_stockTree_description + "\n";
            values += "m_RDNode_maxCost1 = " + m_RDNode_maxCost1 + "\n";
            values += "m_RDNode_maxCost2 = " + m_RDNode_maxCost2 + "\n";
            values += "m_allowTreeSelection = " + m_allowTreeSelection + "\n";

            values += "windowX = " + m_windowRect.x + "\n";
            values += "windowY = " + m_windowRect.y + "\n";
            values += "windowWidth = " + m_windowRect.width + "\n";
            values += "windowHeight = " + m_windowRect.height + "\n";
            values += "m_dropdownMaxSize = " + m_dropdownMaxSize + "\n";
            values += "m_windowTitle = " + m_windowTitle + "\n";
            values += "m_introText = " + m_introText + "\n";
            values += "m_confermButtonText = " + m_confermButtonText + "\n";
            values += "m_portraitURL = " + m_portraitURL + "\n";
            values += "m_portraitName = " + m_portraitName + "\n";
            Debug.Log("YT_TechTreesSettings.ReadConfigFile(): values\n" + values);
#endif
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
        public YT_TechRequiredDatabase()
        {
            m_origonalTechRequired = new Dictionary<string, string>();
            CreateDatabase();
        }


        /************************************************************************\
         * YT_TechRequiredDatabase class                                        *
         * CreateDatabase function                                              *
         *                                                                      *
         * Reads and stores the origonal TechRequired from all parts.           *
        \************************************************************************/
        private void CreateDatabase()
        {
#if DEBUG
            Debug.Log("YT_TechRequiredDatabase.CreateDatabase()");
#endif
            ConfigNode node;
            string techID = null;

            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
#if DEBUG
                Debug.Log("YT_TechRequiredDatabase.CreateDatabase(): looking at: " + part.name);
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
                    Debug.Log("YT_TechRequiredDatabase.CreateDatabase(): WARNING part.partUrlConfig.parent.fullPath is null for " + part.name);
#endif
                    continue;
                }
                if (null == (node = ConfigNode.Load(part.partUrlConfig.parent.fullPath)))
                {
                    Debug.Log("YT_TechRequiredDatabase.CreateDatabase(): ERROR unable to load ConfigNode for " + part.name + ".  from file: " + part.partUrlConfig.parent.fullPath);
                    continue;
                }
                if (null == (node = node.GetNode("PART")) || null == (techID = node.GetValue("TechRequired")))
                {
                    Debug.Log("YT_TechRequiredDatabase.CreateDatabase(): ERROR can't find TechRequired in ConfigNode for " + part.name + ". Node:\n" + node.ToString());
                    continue;
                }

                //Store the TechRequired from the origonal config file with the name of the part
                m_origonalTechRequired.Add(part.name, techID);
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
    }
}
