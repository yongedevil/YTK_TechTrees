using System;
using System.Collections.Generic;

using UnityEngine;
using KSP;

namespace YongeTechKerbal
{
    public class ConfigNodeParseHelper
    {
        static public bool getAsInt(ConfigNode node, string field, out int value, int defaultVal = 0)
        {
            bool success = false;
            value = defaultVal;

            if(node.HasValue(field))
            {
                try
                {
                    value = Convert.ToInt32(node.GetValue(field));
                    success = true;
                }
                catch (OverflowException)
                {
                    Debug.Log("ConfigNodeParseHelper.getAsInt(): ERROR OverflowException.  cost value is outside the range of Int32 type. " + node.GetValue("cost"));
                }
                catch (FormatException)
                {
                    Debug.Log("ConfigNodeParseHelper.getAsInt(): ERROR FormatException.  cost value is not in a recognized format. " + node.GetValue("cost"));
                }
            }

            return success;
        }

        static public bool getAsBool(ConfigNode node, string field, out bool value, bool defaultVal = false)
        {
            bool success = false;
            value = defaultVal;

            if(node.HasValue(field))
            {
                value = "TRUE" == node.GetValue(field).ToUpper();
                success = true;
            }

            return success;
        }
    }
}
