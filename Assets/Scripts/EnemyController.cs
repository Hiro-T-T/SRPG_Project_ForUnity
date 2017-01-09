﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyController : MonoBehaviour {

    UnitController unitcontroller;

    public Text text;
    public GameObject textObj;

    public int enemyID = 0;  // キャラクターID
    public string name = "test";
    public Vector2 enemyPos;
    public int level = 1;           // レベル
    public int hp = 18;             // 体力
    public int attack = 7;          // 攻撃力
    public int deffence = 4;        // 防御力
    public int hit;                 // 技
    public int speed;               // 速さ
    public int lucky;               // 幸運
    public int moveCost = 5;		// 移動力
    public bool isAct = false;
    private int cross = 10;

    public int[] weapon;
    public int selectWeapon;

    void Start()
    {
        // textObj = transform.FindChild("Canvas").gameObject;
        text = gameObject.GetComponentInChildren<Text>();
        unitcontroller = GameObject.FindGameObjectWithTag("UniCon").GetComponent<UnitController>();
        // textObj.SetActive(false);
        selectWeapon = weapon[0];
        ColorChange(0);
        
    }

    void Update()
    {
        transform.position = new Vector3(enemyPos.x * cross, transform.position.y, enemyPos.y * cross);
    }

    public void Damage(int damage)
    {
        ColorChange(1);
        if(damage > 0)
        {
            Debug.Log("EnemyID :" + enemyID + "Damage :" + damage);
            text.text = damage.ToString();
            hp -= damage;
        } else
        {
            text.text = "Miss!";
        }
        StartCoroutine("damageText");
        if(hp <= 0)
        {
            // unitcontroller.EnemyListRemove(enemyID);
            Destroy(gameObject);
        }
    }

    IEnumerator damageText()
    {
        Debug.Log("tex");
        yield return new WaitForSeconds(2);
        ColorChange(0);
        yield break;
    }

    public void ColorChange(int alpha)
    {
        var color = text.color;
        color.a = alpha;
        text.color = color;
    }
}
