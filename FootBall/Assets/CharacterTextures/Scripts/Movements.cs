using UnityEngine;
using System;
using System.Collections.Generic;

public class Movements : MonoBehaviour
{
    // Enum State class
    public enum State
    {
        Idle,
        Move,
        Kick,
        Pass,
        Trip,
        StandUp,
        Tackle,
        BodyBlock,
        DivingSave,
        Catch,
        WaitForNextState
    }

    // Attribute
    // ------------- Private ----------------
    protected Animator Anim;
    protected Transform trans;
    protected Rigidbody rigidBody;
    protected float inputX;
    protected float inputY;
    protected double minY = double.NegativeInfinity;
    protected List<string> animParamList;
    protected bool playerControl = false;
    protected List<bool> maintainHeightCondition;
    protected Vector3 targetPoint;
    protected float neckHeight;
    protected float hipHeight;
    
    // ------------- Public ----------------
    // Player Attribute
    public float walkSpeed;
    public float runSpeed;
    public float powerShoot;
    public float ballControl;
    public float passSpeed;
    public float tacklingDistance;
    public float vision;
    public float positioning;

    // Power Shooting Attribute
    public float powerCharge = 0f;
    public float maxPowerCharge = 0.5f;
    public float chargeSpeed;
    public bool kickHeldDown = false;
    public bool isOnControlled = false;
    public bool isLoft;
    

    // Other
    public GameObject ball;
    public GameObject bar;
    public GameObject barParent;
    public GameObject cursor;
    public GameController gameController;
    public float currentSpeed;
    public State state;


    // Method
    // -----------------------------------------------------------------
    // Start is called before the first frame update
    public void Start()
    {
        // set value of attribute
        walkSpeed = 6.0f;
        runSpeed = 15.0f;
        powerShoot = 60f;
        ballControl = 50.0f;
        passSpeed = 50.0f;
        tacklingDistance = 15f;
        vision = 45;
        positioning = 0.6f;
        // -------------------------------

        Anim = gameObject.GetComponent<Animator>();
        trans = gameObject.GetComponent<Transform>();
        rigidBody = gameObject.GetComponent<Rigidbody>();
        ball = GameObject.FindGameObjectWithTag("Ball");
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        neckHeight = GameObject.FindGameObjectWithTag("Neck").transform.position.y;
        hipHeight = GameObject.FindGameObjectWithTag("Hip").transform.position.y;
        chargeSpeed = 0.5f;
        
        // ------------------------------

        animParamList = new List<string>();
        // Add animation parameter's name to list
        animParamList.Add("isRunning");
        animParamList.Add("isKick");
        animParamList.Add("isWalking");
        animParamList.Add("isPass");
        animParamList.Add("isTackle");
        animParamList.Add("isTrip");
        animParamList.Add("isStandUp");

        maintainHeightCondition = new List<bool>();
        // Make object is never below the floor
        maintainHeightCondition.Add(trans.localPosition.y < minY);
        maintainHeightCondition.Add(!Anim.GetBool("isTackle"));
        maintainHeightCondition.Add(!Anim.GetBool("isTrip"));
        
    }

    // Update is called once per fram
    public void Update()
    {
        
        // Make object never below the floor
        fixHeight();

        // On ball check
        controlBallHandling();

        // Hide charge bar
        if (!kickHeldDown)
            barParent.SetActive(false);

        if (isOnControlled)
        {
            // Draw the game cursor
            if (cursor.activeSelf == false)
                cursor.SetActive(true);

            if (playerControl)
            {
                inputX = Input.GetAxis("Horizontal");
                inputY = Input.GetAxis("Vertical");

                // Action when on control
                chargePowerLowPassing();
                chargePowerShooting();
                chargePowerLoftPassing();
                doTackling();
            }
            else
            {
                inputX = 0;
                inputY = 0;
            }
            
        }
        else
        {
            // Remove the game cursor
            cursor.SetActive(false);
            targetPoint = findTargetPoint();
            moveToTarget();
        }

        // Draw Cursor and grant control 

        if (state.Equals(State.Idle) || state.Equals(State.Move))
        {
            if (inputX != 0 || inputY != 0)
            {
                setState(State.Move);
            }
            else
                setState(State.Idle);
        }

        updateAnimation();
        handleState();
    }

    protected void fixHeight()
    {
        // Check all conditions
        foreach (bool statement in maintainHeightCondition)
            if (!statement)
                return;

        trans.localPosition = new Vector3(trans.localPosition.x, (float)minY, trans.localPosition.z);
    }

    protected void controlBallHandling()
    {
        // Check all conditions
        if (ball.GetComponent<BallController>().belongTo != null && ball.GetComponent<BallController>().belongTo.Equals(gameObject))
            Anim.SetBool("isOnBall", true);
        else
            Anim.SetBool("isOnBall", false);
    }

    protected virtual void handleIdleState()
    {   
        resetAnimation();
    }

    protected float getDegreeFromInput(float ver, float hor)
    {
        // Create an bacsic vector i
        Vector2 vecA = new Vector2(-hor, ver);

        // Calculate the angle between input and vector i
        // the result always get the values from 0 to 180
        float result = Vector2.SignedAngle(new Vector2(0, 1), vecA);
        
        // make the result from 0 to -180 if needed

        return result;
    }

    protected Tuple<float, float> getInputFromDegree(float degree)
    {
        return new Tuple<float, float>(-Mathf.Sin(degree * Mathf.Deg2Rad) , Mathf.Cos(degree * Mathf.Deg2Rad));
    }

    protected virtual void updateAnimation()
    {
        // execute the animation
        if (state.Equals(State.Move))
            Anim.SetBool("isWalking", true);

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            Anim.SetBool("isRunning", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.Joystick1Button5))
        {
            Anim.SetBool("isRunning", false);
        }
    }

    protected virtual void updateMovement(float horizontal, float vertical)
    {
        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1

        if (Anim.GetBool("isRunning") == true)
            currentSpeed = runSpeed;
        else if (Anim.GetBool("isWalking") == true)
            currentSpeed = walkSpeed;
        // Move translation along the object's z-axis
        float translationZ = Math.Max(Math.Abs(vertical), Math.Abs(horizontal)) * currentSpeed;
        rigidBody.velocity = trans.forward * translationZ;


        float inputDegree = getDegreeFromInput(vertical, horizontal);
        // check the angle between player and input

        trans.eulerAngles = new Vector3(trans.eulerAngles.x, inputDegree, trans.eulerAngles.z);
    }
    protected void chargePowerShooting()
    {
        if (state.Equals(State.WaitForNextState))
            return;

        if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && Anim.GetBool("isOnBall") == true)
        {
            kickHeldDown = true;
        }
        else if ((Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.Joystick1Button0)) && Anim.GetBool("isOnBall") == true)
        {
            setState(State.Kick);
        }

        chargePower();
    }

    protected void chargePowerLowPassing()
    {
        if (state.Equals(State.WaitForNextState))
            return;

        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && Anim.GetBool("isOnBall") == true)
        {
            kickHeldDown = true;
        }
        else if ((Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.Joystick1Button0)) && Anim.GetBool("isOnBall") == true)
        {
            setState(State.Pass);
            isLoft = false;
        }

        chargePower();
    }

    protected void chargePowerLoftPassing()
    {
        if (state.Equals(State.WaitForNextState))
            return;

        if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && Anim.GetBool("isOnBall") == true)
        {
            kickHeldDown = true;
        }
        else if ((Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.Joystick1Button0)) && Anim.GetBool("isOnBall") == true)
        {
            setState(State.Pass);
            isLoft = true;
        }

        chargePower();
    }

    private void chargePower()
    {
        if (kickHeldDown)
        {
            barParent.SetActive(true);
            if (powerCharge < maxPowerCharge)
                powerCharge += chargeSpeed * Time.deltaTime;
            else
                powerCharge = maxPowerCharge;

            bar.transform.localScale = new Vector3(powerCharge / maxPowerCharge, bar.transform.localScale.y, bar.transform.localScale.z);
        }
    }

    protected void doTackling()
    {
        if (state.Equals(State.WaitForNextState))
            return;

        if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && Anim.GetBool("isOnBall") == false && rigidBody.velocity.magnitude > walkSpeed * 2)
        {
            setState(State.Tackle);
        }
        
    }

    // Collision handling
    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Floor"))
        {
            if (double.IsNegativeInfinity(minY))
            {
                minY = collision.gameObject.transform.localPosition.y;
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Get the ball

        if (other.gameObject.Equals(ball) && !Anim.GetBool("isKick") && !Anim.GetBool("isPass"))
        {
            List<bool> conditionList = new List<bool>();
            BallController ballController = ball.GetComponent<BallController>();
            Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;

            conditionList.Add(ballController.belongTo == null);
            conditionList.Add(ballController.touchable);
            conditionList.Add(ballVelocity.magnitude <= ballControl);

            // Check all conditions
            foreach (bool statement in conditionList)
                if (!statement)
                    return;

            ballController.belongTo = gameObject;
        }
    }

    protected void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Fence"))
        {
            trans.localPosition += new Vector3(-trans.localPosition.x, trans.localPosition.y, -trans.localPosition.z) * Time.deltaTime * Time.deltaTime;
        }

        // Do tackle
        if (collision.gameObject.tag.Equals("Footballer"))
        {
            List<bool> conditionList = new List<bool>();
            Animator animator = collision.gameObject.GetComponent<Animator>();
            conditionList.Add(!animator.GetBool("isTrip"));
            conditionList.Add(!animator.GetBool("isStandUp"));
            conditionList.Add(animator.GetBool("isOnBall"));
            conditionList.Add(Anim.GetBool("isTackle"));
            

            foreach (bool statement in conditionList)
                if (!statement)
                    return;
            
            collision.gameObject.GetComponent<Movements>().setState(State.Trip);
        }
    }

    public void resetAnimation()
    {
        foreach (string anim in animParamList)
        {
            Anim.SetBool(anim, false);
        }
    }

    public void setAnimation(String state, bool value=true)
    {
        resetAnimation();
        Anim.SetBool(state, value);
    }

    public void grantPlayerControl()
    {
        playerControl = true;
    }

    public void setState(State state)
    {
        this.state = state;
    }

    protected virtual Vector3 findTargetPoint()
    {
        return Vector3.zero;
    }
    protected virtual void moveToTarget()
    {
        inputX = 0f;
        inputY = 0f;
    }

    public virtual void handleState()
    {
        float animationFactor = 0.25f * (1 - powerCharge / maxPowerCharge);

        switch (state)
        {
            case State.Idle:
                handleIdleState();
                break;

            case State.Move:
                updateMovement(inputX, inputY);
                break;

            case State.Pass:
                setAnimation("isPass", true);
                Anim.Play("Pass", 0, animationFactor);
                setState(State.WaitForNextState);
                break;

            case State.Kick:
                setAnimation("isKick");
                Anim.Play("Kick", 0, animationFactor);
                setState(State.WaitForNextState);
                break;

            case State.Trip:
                Anim.SetBool("isTrip", true);
                Anim.Play("Trip", 0, animationFactor);
                setState(State.WaitForNextState);
                break;

            case State.StandUp:
                Anim.SetBool("isStandUp", true);
                Anim.Play("StandUp", 0, animationFactor);
                setState(State.WaitForNextState);
                break;

            case State.Tackle:
                setAnimation("isTackle", true);
                Anim.Play("Tackle", 0, animationFactor);
                rigidBody.velocity = trans.forward.normalized * tacklingDistance * rigidBody.velocity.magnitude * 0.08f;
                rigidBody.velocity += new Vector3(0, -4, 0);
                trans.eulerAngles = new Vector3(trans.eulerAngles.x, trans.eulerAngles.y - 90, trans.eulerAngles.z);
                setState(State.WaitForNextState);
                break;

            default:
                return;
        }
    }
}

