﻿using Accord.MachineLearning.Text.Stemmers;
using SEE.DataModel.DG;
using SEE.Scanner;
using SEE.Tools.ReflexionAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Assets.SEE.Tools.ReflexionAnalysis.AttractFunctions.Document;

namespace Assets.SEE.Tools.ReflexionAnalysis.AttractFunctions
{
    /// <summary>
    /// Base class for <see cref="AttractFunction"/> classes which operates with source
    /// code and documents. This class provides operations to read and process document 
    /// terms from a source code region of a given node.
    /// </summary>
    public abstract class LanguageAttract : AttractFunction
    {
        /// <summary>
        /// Documents cached by the attract function
        /// </summary>
        private Dictionary<string, Document> cachedDocuments = new Dictionary<string, Document>();

        /// <summary>
        /// TokenLanguage used by the node reader to read the source code region associated with a node
        /// </summary>
        public TokenLanguage TokenLanguage { get; set; }

        /// <summary>
        /// Node reader object used to read a source code region of a node
        /// </summary>
        private INodeReader nodeReader;

        /// <summary>
        /// Constructor for Attract functions deriving from LanguageAttract.
        /// 
        /// Reads the TokenLanguage from the given configuration file.
        /// 
        /// </summary>
        /// <param name="graph">Reflexion graph this attraction function is reading on.</param>
        /// <param name="candidateRecommendation">CandidateRecommendation object which uses and is used by the created attract function.</param>
        /// <param name="config">Configuration objects containing parameters to configure this attraction function</param>
        protected LanguageAttract(ReflexionGraph reflexionGraph,
                                  CandidateRecommendation candidateRecommendation,
                                  LanguageAttractConfig config) : base(reflexionGraph, candidateRecommendation, config)
        {
            TokenLanguage = TokenLanguage.GetTokenLanguageByType(config.TokenLanguageType);
            nodeReader = new NodeReader();
        }

        /// <summary>
        /// TODO: Cda is currently not working properly
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="node"></param>
        /// <param name="documents"></param>
        protected void CreateCdaTerms(Node cluster, Node node, Dictionary<string, IDocument> documents)
        {
            // UnityEngine.Debug.Log($"Try to create CDA Terms for {nodeChangedInMapping.ID} and cluster {cluster.ID}...");

            // TODO: Consider whole subtree here?
            List<Edge> edges = node.GetImplementationEdges();

            IDocument clusterDocument = new Document();

            foreach (Edge edge in edges)
            {
                bool edgeIsOutgoing = edge.Source.ID.Equals(node.ID);

                Node neighbor = edgeIsOutgoing ? edge.Target : edge.Source;

                Node neighborCluster = this.reflexionGraph.MapsTo(neighbor);

                if (neighborCluster == null)
                {
                    continue;
                } 
                else
                {
                    // create cda term
                    string term = edgeIsOutgoing ? $"{cluster.ID} -{edge.Type}- {neighborCluster.ID}"
                                                       : $"{neighborCluster.ID} -{edge.Type}- {cluster.ID}";

                    // UnityEngine.Debug.Log($"Created term {term}.");

                    // add for current changed cluster
                    clusterDocument.AddWord(term);

                    // add cda terms for self loops only to the cluster document
                    if (!neighborCluster.ID.Equals(cluster.ID))
                    {
                        if (!documents.TryGetValue(neighborCluster.ID, out IDocument neighborDocument))
                        {
                            neighborDocument = new Document();
                            documents[neighborCluster.ID] = neighborDocument;
                        }
                        neighborDocument.AddWord(term); 
                    }
                }
            }

            if (clusterDocument.WordCount > 0)
            {
                documents.Add(cluster.ID, clusterDocument); 
            }
        }

        /// <summary>
        /// This methods adds package paths and name spaces
        /// of a node to a given document by adding 
        /// all the indentifying names of the ascendants.
        /// 
        /// The property key used to retrieve the words 
        /// is "Source.Name".
        /// 
        /// </summary>
        protected void AddWordsOfAscendants(Node node, IDocument document)
        {
            foreach (Node ascendant in node.Ascendants())
            {
                // TODO: Use HashSet for types?
                string key = "Source.Name";

                if (ascendant.Type.Equals("Class"))
                {
                    if (ascendant.StringAttributes.ContainsKey(key))
                    {
                        document.AddWord(ascendant.GetString(key));
                    }
                }
                else if (ascendant.Type.Equals("Package"))
                {
                    if (ascendant.StringAttributes.ContainsKey(key))
                    {
                        document.AddWord(ascendant.GetString(key));
                    }
                }
                else if (ascendant.Type.Equals("File"))
                {
                    key = "Source.File";
                    if (ascendant.StringAttributes.ContainsKey(key))
                    {
                        document.AddWord(ascendant.GetString(key));
                    }
                }
            }
            return;
        }

        /// <summary>
        /// This method clears the document cache.
        /// </summary>
        public void ClearDocumentCache()
        {
            this.cachedDocuments.Clear();
        }

        /// <summary>
        /// This method returns a document created by merging the standard terms 
        /// of two documents while considering the merging type. 
        /// 
        /// The returned document is cached and should not be changed 
        /// during algorithms. // TODO: clone document?
        /// 
        /// </summary>
        /// <param name="node1">first given node</param>
        /// <param name="node2">second given node</param>
        /// <param name="mergingType">given merging type</param>
        /// <returns>returns the merged document</returns>
        protected Document GetMergedTerms(Node node1, Node node2, DocumentMergingType mergingType)
        {
            string mergedDocId = node1.ID + mergingType.ToString() + node2.ID;

            if (!cachedDocuments.ContainsKey(mergedDocId))
            {
                Document document1 = this.GetStandardDocument(node1);
                Document document2 = this.GetStandardDocument(node2);
                Document mergedDocument = Document.MergeDocuments(document1, document2, mergingType);
                cachedDocuments[mergedDocId] = mergedDocument;
            }
            return cachedDocuments[mergedDocId].Clone();
        }

        /// <summary>
        /// This method returns a document created by reading the standard terms of a given node.
        /// </summary>
        /// <param name="node">given node</param>
        /// <returns>Document object containing the standard terms</returns>
        protected Document GetStandardDocument(Node node)
        {
            if (cachedDocuments.ContainsKey(node.ID))
            {
                return cachedDocuments[node.ID].Clone();     
            } 
            else
            {
                Document doc = new Document();
                this.AddStandardTerms(node, doc);
                return doc;
            }
        }

        /// <summary>
        /// This method reads a source code region of a given node 
        /// and processed the string to generate standard terms.
        /// 
        /// The source code region is read as <see cref="SEEToken"/>
        /// objects and are then filtered. Comments, Identifier 
        /// and StringLiteral tokens are then splitted by whitespaces
        /// and code casing and then stemmed. The read tokens which have 
        /// more than 3 chars are then add to the document as terms. 
        /// Keywords of the current <see cref="TokenLanguage"/> are filtered.
        ///  
        /// </summary>
        /// <param name="node"></param>
        /// <param name="document"></param>
        protected void AddStandardTerms(Node node, Document document)
        {
            if(cachedDocuments.ContainsKey(node.ID))
            {
                document.AddWords(cachedDocuments[node.ID]);
                return;
            }
            
            string codeRegion = nodeReader.ReadRegion(node);

            IList<SEEToken> tokens = SEEToken.FromString(codeRegion, TokenLanguage);

            List<string> words = new List<string>();

            foreach (SEEToken token in tokens)
            {
                if ((token.TokenType == SEEToken.Type.Comment ||
                   token.TokenType == SEEToken.Type.Identifier ||
                   token.TokenType == SEEToken.Type.StringLiteral) 
                   && !TokenLanguage.Keywords.Contains(token.Text))
                {
                    if (token.TokenType == SEEToken.Type.Comment)
                    {
                        words = this.SplitWhiteSpaces(new string[] { token.Text });
                    } 
                    else
                    {
                        words.Add(token.Text);
                    }
                }
            }

            // Maybe treat comments separately in a more complex way(depends on language)
            words = this.SplitCasing(words);

            // TODO: Stemming of differences languages
            words = this.StemWords(words);

            words = words.Where(x => x.Length > 3).Select(x => x.ToLower()).ToList();

            Document cachedDocument = new Document();
            cachedDocument.AddWords(words);
            document.AddWords(cachedDocument);
            cachedDocuments.Add(node.ID, cachedDocument);
        }

        /// <summary>
        /// This methods splits up a word by applying a given split function.
        /// </summary>
        /// <param name="word">given word</param>
        /// <param name="splitFunction">given split function</param>
        /// <param name="keepCharAtSplit">whether the char at the split position should be kept or not.</param>
        /// <returns>A list containing the split words</returns>
        private List<string> Split(string word, Func<char[], int, bool> splitFunction, bool keepCharAtSplit)
        {
            List<string> words = new List<string>();
            char[] chars = word.ToCharArray();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < chars.Length; i++)
            {
                if (splitFunction(chars, i))
                {
                    if (keepCharAtSplit)
                    {
                        builder.Append(chars[i]);
                    }
                    words.Add(builder.ToString());
                    builder.Clear();
                }
                else
                {
                    builder.Append(chars[i]);
                }
            }

            words.Add(builder.ToString());

            return words;
        }

        /// <summary>
        /// Split function to split words which are written in camel case.
        /// 
        /// The function returns for a given char array and a given position
        /// if the word have to be splitted here. Returns true if the next 
        /// position is a upper char.
        /// 
        /// </summary>
        /// <param name="chars">given char array</param>
        /// <param name="i">a position within the given chars array</param>
        /// <returns>if a split should happen at <paramref name="i"/></returns>
        private bool SplitCamelCase(char[] chars, int i)
        {
            if (!char.IsUpper(chars[i]))
            {
                if (i + 1 >= chars.Length)
                {
                    return false;
                }
                if (char.IsUpper(chars[i + 1]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Split function to split words which are written in kebab case.
        /// 
        /// The function returns for a given char array and a given position
        /// if the word have to be splitted here. Returns true if the current 
        /// char is a hyphen.
        /// 
        /// </summary>
        /// <param name="chars">given char array</param>
        /// <param name="i">a position within the given chars array</param>
        /// <returns>if a split should happen at <paramref name="i"/></returns>
        private bool SplitKebabCase(char[] chars, int i)
        {
            return chars[i] == '-';
        }

        /// <summary>
        /// Split function to split words which are written in snake case.
        /// 
        /// The function returns for a given char array and a given position
        /// if the word have to be splitted here. Returns true if the current 
        /// char is a underscore.
        /// 
        /// </summary>
        /// <param name="chars">given char array</param>
        /// <param name="i">a position within the given chars array</param>
        /// <returns>if a split should happen at <paramref name="i"/></returns>
        private bool SplitSnakeCase(char[] chars, int i)
        {
            return chars[i] == '_';
        }

        /// <summary>
        /// The method steams a list of words with the porter stemming algorithm.
        /// </summary>
        /// <param name="words">given list of words</param>
        /// <returns>list containing the porter stemming algorithm</returns>
        private List<string> StemWords(List<string> words)
        {
            List<string> stemmedWords = new List<string>();
            EnglishStemmer stemmer = new EnglishStemmer();

            for (int i = 0; i < words.Count; i++)
            {
                stemmedWords.Add(stemmer.Stem(words[i]));
            }
            return stemmedWords;
        }

        /// <summary>
        /// This method splits a list of words by whitespaces within the words.
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public List<string> SplitWhiteSpaces(IEnumerable<string> words)
        {
            List<string> splittedWords = new List<string>();
            foreach (string word in words)
            {
                splittedWords.AddRange(word.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
            return splittedWords;
        }

        /// <summary>
        /// Applies splitting to a list of given words.
        /// 
        /// If a word contains a underscore the word is splitted by snake case.
        /// If a word contains a hyphen the is word splitted by kebab case.
        /// Otherwise splitting by camel case is applied.
        /// 
        /// </summary>
        /// <param name="words">a given list of words</param>
        /// <returns>a list containing the splitted words</returns>
        public List<string> SplitCasing(List<string> words)
        {
            List<string> splittedWords = new List<string>();

            for (int i = 0; i < words.Count; ++i)
            {
                string word = words[i];
                if (word.Contains('_'))
                {
                    splittedWords.AddRange(this.Split(word, SplitSnakeCase, true));
                }
                else if (word.Contains('-'))
                {
                    splittedWords.AddRange(this.Split(word, SplitKebabCase, false));
                }
                else
                {
                    splittedWords.AddRange(this.Split(word, SplitCamelCase, false));
                }
            }

            return splittedWords;
        }

        /// <summary>
        /// Setter function for an <see cref="INodeReader"/> object.
        /// </summary>
        /// <param name="nodeReader">given node reader object.</param>
        public void SetNodeReader(INodeReader nodeReader)
        {
            this.nodeReader = nodeReader;
        }
    }
}
