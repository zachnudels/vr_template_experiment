using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine.Serialization;

using UXF;


namespace ActionSimilarity
{

    public class Experiment : MonoBehaviour
    {

        public float ITI;
        public float actionTime;
        public float presentationTime;

        public float turnTime;

        public float delayTime;
        public float cueTime;
        public float reportTime;

        public float feedbackTime;
        
        public float endSessionTime;

        public int repeatsPerBlock;
        public int numberOfBlocks;
        
        public int numberOfShapes;
        public int numberOfColors;

        public int condition;

        public bool debug;
        public bool randomSimulationDebug;


        int sessionNumber;
        int actionPos;
        int jitters;

        private bool CheckSimulating()
        {
            GameObject simulateObj = GameObject.Find("Simulate");

            return simulateObj != null && simulateObj.activeInHierarchy;
            
        }

        public void Generate(Session session)
        {

            // Calculate internal variables
            sessionNumber = session.number - 1; // Because UXF doesn't allow 0

            // Set all public experimental variable settings
            session.settings.SetValue("ITI", ITI);
            session.settings.SetValue("actionTime", actionTime);
            session.settings.SetValue("presentationTime", presentationTime);
            session.settings.SetValue("turnTime", turnTime);
            session.settings.SetValue("delayTime", delayTime);
            session.settings.SetValue("cueTime", cueTime);
            session.settings.SetValue("reportTime", reportTime);
            session.settings.SetValue("feedbackTime", feedbackTime);
            session.settings.SetValue("debug", debug);
            session.settings.SetValue("randomSimulationDebug", randomSimulationDebug);
            session.settings.SetValue("simulating", CheckSimulating());
            session.settings.SetValue("triggerCode", 0);

            GenerateBlocks(session);

            Debug.Log($"Created {session.Trials.Count()} trials");

            session.settings.SetValue(
                "data",
                new DataProcessing(actionPos, session.blocks[0].trials.Count));
            session.FirstTrial.Begin();
        }


        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                            (t1, t2) => t1.Concat(new T[] { t2 }))
                .ToList();
        }
        
        void SaveCSV<T>(string path, List<List<T>> rows)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (List<T> row in rows)
                {
                    string line = string.Join(",", row);
                    writer.WriteLine(line);
                }
            }
        }

        void SaveCSV<T>(string path, List<T> rows)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (T word in rows)
                {
                    string line = string.Join(",", word.ToString());
                    writer.WriteLine(line);
                }
            }
        }


        void GenerateBlocks(Session session)
        {
            Block[] blocks = new Block[numberOfBlocks];

            if (numberOfColors != numberOfShapes)
            {
                throw new ArgumentException("Experiment numberOfShapes must equal numberOfColors");
            }

            int start = 0;
            
            // Debug.Log($"Shapes: {numberOfShapes}, repeats: {repeatsPerBlock}");
            // Generate random list of (list of) shapes per position [s1, s4, s3, s2]
            List<List<int>> shapePositions = GetNPermutations(numberOfShapes, repeatsPerBlock);

            // Generate random list of (list of) colors per position [c4, c2, c1, c3]
            List<List<int>> colorPositions = GetNPermutations(numberOfColors, repeatsPerBlock);

            int trialsPerBlock = shapePositions.Count;
            Debug.Log($"Trials per block: {trialsPerBlock}");
            
            // Generate random list of targets and distractors
            List<List<int>> colorCols = GetNPermutations(numberOfColors, repeatsPerBlock);
            Debug.Log($"ColorCols size: {colorCols.Count}");
            List<List<int>> shapeRows = GetNPermutations(numberOfShapes, repeatsPerBlock);
            
            // Generate turning directions
            List<TurnDirection> turnsFront = Enumerable.Repeat(TurnDirection.Left, trialsPerBlock / 4)
                .Concat(Enumerable.Repeat(TurnDirection.Right, trialsPerBlock / 4))
                .ToList();

            List<TurnDirection> turnsBack = Enumerable.Repeat(TurnDirection.Left, trialsPerBlock / 4)
                .Concat(Enumerable.Repeat(TurnDirection.Right, trialsPerBlock / 4))
                .ToList();
            
            
            
            // Debug.Log($"trials per block: {trialsPerBlock}");
            List<List<int>> fullList = new List<List<int>>();
            for (int blockNumber = 0; blockNumber < blocks.Length; blockNumber++)
            {
                blocks[blockNumber] = session.CreateBlock();
                
                shapePositions.Shuffle();
                colorPositions.Shuffle();
                colorCols.Shuffle();
                shapeRows.Shuffle();
                turnsFront.Shuffle();
                turnsBack.Shuffle();
                
                for (int i = 0; i < trialsPerBlock; i++) {
                    UXF.Trial newTrial = blocks[blockNumber].CreateTrial();
                    newTrial.settings.SetValue("shapePositions", shapePositions[i]);
                    newTrial.settings.SetValue("colorPositions", colorPositions[i]);
                    newTrial.settings.SetValue("shapeRows", colorCols[i]);
                    newTrial.settings.SetValue("colorCols", shapeRows[i]);
                    if (i % 2 == 0)
                    { // facing front
                        newTrial.settings.SetValue("turnDirection", turnsFront[(int)(i / 2)]);
                    }
                    else
                    {
                        newTrial.settings.SetValue("turnDirection", turnsBack[(int)(i / 2)]);
                    }
                    fullList.Add(colorPositions[i]);
                }
                // Update shape list range for next block
                // start += trialsPerBlock;
            }

            // SaveCSV("C:\\Users\\ZachPBL\\Desktop\\shapePoses.csv", turns);
        }
        
        public static List<List<int>> WithinBlockShuffling(int numberOfItems, int repeats)
        {
            IEnumerable<int> values = Enumerable.Range(0, numberOfItems);
            IEnumerable<List<int>> result = new List<List<int>> { new List<int>() };

            for (int i = 0; i < numberOfItems; i++)
            {
                result = result.SelectMany(seq => values, (seq, val) => new List<int>(seq) { val });
            }

            List<List<int>> product = result.ToList();
            
            List<List<int>> repeated = Enumerable
                .Repeat(product, repeats) 
                .SelectMany(x => x)      
                .ToList();  
            
            repeated.Shuffle();
            return repeated;
        }
        
        public static List<List<int>> GetNPermutations(int numberOfItems, int n)
        {
            
            var result = new List<List<int>>();
            Permute(Enumerable.Range(0, numberOfItems).ToList(), 0, result);

            int repeats = n;
            List<List<int>> repeated = Enumerable
                .Repeat(result, repeats) 
                .SelectMany(x => x)      
                .ToList();  
            return repeated;
        }

        private static void Permute<T>(List<T> list, int start, List<List<T>> result)
        {
            if (start == list.Count - 1)
            {
                result.Add(new List<T>(list));
                return;
            }

            for (int i = start; i < list.Count; i++)
            {
                (list[start], list[i]) = (list[i], list[start]);  // swap
                Permute(list, start + 1, result);
                (list[start], list[i]) = (list[i], list[start]);  // backtrack
            }
        }
        

        public void QuitApplication(Session session)
        {
            Application.Quit();
        }

    }
}
