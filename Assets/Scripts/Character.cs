using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int hp = 10;
    public int mp = 5;
    public int str = 4;
    public int dex = 4;
    public int skill = 4;
    public int def = 4;
    public int move = 6;
    public string unitClass;

    public bool hasActedThisTurn = false;

    int wepHit = 60;
    int classAvoRate = 10;

    private void Update()
    {
        if(hp <= 0)
        {
            Die();
        }
    }

    public float HitRateCalc()
    {
        float hitRate;
        hitRate = wepHit + (skill * 2) + (str / 2) + (dex / 2);
        return hitRate;
    }

    public float AvoidRateCalc()
    {
        float avoRate;
        avoRate = classAvoRate + (dex * 2);
        return avoRate;
    }

    public int DamageCalc(int atkerStr, int defDef)
    {
        int dmg;
        dmg = atkerStr - defDef;
        return dmg;
    }

    public void Die()
    {
        Debug.Log(gameObject + " Unit died");
        Destroy(this.gameObject);
    }
}
