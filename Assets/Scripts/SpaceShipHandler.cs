using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceShipHandler : MonoBehaviour
{
    [SerializeField] private ScoreStatistics statistics;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            statistics.gameWon = true;
            SceneManager.LoadScene(2);
        }
    }
}