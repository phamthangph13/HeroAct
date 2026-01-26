using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "Game"; //Trang gameplay
    public GameObject settingsPanel;

    public void StartGame() //Bắt đầu game
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings() //Mở trang settings
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings() //Đóng trang settings
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void QuitGame() //Thoát khỏi game
    {
        Application.Quit();
    }
}