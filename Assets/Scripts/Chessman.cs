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
    private bool hasMoved = false;

    public bool HasMoved() => hasMoved;
    public void SetMoved(bool value) => hasMoved = value;

    // 각 문양별 Sprite 묶음
    [Header("♠ Spade")]
    public Sprite spade_queen, spade_knight, spade_bishop, spade_king, spade_rook, spade_pawn;
    [Header("♥ Heart")]
    public Sprite heart_queen, heart_knight, heart_bishop, heart_king, heart_rook, heart_pawn;
    [Header("♦ Diamond")]
    public Sprite diamond_queen, diamond_knight, diamond_bishop, diamond_king, diamond_rook, diamond_pawn;
    [Header("♣ Club")]
    public Sprite club_queen, club_knight, club_bishop, club_king, club_rook, club_pawn;

    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
    }

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetCoords();

        string emblem = (transform.position.y < 0) ?
            PlayerPrefs.GetString("P1_Emblem", "spade") :
            PlayerPrefs.GetString("P2_Emblem", "heart");

        string type = name.Split('_')[1];      // 예: pawn, rook
        player = name.Split('_')[0];           // white / black

        // 문양 + 타입 조합으로 Sprite 설정
        Sprite selectedSprite = GetSpriteForType(emblem, type);
        if (selectedSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = selectedSprite;
        }
    }

    private Sprite GetSpriteForType(string emblem, string type)
    {
        switch (emblem)
        {
            case "spade":
                return GetSpadeSprite(type);
            case "heart":
                return GetHeartSprite(type);
            case "diamond":
                return GetDiamondSprite(type);
            case "club":
                return GetClubSprite(type);
            default:
                return null;
        }
    }

    private Sprite GetSpadeSprite(string type)
    {
        return type switch
        {
            "queen" => spade_queen,
            "king" => spade_king,
            "rook" => spade_rook,
            "knight" => spade_knight,
            "bishop" => spade_bishop,
            "pawn" => spade_pawn,
            _ => null
        };
    }

    private Sprite GetHeartSprite(string type)
    {
        return type switch
        {
            "queen" => heart_queen,
            "king" => heart_king,
            "rook" => heart_rook,
            "knight" => heart_knight,
            "bishop" => heart_bishop,
            "pawn" => heart_pawn,
            _ => null
        };
    }

    private Sprite GetDiamondSprite(string type)
    {
        return type switch
        {
            "queen" => diamond_queen,
            "king" => diamond_king,
            "rook" => diamond_rook,
            "knight" => diamond_knight,
            "bishop" => diamond_bishop,
            "pawn" => diamond_pawn,
            _ => null
        };
    }

    private Sprite GetClubSprite(string type)
    {
        return type switch
        {
            "queen" => club_queen,
            "king" => club_king,
            "rook" => club_rook,
            "knight" => club_knight,
            "bishop" => club_bishop,
            "pawn" => club_pawn,
            _ => null
        };
    }

    // 아래는 좌표 및 움직임 관련 기존 코드 유지
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

    // 나머지 메서드들은 동일하게 유지 (생략 가능)

}
