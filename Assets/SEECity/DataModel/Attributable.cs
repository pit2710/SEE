﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SEE.DataModel
{
    // When you hit the play button in the editor, all the objects in the active scene are 
    // serialized and saved, so that unity can deserialize and return them to their original 
    // state when you stop the execution in the editor. Unity also creates copies of all 
    // objects in the scene, so the changes you do during play mode change the copies, not 
    // the original objects in the scene. During this copy process it deserializes the 
    // objects with the data it saved just before copying, so no visible change is done 
    // on the objects.
    // 
    // All the scripts which inherit from MonoBehaviour are serializable, but custom classes 
    // are not. To inform unity that you want your class to be serialized you have to use 
    // the [System.Serializable] attribute. 
    //
    // Also, unity only serializes the public members in your class, if you want your 
    // private members to be serialized too, you should inform unity with the [SerializeField] 
    // attribute. Note: Unity does not serialize static fields.
    //
    // More on Unity's serialization can be found here: 
    // https://docs.unity3d.com/Manual/script-Serialization.html

    /// <summary>
    /// Specifies and implements attributable objects with named toggle, int, float, and string attributes.
    /// </summary>
    [System.Serializable]
    public abstract class Attributable : UnityEngine.ScriptableObject, ICloneable
    {
        /// <summary>
        /// The set of toggle attributes. A toggle is set if it is contained in this
        /// list, otherwise it is unset. Conceptionally, toggleAttributes is a HashSet,
        /// but HashSets are not serialized by Unity. That is why we use List instead.
        /// </summary>
        [SerializeField]
        private List<string> toggleAttributes = new List<string>();

        public void SetToggle(string attributeName)
        {
            if (!toggleAttributes.Contains(attributeName))
            {
                toggleAttributes.Add(attributeName);
            }
        }

        public bool HasToggle(string attributeName)
        {
            return toggleAttributes.Contains(attributeName);
        }

        // Unity does not serializes Dictionaries. That is why we need to use StringStringDictionary
        // instead here. Note that we need to declare the attribute here as a SerializeField 
        // nevertheless.
        [UnityEngine.SerializeField]
        private StringStringDictionary stringAttributes = new StringStringDictionary();

        public void SetString(string attributeName, string value)
        {
            stringAttributes[attributeName] = value;
        }

        public bool TryGetString(string attributeName, out string value)
        {
            return stringAttributes.TryGetValue(attributeName, out value);
        }

        public string GetString(string attributeName)
        {
            if (stringAttributes.TryGetValue(attributeName, out string value))
            {
                return value;
            }
            else
            {
                throw new UnknownAttribute(attributeName);
            }
        }

        [SerializeField]
        private StringFloatDictionary floatAttributes = new StringFloatDictionary();

        public void SetFloat(string attributeName, float value)
        {
            floatAttributes[attributeName] = value;
        }

        public float GetFloat(string attributeName)
        {
            if (floatAttributes.TryGetValue(attributeName, out float value))
            {
                return value;
            }
            else
            {
                throw new UnknownAttribute(attributeName);
            }
        }

        public bool TryGetFloat(string attributeName, out float value)
        {
            return floatAttributes.TryGetValue(attributeName, out value);
        }

        [SerializeField]
        private StringIntDictionary intAttributes = new StringIntDictionary();

        public void SetInt(string attributeName, int value)
        {
            intAttributes[attributeName] = value;
        }

        public int GetInt(string attributeName)
        {
            if (intAttributes.TryGetValue(attributeName, out int value))
            {
                return value;
            }
            else
            {
                throw new UnknownAttribute(attributeName);
            }
        }

        public bool TryGetInt(string attributeName, out int value)
        {
            return intAttributes.TryGetValue(attributeName, out value);
        }

        public bool TryGetNumeric(string attributeName, out float value)
        {

            if (intAttributes.TryGetValue(attributeName, out int intValue))
            {
                value = intValue;
                return true;
            }
            else
            {
                return floatAttributes.TryGetValue(attributeName, out value);
            }
        }

        public override string ToString()
        {
            string result = "";

            foreach (var attr in toggleAttributes)
            {
                result += " \"" + attr + "\": true,\n";
            }

            foreach (var attr in stringAttributes)
            {
                result += " \"" + attr.Key + "\": \"" + attr.Value + "\",\n";
            }

            foreach (var attr in intAttributes)
            {
                result += " \"" + attr.Key + "\": " + attr.Value + ",\n";
            }

            foreach (var attr in floatAttributes)
            {
                result += " \"" + attr.Key + "\": " + attr.Value + ",\n";
            }
            return result;
        }

        /// <summary>
        /// Returns a deep clone of this attributable. Deep means that the list
        /// of attributes of this attributable are copied, too.
        /// </summary>
        /// <returns>deep clone</returns>
        public virtual object Clone()
        {
            var clone = (Attributable)this.MemberwiseClone();
            HandleCloned(clone);
            return clone;
        }

        /// <summary>
        /// Creates deep copies of attributes where necessary. Is called by
        /// Clone() once the copy is created. Must be extended by every 
        /// subclass that adds fields that should be cloned, too.
        /// </summary>
        /// <param name="clone">the clone receiving the copied attributes</param>
        protected virtual void HandleCloned(object clone)
        {
            Attributable target = (Attributable)clone;
            // deep copies of the attributes
            target.toggleAttributes = this.toggleAttributes.ToList();
            // must be newly created and assigned because MemberwiseClone() creates a shallow
            // copy in which those attributes will all refer to the dictionaries of the original 
            // attributable
            target.stringAttributes = new StringStringDictionary();
            target.floatAttributes = new StringFloatDictionary();
            target.intAttributes = new StringIntDictionary();
            // now we can copy the correspondings attribute values in all lists
            target.stringAttributes.CopyFrom(this.stringAttributes);
            target.floatAttributes.CopyFrom(this.floatAttributes);
            target.intAttributes.CopyFrom(this.intAttributes);
        }
    }
}