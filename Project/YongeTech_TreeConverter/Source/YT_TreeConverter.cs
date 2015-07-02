using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{
    /*======================================================*\
     * YT_TreeConverter class                               *
     * KSPAddon to read existing TechTree config nodes and  *
     * fill in the Unlocks nodes to convert them to         *
     * YongeTech tech trees.                                *
    \*======================================================*/
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class YT_TreeConverter : MonoBehaviour
    {
        /************************************************************************\
         * YT_TreeConverter class                                               *
         * Start function                                                       *
         *                                                                      *
        \************************************************************************/
        public void Start()
        {
            int treeCount = 0;
            foreach (ConfigNode techTreeNode in GameDatabase.Instance.GetConfigNodes("TechTree"))
            {
                CompleteTree(techTreeNode, "tree " + ++treeCount + ".txt");
            }
        }


        /************************************************************************\
         * YT_TreeConverter class                                               *
         * CompleteTree function                                                *
         *                                                                      *
         * Prints out techTreeNode to the debug log with the Unlocks filled in  *
         * with all parts that:                                                 *
         * have a TechRequired of that node                                     *
         * are not listed in another tech node's Unlock section.                *
        \************************************************************************/
        private void CompleteTree(ConfigNode incompletetreeNode, string fileName)
        {
#if DEBUG
            Debug.Log("YT_TreeConverter.CompleteTree()");
#endif
            bool writeFailure = false;

            string techID = null;
            ConfigNode completedTreeNode = null;
            ConfigNode unlocksNode = null;

            List<string> partsInTree = null;
            List<string> partsAttachedtoTech = null;

            //Get a list of parts in Unlock nodes in the tree
            partsInTree = GetPartsContainedInTree(incompletetreeNode);

            //Create a copy of the tree
            completedTreeNode = new ConfigNode(incompletetreeNode.name);
            completedTreeNode.AddData(incompletetreeNode);

            foreach (ConfigNode RDNode in completedTreeNode.GetNodes())
            {
                if ("RDNode" != RDNode.name)
                {
                    continue;
                }
                if (null == (techID = RDNode.GetValue("id")))
                {
                    Debug.Log("YT_TreeConverter.CompleteTree(): ERROR techID not found for RDNode. Node:\n" + RDNode.ToString());
                    continue;
                }

                //Get the Unlocks node if it exsits
                //If not create one
                if (RDNode.HasNode(YT_TreeConverterSettings.RDNode_UNLOCKSNODE_NAME))
                    unlocksNode = RDNode.GetNode(YT_TreeConverterSettings.RDNode_UNLOCKSNODE_NAME);
                else
                    unlocksNode = RDNode.AddNode(YT_TreeConverterSettings.RDNode_UNLOCKSNODE_NAME);

                //Get the list of parts where TechRequired equals this node 
                partsAttachedtoTech = GetPartsWithTechRequired(techID);


                //Remove all parts that are listed in Unlock nodes anywhere in the tree
                for (int i = partsAttachedtoTech.Count - 1; i >= 0; --i)
                {
                    if (partsInTree.Contains(partsAttachedtoTech[i]))
                    {
                        partsAttachedtoTech.RemoveAt(i);
                    }
                }

                //Add remaining parts to this Unlocks node
                foreach(string part in partsAttachedtoTech)
                {
                    unlocksNode.AddValue(YT_TreeConverterSettings.RDNode_UNLOCKSNODE_FIELD_PART, part);
                }
            }


            //Attempt to write file
            try
            {
                Debug.Log("YT_TreeConverter.ConvertTree(): writing tech tree data to: " + UrlDir.CreateApplicationPath(YT_TreeConverterSettings.Instance.TreeWriteDir + fileName));
                System.IO.File.WriteAllText(UrlDir.CreateApplicationPath(YT_TreeConverterSettings.Instance.TreeWriteDir + fileName), completedTreeNode.ToString());
            }
            catch (PathTooLongException)
            {
                Debug.Log("YT_TreeConverter.ConvertTree(): ERROR path too long exception");
                writeFailure = true;
            }
            catch(IsolatedStorageException)
            {
                Debug.Log("YT_TreeConverter.ConvertTree(): ERROR IsolatedStorageException");
                writeFailure = true;
            }

            //If writing to file failed, print to the debug log
            if(writeFailure)
            {
                Debug.Log("YT_TreeConverter.ConvertTree(): Printing converted tree to log:\n" + completedTreeNode.ToString());
            }

        }



        /************************************************************************\
         * YT_TreeConverter class                                               *
         * GetPartsContainedInTree function                                     *
         *                                                                      *
         * Returns a list of parts from all the Unlock nodes in techTree.       *
        \************************************************************************/
        private List<string> GetPartsContainedInTree(ConfigNode techTree)
        {
#if DEBUG
            Debug.Log("YT_TreeConverter.GetPartsContainedInTree()");
#endif
            ConfigNode unlocksNode = null;
            List<string> partList = new List<string>();

            foreach (ConfigNode RDNode in techTree.GetNodes())
            {
                if ("RDNode" != RDNode.name)
                {
                    continue;
                }

                if (RDNode.HasNode("Unlocks"))
                {
                    unlocksNode = RDNode.GetNode("Unlocks");
                    foreach (string value in unlocksNode.GetValues("part"))
                    {
                        //replace _ with . to match the internal format of the game
                        partList.Add(value.Replace("_", "."));
                    }
                }

            }
            return partList;
        }


        /************************************************************************\
         * YT_TreeConverter class                                               *
         * GetPartsWithTechRequired function                                    *
         *                                                                      *
         * Returns a list of all parts that have a TechRequired equal to techID.*
        \************************************************************************/
        private List<string> GetPartsWithTechRequired(string techID)
        {
#if DEBUG
            Debug.Log("YT_TreeConverter.GetPartsWithTechRequired()");
#endif
            List<string> partsList = new List<string>();

            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
                if(part.TechRequired == techID)
                    partsList.Add(part.name);
            }

            return partsList;
        }
    }
}
