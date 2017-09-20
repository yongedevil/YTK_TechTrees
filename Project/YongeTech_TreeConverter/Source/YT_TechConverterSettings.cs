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
    public class YT_TreeConverterSettingsLoader : MonoBehaviour
    {
        public void Start()
        {
            YT_TreeConverterSettings setting = YT_TreeConverterSettings.Instance;
        }
    }

    /*======================================================*\
     * YT_TechTreesSettings class                           *
     * Singleton class to handle reading and storing data   *
     * from the config file.                                *
    \*======================================================*/
    public class YT_TreeConverterSettings
    {
        //Singleton
        private static YT_TreeConverterSettings instance = null;
        public static YT_TreeConverterSettings Instance
        {
            get
            {
                if(null == instance)
                    instance = new YT_TreeConverterSettings();

                return instance;
            }
        }

        //Custom TechTree fields
        public const string RDNode_UNLOCKSNODE_NAME = "Unlocks";
        public const string RDNode_UNLOCKSNODE_FIELD_PART = "part";


        //TechTreesScenario settings
        private bool m_enableConverter;
        public bool EnableConverter { get { return m_enableConverter; } }

        private string m_treeWriteDir;
        public string TreeWriteDir { get { return m_treeWriteDir; } }


        /************************************************************************\
         * YT_TechTreesSettings class                                           *
         * Constructor                                                          *
        \************************************************************************/
        private YT_TreeConverterSettings()
        {
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
            Log.Info("YT_TreeConverterSettings.ReadConfigFile()");

            //Read Mod Configuration File
            KSP.IO.PluginConfiguration configFile = KSP.IO.PluginConfiguration.CreateForType<YT_TreeConverterSettings>();
            configFile.load();
            m_enableConverter = configFile.GetValue<bool>("enable");

            m_treeWriteDir = configFile.GetValue<string>("treeWriteDir");

            if (m_treeWriteDir[m_treeWriteDir.Length - 1] != '/')
            {
                Debug.Log("YT_TreeConverterSettings.ReadConfigFile(): WARRNING treeWriteDir (" + m_treeWriteDir + ") should probably end in a /");
            }
#if DEBUG
            string values = "";
            values += "m_enableConverter = " + m_enableConverter + "\n";
            values += "m_treeWriteDir = " + m_treeWriteDir + "\n";
            Debug.Log("YT_TreeConverterSettings.ReadConfigFile(): values\n" + values);
#endif
        }
    }
}
