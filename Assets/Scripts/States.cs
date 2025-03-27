using UnityEngine;


public class States : MonoBehaviour
{
    public static States S;
    public bool isHoveringPlayer;
    public bool isHoveringEnemy;
    public bool isHoldingPlayer;
    public bool isHoldingEnemy;

    public States(bool isHoveringPlayerDefault, bool isHoveringEnemyDefault, bool isHoldingPlayerDefault, bool isHoldingEnemyDefault)
    {
        isHoveringPlayer = isHoveringPlayerDefault;
        isHoveringEnemy = isHoveringEnemyDefault;
        isHoldingPlayer = isHoldingPlayerDefault;
        isHoldingEnemy = isHoldingEnemyDefault;
    }

    public bool isOnlyHoldingPlayer()
    {
        return isHoldingPlayer && !isHoveringPlayer && !isHoveringEnemy && !isHoldingEnemy;
    }

    void Awake()
    {
        S = this;
    }
}
