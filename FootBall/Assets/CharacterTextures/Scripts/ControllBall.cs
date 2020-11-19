using UnityEngine;
using System.Collections.Generic;

public class ControllBall : MonoBehaviour
{
    public GameObject player;
    private Movements playerScript;
    private Animator Anim;

    private void Start()
    {
        playerScript = player.GetComponent<Movements>();
        Anim = player.GetComponent<Animator>();
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        // Get the ball

        if (other.gameObject.Equals(playerScript.ball) && !Anim.GetBool("isKick") && !Anim.GetBool("isPass"))
        {
            List<bool> conditionList = new List<bool>();
            BallController ballController = playerScript.ball.GetComponent<BallController>();
            Vector3 ballVelocity = playerScript.ball.GetComponent<Rigidbody>().velocity;

            conditionList.Add(ballController.belongTo == null);
            conditionList.Add(ballController.touchable);
            conditionList.Add(ballVelocity.magnitude <= playerScript.ballControl);

            // Check all conditions
            foreach (bool statement in conditionList)
                if (!statement)
                    return;

            ballController.belongTo = gameObject;
        }
    }

}
