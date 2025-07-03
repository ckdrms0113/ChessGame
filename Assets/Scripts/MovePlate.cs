using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    private GameObject reference = null;

    private int matrixX;
    private int matrixY;

    public bool attack = false;

    public void Start()
    {
        if (attack)
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game game = controller.GetComponent<Game>();

        Chessman cm = reference.GetComponent<Chessman>();
        string pieceName = cm.name;
        string player = cm.GetPlayer();

        int oldX = cm.GetXBoard();
        int oldY = cm.GetYBoard();

        // ✅ 앙파상 처리
        if (attack && game.GetPosition(matrixX, matrixY) == null && game.enPassantVictim != null)
        {
            Chessman victim = game.enPassantVictim.GetComponent<Chessman>();
            if (victim.GetXBoard() == matrixX && victim.GetYBoard() == oldY)
            {
                game.SetPositionEmpty(victim.GetXBoard(), victim.GetYBoard());
                Destroy(game.enPassantVictim);
            }
        }
        else if (attack)
        {
            GameObject cp = game.GetPosition(matrixX, matrixY);
            if (cp != null)
            {
                if (cp.name == "white_king") game.Winner("black");
                if (cp.name == "black_king") game.Winner("white");

                Destroy(cp);
            }
        }

        // 기존 위치 비우기
        game.SetPositionEmpty(oldX, oldY);

        // 위치 갱신
        cm.SetXBoard(matrixX);
        cm.SetYBoard(matrixY);
        cm.SetCoords();
        cm.SetMoved(true);

        // ✅ 캐슬링 처리
        if ((pieceName == "white_king" || pieceName == "black_king") && (matrixX == 2 || matrixX == 6))
        {
            int y = matrixY;

            if (matrixX == 2) // 왼쪽 캐슬링
            {
                GameObject rook = game.GetPosition(0, y);
                if (rook != null && rook.name == player + "_rook")
                {
                    game.SetPositionEmpty(0, y);
                    Chessman rookCm = rook.GetComponent<Chessman>();
                    rookCm.SetXBoard(3);
                    rookCm.SetYBoard(y);
                    rookCm.SetCoords();
                    rookCm.SetMoved(true);
                    game.SetPosition(rook);
                }
            }
            else if (matrixX == 6) // 오른쪽 캐슬링
            {
                GameObject rook = game.GetPosition(7, y);
                if (rook != null && rook.name == player + "_rook")
                {
                    game.SetPositionEmpty(7, y);
                    Chessman rookCm = rook.GetComponent<Chessman>();
                    rookCm.SetXBoard(5);
                    rookCm.SetYBoard(y);
                    rookCm.SetCoords();
                    rookCm.SetMoved(true);
                    game.SetPosition(rook);
                }
            }
        }

        // ✅ 앙파상 대상 정보 저장 (2칸 전진한 폰일 경우)
        if (pieceName.Contains("pawn") && Mathf.Abs(matrixY - oldY) == 2)
        {
            game.enPassantTarget = new Vector2Int(matrixX, (matrixY + oldY) / 2);
            game.enPassantVictim = reference;
        }
        else
        {
            game.enPassantTarget = null;
            game.enPassantVictim = null;
        }

        // ✅ 프로모션 처리
        bool shouldPromote =
            (pieceName == "white_pawn" && matrixY == 7) ||
            (pieceName == "black_pawn" && matrixY == 0);

        if (shouldPromote)
        {
            GameObject newQueen = Instantiate(reference, reference.transform.position, Quaternion.identity);
            Chessman newCm = newQueen.GetComponent<Chessman>();

            newCm.name = player + "_queen";
            newCm.SetXBoard(matrixX);
            newCm.SetYBoard(matrixY);
            newCm.Activate();

            game.SetPosition(newQueen);

            reference.GetComponent<Chessman>().DestroyMovePlates();
            Destroy(reference);

            game.NextTurn();
            return;
        }

        // ✅ 마지막 이동 말 기록 (앙파상용)
        game.SetLastMoved(reference);

        // 이동 완료
        game.SetPosition(reference);
        game.NextTurn();
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}
