YongeTech Tech Trees Plugin
version: 1.0
author: yongedevil  
============================


======================================
Contents
1.0 Description
2.0 Installation
3.0 Creating a Tech Tree
  3.1 TechTree ConfigNode
  3.2 Custom Icons
  3.3 Example TechTree ConfigNode
4.0 Licence
--------------------------------------


================
1.0 Description:
----------------
This mod's intention is to make it easier to customize tech trees for Kerbal Space Program.  It allows part unlocks to be listed in the TechTree ConfigNode rather than having to edit every Part ConfigNode.  It also adds a tree selection window to let the player select a tree for each new game.


=================
2.0 Installation:
-----------------
Copy the contents of GameData into the Kerbal GameData folder.

  2.1 Known Issues:
  -----------------
  Both this mod and ModuleManager (on version 2.6.5 as of this writing) attempt to edit the tech tree of the current game.  This mod will override ModuleManager's edits by default. If you would like to keep ModuleManager's edit change the "allowTreeSelection" field in PluginData/YongeTech_TechTreesExpansion/config.xml from '1' to '0'


=========================
3.0 Creating a Tech Tree:
-------------------------
The mod adds new support to the TechTree ConfigNode and will create icons for use in a tech tree from custom textures.

  3.1 TechTree ConfigNode:
  ------------------------
  The mod adds support for new fields and a new subnode in the TechTree ConfigNode.  These are intended to make creating a custom tech tree easier.  These changes do two things: they store information about the tech tree to display to the player when selecting a tree to use, and they allow parts to be assigned to techs in the TechTree node rather than requiring each PART node to be edited.

    3.1.1 TechTree Node Fields:
    ---------------------------
    Support for 3 new fields has been added to the TechTree ConfigNode.

    title: The name of the tree.  Displayed in the tech tree selection window when starting a new game. (approximately a 45 character limit)

    description: Single paragraph description of the tree for the player.  Displayed in the tech tree selection window when starting a new game. (approximately a 400 character limit)

    unlockAllStartParts: If True the mod will automatically unlock all parts in the starting tech node(s), which are any nodes with a cost of 0.  The game does this for all parts with an entryCost of 0, this option extends that to all parts.  This is only relevant if No Entry Purchase Required on Research is disabled in difficulty settings.

    3.1.2 Unlocks Node:
    -------------------
    The Unlocks Node is a new subnode for the RDNode in TechTree.  The Unlocks node has a list of "part" fields containing the names of all the parts unlocked by the RDNode.  The mod will read this data and handle editing the TechRequired on all the parts.


  3.2 Custom Icons:
  -----------------
  The mod also adds support for custom icons.  All icons should be placed in a directory named "RDSimpleIcons".  .png and .tga formats are supported.  The mod will create a custom icon with the same name as the file, without the extension.

  .../RDSimpleIcons/customIcon_tech.png will become "icon = customIcon_tech"


  3.3 Example TechTree ConfigNode:
  --------------------------------

TechTree
{
  title = Sample TechTree
  description = An incomplete TechTree showing the basic setup
  unlockAllStartParts = True
  
  RDNode
  {
    id = start
    title = Starting Technology
    description = We haven't hired the engineering team yet, but our scavengers are the best of the best.
    cost = 0
    hideEmpty = False
    nodepart = node0_0_start
    anyToUnlock = False
    icon = customIcon_tech
    pos = -2200,1000,-1
    scale = 0.6
    
    Unlocks
    {
      part = solidBooster.sm
      part = probeStackAdapter
      part = standardNoseCone
      part = sensorThermometer
      part = basicFin
    }
  }

  ...

}




============
4.0 Licence:
------------