using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ScoreStatistics", menuName = "CustomGameSettings/ScoreStatistics", order = 0)]

    public class ScoreStatistics : ScriptableObject
    {
        [SerializeField] public float score;
        
        
        [SerializeField] public float timeWalking;
        [SerializeField] public float timeWalkingInAir;
        [SerializeField] public int countJumping;
        [SerializeField] public float timeRunning;
        [SerializeField] public float timeIdleOrFloating;
        [SerializeField] public int numberOfHardCrashes;
        [SerializeField] public float scoreWalkingInAir;
        [SerializeField] public float scoreWalking;
        [SerializeField] public float scoreJumping;
        [SerializeField] public float scoreRunning;
        [SerializeField] public float scoreIdleOrFloating;
        [SerializeField] public float scoreHardCrashes;
    }
}