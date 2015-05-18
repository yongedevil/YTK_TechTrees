using System;
using System.Collections.Generic;
//using System.Linq;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{
    /*======================================================*\
     * YT_TechTreeSelectionWindow class                     *
     * Class to handle drawing the tree selection window    *
     * to let the player select a tree to play with.        *
    \*======================================================*/
    class YT_TechTreeSelectionWindow
    {
        private int windowWidth;
        private int windowHeight;

        //Constants for placing elements in the window
        private const int SPACING_SECTION_HORIZONTAL = 12;
        private const int SPACING_SECTION_VERTICAL = 24;

        private const int SPACING_BOARDER_RIGHT = 10;
        private const int SPACING_BOARDER_LEFT = 10;
        private const int SPACING_BOARDER_TOP = 25;
        private const int SPACING_BOARDER_BOTTOM = 10;

        private const int LOGO_WIDTH = 128;
        private const int LOGO_HEIGHT = 128;

        private const int DROPDOWN_LINE = 30;
        private const int DROPDOWN_WIDTH = 350;

        private const int CONFERM_WIDTH = 200;


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


        //GUIStyles
        public GUIStyle WindowStyle { get { return m_windowStyle; } }
        private GUIStyle m_windowStyle;

        private GUIStyle m_textAreaStyle;
        private GUIStyle m_confermButtonStyle;
        private GUIStyle m_dropdownButtonStyle;
        private GUIStyle m_dropdownBoxStyle;
        private GUIStyle m_dropdownListItemStyle;

        
        //Rects for elements in the window
        public Rect windowRect; //public so the class that created the window can access it.
        private Rect m_logoRect;
        private Rect m_intoRect;
        private Rect m_treeDetailsRect;
        private Rect m_dropdownRect;
        private Rect m_confermRect;

        private Rect m_scrollView_positionRect;
        private Rect m_scrollView_viewRect;



        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
         * Constructor                                                          *
         *                                                                      *
        \************************************************************************/
        public YT_TechTreeSelectionWindow()
        {
            //initialize variables
            m_done = false;
            m_dropdownMaxSize = 3;
            m_dropdownOpen = false;
            m_scrollViewVector = Vector2.zero;

            m_windowTitle = null;
            m_introText = null;
            m_confermButtonText = null;

            m_techTrees = null;
            m_choiceIndex = 0;

            //load data from config file
            ReadConfigFile();

            //helper initialization functions
            InitializeStyles();
            InitializeRects();

        }

        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
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
        }

        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
         * InitializeRects function                                             *
         *                                                                      *
         * Helper function for the constructor.                                 *
         * Initializes the Rects used in this window.                           *
        \************************************************************************/
        private void InitializeRects()
        {
            //Setup rectagles for main areas
            windowRect = new Rect(Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight);

            //Logo and intro text areas up top
            m_logoRect = new Rect(SPACING_BOARDER_LEFT, SPACING_BOARDER_TOP, LOGO_WIDTH, LOGO_HEIGHT);
            m_intoRect = new Rect();
            m_intoRect.x = m_logoRect.x + m_logoRect.width + SPACING_SECTION_HORIZONTAL;
            m_intoRect.y = m_logoRect.y;
            m_intoRect.width = windowWidth - (m_logoRect.x + m_logoRect.width + SPACING_SECTION_HORIZONTAL + SPACING_BOARDER_RIGHT);
            m_intoRect.height = m_logoRect.height;

            //Tree description area on the bottom
            m_treeDetailsRect = new Rect();
            m_treeDetailsRect.x = SPACING_BOARDER_LEFT;
            m_treeDetailsRect.y = m_logoRect.y + m_logoRect.height + SPACING_SECTION_VERTICAL * 2 + DROPDOWN_LINE;
            m_treeDetailsRect.width = windowWidth - (SPACING_BOARDER_LEFT + SPACING_BOARDER_RIGHT);
            m_treeDetailsRect.height = windowHeight - (m_treeDetailsRect.y + SPACING_BOARDER_BOTTOM);

            //Dropdown menu in the middle
            m_dropdownRect = new Rect();
            m_dropdownRect.x = SPACING_BOARDER_LEFT;
            m_dropdownRect.y = m_logoRect.y + m_logoRect.height + SPACING_SECTION_VERTICAL;
            m_dropdownRect.width = DROPDOWN_WIDTH;
            m_dropdownRect.height = 3 * DROPDOWN_LINE;

            //ScrollView for the dropdown
            m_scrollView_positionRect = new Rect(m_dropdownRect);
            m_scrollView_positionRect.y = m_dropdownRect.y + DROPDOWN_LINE;

            m_scrollView_viewRect = new Rect(0, 0, m_dropdownRect.width - 25, m_dropdownRect.height);  //-25 to leave space for the scoll bar


            //Conferm button to the right of the dropdown
            m_confermRect = new Rect();
            m_confermRect.x = windowWidth - (CONFERM_WIDTH + SPACING_BOARDER_RIGHT);
            m_confermRect.y = m_dropdownRect.y;
            m_confermRect.width = CONFERM_WIDTH;
            m_confermRect.height = DROPDOWN_LINE;
        }


        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
         * LoadConfigFile function                                              *
         *                                                                      *
         * Helper function for the constructor.                                 *
         * Loads data from the mod's config file.                               *
        \************************************************************************/
        private void ReadConfigFile()
        {
#if DEBUG
            Debug.Log("YT_TechTreeSelectionWindow.ReadConfigFile()");
#endif
            KSP.IO.PluginConfiguration configFile = KSP.IO.PluginConfiguration.CreateForType<YT_TechTreeSelectionWindow>();

            configFile.load();
            windowWidth = configFile.GetValue<int>("window_width");
            windowHeight = configFile.GetValue<int>("window_height");
            m_dropdownMaxSize = configFile.GetValue<int>("dropdown_maxSize");

            m_windowTitle = configFile.GetValue<string>("window_title");
            m_introText = configFile.GetValue<string>("intro_text");
            m_confermButtonText = configFile.GetValue<string>("conferm_text");
#if DEBUG
            string values = "windowWidth = " + windowWidth + "\n";
            values += "windowHeight = " + windowHeight + "\n";
            values += "m_dropdownMaxSize = " + m_dropdownMaxSize + "\n";
            values += "m_windowTitle = " + m_windowTitle + "\n";
            values += "m_introText = " + m_introText + "\n";
            values += "m_confermButtonText = " + m_confermButtonText + "\n";
            Debug.Log("YT_TechTreeSelectionWindow.ReadConfigFile(): values\n" + values);
#endif
        }


        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
         * DrawWindow function                                                  *
         *                                                                      *
        \************************************************************************/
        public void DrawWindow(int id)
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreeSelectionWindow.DrawWindow()");
#endif
            DrawHead();
            DrawTreeDetails();

            DrawDropdown();
            DrawConferm();

            GUI.DragWindow(new Rect(0, 0, windowWidth, windowHeight));

        }


        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
         * DrawHead function                                                    *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the logo and intro text at the top of the window.              *
        \************************************************************************/
        private void DrawHead()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreeSelectionWindow.DrawHead()");
#endif
            GUI.Box(m_logoRect, GameDatabase.Instance.GetTextureInfo(HighLogic.CurrentGame.flagURL).texture);

            GUI.Box(m_intoRect, m_introText, m_textAreaStyle);
        }

        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
         * DrawTreeDetails function                                             *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the tree details section at the bottom of the window.          *
        \************************************************************************/
        private void DrawTreeDetails()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreeSelectionWindow.DrawTreeDetails()");
#endif
            GUI.Box(m_treeDetailsRect, m_techTrees[m_choiceIndex].desctription, m_textAreaStyle);
        }

        /************************************************************************\
         * YT_TechTreeSelectionWindow class                                     *
         * DrawDropDown function                                                *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the dropdown selection for the tech tree.                      *
        \************************************************************************/
        private void DrawDropdown()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreeSelectionWindow.DrawDropdown()");
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
         * YT_TechTreeSelectionWindow class                                     *
         * DrawConferm function                                                 *
         *                                                                      *
         * Helper function for DrawWindow.                                      *
         * Draws the conferm button.                                            *
        \************************************************************************/
        private void DrawConferm()
        {
#if DEBUG_UPDATE
            Debug.Log("YT_TechTreeSelectionWindow.DrawConferm()");
#endif
            if (GUI.Button(m_confermRect, m_confermButtonText, m_confermButtonStyle))
            {
                m_done = true;
            }
        }
    }
}
