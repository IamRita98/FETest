using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPhase : MonoBehaviour
{
    public List<GameObject> allEnemiesOnMap = new List<GameObject>();
    public List<GameObject> enemiesAlreadyActivated = new List<GameObject>();
    List<GameObject> enemiesNotYetActivated = new List<GameObject>();
    List<Vector2> pathToNearestPlayer = new List<Vector2>();
    Selector selector;
    Transform nearestPlayerTrans;
    public bool isEnemyPhase;
    Pathfinding pathfinding;
    GameObject currentEnemy;
    GameObject nearestPlayer;
    Character playerChar;
    Character enemyChar;

    private void Awake()
    {
        allEnemiesOnMap = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        selector = GameObject.FindGameObjectWithTag("Selector").GetComponent<Selector>();
        pathfinding = Camera.main.GetComponent<Pathfinding>();
    }

    public void StartEnemyPhase()
    {
        foreach(GameObject go in allEnemiesOnMap)
        {
            enemiesNotYetActivated.Add(go);
        }
    }

    private void Update()
    {
        if (!isEnemyPhase) return;

        if (enemiesNotYetActivated.Count == 0)
        {
            EndEnemyPhase();
        }
        else if (enemiesNotYetActivated.Count > 0 && nearestPlayer == null)
        {
            nearestPlayerTrans = FindNearestPlayer();
            Pathfind(currentEnemy.transform, nearestPlayerTrans);
            AttackNearestPlayer();
        }
    }

    public Transform FindNearestPlayer()
    {
        currentEnemy = enemiesNotYetActivated.ElementAt(0);
        enemyChar = currentEnemy.GetComponent<Character>();
        Transform tLowest = null;
        float minDist = Mathf.Infinity;

        foreach(GameObject go in selector.allPlayersOnMap)
        {
            Transform t = go.transform;
            float dist = Vector2.Distance(t.position, currentEnemy.transform.position);
            if(dist < minDist)
            {
                nearestPlayer = go;
                tLowest = go.transform;
                minDist = dist;
            }
        }
        playerChar = nearestPlayer.GetComponent<Character>();
        return tLowest;
        
    }

    void Pathfind(Transform startPos, Transform endPos)
    {
        pathfinding.SetStartAndGoalNodes(startPos, endPos);
    }

    void AttackNearestPlayer()
    {
        //Do dmg calc
        pathToNearestPlayer = pathfinding.pathFound;
        int dmg = enemyChar.DamageCalc(enemyChar.str, playerChar.def);
        playerChar.hp -= dmg;

        //Move enemy to player
        currentEnemy.transform.position = pathToNearestPlayer.ElementAt(1);

        //Put enemy into activated enemies list
        enemiesAlreadyActivated.Add(currentEnemy);
        enemiesNotYetActivated.Remove(currentEnemy);
        //Set nearest player and current enemy to null
        nearestPlayer = null;
        currentEnemy = null;
        pathToNearestPlayer = null;
    }

    public void EndEnemyPhase()
    {
        Debug.Log("Enemy phase has ended");
        //Make each PlayerChar activatable again & Clear the activated List
        foreach (GameObject go in selector.allPlayersOnMap)
        {
            Character tempCha = go.GetComponent<Character>();
            tempCha.hasActedThisTurn = false;
            selector.playerCharsAlreadyActivated.Clear();
        }
        isEnemyPhase = false;
        selector.isPlayerPhase = true;
    }
}
