using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UXF;

namespace Oddball {

    public class OddBallExperiment : MonoBehaviour {

        public int numTrials;
        public int trialLength;
        public float duration;
        public float ISI;
        public float ISIAsync;


        int numBlocks;

        // generate the blocks and trials for the session.
        // the session is passed as an argument by the event call.
        public void Generate(Session session) {

            numBlocks = 4;

            RareStimulus[] rares = (RareStimulus[])Enum.GetValues(typeof(RareStimulus));
            DigitHand[] digHand = (DigitHand[])Enum.GetValues(typeof(DigitHand));

            Debug.Log(rares.Length);
            Debug.Log(digHand.Length);

            //Generate numBlocks new blocks each with numTrials trials
            Block[] blocks = new Block[numBlocks];
            for (int i = 0; i < numBlocks; i++) {
                blocks[i] = session.CreateBlock(numTrials);

                blocks[i].settings.SetValue("rareStim", rares[i / 2]);
                blocks[i].settings.SetValue("digitHand", digHand[i % 2]);

            }

            session.settings.SetValue("duration", duration);
            session.settings.SetValue("ISI", ISI);
            session.settings.SetValue("ISIAsync", ISIAsync);
            session.settings.SetValue("trialLength", trialLength);


            Debug.Log(session.currentBlockNum);
            Debug.Log(blocks[0].firstTrial.number);
            Debug.Log(session.FirstTrial.number);

            session.FirstTrial.Begin();





        }

        //GameObject[] buildTrial() {
        //    GameObject[] deviants;
        //    GameObject[] frequents;

        //    RareStimulus deviant = (RareStimulus)trial.settings.GetObject("rareStim");
        //    int trialLength = trial.settings.GetInt("trialLength");

        //    if (deviant == RareStimulus.Digit) {
        //        deviants = digits;
        //        frequents = letters;
        //    } else {
        //        deviants = letters;
        //        frequents = digits;
        //    }

        //    // Randomize deviant spot
        //    int deviantSpot = Random.Range(0, trialLength);

        //    GameObject[] stimuli = new GameObject[trialLength];

        //    for (int i = 0; i != trialLength; i++) {
        //        if (i == deviantSpot) {
        //            stimuli[i] = deviants[Random.Range(0, deviants.Length)];
        //        } else {
        //            stimuli[i] = frequents[Random.Range(0, frequents.Length)];
        //        }
        //    }

        //    return stimuli;
        //}
    }

}
