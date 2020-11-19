using UnityEngine;

public class TacklingBall : MonoBehaviour
{
    public GameObject player;
    public float tacklingDistance;
    Movements playerScript;

    protected Rigidbody playerRigid;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = player.GetComponent<Movements>();
        playerRigid = player.GetComponent<Rigidbody>();
        tacklingDistance = 15f;
    }


    public void finishTackle()
    {
        playerScript.resetAnimation();
        playerScript.setState(Movements.State.Idle);
    }
}
