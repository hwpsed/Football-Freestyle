using UnityEngine;

public class PassingBall : MonoBehaviour
{
    public GameObject player;
    public GameObject gameController;
    public float passFactor;
    public float heightFactor;
    Movements playerScript;

    private void Start()
    {
        playerScript = player.GetComponent<Movements>();
        gameController = GameObject.FindGameObjectWithTag("GameController");
        passFactor = 1.5f;
        heightFactor = 0.225f;
    }

    public void passBall()
    {

        GameObject ball = playerScript.ball;
        BallController ballScript = ball.GetComponent<BallController>();

        if (ballScript.belongTo != null && ballScript.belongTo.Equals(player) && ball.transform.localPosition.y <= 1.2f)
        {
            float powerCharge = playerScript.powerCharge;
            float maxPowerCharge = playerScript.maxPowerCharge;
            float ratioCharge = powerCharge / maxPowerCharge;

            ball.GetComponent<BallController>().belongTo = null;
            Vector3 direction = gameController.GetComponent<GameController>().fixPassDirection(player, player.transform.forward, ratioCharge);

            if (playerScript.isLoft) // Loft Pass
            {
                Vector3 heightVector = new Vector3(0, heightFactor, 0);
                Vector3 finalDirection = (direction + heightVector * ratioCharge * 4.0f).normalized;
                float angleAlpha = Vector3.Angle(direction, finalDirection);

                if (ballScript.onWaitingForPass != null)
                {
                    float distanceToPass = Vector3.Distance(ballScript.onWaitingForPass.transform.position, ball.transform.position);

                    // velocity
                    ball.GetComponent<Rigidbody>().velocity = finalDirection * Mathf.Sqrt(9.8f * distanceToPass / Mathf.Sin(2 * Mathf.Deg2Rad * angleAlpha));
                }
                else
                    ball.GetComponent<Rigidbody>().velocity = finalDirection * 100f * ratioCharge;

            }
            else // Low Pass
                ball.GetComponent<Rigidbody>().velocity = direction * playerScript.passSpeed * passFactor * ratioCharge;
        }

        playerScript.powerCharge = 0f;
        playerScript.kickHeldDown = false;
    }

    public void finishPass()
    {
        playerScript.resetAnimation();
        playerScript.setState(Movements.State.Idle);
    }
}
