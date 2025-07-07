using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Game : MonoBehaviour
{
    public GameObject chesspiece;

    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack;
    private GameObject[] playerWhite;

    private bool gameOver = false;

    public GameObject lastMovedPiece = null;
    public Vector2Int? enPassantTarget = null;
    public GameObject enPassantVictim = null;

    private TextMeshProUGUI winnerText;
    private TextMeshProUGUI restartText;

    public CutsceneManager cutsceneManager;
    public List<Sprite> cutsceneSprites;
    private Dictionary<string, Sprite> cutsceneDict = new Dictionary<string, Sprite>();

    void Start()
    {
        var winnerObj = GameObject.FindGameObjectWithTag("WinnerText");
        if (winnerObj != null)
        {
            winnerText = winnerObj.GetComponent<TextMeshProUGUI>();
            winnerText.enabled = false;
        }
        var restartObj = GameObject.FindGameObjectWithTag("RestartText");
        if (restartObj != null)
        {
            restartText = restartObj.GetComponent<TextMeshProUGUI>();
            restartText.enabled = false;
        }

        BuildCutsceneDictionary();

        if (TurnManager.Instance.HasSavedBoard())
            RestoreBoard();
        else
            CreateInitialBoard();
    }

    void BuildCutsceneDictionary()
    {
        cutsceneDict.Clear();
        foreach (var sprite in cutsceneSprites)
        {
            string baseName = sprite.name.Replace("_0", "");

            if (!cutsceneDict.ContainsKey(baseName))
            {
                cutsceneDict.Add(baseName, sprite);
                Debug.Log($"[컷씬 등록] {baseName}");
            }
        }
    }

    void CreateInitialBoard()
    {
        playerWhite = new GameObject[] {
            Create("white_rook", 0, 0), Create("white_knight", 1, 0), Create("white_bishop", 2, 0),
            Create("white_queen", 3, 0), Create("white_king", 4, 0), Create("white_bishop", 5, 0),
            Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1),
            Create("white_pawn", 3, 1), Create("white_pawn", 4, 1), Create("white_pawn", 5, 1),
            Create("white_pawn", 6, 1), Create("white_pawn", 7, 1)
        };
        playerBlack = new GameObject[] {
            Create("black_rook", 0, 7), Create("black_knight", 1, 7), Create("black_bishop", 2, 7),
            Create("black_queen", 3, 7), Create("black_king", 4, 7), Create("black_bishop", 5, 7),
            Create("black_knight", 6, 7), Create("black_rook", 7, 7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6),
            Create("black_pawn", 3, 6), Create("black_pawn", 4, 6), Create("black_pawn", 5, 6),
            Create("black_pawn", 6, 6), Create("black_pawn", 7, 6)
        };
        foreach (var piece in playerWhite.Concat(playerBlack))
            SetPosition(piece);
    }

    void RestoreBoard()
    {
        var data = TurnManager.Instance.GetSavedPieces();
        foreach (var p in data)
        {
            if (!p.isAlive)
                continue;
            var obj = Create(p.name, p.x, p.y);
            var cm = obj.GetComponent<Chessman>();
            cm.SetMoved(p.hasMoved);
            SetPosition(obj);
        }
    }

    public GameObject Create(string name, int x, int y)
    {
        var obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        obj.tag = "ChessPiece";
        var cm = obj.GetComponent<Chessman>();
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        var cm = obj.GetComponent<Chessman>();
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y) => positions[x, y] = null;
    public GameObject GetPosition(int x, int y) => positions[x, y];
    public bool PositionOnBoard(int x, int y) => x >= 0 && y >= 0 && x < 8 && y < 8;
    public string GetCurrentPlayer() => TurnManager.Instance.GetWinner() == "P1" ? "white" : "black";
    public bool IsGameOver() => gameOver;

    public void NextTurn()
    {
        lastMovedPiece = null;
        TurnManager.Instance.AdvanceTurn();
    }

    public void SetLastMoved(GameObject piece) => lastMovedPiece = piece;

    public void Winner(string playerWinner)
    {
        gameOver = true;
        if (winnerText != null)
        {
            winnerText.enabled = true;
            winnerText.text = playerWinner + " is the winner!";
        }
        if (restartText != null)
            restartText.enabled = true;
    }

    public void ShowCutsceneFor(string emblem, string pieceType, System.Action callback = null)
    {
        string normalizedType = char.ToUpper(pieceType[0]) + pieceType.Substring(1).ToLower();
        string key = $"{emblem}_{normalizedType}";

        if (!cutsceneDict.TryGetValue(key, out Sprite sprite))
        {
            Debug.LogWarning($"[컷씬 스킵] key '{key}' 없음 ⇒ 턴 바로 넘김");
            callback?.Invoke();
            return;
        }

        Debug.Log($"[컷씬 호출] key={key}");
        cutsceneManager.PlayCutscene(sprite, emblem, callback);
    }

    void Update()
    {
        if (gameOver && Input.GetMouseButtonDown(0))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
