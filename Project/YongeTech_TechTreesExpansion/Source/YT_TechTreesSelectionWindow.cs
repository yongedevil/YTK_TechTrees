using System;
using System.Collections.Generic;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{
    /*======================================================*\
     * YT_TechTreesSelectionWindow class                    *
     * Class to handle drawing the tree selection window    *
     * to let the player select a tree to play with.        *
    \*======================================================*/
    class YT_TechTreesSelectionWindow
    {
        private int windowX;
        private int windowY;
        private int windowWidth;
        private int windowHeight;

        //Constants for placing elements in the window
        private const int SPACING_SECTION_HORIZONTAL = 12;
        private const int SPACING_SECTION_VERTICAL = 8;

        private const int SPACING_BOARDER_RIGHT = 10;
        private const int SPACING_BOARDER_LEFT = 10;
        private const int SPACING_BOARDER_TOP = 25;
        private const int SPACING_BOARDER_BOTTOM = 10;

        private const int PORTRAIT_WIDTH = 128;
        private const int PORTRAIT_HEIGHT = 128;
        private const int PORTRAIT_NAME_HEIGHT = 40;

        private const int DROPDOWN_LINE = 30;
        private const int DROPDOWN_WIDTH = 350;

        private const int CONFERM_WIDTH = 200;

        private const int STATS_HEADER_HEIGHT = 20;
        private const int STATS_WIDTH = 200;


        //Done is set to true when the user has clicked conferm
        private bool m_done;
        public bool Done { get { return m_done; } }

        private int m_dropdownMaxSize;
        private bool m_dropdownOpen;
        private Vector2 m_scrollViewVector;


        //Tech Tree variables hold the list of trees and return the selected tree url
        public string TechTreeURL { get { return m_techTrees[m_choiceIndex].url; } }
        public YT_TreeDeclaration[] TechTrees
        {
            set
            {
                //sets the array of tree declarations
                //sets the size of the dropdown list to m_dropdownMaxSize, or the length of the array, whichever is shorter
                //sets the size of the scrollView to the size of the dropdown or the length of the array, whichever is longer (scoll bar only shows when the array is longer)
                m_techTrees = value;
                m_dropdownRect.height = Math.Min(m_dropdownMaxSize, m_techTrees.Length) * DROPDOWN_LINE;
                m_scrollView_viewRect.height = Mathf.Max(m_dropdownRect.height, (m_techTrees.Length * DROPDOWN_LINE));
            }
        }

        private YT_TreeDeclaration[] m_techTrees;
        private int m_choiceIndex;


        //strings storing texted used
        public string WindowTitle { get { return m_windowTitle; } }
        private string m_windowTitle;
        private string m_introText;
        private string m_confermButtonText;

        //stores the kerbal portrait displayed in the window
        private Texture m_portraitTexture;
        private string m_portraitURL;
        private string m_portraitName;


        //GUIStyles
        public GUIStyle WindowStyle { get { return m_windowStyle; } }
        private GUIStyle m_windowStyle;
        private GUIStyle m_textAreaStyle;
        private GUIStyle m_textAreaBoxStyle;
        private GUIStyle m_textAreaHeaderStyle;
        private GUIStyle m_confermButtonStyle;
        private GUIStyle m_dropdownButtonStyle;
        private GUIStyle m_dropdownBoxStyle;
        private GUIStyle m_dropdownListItemStyle;
        private GUIStyle m_portraitStyle;

        
        //Rects for elements in the window
        public Rect windowRect; //public so the class that created the window can access it.
        private Rect m_portraitRect;
        private Rect m_portraitNameRect;
        private Rect m_intoRect;
        private Rect m_treeDetailsRect;
        private Rect m_treeStatsHeaderRect;
        private Rect m_treeStatsRect;
        private Rect m_dropdownRect;
        private Rect m_confermRect;

        private Rect m_scrollView_positionRect;
        private Rect m_scrollView_viewRect;



        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public YT_TechTreesSelectionWindow()
        {
            //initialize variables
            m_done = false;
            m_dropdownMaxSize = 3;
            m_dropdownOpen = false;
            m_scrollViewVector = Vector2.zero;

            m_windowTitle = null;
            m_introText = null;
            m_confermButtonText = null;

            m_portraitTexture = null;
            m_portraitURL = null;
            m_portraitName = null;

            m_techTrees = null;
            m_choiceIndex = 0;

            //load data from config file
            ReadConfigFile();

            //helper initialization functions
            InitializeStyles();
            InitializeRects();
            InitializeTextures();

        }

        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * InitializeStyles function                                            *
         *                                                                      *
         * Helper function for the constructor.                                 *
         * Initializes the GUIStyles used in this window.                       *
        \************************************************************************/
        private void InitializeStyles()
        {
            m_windowStyle = new GUIStyle(HighLogic.Skin.box);
            m_windowStyle.padding.top = 10;
            m_windowStyle.alignment = TextAnchor.UpperCenter;
            m_windowStyle.fontSize = 14;
            m_windowStyle.normal.textColor = new Color(255f / 255f, 209f / 255f, 0f / 255f);
            m_windowStyle.focused.textColor = new Color(200f / 255f, 169f / 255f, 20f / 255f);

            m_dropdownButtonStyle = new GUIStyle(HighLogic.Skin.button);
            m_dropdownBoxStyle = new GUIStyle(HighLogic.Skin.box);
            m_dropdownListItemStyle = new GUIStyle(HighLogic.Skin.button);

            m_confermButtonStyle = new GUIStyle(HighLogic.Skin.button);
            m_confermButtonStyle.normal.textColor = new Color(242f / 255f, 198f / 255f, 0f / 255f);
            m_confermButtonStyle.hover.textColor = new Color(247f / 255f, 255f / 255f, 15f / 255f);
            m_confermButtonStyle.active.textColor = new Color(255f / 255f, 198f / 255f, 0f / 255f);

            m_textAreaStyle = new GUIStyle(HighLogic.Skin.textArea);
            m_textAreaStyle.normal.background = null;
            m_textAreaStyle.fontSize = 12;

            m_textAreaBoxStyle = new GUIStyle(HighLogic.Skin.textArea);
            m_textAreaStyle.fontSize = 12;

            m_textAreaHeaderStyle = new GUIStyle(HighLogic.Skin.textArea);
            m_textAreaHeaderStyle.padding = new RectOffset(3, 3, 3, 3);
            m_textAreaHeaderStyle.alignment = TextAnchor.UpperCenter;
            m_textAreaHeaderStyle.normal.background = null;
            m_textAreaHeaderStyle.fontSize = 14;
            m_textAreaHeaderStyle.fontStyle = FontStyle.Bold;
            m_textAreaHeaderStyle.normal.textColor = new Color(194f / 255f, 255f / 255f, 0f / 255f);

            m_portraitStyle = new GUIStyle(HighLogic.Skin.box);
            m_portraitStyle.padding = new RectOffset(0, 0, 0, 0);
            m_portraitStyle.normal.background = null;
        }

        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * InitializeRects function                                             *
         *                                                                      *
         * Helper function for the constructor.                                 *
         * Initializes the Rects used in this window.                           *
        \************************************************************************/
        private void InitializeRects()
        {
            //Setup rectagles for main areas
            //old code for centring the window
            //windowRect = new Rect(Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight);
            windowRect = new Rect(windowX, windowY, windowWidth, windowHeight);

            //Portrait and intro text areas up top
            m_portraitRect = new Rect(SPACING_BOARDER_LEFT, SPACING_BOARDER_TOP, PORTRAIT_WIDTH, PORTRAIT_HEIGHT);
            m_portraitNameRect = new Rect(m_portraitRect);
            m_portraitNameRect.y = m_portraitRect.y + m_portraitRect.height;
            m_portraitNameRect.height = PORTRAIT_NAME_HEIGHT;

            m_intoRect = new Rect();
            m_intoRect.x = m_portraitRect.x + m_portraitRect.width + SPACING_SECTION_HORIZONTAL;
            m_intoRect.y = m_portraitRect.y;
            m_intoRect.width = windowWidth - (m_portraitRect.x + m_portraitRect.width + SPACING_SECTION_HORIZONTAL + SPACING_BOARDER_RIGHT);
            m_intoRect.height = m_portraitNameRect.yMax - m_portraitRect.yMin;

            //Tree stats and description area on the bottom
            m_treeStatsHeaderRect = new Rect();
            m_treeStatsHeaderRect.x = windowRect.width - (SPACING_BOARDER_RIGHT + STATS_WIDTH);;
            m_treeStatsHeaderRect.y = m_intoRect.y + m_intoRect.height + SPACING_SECTION_VERTICAL * 2 + DROPDOWN_LINE;
            m_treeStatsHeaderRect.width = STATS_WIDTH;
            m_treeStatsHeaderRect.height = STATS_HEADER_HEIGHT;

            m_treeStatsRect = new Rect();
            m_treeStatsRect.x = windowRect.width - (SPACING_BOARDER_RIGHT + STATS_WIDTH);
            m_treeStatsRect.y = m_treeStatsHeaderRect.y + m_treeStatsHeaderRect.height;
            m_treeStatsRect.width = STATS_WIDTH;
            m_treeStatsRect.height = windowRect.height - (m_treeStatsRect.y + SPACING_BOARDER_BOTTOM);

            m_treeDetailsRect = new Rect();
            m_treeDetailsRect.x = SPACING_BOARDER_LEFT;
            m_treeDetailsRect.y = m_intoRect.y + m_intoRect.height + SPACING_SECTION_VERTICAL * 2 + DROPDOWN_LINE;
            m_treeDetailsRect.width = windowRect.width - (SPACING_BOARDER_LEFT + SPACING_SECTION_HORIZONTAL + m_treeStatsRect.width + SPACING_BOARDER_RIGHT);
            m_treeDetailsRect.height = windowRect.height - (m_treeDetailsRect.y);



            //Dropdown menu in the middle
            m_dropdownRect = new Rect();
            m_dropdownRect.x = SPACING_BOARDER_LEFT;
            m_dropdownRect.y = m_intoRect.y + m_intoRect.height + SPACING_SECTION_VERTICAL;
            m_dropdownRect.width = DROPDOWN_WIDTH;
            m_dropdownRect.height = 3 * DROPDOWN_LINE;

            //ScrollView for the dropdown
            m_scrollView_positionRect = new Rect(m_dropdownRect);
            m_scrollView_positionRect.y = m_dropdownRect.y + DROPDOWN_LINE;

            m_scrollView_viewRect = new Rect(0, 0, m_dropdownRect.width - 25, m_dropdownRect.height);  //-25 to leave space for the scoll bar


            //Conferm button to the right of the dropdown
            m_confermRect = new Rect();
            m_confermRect.x = windowRect.width - (CONFERM_WIDTH + SPACING_BOARDER_RIGHT);
            m_confermRect.y = m_dropdownRect.y;
            m_confermRect.width = CONFERM_WIDTH;
            m_confermRect.height = DROPDOWN_LINE;
        }

        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * InitializeTextures function                                          *
         *                                                                      *
         * Helper function for the constructor.                                 *
         * Initializes the Textures used in this window.                        *
        \************************************************************************/
        private void InitializeTextures()
        {
            GameDatabase.TextureInfo texInfo = GameDatabase.Instance.GetTextureInfo(m_portraitURL);

            if (null == texInfo)
            {
                Debug.Log("YT_TechTreesSelectionWindow.InitializeTextures(): ERROR unable to load portrait texture from " + m_portraitURL);
                return;
            }

            m_portraitTexture = GameDatabase.Instance.GetTextureInfo(m_portraitURL).texture;
        }


        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * LoadConfigFile function                                              *
         *                                                                      *
         * Helper function for the constructor.                                 *
         * Loads data from the mod's config file.                               *
        \************************************************************************/
        private void ReadConfigFile()
        {
#if DEBUG
            Debug.Log("YT_TechTreesSelectionWindow.ReadConfigFile()");
#endif
            KSP.IO.PluginConfiguration configFile = KSP.IO.PluginConfiguration.CreateForType<YT_TechTreesSelectionWindow>();

            configFile.load();
            windowX = configFile.GetValue<int>("window_x");
            windowY = configFile.GetValue<int>("window_y");
            windowWidth = configFile.GetValue<int>("window_width");
            windowHeight = configFile.GetValue<int>("window_height");
            m_dropdownMaxSize = configFile.GetValue<int>("dropdown_maxSize");

            m_windowTitle = configFile.GetValue<string>("window_title");
            m_introText = configFile.GetValue<string>("intro_text");
            m_confermButtonText = configFile.GetValue<string>("conferm_text");

            m_portraitURL = configFile.GetValue<string>("portrait_textureUrl");
            m_portraitName = configFile.GetValue<string>("portrait_name");

#if DEBUG
            string values = "";
            values += "windowX = " + windowX + "\n";
            values += "windowY = " + windowY + "\n";
            values += "windowWidth = " + windowWidth + "\n";
            values += "windowHeight = " + windowHeight + "\n";
            values += "m_dropdownMaxSize = " + m_dropdownMaxSize + "\n";
            values += "m_windowTitle = " + m_windowTitle + "\n";
            values += "m_introText = " + m_introText + "\n";
            values += "m_confermButtonText = " + m_confermButtonText + "\n";
            values += "m_portraitURL = " + m_portraitURL + "\n";
            values += "m_portraitName = " + m_portraitName + "\n";
            Debug.Log("YT_TechTreesSelectionWindow.ReadConfigFile(): values\n" + values);
#endif
        }


        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * DrawWindow function                                                  *
         *                                                                      *
        \************************************************************************/
        public void DrawWindow(int id)
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreesSelectionWindow.DrawWindow()");
#endif
            DrawHead();
            DrawTreeDetails();

            DrawDropdown();
            DrawConferm();

            GUI.DragWindow(new Rect(0, 0, windowWidth, windowHeight));
        }


        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * DrawHead function                                                    *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the logo and intro text at the top of the window.              *
        \************************************************************************/
        private void DrawHead()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreesSelectionWindow.DrawHead()");
#endif
            if (null != m_portraitTexture)
            {
                GUI.Box(m_portraitRect, GameDatabase.Instance.GetTextureInfo(m_portraitURL).texture, m_portraitStyle);
                GUI.Box(m_portraitNameRect, m_portraitName, m_textAreaHeaderStyle);
            }
            else
                GUI.Box(m_portraitRect, GameDatabase.Instance.GetTextureInfo(HighLogic.CurrentGame.flagURL).texture, m_portraitStyle);

            GUI.Box(m_intoRect, m_introText, m_textAreaStyle);
        }

        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * DrawTreeDetails function                                             *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the tree details section at the bottom of the window.          *
        \************************************************************************/
        private void DrawTreeDetails()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreesSelectionWindow.DrawTreeDetails()");
#endif
            string stats = "";
            stats += "Total Cost: " + m_techTrees[m_choiceIndex].totalCost;
            stats += "\nTotal Nodes: " + m_techTrees[m_choiceIndex].numNodes;
            stats += "\n  Teir 1 Nodes: " + m_techTrees[m_choiceIndex].numNodes_level1;
            stats += "\n  Teir 2 Nodes: " + m_techTrees[m_choiceIndex].numNodes_level2;
            stats += "\n  Teir 3 Nodes: " + m_techTrees[m_choiceIndex].numNodes_level3;

            GUI.Box(m_treeDetailsRect, m_techTrees[m_choiceIndex].desctription, m_textAreaStyle);

            GUI.TextArea(m_treeStatsHeaderRect, "Tree Stats", m_textAreaHeaderStyle);

            GUI.Box(m_treeStatsRect, stats, m_textAreaBoxStyle);
        }


        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * DrawDropDown function                                                *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the dropdown selection for the tech tree.                      *
        \************************************************************************/
        private void DrawDropdown()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreesSelectionWindow.DrawDropdown()");
#endif
            //Dropdown button displaying the current choice
            if (GUI.Button(new Rect(m_dropdownRect.x, m_dropdownRect.y, m_dropdownRect.width, DROPDOWN_LINE), m_techTrees[m_choiceIndex].title, m_dropdownButtonStyle))
                m_dropdownOpen = !m_dropdownOpen;

            //Dropdown menu
            if (m_dropdownOpen)
            {
                //Create scollview for dropdown
                m_scrollViewVector = GUI.BeginScrollView(m_scrollView_positionRect, m_scrollViewVector, m_scrollView_viewRect, HighLogic.Skin.horizontalScrollbar, HighLogic.Skin.verticalScrollbar);
                GUI.Box(new Rect(0, 0, m_dropdownRect.width, Mathf.Max(m_dropdownRect.height, (m_techTrees.Length * DROPDOWN_LINE))), "", m_dropdownBoxStyle);

                //Traverse through array of tree declarations and add each one to the dropdown list
                for (int i = 0; i < m_techTrees.Length; ++i)
                {
                    if (GUI.Button(new Rect(0, i * DROPDOWN_LINE, m_dropdownRect.width, DROPDOWN_LINE), m_techTrees[i].title, m_dropdownListItemStyle))
                    {
                        m_dropdownOpen = false;
                        m_choiceIndex = i;
                    }
                }
                GUI.EndScrollView();
            }
        }

        /************************************************************************\
         * YT_TechTreesSelectionWindow class                                    *
         * DrawConferm function                                                 *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the conferm button.                                            *
        \************************************************************************/
        private void DrawConferm()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreesSelectionWindow.DrawConferm()");
#endif
            if (GUI.Button(m_confermRect, m_confermButtonText, m_confermButtonStyle))
            {
                m_done = true;
            }
        }
    }
}
