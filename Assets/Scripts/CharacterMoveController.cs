﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterMoveController : MonoBehaviour {


    /// <summary>
    /// 各種参照
    /// </summary>
    [SerializeField] private MapController map;
    [SerializeField] private MovableScript movableScript;
    [SerializeField] private AttackController attackController;
    [SerializeField] private CirsorController cirsorController;
    [SerializeField] private UnitController unit;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameController gm;
    [SerializeField] private WeaponData weapondata;
    [SerializeField] private GameObject cirsor;
    [SerializeField] private GameObject[] window;
    [SerializeField] private GameObject eventSystem;

    /// <summary>
    /// 各種ブーリアン
    /// </summary>
    public bool isCirsor = false;   // カーソルがキャラ上にあるか
    public bool isMove = false;
    public bool isMenu = false;
    public bool backMenu = false;
    public int stateCount = -1; // ステート番号
    private int color = 0;

    /// <summary>
    /// ユニット情報
    /// </summary>
    public int playerID = 0;
    public int enemyNumber = 0;
    GameObject unitObj;
    GameObject enemyObj;

    /// <summary>
    /// ユニット情報
    /// </summary>
    public int x = 0;
    public int y = 0;
    public int cirsorX = 0;
    public int cirsorY = 0;

    public Vector3 pos;
    public Vector2 savePos;
    private int div = 10;
    private float divF = 10F;

    public enum PlayerState
    {
        START,
        MOVE,
        MENU,
        SELECT_ATTACK,
        ATTACK,
    }

    public PlayerState playerState;

    void Start()
    {
        eventSystem.SetActive(true);
        for(int i = 0;i < window.Length; i++)
        {
            window[i].SetActive(false);
        }
    }

    void Update()
    {
        /// <summary>
        /// Playerターンなら
        /// </summary>
        if (gm.playerTurn && unit.selectUnit != 99)
        {
            if(stateCount <= 1)
            {
                MoveSearch();
            }
            

            if (!unit.playerController[playerID].isAct)
            {
                switch (playerState)
                {
                    case PlayerState.START:
                        // ステート番号
                        stateCount = 0;

                        /// <summary>
                        /// カーソル判定
                        /// </summary>
                        if (x == cirsorX && y == cirsorY)
                        {
                            isCirsor = true;
                            color = 1;

                        }
                        else
                        {
                            playerID = 99;
                            movableScript.Initialize();
                            isMove = false;
                            isCirsor = false;
                            color = 0;
                        }

                        // キャラ上でボタンを押したらMOVEに移る
                        if (Input.GetButtonDown("Submit") && isCirsor && stateCount == 0)
                        {
                            isMove = true;
                            playerState = PlayerState.MOVE;     // ステート移動
                        }
                        
                        map.DrawMap();
                        break;

                    case PlayerState.MOVE:
                        // ステート番号
                        stateCount = 1;

                        /// <summary>
                        /// 探索
                        /// </summary>
                        if (map.block[cirsorX, cirsorY].movable)
                        {
                            /// <summary>
                            /// 移動処理
                            /// </summary>
                            if (Input.GetButtonDown("Submit") && isMove && !backMenu)
                            {
                                savePos = unit.playerController[playerID].playerPos;    // posを保存しておく
                                                                                        // カーソルの位置にキャラを移動
                                unit.playerController[playerID].playerPos = cirsorController.cirsorPos;
                                isMove = false;
                                movableScript.Initialize();
                                MenuFunc(0);
                                playerState = PlayerState.MENU;         // ステート移動
                            }
                        }
                        // 色塗り
                        map.DrawMap();
                        break;
                    case PlayerState.MENU:
                        movableScript.Initialize();
                        AttackRange();
                        // ステート番号
                        stateCount = 2;
                        // メニュー関数を実行
                        break;
                    case PlayerState.SELECT_ATTACK:
                        movableScript.Initialize();
                        MenuFunc(1);
                        break;
                    case PlayerState.ATTACK:
                        movableScript.Initialize();
                        AttackRange();
                        Debug.Log("AttackFase");
                        if (map.block[cirsorX, cirsorY].enemyOn && !backMenu)
                        {
                            Debug.Log("if");
                            EnemySearch();
                            attackController.PlayerAttack(unit.selectUnit);
                            attackController.EnemyAttack(unit.selectEnemy);
                            attackController.BattleParam();
                            if (Input.GetButtonDown("Submit"))
                            {
                                Debug.Log("enter");
                                attackController.Battle();
                                EndAct();
                                movableScript.Initialize();
                                MoveState();
                            }
                        }
                        break;
                }

                /// <summary>
                /// キャンセル
                /// </summary>
                if (Input.GetButtonDown("Cancel"))
                {
                    Cancel();
                }
                map.DrawMap();
            }
        }
        
    }

    /// <summary>
    /// 移動範囲探索
    /// </summary>
    public void MoveSearch()
    {
        movableScript.Initialize();

        /// <summary>
        /// 必要な情報の取得
        /// </summary>
        playerID = unit.selectUnit;                                       // 選択ユニットの番号
        unitObj = unit.playerObj[playerID];                               // ユニットの取得
        
        x = Mathf.RoundToInt(unit.playerController[playerID].playerPos.x);       // ユニットのX座標
        y = Mathf.RoundToInt(unit.playerController[playerID].playerPos.y);       // ユニットのZ座標
        cirsorX = Mathf.FloorToInt(cirsorController.cirsorPos.x);  // カーソルのX座標
        cirsorY = Mathf.FloorToInt(cirsorController.cirsorPos.y);  // カーソルのZ座標

        unit.UnitMovable();
        movableScript.moveSearch(x, y, unit.playerController[playerID].moveCost, stateCount);
    }

    /// <summary>
    /// state移動
    /// </summary>
    public void MoveState()
    {
        if (stateCount == 0)
        {
            movableScript.Initialize();
            playerState = PlayerState.START;
        }
        else if (stateCount == 1)
        {
            playerState = PlayerState.MOVE;
        }
        else if (stateCount == 3)
        {
            playerState = PlayerState.SELECT_ATTACK;
        }
        else if(stateCount == 4)
        {
            playerState = PlayerState.ATTACK;
        }
    }

    /// <summary>
    /// メニュー処理
    /// </summary>
    public void MenuFunc(int i)
    {
        if (!isMenu)
        {
            isMenu = true;
            eventSystem.SetActive(false);
            window[i].SetActive(true);
            if(i == 1)
            {
                GameObject.Find("Content").GetComponent<WeaponScrollController>().NodeInstance();
            }
        } 
    }

    public void MenuEnd()
    {
        isMenu = false;
        eventSystem.SetActive(true);
        for(int i = 0;i < window.Length; i++)
        {
            window[i].SetActive(false);
        }
        //stateCount = 0;
    }

    public void EndAct()
    {
        isCirsor = false;
        isMove = false;
        isMenu = false;
        backMenu = false;
        unit.playerController[playerID].isAct = true;
        unit.stayCount++;
        stateCount = 0;
        MoveState();
    }

    public void ReturnPos()
    {
        isMove = true;
        unit.playerController[playerID].playerPos = savePos;
        cirsor.transform.position = new Vector3(savePos.x * div, cirsor.transform.position.y, savePos.y * div);
        cirsorController.cirsorPos = new Vector2(savePos.x, savePos.y);
        cirsorController.isMove = true;
        cirsorController.Move();
        movableScript.Initialize();

    }

    public void Cancel()
    {
        if (stateCount == 2)
        {
            MenuEnd();
            ReturnPos();
        }
        stateCount--;
        // キャンセル後のstate移動
        MoveState();
    }

    
    public void EnemySearch()
    {
        for(int i = 0;i < unit.enemyObj.Length; i++)
        {
            if(cirsorX == unit.enemyController[i].enemyPos.x && cirsorY == unit.enemyController[i].enemyPos.y)
            {
                unit.selectEnemy = i;
            }
        }
    }

    /// <summary>
    /// 攻撃範囲取得
    /// </summary>
    public void AttackRange()
    {
        movableScript.Initialize();


        /// <summary>
        /// 必要な情報の取得
        /// </summary>
        playerID = unit.selectUnit;                                       // 選択ユニットの番号
        unitObj = unit.playerObj[playerID];                               // ユニットの取得

        x = Mathf.RoundToInt(unit.playerController[playerID].playerPos.x);       // ユニットのX座標
        y = Mathf.RoundToInt(unit.playerController[playerID].playerPos.y);       // ユニットのZ座標
        cirsorX = Mathf.FloorToInt(cirsorController.cirsorPos.x);  // カーソルのX座標
        cirsorY = Mathf.FloorToInt(cirsorController.cirsorPos.y);  // カーソルのZ座標

        movableScript.AttackRange(x, y, 2);
        map.DrawMap();
    }
}
