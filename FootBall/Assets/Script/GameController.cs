using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    Vector3[] OnBallTeamLocations = { new Vector3(0, 0.1f, 0), new Vector3(7, 0.1f, 5.5f), new Vector3(7, 0.1f, -5.5f), new Vector3(11, 0.1f, 0) };
    Vector3[] OnDefenseTeamLocations = { new Vector3(7, 0.1f, 0), new Vector3(3, 0.1f, 5.5f), new Vector3(3, 0.1f, -5.5f), new Vector3(11, 0.1f, 0) };

    // Remy prefab
    public GameObject remyPrefabTeamA;
    public GameObject remyPrefabTeamB;
    public GameObject gkPrefabTeamA;
    public GameObject gkPrefabTeamB;
    
    public GameObject stadium;
    public GameObject ball;
    public GameObject leftGoal;
    public GameObject rightGoal;
    public Text myTeamScore;
    public Text enemyTeamScore;
    public Text gameTime;

    private int gameMinute = 0;
    private int gameSecond = 0;
    private FootballTeam myTeam;
    private FootballTeam enemyTeam;

    // Start is called before the first frame update
    void Start()
    {
        int teamAttack = UnityEngine.Random.Range(0, 2); // 0: team A first, 1: team B first
        myTeam = new FootballTeam(remyPrefabTeamA, gkPrefabTeamA, new List<GameObject>(), myTeamScore);
        enemyTeam = new FootballTeam(remyPrefabTeamB, gkPrefabTeamB, new List<GameObject>(), enemyTeamScore);

        spawnTeams(0, myTeam, enemyTeam);
        grantControlToTeam(myTeam);

        InvokeRepeating("updateTime", 0, 0.1f);
    }

    void Update()
    {
        updateCursor(myTeam);
        updateCursor(enemyTeam);
    }

    private void updateTime()
    {
        ++gameSecond;
        if (gameSecond == 60)
        {
            gameSecond = 0;
            gameMinute++;
        }

        if (gameMinute < 10) gameTime.text = "0" + gameMinute.ToString(); else gameTime.text = gameMinute.ToString();
        gameTime.text += ":";
        if (gameSecond < 10) gameTime.text += "0" + gameSecond.ToString(); else gameTime.text += gameSecond.ToString();
    }

    private void updateCursor(FootballTeam team)
    {
        BallController ballScript = ball.GetComponent<BallController>();

        // if our team doesn't have the ball's possession, then update cursor
        if (ball != null && !team.teamList.Contains(ball.GetComponent<BallController>().belongTo))
        {
            GameObject temp;

            if (ballScript.onWaitingForPass == null)
                temp = team.findNearestPlayerToChangeCursor(ball);
            else
                temp = ballScript.onWaitingForPass;

            foreach(GameObject player in team.teamList)
                player.GetComponent<Movements>().isOnControlled = false;

            team.onControlPlayer = temp;
            team.onControlPlayer.GetComponent<Movements>().isOnControlled = true;

        }
        else
        {
            foreach (GameObject player in team.teamList)
                player.GetComponent<Movements>().isOnControlled = false;
            ballScript.belongTo.GetComponent<Movements>().isOnControlled = true;
            team.onControlPlayer = ballScript.belongTo;
        }            

    }   
    private void spawnTeams(int teamAttack, FootballTeam teamA, FootballTeam teamB)
    {

        rightGoal.GetComponent<ScoreController>().belongTo = teamA;
        leftGoal.GetComponent<ScoreController>().belongTo = teamB;

        if (teamAttack == 0)
        {
            for (int i = 0; i < OnBallTeamLocations.Length; i++)
            {
                // Add team 
                if (i == OnBallTeamLocations.Length - 1)
                {
                    // if this is the last player, it must be GoalKeeper
                    teamA.teamList.Add(Instantiate(teamA.gkPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 270, 0)) as GameObject);
                    teamA.goolie = teamA.teamList[i];
                    teamA.goolie.GetComponent<GKMovements>().Goal = rightGoal;
                }
                else
                    // else it is normal player
                    teamA.teamList.Add(Instantiate(teamA.playerPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 270, 0)) as GameObject);

                // Set the parent
                teamA.teamList[i].transform.parent = stadium.transform;
                teamA.teamList[i].transform.localPosition = OnBallTeamLocations[i];

                // First player holds the ball
                if (i == 0)
                {
                    teamA.teamList[i].transform.eulerAngles = new Vector3(0, 90, 0); // Set the facing angle of the player on the ball
                    ball.GetComponent<BallController>().belongTo = teamA.teamList[i]; // Give the ball to the player
                    teamA.onControlPlayer = teamA.teamList[i];
                    teamA.teamList[i].GetComponent<Movements>().isOnControlled = true;
                }

                // Add team B

                if (i == OnBallTeamLocations.Length - 1)
                {
                    // if this is the last player, it must be GoalKeeper
                    teamB.teamList.Add(Instantiate(teamB.gkPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0)) as GameObject);
                    teamB.goolie = teamB.teamList[i];
                    teamB.goolie.GetComponent<GKMovements>().Goal = leftGoal;
                }
                else
                    // else it is normal player
                    teamB.teamList.Add(Instantiate(teamB.playerPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0)) as GameObject);

                // Set the parent
                teamB.teamList[i].transform.parent = stadium.transform;
                Vector3 temp = OnDefenseTeamLocations[i];
                teamB.teamList[i].transform.localPosition = new Vector3(temp.x * -1, temp.y, temp.z * -1); // Reverse the position
            }
        }
        else
        {
            for (int i = 0; i < OnBallTeamLocations.Length; i++)
            {
                // Add team A
                if (i == OnBallTeamLocations.Length - 1)
                {
                    // if this is the last player, it must be GoalKeeper
                    teamA.teamList.Add(Instantiate(teamA.gkPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0)) as GameObject);
                    teamA.goolie = teamA.teamList[i];
                    teamA.goolie.GetComponent<GKMovements>().Goal = leftGoal;
                }
                else
                    // else it is normal player
                    teamA.teamList.Add(Instantiate(teamA.playerPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0)) as GameObject);
                teamA.teamList[i].transform.parent = stadium.transform;
                teamA.teamList[i].transform.localPosition = OnDefenseTeamLocations[i];

                // Add team B
                if (i == OnBallTeamLocations.Length - 1)
                {
                    // if this is the last player, it must be GoalKeeper
                    teamB.teamList.Add(Instantiate(teamB.gkPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 270, 0)) as GameObject);
                    teamB.goolie = teamB.teamList[i];
                    teamB.goolie.GetComponent<GKMovements>().Goal = rightGoal;
                }
                else
                    // else it is normal player
                    teamB.teamList.Add(Instantiate(teamB.playerPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 270, 0)) as GameObject);

                // Set the parent
                teamB.teamList[i].transform.parent = stadium.transform;
                Vector3 temp = OnBallTeamLocations[i];
                teamB.teamList[i].transform.localPosition = new Vector3(temp.x * -1, temp.y, temp.z * -1); // Reverse the position

                // First player holds the ball
                if (i == 0)
                {
                    teamB.teamList[i].transform.eulerAngles = new Vector3(0, 270, 0); // Set the facing angle of the player on the ball
                    ball.GetComponent<BallController>().belongTo = teamB.teamList[i]; // Give the ball to the player
                    teamB.onControlPlayer = teamB.teamList[i];
                    teamB.teamList[i].GetComponent<Movements>().isOnControlled = true;
                }
            }
        }
    }

    public void grantControlToTeam(FootballTeam team)
    {
        foreach (GameObject player in team.teamList)
        {
            player.GetComponent<Movements>().grantPlayerControl();
        }
    }

    public Vector3 fixPassDirection(GameObject playerPass, Vector3 direction, float forceRatio)
    {
        FootballTeam team = myTeam;
        Vector3 fixedDirection = Vector3.zero;
        float minDistance = (float)double.MaxValue;
        Vector3 passPos = playerPass.transform.position + direction.normalized * 80 * forceRatio;

        GameObject passToPlayer = null;

        if (enemyTeam.teamList.Contains(playerPass))
            team = enemyTeam;

        foreach (GameObject player in team.teamList)
        {
            if (player.Equals(playerPass))
                continue;

            float distance = Vector3.Distance(passPos, player.transform.position);
            Vector3 vecReceivePass = player.transform.position - playerPass.transform.position;
            float angle = Vector3.Angle(playerPass.transform.forward, vecReceivePass);

            if (distance < minDistance && angle < playerPass.GetComponent<Movements>().vision)
            {
                minDistance = distance;
                fixedDirection = vecReceivePass;
                passToPlayer = player;
            }
        }

        if (passToPlayer != null)
            ball.GetComponent<BallController>().onWaitingForPass = passToPlayer;
            

        if (fixedDirection.Equals(Vector3.zero))
            return direction;
        return fixedDirection.normalized;
    }

    public Vector3 fixShootDirection(GameObject playerShoot, Vector3 directionBase)
    {
        Vector3 fixedDirection = Vector3.zero;
        Vector3 direction = new Vector3(directionBase.x, 0, directionBase.z);
        FootballTeam team = myTeam;
        GoalDetection goalDetection = rightGoal.GetComponent<GoalDetection>();
        float minAngle = (float)double.MaxValue;

        if (enemyTeam.teamList.Contains(playerShoot))
            team = enemyTeam;

        if (rightGoal.GetComponent<ScoreController>().belongTo.Equals(team))
            goalDetection = leftGoal.GetComponent<GoalDetection>();

        List<GameObject> listDirection = new List<GameObject>();

        listDirection.Add(goalDetection.middle);
        listDirection.Add(goalDetection.left);
        listDirection.Add(goalDetection.right);

        foreach (GameObject direct in listDirection)
        {
            Vector3 directTwoPoint = direct.transform.position - playerShoot.transform.position;
            directTwoPoint = new Vector3(directTwoPoint.x, 0, directTwoPoint.z);

            float angleBetween = Vector3.Angle(direction, directTwoPoint);


            if (angleBetween < minAngle)
            {
                minAngle = angleBetween;
                fixedDirection = directTwoPoint;
            }
        }

        if (!fixedDirection.Equals(Vector3.zero))
        {
            fixedDirection = new Vector3(fixedDirection.x, directionBase.y, fixedDirection.z);
            return fixedDirection.normalized;
        }
        return directionBase;
    }

    public bool isInSameTeam(GameObject myPlayer, GameObject checkPlayer)
    {
        List<GameObject> teamList = myTeam.teamList;
        if (enemyTeam.teamList.Contains(myPlayer))
            teamList = enemyTeam.teamList;

        if (teamList.Contains(checkPlayer))
            return true;
        return false;
    }
}

public class FootballTeam
{
    public GameObject playerPrefab;
    public GameObject gkPrefab;
    public List<GameObject> teamList;
    public GameObject onControlPlayer = null;
    public GameObject goolie = null;
    public int score;
    public Text textScore;
    public FootballTeam(GameObject playerPrefab, GameObject gkPrefab, List<GameObject> teamList, Text textScore)
    {
        this.playerPrefab = playerPrefab;
        this.gkPrefab = gkPrefab;
        this.teamList = teamList;
        this.textScore = textScore;
        score = 0;
    }

    public void Goal()
    {
        ++score;
        textScore.text = score.ToString();
    }

    public GameObject findNearestPlayerToChangeCursor(GameObject ball)
    {
        GameObject minDistancePlayer = null;
        float minDistance = (float)double.PositiveInfinity;
        

        for (int i = 0; i < teamList.Count; i++)
        {
            Animator animator = teamList[i].GetComponent<Animator>();
            float temp = Vector3.Distance(teamList[i].transform.position, ball.transform.position);
            if (temp < minDistance && !animator.GetBool("isTrip") && !animator.GetBool("isStandUp"))
            {
                if (!teamList.Contains(ball.GetComponent<BallController>().belongTo) && teamList[i].Equals(goolie))
                    continue;

                minDistance = temp;
                minDistancePlayer = teamList[i];
            }
        }

        return minDistancePlayer;
    }
}
