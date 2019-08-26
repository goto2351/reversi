using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ReversiComScript : MonoBehaviour {

    //評価関数(とりあえず石の数をそのまま評価値にするやつ)
    int evaluation(int[,] boarddata,int turn)
    {
        int counter = 0; //石の数
        int[] board_1dim = boarddata.Cast<int>().ToArray();
        foreach(int disk in board_1dim)
        {
            if(disk == turn)
            {
                counter += 1;
            }
        }
        return counter;
    }

    //alpha-beta法による手の選択
    //memo:最善手を入れる配列を外に用意してalphabetaはintにする?
    List<Vector2> bestplaces = new List<Vector2>(); //深さごとの最善手を入れるリスト

    public int Alphabeta(int[,] boarddata, int tmpturn,int comturn, int depth, int alpha,int beta)
    {
        //今の局面で選択できる手を求める
        ReversiScript rs = Camera.main.GetComponent<ReversiScript>();
        List<Vector2> puttablelist = rs.FindPuttablePlace(boarddata, tmpturn);

        //終端ノード(打てる手がない)or充分な深さで打ち切り
        if(puttablelist.Count() == 0 || depth == 0)
        {
            return evaluation(boarddata, tmpturn);
        }

        if(tmpturn == comturn)
        {
            //ノードが自分のターンの時
            foreach(Vector2 place in puttablelist)
            {
                //置いた後の盤面を生成
                int[,] newboard = rs.flip(boarddata, tmpturn, place);
                //子ノードの評価値を求める
                int point = Alphabeta(newboard, tmpturn * (-1), comturn, depth - 1, alpha, beta);
                if (point > alpha)
                {
                    //評価値がalphaを上回るときは手とalphaを更新
                    Vector2 tmpvector = place;
                    bestplaces[depth - 1] = tmpvector;
                    alpha = point;
                }
                else
                {
                    //alphaカット
                    break;
                }
            }
            return alpha;
        }
        else
        {
            //ノードが相手のターンの時
            foreach(Vector2 place in puttablelist)
            {
                //石を置いた後の盤面を生成
                int[,] newboard = rs.flip(boarddata, tmpturn, place);
                //子ノードの評価値を求める
                int point = Alphabeta(newboard, tmpturn * (-1), comturn, depth - 1, alpha, beta);
                if(point < beta)
                {
                    //評価値がbetaを下回るときは手とbetaを更新
                    Vector2 tmpvector = place;
                    
                    bestplaces[depth - 1] = tmpvector;
                    beta = point;
                }
                else
                {
                    //betaカット
                    break;
                }
            }
            return beta;
        }
    }

    //最善手格納用リストを初期化する
    public void ClearList()
    {
        bestplaces.Clear();
        for (int i = 0; i < 3; i++)
        {
            bestplaces.Add(new Vector2(0, 0));
        }
    }

    //最善手を返す
    public Vector2 Findbestplace()
    {
        return bestplaces[2];
    }

    // Use this for initialization
    void Start () {
		for(int i = 0; i < 3; i++)
        {
            bestplaces.Add(new Vector2(0, 0));
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
