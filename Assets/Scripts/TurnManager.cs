using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TurnPhase
{
    CardSelect,
    CardCompare,
    MovePhase,
    EndTurn
}

[System.Serializable]
public class PieceState
{
    public string name;
    public int x, y;
    public bool isAlive;
    public bool hasMoved;
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.CardSelect;
    public string CurrentPlayer => isP1Turn ? "P1" : "P2";
    public string OpponentPlayer => isP1Turn ? "P2" : "P1";
    public bool IsP1Turn => isP1Turn;

    private bool isP1Turn = true;

    private List<int> p1Hand = new();
    private List<int> p2Hand = new();
    private List<int> p1Discard = new();
    private List<int> p2Discard = new();

    private int? p1SelectedCard = null;
    private int? p2SelectedCard = null;

    private string currentWinner = "P1";

    private List<PieceState> savedPieces = new List<PieceState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitHands();
    }

    private void InitHands()
    {
        p1Hand = Enumerable.Range(0, 14).ToList();
        p2Hand = Enumerable.Range(0, 14).ToList();
        p1Discard.Clear();
        p2Discard.Clear();
        p1SelectedCard = null;
        p2SelectedCard = null;
        CurrentPhase = TurnPhase.CardSelect;
        currentWinner = "P1";
    }

    public void CheckAndRefill(string player)
    {
        if (player == "P1" && p1Hand.Count == 0)
        {
            p1Hand = p1Discard.ToList();
            p1Discard.Clear();
        }
        else if (player == "P2" && p2Hand.Count == 0)
        {
            p2Hand = p2Discard.ToList();
            p2Discard.Clear();
        }
    }

    public List<int> GetHand(string player) =>
        player == "P1" ? p1Hand : p2Hand;

    public int GetCardIndex(string player, int handSlot) =>
        player == "P1" ? p1Hand[handSlot] : p2Hand[handSlot];

    public void UseCard(string player, int handSlot)
    {
        if (player == "P1")
        {
            p1Discard.Add(p1Hand[handSlot]);
            p1Hand.RemoveAt(handSlot);
        }
        else
        {
            p2Discard.Add(p2Hand[handSlot]);
            p2Hand.RemoveAt(handSlot);
        }
    }

    public void SetSelectedCard(string player, int cardIndex)
    {
        if (player == "P1") p1SelectedCard = cardIndex;
        else                p2SelectedCard = cardIndex;
    }

    public int? GetSelectedCard(string player)
    {
        return (player == "P1") ? p1SelectedCard : p2SelectedCard;
    }

    public bool BothPlayersSelectedCard() =>
        p1SelectedCard.HasValue && p2SelectedCard.HasValue;

    public void ResolveCardComparison()
    {
        if (!BothPlayersSelectedCard()) return;

        int v1 = ConvertCardValue(p1SelectedCard.Value);
        int v2 = ConvertCardValue(p2SelectedCard.Value);

        if (v1 > v2) currentWinner = "P1";
        else if (v2 > v1) currentWinner = "P2";
        else currentWinner = "Draw";

        CurrentPhase = TurnPhase.MovePhase;
    }

    private int ConvertCardValue(int idx)
    {
        if (idx == 13) return 15; // Joker
        if (idx == 0)  return 14; // A
        if (idx >= 10) return 11 + (idx - 10); // J/Q/K
        return idx + 1; // 2~10
    }

    public void AdvanceTurn()
    {
        isP1Turn = !isP1Turn;
        CurrentPhase = TurnPhase.CardSelect;
        SceneManager.LoadScene("CardScene");
    }

    public void SetPhase(TurnPhase next)
    {
        CurrentPhase = next;
    }

    public void SetWinner(string winner)
    {
        currentWinner = winner;
    }

    public string GetWinner() => currentWinner;

    public void ResetGame()
    {
        isP1Turn = true;
        InitHands();
        ClearBoardState();
    }

    public void SaveBoardState()
    {
        savedPieces.Clear();
        foreach (var obj in GameObject.FindGameObjectsWithTag("ChessPiece"))
        {
            var cm = obj.GetComponent<Chessman>();
            if (cm == null) continue;

            savedPieces.Add(new PieceState {
                name = cm.name,
                x = cm.GetXBoard(),
                y = cm.GetYBoard(),
                isAlive = true,
                hasMoved = cm.HasMoved()
            });
        }
    }

    public List<PieceState> GetSavedPieces() => savedPieces;
    public bool HasSavedBoard() => savedPieces.Count > 0;
    public void ClearBoardState() => savedPieces.Clear();
}
