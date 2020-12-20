﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SEE.Utils
{
    public abstract class ConfigIO
    {        
        /// <summary>
        /// The separator between a label and its value.
        /// </summary>
        protected const char LabelSeparator = ':';

        /// <summary>
        /// The separator between attribute specifications.
        /// </summary>
        protected const char AttributeSeparator = ';';

        /// <summary>
        /// The opening token for a composite attribute value.
        /// </summary>
        protected const char Open = '{';
        /// <summary>
        /// The closing token for a composite attribute value.
        /// </summary>
        protected const char Close = '}';
        /// <summary>
        /// The opening token for a list attribute value.
        /// </summary>
        protected const char OpenList = '[';
        /// <summary>
        /// The closing token for a list attribute value.
        /// </summary>
        protected const char CloseList = ']';

        /// <summary>
        /// Label for the red part of a color.
        /// </summary>
        protected const string RedLabel = "Red";
        /// <summary>
        /// Label for the green part of a color.
        /// </summary>
        protected const string GreenLabel = "Green";
        /// <summary>
        /// Label for the blue part of a color.
        /// </summary>
        protected const string BlueLabel = "Blue";
        /// <summary>
        /// Label for the alpha part (transparency) of a color.
        /// </summary>
        protected const string AlphaLabel = "Alpha";

        /// <summary>
        /// Looks up the <paramref name="value"/> in <paramref name="attributes"/> using the 
        /// key <paramref name="label"/>. If no such <paramref name="label"/> exists, false
        /// is returned and <paramref name="value"/> remains unchanged. Otherwise <paramref name="value"/>
        /// receives the looked up value.
        /// 
        /// Note: For types <typeparamref name="T"/> that are enums, use <see cref="RestoreEnum()"/>
        /// instead. For Color, use <see cref="RestoreColor()"/>.
        /// </summary>
        /// <typeparam name="T">the type of <paramref name="value"/></typeparam>
        /// <param name="attributes">where to look up the <paramref name="label"/></param>
        /// <param name="label">the label to look up</param>
        /// <param name="value">the value of the looked up <paramref name="label"/> if the <paramref name="label"/>
        /// exists</param>
        /// <returns>true if the <paramref name="label"/> was found</returns>
        public static bool Restore<T>(Dictionary<string, object> attributes, string label, ref T value)
        {
            if (attributes.TryGetValue(label, out object v))
            {
                try
                {
                    value = (T)v;
                    return true;
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException($"Types are not assignment compatible for attribute {label}. Expected type: {typeof(T)}. Actual type: {v.GetType()}");
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Looks up the <paramref name="value"/> in <paramref name="attributes"/> using the 
        /// key <paramref name="label"/>. If no such <paramref name="label"/> exists, false
        /// is returned and <paramref name="value"/> remains unchanged. Otherwise <paramref name="value"/>
        /// receives the looked up value. Note that only those parts of the color (red, green, blue,
        /// alpha) will be updated in <paramref name="value"/> that are actually found in <paramref name="attributes"/>;
        /// all others remain unchanged.
        /// 
        /// Note: This method is intended specifically for Color. For enums use <see cref="RestoreEnum()"/>
        /// and for all other types, use <see cref="Restore{T}()"/> instead. 
        /// </summary>
        /// <param name="attributes">where to look up the <paramref name="label"/></param>
        /// <param name="label">the label to look up</param>
        /// <param name="value">the value of the looked up <paramref name="label"/> if the <paramref name="label"/>
        /// exists</param>
        /// <returns>true if the <paramref name="label"/> was found</returns>
        internal static bool RestoreColor(Dictionary<string, object> attributes, string label, ref Color value)
        {
            if (attributes.TryGetValue(label, out object dictionary))
            {
                Dictionary<string, object> values = dictionary as Dictionary<string, object>;
                if (values == null)
                {
                    throw new InvalidCastException($"Types are not assignment compatible for attribute {label}. Expected type: Dictionary<string, float>. Actual type: {dictionary.GetType()}");
                }
                if (values.TryGetValue(RedLabel, out object red))
                {
                    value.r = (float)red;
                }
                if (values.TryGetValue(GreenLabel, out object green))
                {
                    value.g = (float)green;
                }
                if (values.TryGetValue(BlueLabel, out object blue))
                {
                    value.b = (float)blue;
                }
                if (values.TryGetValue(AlphaLabel, out object alpha))
                {
                    value.a = (float)alpha;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool Restore(Dictionary<string, object> attributes, string label, ref HashSet<string> value)
        {
            if (attributes.TryGetValue(label, out object storedValue))
            {
                List<object> values = storedValue as List<object>;
                if (values == null)
                {
                    throw new InvalidCastException($"Types are not assignment compatible for attribute {label}. Expected type: List<string>. Actual type: {storedValue.GetType()}");
                }
                else
                {
                    value = new HashSet<string>();
                    foreach (object item in values)
                    {
                        value.Add((string)item);
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        internal static bool Restore(Dictionary<string, object> attributes, string label, ref Dictionary<string, bool> value)
        {
            if (attributes.TryGetValue(label, out object list))
            {
                // The original dictionary was flattened as a list of pairs where each 
                // pair is represented as a list of two elements: the first one is the key
                // and the second one is the value of the original dictionary.
                List<object> values = list as List<object>;
                if (values == null)
                {
                    throw new InvalidCastException($"Types are not assignment compatible for attribute {label}. Expected type: Dictionary<string, bool>. Actual type: {list.GetType()}");
                }
                else
                {
                    value = new Dictionary<string, bool>();
                    foreach (var item in values)
                    {
                        List<object> pair = item as List<object>;
                        if (pair.Count == 2)
                        {
                            value[(string)pair[0]] = (bool)pair[1];
                        }
                        else
                        {
                            throw new Exception("Pair expected.");
                        }
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Looks up the enum <paramref name="value"/> in <paramref name="attributes"/> using the 
        /// key <paramref name="label"/>. If no such enum <paramref name="label"/> exists, false
        /// is returned and <paramref name="value"/> remains unchanged. Otherwise <paramref name="value"/>
        /// receives the looked up enum value.
        /// 
        /// Note: This method is intended for enums <typeparamref name="E"/>; for other types, use <see cref="Restore()"/>
        /// instead. For Color, use <see cref="RestoreColor()"/>.
        /// </summary>
        /// <typeparam name="E">the enum type of <paramref name="value"/></typeparam>
        /// <param name="attributes">where to look up the <paramref name="label"/></param>
        /// <param name="label">the label to look up</param>
        /// <param name="value">the value of the looked up <paramref name="label"/> if the <paramref name="label"/>
        /// exists</param>
        /// <returns>true if the <paramref name="label"/> was found</returns>
        public static bool RestoreEnum<E>(Dictionary<string, object> attributes, string label, ref E value) where E : struct, IConvertible
        {
            if (!typeof(E).IsEnum)
            {
                throw new ArgumentException("Generic type parameter E must be an enumerated type");
            }
            else
            {
                // enum values are stored as string
                string stringValue = "";
                if (Restore<string>(attributes, label, ref stringValue))
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        throw new Exception("Enum value must neither be null nor the empty string.");
                    }
                    else if (Enum.TryParse<E>(stringValue, out E enumValue))
                    {
                        value = enumValue;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }        
    }
}
