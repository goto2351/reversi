using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ReversiGUIScript : MonoBehaviour {
    int[,] board = new int[8, 8]; //盤面のデータ(先手(黒)->-1,後手(白)->1)
    int turn = -1; //手番(先手->-1,後手->1)
    GameObject[,] Planes = new GameObject[8, 8]; //盤面のオブジェクトの格納用
    //GUI
    public Canvas canvas;
    public Text text;

    /*PutDisksfromMatrix(void):盤面データ(board)の通りに石を配置する
     * 引数 board(int[,]) 盤面データ
     * 返り値 なし*/
    void PutDisksfromMatrix(int[,] boarddata)
    {
        for(int i = 0; i <= 7; i++)
        {
            for(int j = 0; j <= 7; j++)
            {
                switch (boarddata[i, j])
                {
                    case -1:
                        //黒の石を置く
                        GameObject disk_black = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Vector3 v = new Vector3(0, 0.1f, 0);
                        disk_black.transform.position = GameObject.Find("p" + i + j).transform.position + v;
                        disk_black.transform.localScale = new Vector3(7, 0.1f, 7);
                        disk_black.GetComponent<Renderer>().material.color = Color.black;
                        //タグ"disk"をつける
                        disk_black.tag = "disk";
                        break;
                    case 1:
                        //白の石を置く
                        GameObject disk_white = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Vector3 v2 = new Vector3(0, 0.1f, 0);
                        disk_white.transform.position = GameObject.Find("p" + i + j).transform.position + v2;
                        disk_white.transform.localScale = new Vector3(7, 0.1f, 7);
                        disk_white.GetComponent<Renderer>().material.color = Color.white;
                        //タグ"disk"をつける
                        disk_white.tag = "disk";
                        break;
                }            
            }
        }
    }

    /*ClearBoard(void):石を全て削除する(更新時用)
     * 引数 なし
     * 返り値 なし　*/
     void ClearBoard()
    {
        GameObject[] disks = GameObject.FindGameObjectsWithTag("disk");
        foreach(GameObject obj in disks)
        {
            GameObject.Destroy(obj);
        }
    }

	// Use this for initialization
	void Start () {
        //GUIを非表示にしておく
        canvas.enabled = false;

        //手番を先手に設定
        turn = -1;

        //盤面を作る
        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 7; j++)
            {
                Planes[i, j] = GameObject.CreatePrimitive(PrimitiveType.Plane);
                Planes[i, j].transform.position = new Vector3(10 * j, 0, 10 * i);
                Planes[i, j].GetComponent<Renderer>().material.color = new Color(0.13f, 1.0f, 0);
                Planes[i, j].name = "p" + i + j;
                Planes[i, j].tag = "Board";
            } 
        }
        //仕切り
        GameObject[] lines = new GameObject[14];
        for (int i = 0; i <= 6; i++)
        {
            lines[i] = GameObject.CreatePrimitive(PrimitiveType.Plane);
            lines[i].transform.position = new Vector3((10 * i)+5, 0.1f, 35);
            lines[i].transform.localScale = new Vector3(0.05f, 0, 8);
            lines[i].GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        }
        for (int i = 0; i <= 6; i++)
        {
            lines[i] = GameObject.CreatePrimitive(PrimitiveType.Plane);
            lines[i].transform.position = new Vector3(35, 0.1f, (10 * i) + 5);
            lines[i].transform.localScale = new Vector3(8, 0, 0.05f);
            lines[i].GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        }

        //盤面の初期値を設定
        board[3, 3] = -1;
        board[3, 4] = 1;
        board[4, 3] = 1;
        board[4, 4] = -1;
        //石を置く
        PutDisksfromMatrix(board);
        ChangeColorofPuttablePlace(board, turn);
    }

    /*GameEnd(void):ゲームが終わった時の処理
     * 引数:board
     * 返り値:なし*/
     void GameEnd(int[,]boarddata)
    {
        //検索のため盤面データを1次元配列に変換
        int[] board_1dim = boarddata.Cast<int>().ToArray();
        //石の個数
        int black = 0;
        int white = 0;
        //石の数をカウント
        foreach(int disk in board_1dim)
        {
            switch (disk)
            {
                case  -1:
                    black += 1;
                    break;
                case 1:
                    white += 1;
                    break;
            }
        }
        //石の数を表示する
        canvas.enabled = true;
        text.text = "黒: " + black + "白: " + white;

    }
	
    /*ChangeColorofPuttablePlace(void):ある手番で石を置ける場所の盤面の色を変える
     * 引数:int[,]boarddata(盤面),int turn(手番)
     * 返り値:なし*/
     void ChangeColorofPuttablePlace(int[,]boarddata,int turn)
    {
        //石を置ける場所のリストを取得
        ReversiScript rs = Camera.main.GetComponent<ReversiScript>();
        List<Vector2> puttablelist = rs.FindPuttablePlace(boarddata, turn);
        
        //リストから要素を取り出し１箇所ずつ色を変える
        foreach(Vector2 place in puttablelist)
        {
            Planes[(int)place.x, (int)place.y].GetComponent<Renderer>().material.color = new Color(0, 1, 0.87f);
        }
    }

    /*ChangeColorofPlanestoGreen(void):盤面の色をもとに戻す
     * 引数:なし
     * 返り値:なし*/
     void ChangeColorofPlanestoGreen()
    {
        foreach(GameObject plane in Planes)
        {
            plane.GetComponent<Renderer>().material.color = new Color(0.13f, 1.0f, 0);
        }
    }

    /*PutDisk(void):ターン中処理(盤面の更新、再描画、次のターンに向けた処理)
     * 引数:int[,]boarddata,int turn(手番),int[] index(石を置く位置)
     * 返り値:なし*/
     void PutDisk(int[,] boarddata,int turn_tmp,int[] index)
    {
        ReversiScript rs = Camera.main.GetComponent<ReversiScript>();
        Debug.Log(turn_tmp);
        Debug.Log(index[0] + "," + index[1]);
        //そこに石が置けるか
        if (rs.IsPuttable(boarddata, turn_tmp, new Vector2(index[0], index[1])))
        {
            //盤面の色を戻す
            ChangeColorofPlanestoGreen();
            //石を置いて盤面データを更新
            board = rs.flip(boarddata, turn_tmp, new Vector2(index[0], index[1]));
            //盤面を再描画
            ClearBoard();
            PutDisksfromMatrix(board);
            //次の手番に向けた処理
            //終了判定
            turn = turn * (-1);
            if (rs.IsEnd(board, turn_tmp * (-1)))
            {
                //今石を置いてゲームが終わるとき
                GameEnd(board);
            }
            else
            {
                //ゲームが続くとき
                //置ける場所の色を変える
                ChangeColorofPuttablePlace(board, turn_tmp * (-1));
            }
        }
        
    }

	// Update is called once per frame
	void Update () {
        //クリックイベント
        if (Input.GetMouseButtonUp(0))
        {
            //先手(プレイヤー)のとき(暫定)
            if (turn == -1)
            {
                //クリックしたオブジェクトを検出
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);
                GameObject hitobj = hit.collider.gameObject;
                //クリックしたのが盤面なら手番に応じて処理
                if (hitobj.tag == "Board")
                {
                    //クリックしたマスのインデックスをオブジェクトの名前から取得
                    int[] index = { int.Parse(hitobj.name.Substring(1, 1)), int.Parse(hitobj.name.Substring(2, 1)) };
                    //ターン中の処理(石を置く->次のターンの準備)
                    PutDisk(board, turn, index);
                    turn = 1;

                    //comのターン(2/27追加)
                    ReversiComScript com = Camera.main.GetComponent<ReversiComScript>();
                    //最善手リストの初期化
                    com.ClearList();
                    //alpha-beta法
                    int[,] board_copy = board;
                    int tmp = com.Alphabeta(board_copy, 1, 1, 3, -1, 100);
                    Vector2 place = com.Findbestplace();
                    int[] place_array = { (int)place.x, (int)place.y };
                    Debug.Log(turn);
                    PutDisk(board, turn, place_array);
                    
                    turn = -1;

                }
            }
        }
	}
}
