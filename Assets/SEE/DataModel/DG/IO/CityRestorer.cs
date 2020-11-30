﻿
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using SEE.Game;
using System.Linq;
using System;
using SEE.DataModel.DG;

/// <summary>
/// This class is responsible for the export and the restore of either a SEECity or a SEECityEvolution object. 
/// The required data is stored in a .json-file.
/// The object stores values and selections such as specific metrics of a city or selected nodetypes as well which were chosen  
/// by the user before saving his or her profile.
/// </summary>
public class CityRestorer
{
   
    /// <summary>
    ///  Converts the <paramref name="city"/> in a json-formatted string and saves this string in a .json-file in the directory
    ///  <paramref name="dataPath"/>.
    /// </summary>
    /// <param name="dataPath"> The directory in which the json-file will be saved </param>
    /// <param name="city"> The city which will be stored in the json-file</param>
    public static void Persist(string dataPath, AbstractSEECity city)
    {
        string citySettingsJson = JsonUtility.ToJson(city, true);
        System.IO.File.WriteAllText(dataPath, citySettingsJson);
        UnityEngine.Debug.Log("Export sucessfully\n");
    }

    /// <summary>
    /// Loads a city from the given <paramref name="importFilename"/> and overwrites the <paramref name="city"/>
    /// </summary>
    /// <param name="importFilename"> The given json-file-path </param>
    /// <param name="city"> The city which is to be overwritten </param>
    public static void RestoreCity(string importFilename, AbstractSEECity city)
    {
        string jsonContent = File.ReadAllText(importFilename);
        if (!(VerifyCityType(city, jsonContent)))
        {
            return; 
        }
        string pathPrefixOfCity = GetPathPrefixFromJSON(importFilename);
        UnityEngine.Debug.Log("path ist " + pathPrefixOfCity);
        if (!(Directory.Exists(pathPrefixOfCity)))
        {
            return;
        }

        List<string> newNodeTypes = new List<string>(); 
        city.pathPrefix = pathPrefixOfCity;
        if (!(ReloadGraphByCityType(city))){
           return; 
        }
        newNodeTypes = city.SelectedNodeTypes.Keys.ToList();
         
        
        // We have to store the current enumeration of the nodetypes of the current version in order to compare 
        // it afterwards with the stored one in the method DifferentNodeTypes
        // As the user picks the directory via a directory picker/ the GUI , no specific error handling is needed at this point.

        JsonUtility.FromJsonOverwrite(jsonContent, city);
        Dictionary<string, bool> oldNodetypes = city.SelectedNodeTypes;
        DifferentNodeTypes(oldNodetypes, jsonContent, newNodeTypes, city);
    }

    /// <summary>
    /// Veryfies wether the types of the city and the json-file are matching.
    /// </summary>
    /// <param name="city">the city, which has to be overwritten</param>
    /// <param name="jsonContent">the content of the .json-file as a string</param>
    /// <returns>true, if the types are matching, otherwise false</returns>
    private static bool VerifyCityType(AbstractSEECity city, string jsonContent)
    {
        if ((jsonContent.Contains("isAnSEECityObject") && city is SEECity) || (jsonContent.Contains("isAnSEECityEvolutionObject") && city is SEECityEvolution))
        {
          UnityEngine.Debug.Log("Loaded successfully\n");
          return true;
        }
        else
        {
          UnityEngine.Debug.LogErrorFormat("The types of the scene and the loaded .json-file are not matching\n");
          return false;
        }
    }

    /// <summary>
    /// Analyazes if there is a difference between the stored nodetypes and the current nodetypes.
    /// </summary>
    /// <param name="jsonFile">the .json-file with the settings for the city</param>
    /// <param name="oldNodeTypes>a dictionary of the stored nodeTypes</param>
    /// <param name="newNodes> the city, which has to be overwritten</param>
    /// <returns> nothing, except a DebugLog to inform the user in case of any changes regarding the nodetypes.
    private static void DifferentNodeTypes(Dictionary<string, bool> oldNodeTypes, string jsonFile, List<string> newNodes, AbstractSEECity city)
    {
        List<string> oldNodes = oldNodeTypes.Keys.ToList();
        List<string> intermediateResults = new List<string>();
        List<string> finalResults = new List<string>();
        string differentNodeTypes = "";

        intermediateResults = oldNodes.Except(newNodes).ToList();
        finalResults = newNodes.Except(oldNodes).ToList();

        // In order to build the "real" difference between the stored parameters and the current one , we have to remove the duplicates.
        // and concatenate the result afterwards in the list finalResults.
        // The result will show if either there are new nodetypes in the latest version of the specific city or 
        // if there are nodetypes stored which are not in the current version of the Evolutioncity anymore.

        finalResults = finalResults.Concat(intermediateResults).ToList();

        // Depending on the actual amount of difference, three cases are possible:
        // First case = There are no differenct objects, thus we can break instantly
        // Second case = There is only one object, so it is possible to print out the result instantly and break.
        // Third case = There are more then two objects which have changed and we have to enumerate them and consequently cut the string for the 
        // printing-log to get a neat output for the user.

        // First case
        if (finalResults.Count == 0)
        {
            UnityEngine.Debug.Log("There have not been added any new nodytpes added since you saved your profile" + "\n");
            return;
        }
        // Second case
        else if (finalResults.Count == 1)
        {
            if (intermediateResults.Count == 1)
            {
                UnityEngine.Debug.Log("Since you had saved your profile the following Nodetype was been deleted in the meantime :\n" + finalResults.First());
                return;
            }
            if (newNodes.Count > oldNodes.Count && finalResults.Count == 1)
            {
                AddNodeTypes(city,finalResults);
                UnityEngine.Debug.Log("Since you had saved your profile the following Nodetype was added in the meantime :\n" + finalResults.First());
                return;
            }
            else
            {
                UnityEngine.Debug.Log("There have not been added any new nodytpes added since you saved your profile" + "\n");
                return;
            }
        }
        // Third case
        else
        {
            foreach (string str in finalResults)
            {
                AddNodeTypes(city, finalResults);
                differentNodeTypes += str + ",";
            }
            StringBuilder sb = new StringBuilder(differentNodeTypes);
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            UnityEngine.Debug.Log("Since you saved your profile and today the following Nodetypes have changed :\n" + sb.ToString());
        }
    }


    /// <summary>
    /// Returns the pathPrefixes  saved in the .json File
    /// </summary>
    /// <param name="jsonFile">the .json-file with the settings and the exact pathPrefix of the saved city</param>
    /// <param name="newNodes> the city, which has to be overwritten</param>
    /// <returns>  
    private static string GetPathPrefixFromJSON(string jsonFile)
    {
        string pathPrefix = "";
        StreamReader sr = new StreamReader(jsonFile);
        while (!(sr.ReadLine() == null))
        {
            string firstLine = sr.ReadLine();
            string secondLine = sr.ReadLine();
            

            if ((secondLine != null) && (secondLine.Contains("pathPrefix")))
            {
                UnityEngine.Debug.Log("found second line");
                pathPrefix = OutputAndFormatDirectoryString(secondLine);
            } 
                    
            if ((firstLine != null) && (firstLine.Contains("pathPrefix")))
            {
                pathPrefix = OutputAndFormatDirectoryString(firstLine);
                UnityEngine.Debug.Log("found second line");
            }         
        }
        return pathPrefix;
    }

    /// <summary>
    /// Adds new Nodetypes to the current version of the city- if not already stored.
    /// </summary>
    /// <param name="city">the current city</param>
    /// <param name="newNodeTypes"> A list of strings which are added to the Dictionary SelectedNodetypes - the types are per default selected, thus "true" </param>
    /// <returns>  
    private static void AddNodeTypes(AbstractSEECity city, List<string> newNodeTypes)
    {
        
        if(newNodeTypes != null)
        {
            foreach (string node in newNodeTypes)
            {
                if (!(city.SelectedNodeTypes.Keys.Contains(node)))
                {
                    city.SelectedNodeTypes.Add(node, true);
                }
            }
        } 
        
    }

    /// <summary>
    /// Reloads the graph - and thus the nodetypes - depending on the objecttype of the specific AbstractSEECity object.
    /// </summary>
    /// <param name="city">The current city object- either a SEECityEvolution or an SEECity object</param>
    /// <returns> "true" - in case the reloaded graph is not null, else "false".
    private static bool ReloadGraphByCityType(AbstractSEECity city)
    {
        if(city is SEECityEvolution)
        {
            SEECityEvolution evoCity = (SEECityEvolution)city;
            Graph graph = evoCity.LoadFirstGraph();
            if (graph != null) {
                evoCity.InspectSchema(graph);
                return true;
            }
            return false;
        }else
        {
            SEECity seeCity = (SEECity)city;
            if( (seeCity.LoadedGraph) != null)
            {
                return true; 
            }
            return false; 
        }
    }

    /// <summary>
    /// Cuts and formats a given string into a string which contains the directory of the stored .gxl File
    /// </summary>
    /// <param name="line">A line from the .json file which contains a directory</param>
    /// <returns> the name of specific directory
    private static string OutputAndFormatDirectoryString(string line)
    {
        UnityEngine.Debug.Log("line ist " + line);
        //Whether the string is null or not has already been tested before
        StringBuilder sb = new StringBuilder(line);
        sb.Remove(0, 19);
        sb.Remove((sb.Length - 2), 2);
        sb.Append("\\");
        return sb.ToString();
    }
}