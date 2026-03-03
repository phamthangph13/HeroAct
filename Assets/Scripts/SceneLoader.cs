using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý chuyển scene. Gắn vào button hoặc gọi từ script khác.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadHub()
    {
        SceneManager.LoadScene("Hub");
    }

    public void LoadWorldZombie()
    {
        SceneManager.LoadScene("World_Zombie");
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Thoát game!");
        Application.Quit();
    }
}
