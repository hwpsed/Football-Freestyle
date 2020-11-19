using UnityEngine;

public class ShootingBall : MonoBehaviour
{
    public GameObject player;
    public Movements playerScript;
    public float maxBonus = 20f;
    public float heightFactor = 0.6f;
    public GameController gameController;

    private void Start()
    {
        playerScript = player.GetComponent<Movements>();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        maxBonus = 20f;
        heightFactor = 0.6f;
}
    public void ballKick()
    {
        GameObject ball = playerScript.ball;
        BallController ballScript = ball.GetComponent<BallController>();

        if (ballScript.belongTo != null && ballScript.belongTo.Equals(player))
        {

            float powerCharge = playerScript.powerCharge;
            float maxPowerCharge = playerScript.maxPowerCharge;

            ball.GetComponent<BallController>().belongTo = null;

            Vector3 heightVector = new Vector3(0, powerCharge * heightFactor, 0);
            float bonusPowerCharge = powerCharge / maxPowerCharge * maxBonus;
            Vector3 direction = gameController.fixShootDirection(player, player.transform.forward);
            player.transform.rotation = Quaternion.LookRotation(direction);

            ball.transform.position = ball.transform.position + transform.forward * 0.8f;
            ball.GetComponent<Rigidbody>().velocity = (direction + heightVector) * (playerScript.powerShoot + bonusPowerCharge);

            //ball.GetComponent<Rigidbody>().velocity = (direction) * (playerScript.powerShoot + bonusPowerCharge);   
        }

        playerScript.powerCharge = 0f;
        playerScript.kickHeldDown = false;
        
    }

    public void finishShooting()
    {
        playerScript.resetAnimation();
        playerScript.setState(Movements.State.Idle);
    }
}
