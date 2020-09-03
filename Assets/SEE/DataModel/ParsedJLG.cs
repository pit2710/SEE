﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SEE.DataModel
{
    public class ParsedJLG
    {
        /// <summary>
        /// Contains a list with all paths to all java classes of the logged programm.
        /// </summary>
        [SerializeField]
        private List<string> filesOfProject;

        /// <summary>
        /// Lookuptable for the location of a JavaStatement. The number saved in the location of a JavaStatement equals the index in this table of its location.
        /// </summary>   
        [SerializeField]
        private List<string> locationLookupTable;

        /// <summary>
        /// Lookuptable for the names of all fields in the parsed javalog. The number used to identify a field is the index of its name in the lookuptable.
        /// </summary>
        [SerializeField]
        private List<string> fieldLookupTable;

        /// <summary>
        /// List containing all parsed JavaStatements.
        /// </summary>
        [SerializeField]
        private List<JavaStatement> allStatements;

        /// <summary>
        /// Constructs a new ParsedJLG.
        /// </summary>
        /// <param name="filesOfProject"></param>
        /// <param name="locationLookupTable"></param>
        /// <param name="fieldLookupTable"></param>
        /// <param name="allStatements"></param>
        public ParsedJLG(List<string> filesOfProject, List<string> locationLookupTable, List<string> fieldLookupTable, List<JavaStatement> allStatements)
        {
            this.filesOfProject = filesOfProject;
            this.locationLookupTable = locationLookupTable;
            this.fieldLookupTable = fieldLookupTable;
            this.allStatements = allStatements;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">index of the Java statement in List allStatements</param>
        /// <returns>The Location String from LocationLookupTable</returns>
        public string GetStatementLocationString(int i) {
            return locationLookupTable[int.Parse(allStatements[i].Location)];
        }

        private string LookupFieldLocation(string s) {
            int i = s.IndexOf('=');
            return fieldLookupTable[int.Parse(s.Substring(0, i))] + s.Substring(i); 
        }

        /// <summary>
        /// getter for the fields of a ParsedJLG
        /// </summary>
        public List<string> FilesOfProject { get => filesOfProject; }
        public List<string> LocationLookupTable { get => locationLookupTable; }
        public List<string> FieldLookupTable { get => fieldLookupTable; }
        public List<JavaStatement> AllStatements { get => allStatements; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statementCounter"></param>
        /// <returns></returns>
        internal string CreateStatementInfoString(int statementCounter)
        {
            JavaStatement js = allStatements[statementCounter];
            string info = "Line " + js.Line + Environment.NewLine;

            //Add local variables to info string
            if (js.LocalVariables.Count != 0)
            {
                info = info + "Local variables accessible at this line:";
                foreach (string s in js.LocalVariables)
                {
                    info = info + Environment.NewLine + s;
                }
            }

            //Add field changes to info string
            if (js.FieldChanges.Count != 0)
            {
                info = info + Environment.NewLine + "Field Changes at this line:";
                foreach (string s in js.FieldChanges)
                {
                    info = info + Environment.NewLine + LookupFieldLocation(s);
                }
            }

            //Add the return value of this statement (and its method) to the info string
            if (js.ReturnValue != null)
            {
                info = info + Environment.NewLine +"Returns: "+ js.ReturnValue;
            }
            return info;
        }
    }
}
