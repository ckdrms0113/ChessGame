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
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string currentPlayer = "white";
    private bool gameOver = false;

    // 앙파상용 정보
    public GameObject lastMovedPiece = null;
    public Vector2Int? enPassantTarget = null;
    public GameObject enPassantVictim = null;

    private TextMeshProUGUI winnerText;
    private TextMeshProUGUI restartText;

    // 컷씬 연출 관련
    public CutsceneManager cutsceneManager;
    public List<Sprite> cutsceneSprites;
    private Dictionary<string, Sprite> cutsceneDict = new Dictionary<string, Sprite>();

    void Start()
    {
        // Winner / Restart 텍스트 초기화
        GameObject winnerObj = GameObject.FindGameObjectWithTag("WinnerText");
        if (winnerObj != null)
        {
            winnerText = winnerObj.GetComponent<TextMeshProUGUI>();
            winnerText.enabled = false;
        }

        GameObject restartObj = GameObject.FindGameObjectWithTag("RestartText");
        if (restartObj != null)
        {
            restartText = restartObj.GetComponent<TextMeshProUGUI>();
            restartText.enabled = false;
        }

        // 컷씬 스프라이트 딕셔너리 초기화 (이름 정규화 포함)
        foreach (Sprite sprite in cutsceneSprites)
        {
            string raw = sprite.name;
            string[] parts = raw.Split('_').Where(p => !int.TryParse(p, out _)).ToArray();
            if (parts.Length < 2) continue;

            string partA = parts[0].ToLower();
            string partB = parts[1].ToLower();

            string emblem = "";
            string piece = "";

            if (IsEmblem(partB) && IsPiece(partA))
            {
                emblem = Capitalize(partB);
                piece = MapPiece(partA);
            }
            else if (IsEmblem(partA) && IsPiece(partB))
            {
                emblem = Capitalize(partA);
                piece = MapPiece(partB);
            }
            else
            {
                Debug.LogWarning($"컷씬 무시됨: 예상 못한 이름 구조 → {raw}");
                continue;
            }

            string key = $"{emblem}_{piece}";
            if (!cutsceneDict.ContainsKey(key))
            {
                cutsceneDict[key] = sprite;
                Debug.Log($"컷씬 등록: {key} → {raw}");
            }
        }

        // 기물 생성 및 배치
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

        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
    }

    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool PositionOnBoard(int x, int y)
    {
        return x >= 0 && y >= 0 && x < positions.GetLength(0) && y < positions.GetLength(1);
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public void NextTurn()
    {
        lastMovedPiece = null;
        currentPlayer = (currentPlayer == "white") ? "black" : "white";
    }

    public void SetLastMoved(GameObject piece)
    {
        lastMovedPiece = piece;
    }

    public void Winner(string playerWinner)
    {
        gameOver = true;

        if (winnerText != null)
        {
            winnerText.enabled = true;
            winnerText.text = playerWinner + " is the winner!";
        }

        if (restartText != null)
        {
            restartText.enabled = true;
        }
    }

    public void ShowCutsceneFor(string emblem, string pieceType)
    {
        string key = emblem + "_" + pieceType;
        Debug.Log("컷씬 호출 요청: " + key);

        if (cutsceneDict.ContainsKey(key))
        {
            if (cutsceneManager != null)
            {
                Debug.Log("컷씬 재생: " + key);
                cutsceneManager.PlayCutscene(cutsceneDict[key]);
            }
            else
            {
                Debug.LogWarning("❗ cutsceneManager가 연결되지 않았습니다!");
            }
        }
        else
        {
            Debug.LogWarning("❗ 컷씬 딕셔너리에 키가 없습니다: " + key);
        }
    }

    void Update()
    {
        if (gameOver && Input.GetMouseButtonDown(0))
        {
            gameOver = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // 👇 헬퍼 함수들
    private bool IsPiece(string s)
    {
        s = s.ToLower();
        return new[] { "pawn", "rook", "bishop", "knight", "queen", "king", "look" }.Contains(s);
    }

    private bool IsEmblem(string s)
    {
        s = s.ToLower();
        return new[] { "spade", "heart", "dia", "club" }.Contains(s);
    }

    private string MapPiece(string s)
    {
        s = s.ToLower();
        return s == "look" ? "rook" : s;
    }

    private string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.ToLower();
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
