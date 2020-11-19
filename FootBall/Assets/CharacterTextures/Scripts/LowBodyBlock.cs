using UnityEngine;

public class LowBodyBlock : MonoBehaviour
{ 
    Movements playerScript;

    private void Start()
    {
        playerScript = gameObject.GetComponent<GKMovements>();
    }
    public void blockBall()
    {
       
    }

    public void finishBlock()
    {
        playerScript.resetAnimation();
        playerScript.setState(Movements.State.Idle);
    }
}
