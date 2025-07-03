using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnGameStart()
    {
        SceneManager.LoadScene("EmblemScene"); // 씬 이름 정확히 입력
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
