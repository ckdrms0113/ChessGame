using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    private GameObject reference; 
    private int matrixX;
    private int matrixY;
    public bool attack;

    void Start()
    {
        if (attack)
            GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 1f, 1f);
    }

    void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game game = controller.GetComponent<Game>();

        Chessman cm = reference.GetComponent<Chessman>();
        string pieceName = cm.name;
        string player = cm.GetPlayer();
        int oldX = cm.GetXBoard();
        int oldY = cm.GetYBoard();
        bool didAttack = false;

        // 위치 선반영
        game.SetPositionEmpty(oldX, oldY);
        cm.SetXBoard(matrixX);
        cm.SetYBoard(matrixY);
        cm.SetCoords();
        cm.SetMoved(true);

        // 앙파상
        if (attack && game.GetPosition(matrixX, matrixY) == null && game.enPassantVictim != null)
        {
            Chessman victim = game.enPassantVictim.GetComponent<Chessman>();
            if (victim.GetXBoard() == matrixX && victim.GetYBoard() == oldY)
            {
                game.SetPositionEmpty(victim.GetXBoard(), victim.GetYBoard());
                Destroy(game.enPassantVictim);
                didAttack = true;
            }
        }
        else if (attack)
        {
            GameObject cp = game.GetPosition(matrixX, matrixY);
            if (cp != null)
            {
                string attackerPlayer = cm.name.StartsWith("white") ? "white" : "black";
                string attackerType = cm.name.Split('_')[1];
                string attackerEmblem = PlayerPrefs.GetString(attackerPlayer == "white" ? "P1_Emblem" : "P2_Emblem");

                if (cp.name == "white_king") game.Winner("black");
                if (cp.name == "black_king") game.Winner("white");

                Destroy(cp);
                didAttack = true;

                // 위치 재설정 + 보드 저장
                game.SetPosition(reference);
                TurnManager.Instance.SaveBoardState();

                // 컷씬 후 턴 전환
                game.ShowCutsceneFor(attackerEmblem, attackerType, () => {
                    game.NextTurn();
                });

                reference.GetComponent<Chessman>().DestroyMovePlates();
                return;
            }
        }

        game.SetPosition(reference);
        TurnManager.Instance.SaveBoardState();

        // 캐슬링
        if ((pieceName == "white_king" || pieceName == "black_king") && (matrixX == 2 || matrixX == 6))
        {
            int y = matrixY;
            int rookX = (matrixX == 2) ? 0 : 7;
            int newRookX = (matrixX == 2) ? 3 : 5;
            GameObject rook = game.GetPosition(rookX, y);
            if (rook != null && rook.name == player + "_rook")
            {
                game.SetPositionEmpty(rookX, y);
                Chessman rookCm = rook.GetComponent<Chessman>();
                rookCm.SetXBoard(newRookX);
                rookCm.SetYBoard(y);
                rookCm.SetCoords();
                rookCm.SetMoved(true);
                game.SetPosition(rook);
            }
        }

        // 앙파상 대상 설정
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

        // 프로모션 처리
        bool promote = (pieceName == "white_pawn" && matrixY == 7) ||
                       (pieceName == "black_pawn" && matrixY == 0);
        if (promote)
        {
            GameObject newQueen = Instantiate(reference, reference.transform.position, Quaternion.identity);
            Chessman newCm = newQueen.GetComponent<Chessman>();
            newCm.name = player + "_queen";
            newCm.SetXBoard(matrixX);
            newCm.SetYBoard(matrixY);
            newCm.Activate();

            game.SetPosition(newQueen);
            TurnManager.Instance.SaveBoardState();

            reference.GetComponent<Chessman>().DestroyMovePlates();
            Destroy(reference);

            game.ShowCutsceneFor(player, "queen", () => {
                game.NextTurn();
            });
            return;
        }

        game.NextTurn();
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    public void SetCoords(int x, int y) => (matrixX, matrixY) = (x, y);
    public void SetReference(GameObject obj) => reference = obj;

    public void DestroyMovePlates()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(obj);
    }
}
