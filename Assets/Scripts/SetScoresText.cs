using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using TMPro;
using UnityEngine;

public class SetScoresText : MonoBehaviour
{
    [SerializeField] private ScoreStatistics scoreStatistics;

    void Start()
    {
        GetComponent<TMP_Text>().text = $@"
Score:
  Game won: {scoreStatistics.gameWon} (+ {scoreStatistics.scoreGameWon}Pts)
  Jumps: {scoreStatistics.countJumping} (+ {scoreStatistics.scoreJumping * scoreStatistics.countJumping}Pts)
  Time idling: {scoreStatistics.timeIdleOrFloating} (+ {scoreStatistics.timeIdleOrFloating * scoreStatistics.scoreIdleOrFloating}Pts)
  Time running: {scoreStatistics.timeRunning}s (+ {scoreStatistics.timeRunning * scoreStatistics.scoreRunning}Pts)
  Time walking in air: {scoreStatistics.timeWalkingInAir} (+ {scoreStatistics.timeWalkingInAir * scoreStatistics.scoreWalkingInAir}Pts)
  Hard crashes: {scoreStatistics.numberOfHardCrashes * .5f} (- {scoreStatistics.numberOfHardCrashes * .5f * scoreStatistics.scoreHardCrashes * -1}Pts)
------------------------------------
TOTAL: {scoreStatistics.Score}
";
    }
}