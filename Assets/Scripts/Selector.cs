using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class Selector : MonoBehaviour
{
    public GameObject goUnderSelector;
    public GameObject goHeldBySelector;
    Vector3 pickUpLocation;
    States states;
    Character chaHeldUnit;
    Character chaHoveredUnit;
    List<Vector2> tilesMovedWithPlayerHeld = new List<Vector2>();
    public List<GameObject> playerCharsAlreadyActivated = new List<GameObject>();
    public List<GameObject> allPlayersOnMap = new List<GameObject>();
    public bool isPlayerPhase;
    EnemyPhase enemyPhase;

    void Awake()
    {
        states = Camera.main.GetComponent<States>();
        states.isHoveringPlayer = false;
        states.isHoveringEnemy = false;
        states.isHoldingPlayer = false;
        states.isHoldingEnemy = false;

        enemyPhase = Camera.main.GetComponent<EnemyPhase>();
        isPlayerPhase = true;

        allPlayersOnMap = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Checking gameobject hovered
        goUnderSelector = collision.gameObject;
        if (goUnderSelector.CompareTag("Player"))
        {
            states.isHoveringPlayer = true;
            chaHoveredUnit = goUnderSelector.GetComponent<Character>();
        }
        if (goUnderSelector.CompareTag("Enemy"))
        {
            states.isHoveringEnemy = true;
            chaHoveredUnit = goUnderSelector.GetComponent<Character>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject goMovedOffOf = collision.gameObject;
        if (goMovedOffOf.CompareTag("Player"))
        {
            states.isHoveringPlayer = false;
            goUnderSelector = null;
        }
        if (goMovedOffOf.CompareTag("Enemy"))
        {
            states.isHoveringEnemy = false;
            goUnderSelector = null;
        }
    }

    void Interacting()
    {
        //----------------------Gamer has pressed the Interact button-------------------------

        //If Gamer pressed interact while hovering a player
        if (states.isHoveringPlayer)
        {
            if (chaHoveredUnit.hasActedThisTurn) { return; }
            PickUpPlayer();
            return;
        }
        //If Gamer pressed Interact while hovering an enemy
        if (states.isHoveringEnemy && !states.isHoldingPlayer)
        {
            Debug.Log("Selector interacted w/ Enemy");
            return;
        }
        //If Gamer pressed Interact while holding a player
        if (states.isOnlyHoldingPlayer())
        {
            PlacePlayerAtLocation();
            return;
        }
        //If Gamer pressed Interact while holding a player && is hovering an enemy
        if(states.isHoldingPlayer && states.isHoveringEnemy)
        {
            PlayerAttackingEnemy();
            return;
        }
    }

    void Canceling()
    {
        //---------------------Gamer has pressed the cancel button-----------------------------------
        if (states.isHoldingPlayer)
        {
            DropPlayer();
        }
    }

    void Examining()
    {
        Debug.Log("HP: " + chaHoveredUnit.hp);
        Debug.Log("MP: " + chaHoveredUnit.mp);
        Debug.Log("Str: " + chaHoveredUnit.str);
    }

    private void FixedUpdate()
    {
        if(playerCharsAlreadyActivated.Count == allPlayersOnMap.Count && isPlayerPhase)
        {
            EndPlayerPhase();
        }
    }

    void Update()
    {
        if (!isPlayerPhase) return;
        //----------------------------------INPUTS---------------------------------------------------
        //movement
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left;
            if (states.isHoldingPlayer)
            {
                tilesMovedWithPlayerHeld.Add(new Vector2(transform.position.x, transform.position.y));
            }
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;
            if (states.isHoldingPlayer)
            {
                tilesMovedWithPlayerHeld.Add(new Vector2(transform.position.x, transform.position.y));
            }
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.position += Vector3.down;
            if (states.isHoldingPlayer)
            {
                tilesMovedWithPlayerHeld.Add(new Vector2(transform.position.x, transform.position.y));
            }
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.position += Vector3.up;
            if (states.isHoldingPlayer)
            {
                tilesMovedWithPlayerHeld.Add(new Vector2(transform.position.x, transform.position.y));
            }
        }

        //Interact key Input
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J))
        {
            Interacting();
        }

        //Cancel key input
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.K))
        {
            Canceling();
        }

        //Examine key Input
        if(Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.L))
        {
            Examining();
        }
    }

    public void PickUpPlayer()
    {
        //Selector is now holding hovered player: pickup location saved, make the goUnderSelector into a child of this go,
        //change it to goHeldBySelector, change state from hovering to holding player, & get players Char component
        pickUpLocation = goUnderSelector.transform.position;
        goUnderSelector.transform.parent = this.gameObject.transform;
        goHeldBySelector = goUnderSelector;
        goUnderSelector = null;
        states.isHoldingPlayer = true;
        states.isHoveringPlayer = false;
        chaHeldUnit = goHeldBySelector.GetComponent<Character>();
    }

    //For when you target & kill an enemy OR just move a unit
    public void PlacePlayerAtLocation()
    {
        //Add this player to activated player list > drop player by de-linking it as child > set goHeldBySelector to null > turn of isHoldingPlayer state > 
        //Set playerHaActedThisturn > Clear tilesMovedWithPlayer List
        playerCharsAlreadyActivated.Add(goHeldBySelector);
        goHeldBySelector.transform.parent = null;
        goHeldBySelector = null;
        states.isHoldingPlayer = false;
        chaHeldUnit.hasActedThisTurn = true;
        tilesMovedWithPlayerHeld.Clear();
    }

    public void PlayerAttackingEnemy()
    {
        int dmg = chaHeldUnit.DamageCalc(chaHeldUnit.str, chaHoveredUnit.def);
        chaHoveredUnit.hp -= dmg;
        chaHeldUnit.hasActedThisTurn = true;
        Debug.Log(goHeldBySelector + " Attacked " + goUnderSelector);
        Debug.Log("HP Left: " + chaHoveredUnit.hp);

        if(chaHoveredUnit.hp <= 0)
        {
            PlacePlayerAtLocation();
        }
        else
        {
            PlacePlayerAtLastLocation();
        }
    }

    //For when you cancel w/ char held
    public void DropPlayer()
    {
        //Drop the hovered player, reset goUnderSelector to null, reset eState to idle
        goHeldBySelector.transform.parent = null;
        goHeldBySelector.transform.position = pickUpLocation;
        goHeldBySelector = null;
        states.isHoldingPlayer = false;
        tilesMovedWithPlayerHeld.Clear();
    }

    //For when you attack and don't kill enemy
    public void PlacePlayerAtLastLocation()
    {
        //Adds go to list of activated chars > gets last Location from list of tiles moved in & set players pos to it > de-link parent child > set goHeldBySelector to null >
        //set states.isHoldingPlayer to false
        playerCharsAlreadyActivated.Add(goHeldBySelector);
        Vector2 lastLocation = tilesMovedWithPlayerHeld[tilesMovedWithPlayerHeld.Count - 2];
        goHeldBySelector.transform.position = lastLocation;
        goHeldBySelector.transform.parent = null;
        goHeldBySelector = null;
        states.isHoldingPlayer = false;
    }

    public void EndPlayerPhase()
    {
        Debug.Log("Player phase ended");
        //Make each enemyChar activatable again & Clear the activated List
        foreach (GameObject go in enemyPhase.allEnemiesOnMap)
        {
            Character tempCha = go.GetComponent<Character>();
            tempCha.hasActedThisTurn = false;
            enemyPhase.enemiesAlreadyActivated.Clear();
        }
        enemyPhase.StartEnemyPhase();
        isPlayerPhase = false;
        enemyPhase.isEnemyPhase = true;
    }
}

