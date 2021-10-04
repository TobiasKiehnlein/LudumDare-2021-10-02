using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using Utils;

public class SetScoresText : MonoBehaviour
{
    [SerializeField] private ScoreStatistics scoreStatistics;

    void Start()
    {
        GetComponent<TMP_Text>().text = $@"
Score:
  Game won: {scoreStatistics.gameWon} (+ {(int) scoreStatistics.scoreGameWon * scoreStatistics.gameWon.ToInt()}Pts)
  Jumps: {(int) Mathf.Round(scoreStatistics.countJumping)} (+ {(int) Mathf.Round(scoreStatistics.scoreJumping * scoreStatistics.countJumping)}Pts)
  Time idling: {(int) Mathf.Round(scoreStatistics.timeIdleOrFloating)} (+ {(int) Mathf.Round(scoreStatistics.timeIdleOrFloating * scoreStatistics.scoreIdleOrFloating)}Pts)
  Time running: {(int) Mathf.Round(scoreStatistics.timeRunning)}s (+ {(int) Mathf.Round(scoreStatistics.timeRunning * scoreStatistics.scoreRunning)}Pts)
  Time walking in air: {(int) Mathf.Round(scoreStatistics.timeWalkingInAir)} (+ {(int) Mathf.Round(scoreStatistics.timeWalkingInAir * scoreStatistics.scoreWalkingInAir)}Pts)
  Hard crashes: {(int) Mathf.Round(scoreStatistics.numberOfHardCrashes * .5f)} (- {(int) Mathf.Round(scoreStatistics.numberOfHardCrashes * .5f * scoreStatistics.scoreHardCrashes * -1)}Pts)
------------------------------------
TOTAL: {(int) Mathf.Round(scoreStatistics.Score)}
";
    }
}