YongeTech Tech Tree Selection
version: 1.0
author: yongedevil  
============================


======================================
Contents
1.0 Description
2.0 Creating Tech Tree
  2.1 TechTree ConfigNode
  2.2 Custom Icons
  2.3 Example TechTree ConfigNode
3.0 Installation
4.0 Licence
----------------------------------------


================
1.0 Description:
----------------
The mod's intention is to make it easier to customize tech trees for Kerbal Space Program.  It allows part unlocks to be listed in the TechTree ConfigNode rather than having to edit every Part ConfigNode.  It also adds a tree selection window to let the player select a tree for each new game.



=======================
2.0 Creating Tech Tree:
-----------------------
The mod adds new support to the TechTree ConfigNode and will create custom icons for use in a tech tree from textures.

  2.1 TechTree ConfigNode:
  ------------------------
  The mod adsd support for new fields and a new subnode in the TechTree ConfigNode indented to make creating a custom tech tree easier.  These changes do two things: they store information about the techTree to display to the player when selecting a tree to use, and they allow parts to be assigned to techs in the TechTree node rather than requiring each PART node to be edited.

    2.1.1 TechTree Node Fields:
    ---------------------------
    Support for 3 new fields has been added to the TechTree ConfigNode.

    title: The name of the tree.  Displayed in the tech tree selection window when starting a new game.
    
    description: Single paragraph discription of the tree for the player.  Displayed in the tech tree selection window when starting a new game.
    
    unlockAllStartParts: If True the mod will automitically unlock all parts in the starting tech node.  Normally the game will only unlock parts with an entryCost of 0.

    2.1.2 Unlocks Node:
    -------------------
    The Unlocks Node is a new subnode for the RDNode in TechTree.  The Unlocks node has a list of part fields containing the names of all the parts unlocked by the RDNode.  The mod will read this data and handle editing the TechRequired on all the parts.

    Note: that the game converts '_' to '.' so for parts such as the RT-5, its name is "solidBooster_sm", but its entry in Unlocks needs to be "solidBooster.sm".


  2.2 Custom Icons:
  -----------------
  The mod also adds support for custom icons.  All icons should be placed in a directory named "RDSimpleIcons" and be in .png format.  The mod will create a custom icon with the same name as the file, without the extention.

  .../RDSimpleIcons/customIcon.png will become "icon = customIcon"


  2.3 Example TechTree ConfigNode:
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
    icon = RDicon_customIcon
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
  
  RDNode
  {
    id = telemetry
    title = Telemetry
    description = With this new technology we well be able to transmit data from spacecraft.  We can only get so much data this way though, it would be much better if we had a kerbal there to record the results in person.
    cost = 2
    hideEmpty = False
    nodepart = node1_1_telemetry
    anyToUnlock = False
    icon = RDiconTelemetry
    pos = -2150,1050,-1
    scale = 0.6
    Unlocks
    {
      part = sensorBarometer
      part = longAntenna
      part = batteryPack
    }
  
    Parent
    {
      parentID = start
      lineFrom = TOP
      lineTo = LEFT
    }
  }

  ...

}



=================
3.0 Installation:
-----------------
Copy the contents of GameData into the Kerbal GameData folder.


============
4.0 Licence:
------------