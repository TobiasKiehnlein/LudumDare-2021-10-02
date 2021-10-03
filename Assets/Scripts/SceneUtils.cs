using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneUtils : MonoBehaviour
{
    [SerializeField]
    private Animator crossFade;

    public void LoadNextScene()
    {
        var scene = SceneManager.GetActiveScene();
        var nextLevelBuildIndex = (scene.buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        StartCoroutine(LoadSceneCoroutine(nextLevelBuildIndex));
    }

    private IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        crossFade.SetTrigger("Start");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(sceneIndex);
        crossFade.SetTrigger("Stop");
    }

    public void LoadMenuScene()
    {
        StartCoroutine(LoadSceneCoroutine(0));
    }
    
    
}