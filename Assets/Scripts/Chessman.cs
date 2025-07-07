using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    public GameObject controller;
    public GameObject movePlate;

    private int xBoard = -1;
    private int yBoard = -1;
    private string player;
    private string emblem;
    private bool hasMoved = false;

    public bool HasMoved() => hasMoved;
    public void SetMoved(bool value) => hasMoved = value;

    // 각 문양별 스프라이트
    public Sprite Spade_queen, Spade_knight, Spade_bishop, Spade_king, Spade_rook, Spade_pawn;
    public Sprite Heart_queen, Heart_knight, Heart_bishop, Heart_king, Heart_rook, Heart_pawn;
    public Sprite Club_queen, Club_knight, Club_bishop, Club_king, Club_rook, Club_pawn;
    public Sprite Dia_queen, Dia_knight, Dia_bishop, Dia_king, Dia_rook, Dia_pawn;

    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
    }

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetCoords();

        player = name.StartsWith("white") ? "white" : "black";
        emblem = PlayerPrefs.GetString(player == "white" ? "P1_Emblem" : "P2_Emblem", "Spade");

        string pieceType = name.Split('_')[1];
        Sprite selectedSprite = GetSprite(emblem, pieceType);
        if (selectedSprite != null)
            GetComponent<SpriteRenderer>().sprite = selectedSprite;
    }

    private Sprite GetSprite(string emblem, string pieceType)
    {
        return emblem switch
        {
            "Spade" => GetSpadeSprite(pieceType),
            "Heart" => GetHeartSprite(pieceType),
            "Club"  => GetClubSprite(pieceType),
            "Dia"   => GetDiaSprite(pieceType),
            _        => null
        };
    }

    private Sprite GetSpadeSprite(string piece) => piece switch
    {
        "queen"  => Spade_queen,
        "king"   => Spade_king,
        "rook"   => Spade_rook,
        "bishop" => Spade_bishop,
        "knight" => Spade_knight,
        "pawn"   => Spade_pawn,
        _         => null
    };

    private Sprite GetHeartSprite(string piece) => piece switch
    {
        "queen"  => Heart_queen,
        "king"   => Heart_king,
        "rook"   => Heart_rook,
        "bishop" => Heart_bishop,
        "knight" => Heart_knight,
        "pawn"   => Heart_pawn,
        _         => null
    };

    private Sprite GetClubSprite(string piece) => piece switch
    {
        "queen"  => Club_queen,
        "king"   => Club_king,
        "rook"   => Club_rook,
        "bishop" => Club_bishop,
        "knight" => Club_knight,
        "pawn"   => Club_pawn,
        _         => null
    };

    private Sprite GetDiaSprite(string piece) => piece switch
    {
        "queen"  => Dia_queen,
        "king"   => Dia_king,
        "rook"   => Dia_rook,
        "bishop" => Dia_bishop,
        "knight" => Dia_knight,
        "pawn"   => Dia_pawn,
        _         => null
    };

    public void SetCoords()
    {
        float x = xBoard * 0.66f - 2.3f;
        float y = yBoard * 0.66f - 2.3f;
        transform.position = new Vector3(x, y, -1.0f);
    }

    public int GetXBoard() => xBoard;
    public int GetYBoard() => yBoard;
    public void SetXBoard(int x) => xBoard = x;
    public void SetYBoard(int y) => yBoard = y;
    public string GetPlayer() => player;

    private void OnMouseUp()
    {
        if (!controller.GetComponent<Game>().IsGameOver() &&
            controller.GetComponent<Game>().GetCurrentPlayer() == player)
        {
            DestroyMovePlates();
            InitiateMovePlates();
        }
    }

    public void DestroyMovePlates()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MovePlate"))
        {
            Destroy(obj);
        }
    }

    public void InitiateMovePlates()
    {
        switch (name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0); LineMovePlate(0, 1);
                LineMovePlate(1, 1); LineMovePlate(-1, 0);
                LineMovePlate(0, -1); LineMovePlate(-1, -1);
                LineMovePlate(-1, 1); LineMovePlate(1, -1);
                break;
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1); LineMovePlate(1, -1);
                LineMovePlate(-1, 1); LineMovePlate(-1, -1);
                break;
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                TryCastling();
                break;
            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0); LineMovePlate(0, 1);
                LineMovePlate(-1, 0); LineMovePlate(0, -1);
                break;
            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
                break;
        }
    }

    public void TryCastling()
    {
        Game sc = controller.GetComponent<Game>();
        if (hasMoved) return;

        int y = (player == "white") ? 0 : 7;

        GameObject rightRook = sc.GetPosition(7, y);
        if (rightRook != null && rightRook.name == player + "_rook")
        {
            Chessman rookCm = rightRook.GetComponent<Chessman>();
            if (!rookCm.HasMoved() &&
                sc.GetPosition(5, y) == null && sc.GetPosition(6, y) == null)
            {
                MovePlateSpawn(6, y);
            }
        }

        GameObject leftRook = sc.GetPosition(0, y);
        if (leftRook != null && leftRook.name == player + "_rook")
        {
            Chessman rookCm = leftRook.GetComponent<Chessman>();
            if (!rookCm.HasMoved() &&
                sc.GetPosition(1, y) == null && sc.GetPosition(2, y) == null && sc.GetPosition(3, y) == null)
            {
                MovePlateSpawn(2, y);
            }
        }
    }

    public void LineMovePlate(int xInc, int yInc)
    {
        Game sc = controller.GetComponent<Game>();
        int x = xBoard + xInc;
        int y = yBoard + yInc;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xInc;
            y += yInc;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
                MovePlateSpawn(x, y);
            else if (cp.GetComponent<Chessman>().player != player)
                MovePlateAttackSpawn(x, y);
        }
    }

    public void PawnMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();

        int direction = (player == "white") ? 1 : -1;
        int startRow = (player == "white") ? 1 : 6;

        int currentX = GetXBoard();
        int currentY = GetYBoard();

        // 1칸 전진
        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);

            // 2칸 전진 가능 여부
            int twoStepY = y + direction;
            if (currentY == startRow && sc.PositionOnBoard(x, twoStepY) && sc.GetPosition(x, twoStepY) == null)
            {
                MovePlateSpawn(x, twoStepY);
            }
        }

        // 대각 공격
        for (int dx = -1; dx <= 1; dx += 2)
        {
            int targetX = currentX + dx;
            int targetY = currentY + direction;

            if (sc.PositionOnBoard(targetX, targetY))
            {
                GameObject cp = sc.GetPosition(targetX, targetY);
                if (cp != null && cp.GetComponent<Chessman>().player != player)
                {
                    MovePlateAttackSpawn(targetX, targetY);
                }
            }
        }

        // 앙파상 처리
        for (int dx = -1; dx <= 1; dx += 2)
        {
            int sideX = currentX + dx;
            int sideY = currentY;

            if (sc.PositionOnBoard(sideX, sideY))
            {
                GameObject sidePawn = sc.GetPosition(sideX, sideY);
                if (sidePawn != null)
                {
                    Chessman sideCm = sidePawn.GetComponent<Chessman>();
                    if (sideCm.name.Contains("pawn") && sideCm.GetPlayer() != player && sideCm.HasMoved() && sidePawn == sc.enPassantVictim)
                    {
                        int targetY = currentY + direction;
                        MovePlateAttackSpawn(sideX, targetY);
                    }
                }
            }
        }
    }

    public void MovePlateSpawn(int x, int y)
    {
        float px = x * 0.66f - 2.3f;
        float py = y * 0.66f - 2.3f;

        GameObject mp = Instantiate(movePlate, new Vector3(px, py, -3.0f), Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(x, y);
    }

    public void MovePlateAttackSpawn(int x, int y)
    {
        float px = x * 0.66f - 2.3f;
        float py = y * 0.66f - 2.3f;

        GameObject mp = Instantiate(movePlate, new Vector3(px, py, -3.0f), Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(x, y);
    }
}
