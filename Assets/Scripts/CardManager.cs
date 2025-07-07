using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum CardSelectPhase
{
    Player1,
    Player2,
    Done
}

public class CardSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public List<GameObject> slotObjects;   // 카드 슬롯들 (14개)
    public GameObject confirmButton;
    public Image selectedCardPreviewImage;

    [Header("Card Sprites")]
    public Sprite[] spadeSprites;
    public Sprite[] heartSprites;
    public Sprite[] diamondSprites;
    public Sprite[] clubSprites;

    private string emblemP1;
    private string emblemP2;
    private int selectedCardIndex = -1;
    private CardSelectPhase phase = CardSelectPhase.Player1;

    private const string boardSceneName = "BoardScene";

    private void Start()
    {
        emblemP1 = PlayerPrefs.GetString("P1_Emblem", "Spade");
        emblemP2 = PlayerPrefs.GetString("P2_Emblem", "Heart");

        TurnManager.Instance.CheckAndRefill("P1");
        TurnManager.Instance.CheckAndRefill("P2");

        EnterPhase(CardSelectPhase.Player1);

        confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmCard);
    }

    private void EnterPhase(CardSelectPhase next)
    {
        phase = next;
        selectedCardIndex = -1;
        ClearSelectedCardPreview();
        confirmButton.SetActive(false);
        GenerateHandUI();
    }

    private void GenerateHandUI()
    {
        string player = (phase == CardSelectPhase.Player1) ? "P1" : "P2";
        string emblem = (player == "P1") ? emblemP1 : emblemP2;
        var hand = TurnManager.Instance.GetHand(player);
        var sprites = GetSpritesByEmblem(emblem);

        for (int i = 0; i < slotObjects.Count; i++)
        {
            var slot = slotObjects[i];
            var img = slot.GetComponentInChildren<Image>();
            var btn = slot.GetComponent<Button>();

            if (i < hand.Count)
            {
                slot.SetActive(true);
                img.sprite = sprites[hand[i]];
                img.color = Color.white;
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                int idx = i;
                btn.onClick.AddListener(() => OnSelectCard(idx));
            }
            else
            {
                slot.SetActive(false);
                img.sprite = null;
                img.color = new Color(1, 1, 1, 0);
                btn.interactable = false;
            }
        }
    }

    private Sprite[] GetSpritesByEmblem(string emblem) => emblem switch
    {
        "Spade"   => spadeSprites,
        "Heart"   => heartSprites,
        "Diamond" => diamondSprites,
        "Club"    => clubSprites,
        _         => spadeSprites
    };

    private void OnSelectCard(int index)
    {
        var slot = slotObjects[index];
        var img = slot.GetComponentInChildren<Image>();
        if (!slot.activeSelf || img.sprite == null) return;

        selectedCardIndex = index;
        confirmButton.SetActive(true);

        for (int i = 0; i < slotObjects.Count; i++)
        {
            var ii = slotObjects[i].GetComponentInChildren<Image>();
            ii.color = (!slotObjects[i].activeSelf) ? new Color(1, 1, 1, 0) : (i == index ? Color.yellow : Color.white);
        }

        UpdateSelectedCardPreview();
    }

    private void UpdateSelectedCardPreview()
    {
        if (selectedCardPreviewImage == null || selectedCardIndex < 0) return;

        string player = (phase == CardSelectPhase.Player1) ? "P1" : "P2";
        int cardIdx = TurnManager.Instance.GetCardIndex(player, selectedCardIndex);
        var sprites = GetSpritesByEmblem(player == "P1" ? emblemP1 : emblemP2);

        selectedCardPreviewImage.sprite = sprites[cardIdx];
        selectedCardPreviewImage.color = Color.white;
    }

    private void ClearSelectedCardPreview()
    {
        if (selectedCardPreviewImage == null) return;
        selectedCardPreviewImage.sprite = null;
        selectedCardPreviewImage.color = new Color(1, 1, 1, 0);
    }

    /// <summary>
    /// 카드 확정 버튼
    /// - P1 선택 → P2 선택으로 이동
    /// - P2 선택 → 카드 비교 후 기물 이동권 보유자 결정 → BoardScene 이동
    /// </summary>
    private void OnConfirmCard()
    {
        if (selectedCardIndex < 0) return;

        string playerKey = (phase == CardSelectPhase.Player1) ? "P1" : "P2";
        int cardIdx = TurnManager.Instance.GetCardIndex(playerKey, selectedCardIndex);

        TurnManager.Instance.UseCard(playerKey, selectedCardIndex);
        TurnManager.Instance.SetSelectedCard(playerKey, cardIdx);

        if (phase == CardSelectPhase.Player1)
        {
            EnterPhase(CardSelectPhase.Player2);
        }
        else
        {
            TurnManager.Instance.ResolveCardComparison();
            SceneManager.LoadScene(boardSceneName);
        }
    }
}