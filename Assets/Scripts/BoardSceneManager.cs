using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BoardSceneManager : MonoBehaviour
{
    public Image cardBackP1, cardBackP2;
    public Image cardFrontP1, cardFrontP2;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI winnerText;

    public Sprite[] spadeSprites;
    public Sprite[] heartSprites;
    public Sprite[] diaSprites;
    public Sprite[] clubSprites;

    private int p1Value, p2Value;
    private int p1CardId, p2CardId;

    void Start()
    {
        winnerText.text = "";
        winnerText.enabled = false;
        countdownText.enabled = true;

        cardFrontP1.gameObject.SetActive(false);
        cardFrontP2.gameObject.SetActive(false);

        LoadCards();
        StartCoroutine(CardCompareRoutine());
    }

    void LoadCards()
    {
        string emblemP1 = PlayerPrefs.GetString("P1_Emblem", "Spade");
        string emblemP2 = PlayerPrefs.GetString("P2_Emblem", "Heart");

        Sprite[] p1Set = GetSpriteSet(emblemP1);
        Sprite[] p2Set = GetSpriteSet(emblemP2);

        p1CardId = TurnManager.Instance.GetSelectedCard("P1") ?? -1;
        p2CardId = TurnManager.Instance.GetSelectedCard("P2") ?? -1;

        if (p1CardId >= 0 && p1CardId < p1Set.Length)
            cardFrontP1.sprite = p1Set[p1CardId];
        if (p2CardId >= 0 && p2CardId < p2Set.Length)
            cardFrontP2.sprite = p2Set[p2CardId];

        cardFrontP1.gameObject.SetActive(false);
        cardFrontP2.gameObject.SetActive(false);

        p1Value = GetCardValue(p1CardId);
        p2Value = GetCardValue(p2CardId);
    }

    Sprite[] GetSpriteSet(string emblem)
    {
        return emblem switch
        {
            "Spade" => spadeSprites,
            "Heart" => heartSprites,
            "Dia"   => diaSprites,
            "Club"  => clubSprites,
            _       => spadeSprites,
        };
    }

    int GetCardValue(int index)
    {
        if (index == 13) return 15; // Joker
        if (index == 0)  return 14; // Ace
        return index + 1;           // 2~K
    }

    IEnumerator CardCompareRoutine()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(0.4f);
        }
        countdownText.enabled = false;

        cardBackP1.gameObject.SetActive(false);
        cardBackP2.gameObject.SetActive(false);
        cardFrontP1.gameObject.SetActive(true);
        cardFrontP2.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.6f);

        string winner = null;
        if (p1Value > p2Value)
        {
            winner = "P1";
            winnerText.text = "PLAYER 1 WINS!";
        }
        else if (p1Value < p2Value)
        {
            winner = "P2";
            winnerText.text = "PLAYER 2 WINS!";
        }
        else
        {
            winnerText.text = "DRAW!";
            winnerText.enabled = true;
            yield return new WaitForSeconds(1.2f);

            TurnManager.Instance.SetPhase(TurnPhase.CardSelect);
            SceneManager.LoadScene("CardScene");
            yield break;
        }

        // 승자 결정 후 MovePhase 설정
        winnerText.enabled = true;
        TurnManager.Instance.SetPhase(TurnPhase.MovePhase);
        TurnManager.Instance.SetWinner(winner);

        yield return new WaitForSeconds(1.2f);

        SceneManager.LoadScene("Game");
    }
}
