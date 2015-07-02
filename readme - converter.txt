YongeTech Tech Tree Converter
version: 1.0
author: yongedevil  
============================


======================================
Contents
1.0 Description
2.0 Installation
3.0 Converting a Tech Tree
4.0 Licence
--------------------------------------


================
1.0 Description:
----------------
This plugin runs at startup and will generate a copy of each TechTree ConfigNode currently installed with the Unlocks nodes filled in.
It adds to the Unlocks node all parts that:
 - have a TechRequired of that node.
 - are not listed in another tech node's Unlock section.
 
The results are writen to files inside the "ConvertedTrees" directory where the mod is installed in Kerbal.

This makes it possible to convert trees that use ModuleManager, albeit one at a time.


=================
2.0 Installation:
-----------------
Copy the contents of GameData into the Kerbal GameData folder.


===========================
3.0 Converting a Tech Tree:
---------------------------
To convert a tech tree install it into Kerbal along with this Converter.  YongeTech Tech Trees Plugin does not need to be installed.  Once the main menu loads the converter will run and write a file with a copy of each TechTree.  The output files are not named so inspect each one to locate the tree you are looking for.  Copy the data into a .cfg file, and add the "title" and "description" fields.


============
4.0 Licence:
------------
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.