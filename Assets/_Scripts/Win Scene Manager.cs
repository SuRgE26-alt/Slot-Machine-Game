using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayAgainButtonClicked()
    {
        // Load the main game scene
        SceneManager.LoadScene("GameScene");
    }

    public void OnQuitButtonClicked()
    {
        // Quit the application
        Application.Quit();
    }
}
