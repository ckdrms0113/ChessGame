using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EmblemSelectManager : MonoBehaviour
{
    private string selectedEmblemP1 = "";
    private string selectedEmblemP2 = "";
    private int currentPlayer = 1;

    public TextMeshProUGUI infoText;
    public GameObject confirmButton;

    void Start()
    {
        if (infoText != null)
            infoText.text = "Player 1\nChoose your emblem";

        if (confirmButton != null)
            confirmButton.SetActive(false);
    }

    public void OnEmblemSelected(string emblem)
    {
        if (currentPlayer == 1)
        {
            selectedEmblemP1 = emblem;
            PlayerPrefs.SetString("P1_Emblem", emblem);
            PlayerPrefs.Save();  // 수정: 저장 보장
            currentPlayer = 2;

            if (infoText != null)
                infoText.text = "Player 2\nChoose your emblem";

            if (confirmButton != null)
                confirmButton.SetActive(false);
        }
        else if (currentPlayer == 2)
        {
            if (emblem == selectedEmblemP1)
            {
                Debug.LogWarning("같은 문양은 선택할 수 없습니다!");
                return;
            }

            selectedEmblemP2 = emblem;
            PlayerPrefs.SetString("P2_Emblem", emblem);
            PlayerPrefs.Save();  // 수정: 저장 보장

            if (confirmButton != null)
                confirmButton.SetActive(true);
        }
    }

    public void OnConfirmSelection()
    {
        SceneManager.LoadScene("CardScene");
    }

    public void OnBackButton()
    {
        if (currentPlayer == 2)
        {
            currentPlayer = 1;
            selectedEmblemP2 = "";
            PlayerPrefs.DeleteKey("P2_Emblem");

            if (infoText != null)
                infoText.text = "Player 1\nChoose your emblem";

            if (confirmButton != null)
                confirmButton.SetActive(false);
        }
        else
        {
            SceneManager.LoadScene("Main");
        }
    }
}
