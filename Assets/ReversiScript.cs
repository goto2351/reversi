using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversiScript : MonoBehaviour {
    //public bool[,] board = new bool[7, 7]; //盤面のデータ(1->白, -1->黒)
    //public bool turn = true; //手番(1->後手, -1->先手)

    /*FindFlipDisks:ひっくり返す石を返す関数
     * 引数:(board,turn,place) ,place:石を置こうとする座標
     * 返り値:ひっくり返す石を要素とするリスト ex)[[1,2],...]*/
     List<Vector2> FindFlipDisks(int[,] board,int turn,Vector2 place)
    {
        //探索方向のベクトル
        Vector2[] vectors = new Vector2[8];
        vectors[0] = new Vector2(1, 0);
        vectors[1] = new Vector2(1, 1);
        vectors[2] = new Vector2(0, 1);
        vectors[3] = new Vector2(-1, 1);
        vectors[4] = new Vector2(-1, 0);
        vectors[5] = new Vector2(-1, -1);
        vectors[6] = new Vector2(0, -1);
        vectors[7] = new Vector2(1, -1);
        //ひっくり返す石の配列
        List<Vector2> fliplist = new List<Vector2>();

        /*ひっくり返す石を探索
         * ①隣の石が相手の色か
         * ②壁に到達するまでに自分の石があるか(壁or自分の石に到着で打ち切り)memo:ここで石をリストに追加していく
         * これをv1~v8の8方向について行う*/
        foreach (Vector2 vec in vectors)
        {
            //①隣が相手の石か
            Vector2 tmpvec = place + vec;
            //2/5追加:今から探索するマスが盤面からはみ出ていないか->outofrange問題解決
            if (IsOutofBoard(tmpvec) == false)
            {
                if (board[(int)tmpvec.x, (int)tmpvec.y] * turn == -1)
                {
                    List<Vector2> tmplist = new List<Vector2>(); //fliplist追加前の石のリスト
                    tmplist.Add(new Vector2((int)tmpvec.x, (int)tmpvec.y));
                    int counter = 1; //placeにvecを足した回数
                    while (true)
                    {
                        //1つ隣に移動
                        tmpvec += vec;
                        //debugmemo:縁に石があるときここでoutofrangeexception
                        //相手の石か判定
                        if (IsOutofBoard(tmpvec) == false)
                        {
                            if (board[(int)tmpvec.x, (int)tmpvec.y] * turn == -1)
                            {
                                //移動した先も相手の石のとき
                                //fliplistへの追加候補にその石を追加
                                tmplist.Add(new Vector2((int)tmpvec.x, (int)tmpvec.y));
                                //今探索しているのが端かどうか判定
                                if ((int)tmpvec.x % 7 == 0 || (int)tmpvec.y % 7 == 0)
                                {
                                //端なら脱出
                                    break;
                                }
                                else
                                {
                                    //カウンターを1増やして次へ
                                    counter += 1;
                                }
                            }
                            else
                            {
                                //移動した先が自分の石のとき
                                //挟まれた相手の石をfliplistに追加する
                                if (board[(int)tmpvec.x, (int)tmpvec.y] * turn == 1)
                                {
                                    foreach (Vector2 flipdisk in tmplist)
                                    {
                                        fliplist.Add(flipdisk);
                                    }
                                }
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        //ひっくり返す石のリストを返す
        return fliplist;

    }

    /*IsPuttable:その場所に石が置けるかを判定
     * 引数:board,turn,place
     * 返り値:true->置ける, false->置けない*/
    public bool IsPuttable(int[,] board, int turn, Vector2 place)
    {
        //1枚でもひっくり返せるなら置ける
        bool puttable = false;
        List<Vector2> fliplist = FindFlipDisks(board, turn, place);
        if(fliplist.Count != 0)
        {
            //ひっくり返せる石の数が0でないときは置ける
            puttable = true;
        }

        return puttable;
    }

    /*flip(int[,]):石を置いてひっくり返す
     * 引数:board,turn,place
     * 返り値:石を置いてひっくり返した後の盤面データ*/
     public int[,] flip(int[,] board,int turn,Vector2 place)
    {
        //memo:置けない場所に石を置こうとしたときの処理はguiの方で
        //盤面の指定した位置に石を置く
        board[(int)place.x, (int)place.y] = turn;
        //ひっくり返す石を調べる
        List<Vector2> fliplist = FindFlipDisks(board, turn, place);
        //リストから順に要素を取り出してひっくり返す
        foreach(Vector2 disk in fliplist)
        {
            board[(int)disk.x, (int)disk.y] *= -1;
        }
        return board;

    }

    /*FindPuttablePlace:ある局面で石を置ける場所を探す
     * 引数:int[,]board(盤面),int turn(手番)
     * 返り値:石を置ける座標のリスト (List)*/
     public List<Vector2> FindPuttablePlace(int[,] board,int turn)
    {
        List<Vector2> puttablelist = new List<Vector2>();
        //全てのマスについて探索
        for(int i = 0; i <= 7; i++)
        {
            for(int j = 0; j <= 7; j++)
            {
                //既に石が置かれていないか
                if (board[i, j] != 1 && board[i, j] != -1)
                {
                    bool flg = IsPuttable(board, turn, new Vector2(i, j));
                    if (flg)
                    {
                        puttablelist.Add(new Vector2(i, j));
                    }
                }
            }
        }
        return puttablelist;

    }

    /*IsEnd(bool):終了判定
     * 引数:int[,]board(盤面),int turn(次の手番)
     * 返り値:true->ゲーム終了、false->まだ続く*/
    public bool IsEnd(int[,] board,int turn)
    {
        List<Vector2> puttablelist = FindPuttablePlace(board, turn);
        bool result = false;
        if(puttablelist.Count == 0)
        {
            //次の人がどこにも石を置けないとき
            result = true;
        }
        return result;
    }

    /*IsOutofBoard:探索するマスが盤面をはみ出てないか(FindFlipDisks用)
     * 引数:Vector2 vec
     * 返り値:true->はみ出てる,false->はみ出てない*/
     bool IsOutofBoard(Vector2 vec)
    {
        bool flg = true;
        if(vec.x >= 0 && vec.x <= 7)
        {
            if(vec.y >= 0 && vec.y <= 7)
            {
                flg = false;
            }
        }
        return flg;
    }
    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
