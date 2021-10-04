using ScriptableObjects;
using TMPro;
using UnityEngine;

public class SetGameOverText : MonoBehaviour
{
    [SerializeField] private ScoreStatistics scoreStatistics;
    // Start is called before the first frame update
    void Start()
    {
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
