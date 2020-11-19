using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingSave : MonoBehaviour
{

    Movements playerScript;

    private void Start()
    {
        playerScript = gameObject.GetComponent<GKMovements>();
    }
    public void catchBall()
    {

    }

    public void finishCatch()
    {
        playerScript.resetAnimation();
        playerScript.setState(Movements.State.Idle);
    }
}
