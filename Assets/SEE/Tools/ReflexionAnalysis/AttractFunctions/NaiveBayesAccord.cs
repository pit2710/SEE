﻿using Accord;
using Accord.MachineLearning.Bayes;
using Accord.Math;
using Accord.Statistics.Filters;
using OpenCVForUnityExample;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RootMotion.FinalIK.Finger;
using static RTG.CameraFocus;
using NaiveBayesModel = Accord.MachineLearning.Bayes.NaiveBayes;

namespace Assets.SEE.Tools.ReflexionAnalysis.AttractFunctions
{
    public class NaiveBayesAccord : ITextClassifier
    {
        private Dictionary<string, Document> trainingData = new Dictionary<string, Document>();

        private string[] classIdxToStringMapper;
        private string[] wordIdxToStringMapper;

        private Dictionary<string, int> classStringToIdxMapper = new Dictionary<string, int>();
        private Dictionary<string, int> wordStringToIdxMapper = new Dictionary<string, int>();

        public int NumberClasses
        {
            get
            {
                return classIdxToStringMapper?.Length ?? 0;
            }
        }

        NaiveBayesModel classifier;

        private bool dirty;

        public void AddDocument(string clazz, Document document)
        {
            if (!trainingData.ContainsKey(clazz))
            {
                trainingData.Add(clazz, document);
            }
            else
            {
                trainingData[clazz].AddWords(document);
            }
            dirty = true;
        }

        public string ClassifyDocument(Document document)
        {
            if (dirty) UpdateModel();
            int[] input = CreateInputRepresentation(document);
            int decision = classifier.Decide(input);
            // handle artifical extra class
            if (decision == NumberClasses + 1) return null;
            return classIdxToStringMapper[decision];
        }

        public void DeleteDocument(string clazz, Document document)
        {
            if (clazz == null) throw new Exception("Invalid class given.");

            if (!trainingData.ContainsKey(clazz))
            {
                throw new Exception("Given class is unknown.");
            }

            trainingData[clazz].RemoveWords(document);
            dirty = true;
        }

        public double ProbabilityForClass(string clazz, Document document)
        {
            if (dirty) UpdateModel();

            // class is unknown
            if (!classStringToIdxMapper.ContainsKey(clazz)) return 0.0;

            int[] input = CreateInputRepresentation(document);
            int classIdx = classStringToIdxMapper[clazz];

            return classifier.Probability(input, classIdx);
        }

        private int[] CreateInputRepresentation(Document document)
        {
            int[] input = new int[wordIdxToStringMapper.Length];
            for (int wordIdx = 0; wordIdx < wordIdxToStringMapper.Length; wordIdx++)
            {
                input[wordIdx] = document.GetFrequency(wordIdxToStringMapper[wordIdx]);
            }

            return input;
        }

        private void UpdateModel()
        {
            Document allDocuments = new Document();
            trainingData.Values.ForEach(d => allDocuments.AddWords(d));

            int[,] inputs = new int[this.trainingData.Keys.Count, allDocuments.NumberWords];
            int[] outputs = new int[this.trainingData.Keys.Count];

            classStringToIdxMapper.Clear();
            wordStringToIdxMapper.Clear();
            classIdxToStringMapper = new string[this.trainingData.Keys.Count];
            wordIdxToStringMapper = new string[allDocuments.NumberWords];

            // setup word mapping
            int wordIdx = 0;
            foreach (string word in allDocuments.GetContainedWords())
            {
                wordIdxToStringMapper[wordIdx] = word;
                wordStringToIdxMapper.Add(word, wordIdx);
                wordIdx++;
            }

            // setup class mapping and set training input
            int classIdx = 0;
            int highestFrequency = int.MinValue;
            foreach (string currentClass in trainingData.Keys)
            {
                classIdxToStringMapper[classIdx] = currentClass;
                classStringToIdxMapper.Add(currentClass, classIdx);

                for (wordIdx = 0; wordIdx < allDocuments.NumberWords; wordIdx++)
                {
                    int frequency = trainingData[currentClass].GetFrequency(wordIdxToStringMapper[wordIdx]);
                    if (frequency > highestFrequency) highestFrequency = frequency;
                    inputs[classIdx, wordIdx] = frequency;
                }

                outputs[classIdx] = classIdx;
                classIdx++;
            }

            int[] symbols = new int[allDocuments.NumberWords];
            Array.Fill(symbols, highestFrequency + 1);

            // Add one artifical extra class because at least two classes are required
            classifier = new NaiveBayesModel(classes: NumberClasses + 1,
                                             symbols: symbols);

            NaiveBayesLearning naiveBayesLearning = new NaiveBayesLearning() { Model = classifier };
            naiveBayesLearning.Options.InnerOption.UseLaplaceRule = true;
            int[][] inputJagged = inputs.ToJagged();
            classifier = naiveBayesLearning.Learn(inputJagged, outputs);
        }

        public Dictionary<string, int> GetTrainingsData(string clazz)
        {
            if (!this.trainingData.ContainsKey(clazz)) return new Dictionary<string, int>();
            return trainingData[clazz].GetWordFrequencies();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return trainingData.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
