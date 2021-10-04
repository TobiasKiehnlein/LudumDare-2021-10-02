using UnityEngine;
using Utils;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ScoreStatistics", menuName = "CustomGameSettings/ScoreStatistics", order = 0)]
    public class ScoreStatistics : ScriptableObject
    {
        public float Score => timeRunning * scoreRunning +
                              timeWalking * scoreWalking +
                              countJumping * scoreJumping +
                              scoreHardCrashes * .5f * numberOfHardCrashes +
                              timeIdleOrFloating * scoreIdleOrFloating +
                              timeWalkingInAir * scoreWalkingInAir +
                              gameWon.ToInt() * scoreGameWon;

        public bool gameWon;
        public float scoreGameWon;

        public float timeWalking;
        public float timeWalkingInAir;
        public int countJumping;
        public float timeRunning;
        public float timeIdleOrFloating;
        public int numberOfHardCrashes;
        public float scoreWalkingInAir;
        public float scoreWalking;
        public float scoreJumping;
        public float scoreRunning;
        public float scoreIdleOrFloating;
        public float scoreHardCrashes;
    }
}