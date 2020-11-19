
using UnityEngine;

public class Tripped : MonoBehaviour
{

    Movements playerScript;
 
    void Start()
    {
        playerScript = GetComponent<Movements>();
    }

    // Update is called once per frame
    public void standingUp ()
    {
        playerScript.resetAnimation();
        playerScript.ball.GetComponent<BallController>().belongTo = null;

        playerScript.setState(Movements.State.StandUp);
    }

    public void finishStandUp()
    {
        playerScript.resetAnimation();
        playerScript.setState(Movements.State.Idle);
    }
}
