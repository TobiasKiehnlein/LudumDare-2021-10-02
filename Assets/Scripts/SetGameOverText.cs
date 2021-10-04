using Enums;
using ScriptableObjects;
using TMPro;
using UnityEngine;

public class SetGameOverText : MonoBehaviour
{
    [SerializeField] private ScoreStatistics scoreStatistics;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.StopSound(Sfx.FootSteps);
        AudioManager.Instance.StartSound(Music.Menu);
        if (scoreStatistics.gameWon)
        {
            GetComponent<TMP_Text>().text = "UNGRAVITY\nYOU WIN!!!";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
