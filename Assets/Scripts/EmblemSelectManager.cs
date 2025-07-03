using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EmblemSelectManager : MonoBehaviour
{
    public TextMeshProUGUI infoText;

    private int currentPlayer = 1;
    private string player1Emblem = "";
    private string player2Emblem = "";
    private bool isSelectionLocked = false;

    public void OnEmblemSelected(string emblem)
    {
        // 🔙 'Back' 버튼이 OnEmblemSelected로 연결되었을 경우
        if (emblem == "Back")
        {
            HandleBack();
            return;
        }

        if (isSelectionLocked) return;

        if (currentPlayer == 1)
        {
            player1Emblem = emblem;
            currentPlayer = 2;
            infoText.text = "Player 2: Choose your emblem";
        }
        else if (currentPlayer == 2)
        {
            player2Emblem = emblem;

            isSelectionLocked = true; // 중복 방지

            PlayerPrefs.SetString("P1_Emblem", player1Emblem);
            PlayerPrefs.SetString("P2_Emblem", player2Emblem);

            SceneManager.LoadScene("Game");
        }
    }

    private void HandleBack()
    {
        if (currentPlayer == 1)
        {
            SceneManager.LoadScene("Main");
        }
        else
        {
            currentPlayer = 1;
            player2Emblem = "";
            infoText.text = "Player 1: Choose your emblem";
        }
    }
}
