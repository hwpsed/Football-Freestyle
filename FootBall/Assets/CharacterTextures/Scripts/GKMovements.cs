using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GKMovements : Movements
{
    // ----- Private ---------
    private List<Vector3> targetList;

    private string directionSave;
    private Vector3 velocitySave;
    private float timeToPerformSave;
    private static float timeToPerformBodyBlock = 1.06504f;
    private static float timeToFinishBlock = timeToPerformBodyBlock + 1.42f;
    private static float timeToPerformDivingSave = 1.16f;
    private static float timeToFinishDivingSave = 3.26f;


    // ----- Public ----------
    // ----- Attribute -------

    public float gkBlock = 99f;
    public float gkDiving = 99f;
    public float gkJump = 99f;


    // ----- Other -----------
    public GameObject Goal;

    
    // Start is called before the first frame update
    new void Start()
    {
        // Initializer
        GoalDetection goalDetection = Goal.GetComponent<GoalDetection>();
        targetList = goalDetection.frontGoal;

        // set value of attribute
        gkBlock = 99f;
        gkDiving = 99f;
        gkJump = 99f;

        base.Start();

        // Add animation parameter's name to list
        animParamList.Add("isBlock");
        animParamList.Add("isDiving");
        animParamList.Add("isRightSidestep");
        animParamList.Add("isLeftSidestep");

        // Make object is never below the floor
        maintainHeightCondition.Add(!Anim.GetBool("isBlock"));
    }

    // Update is called once per frame
    new void Update()
    {
        
        base.Update();
        if (!isOnControlled)
            makeDecision();
    }

    protected override Vector3 findTargetPoint()
    {
        float minDistance = (float) double.MaxValue;
        Vector3 target = Vector3.zero;

        foreach(Vector3 point in targetList)
        {
            float distance = Vector3.Distance(point, ball.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                target = point;
            }
        }

        return target;
    }
    protected override void moveToTarget()
    {
        if (!(state.Equals(State.Idle) || state.Equals(State.Move)))
            return;

        Vector3 projectPositionToFloor = new Vector3(trans.position.x, 0, trans.position.z);
        
        if (Vector3.Distance(projectPositionToFloor, targetPoint) < positioning)
        {
            inputX = 0;
            inputY = 0;
            return;
        }

        Vector3 directToTarget = targetPoint - trans.position;
        float angle = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(directToTarget.x, directToTarget.z));
        Tuple<float, float> input = getInputFromDegree(angle);

        inputX = input.Item1;
        inputY = input.Item2;
    }

    void makeDecision()
    {
        if (state.Equals(State.WaitForNextState))
            return;

        Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;
        Vector3 reverseProjectedBallDirection = new Vector3(-ballVelocity.x, 0, -ballVelocity.z);
        RaycastHit hitInfo;

        if (ball.GetComponent<BallController>().belongTo == null)
        {
            if (ballVelocity.magnitude > 30f)
            {
                if (Physics.Raycast(ball.transform.position, ballVelocity, out hitInfo, ballVelocity.magnitude))
                {
                    Debug.DrawRay(ball.transform.position, ballVelocity * ballVelocity.magnitude, Color.red, 0.5f);

                    // Get all colliders of goal
                    List<BoxCollider> collider = Goal.GetComponents<BoxCollider>().ToList();
                    List<Collider> playerCollider = GetComponentsInChildren<Collider>().ToList();

                    if (collider.Contains(hitInfo.collider) && Vector3.Distance(hitInfo.point, trans.position) > 10)
                    {
                        // Look to the direction parallel to the ball velocity to perform a body block
                        trans.rotation = Quaternion.LookRotation(reverseProjectedBallDirection, Vector3.up);

                        performSave(hitInfo, ballVelocity);
                    }
                }
            }
        }
    }

    private void performSave(RaycastHit hitInfo, Vector3 ballVelocity)
    {

        float realtimeToBlock;
        // the time took animation to do the save
        float timeToDoSave;
        float timeToFinishSave;

        float landmark = (hipHeight + neckHeight) / 2;

        if (hitInfo.point.y <= landmark)
        {
            timeToDoSave = timeToPerformBodyBlock;
            timeToFinishSave = timeToFinishBlock;
            realtimeToBlock = timeToDoSave * (100 - gkBlock) / 100;
        }
        else
        {
            timeToDoSave = timeToPerformDivingSave;
            timeToFinishSave = timeToFinishDivingSave;
            realtimeToBlock = timeToDoSave * (100 - gkDiving) / 100;
        }
        // project the ball direction the the 2 side of the gk
        velocitySave = Vector3.ProjectOnPlane(hitInfo.point - trans.position, -trans.forward);
        // calculate the time to perform diving save exactly
        Vector3 intersection = trans.position + velocitySave;
        float timeToIntersection = Vector3.Distance(intersection, ball.transform.position) / ballVelocity.magnitude;

        Debug.DrawRay(transform.position, velocitySave, Color.blue, 0.5f);
        // Calculate how fast gk needs to jump
        if (hitInfo.point.y > landmark)
        {
            float distance = Vector3.Distance(trans.position, intersection);

            float speedSave = distance / timeToIntersection;
            float tempY = velocitySave.y * 0.75f;
            float jumpFactor = 0.85f; // This is used for accuraccy jump, let the ball move to chest

            if (speedSave > gkJump * 0.2f)
                speedSave = gkJump * 0.2f;

            velocitySave = velocitySave.normalized * speedSave * jumpFactor;
            velocitySave = new Vector3(velocitySave.x, tempY, velocitySave.z);

        }

        if (timeToIntersection <= timeToDoSave)
        {
            if (timeToIntersection >= realtimeToBlock)
            {
                timeToPerformSave = timeToDoSave - timeToIntersection;
            }
            else
                timeToPerformSave = timeToDoSave - realtimeToBlock;
        }
        else return;

        timeToPerformSave = timeToPerformSave / timeToFinishSave;

        // Set the velocity when save
        if (Vector3.SignedAngle(velocitySave, trans.forward, Vector3.up) > 0)
            directionSave = "left";
        else
            directionSave = "right";

        if (hitInfo.point.y <= landmark)
            setState(State.BodyBlock);
        else
            setState(State.DivingSave);
    }

    private void doBlock()
    {
        if (state.Equals(State.WaitForNextState))
            return;

        Anim.SetBool("isBlock", true);

        if (directionSave.Equals("right"))
            Anim.Play("RightBodyBlock", 0, timeToPerformSave);
        else
            Anim.Play("LeftBodyBlock", 0, timeToPerformSave);

        rigidBody.velocity = velocitySave;
    }

    private void divingSave()
    {
        if (state.Equals(State.WaitForNextState))
            return;

        Anim.SetBool("isDiving", true);
        
        if (directionSave.Equals("right"))
            Anim.Play("RightDivingSave", 0, timeToPerformSave);
        else
            Anim.Play("LeftDivingSave", 0, timeToPerformSave);

        rigidBody.velocity = velocitySave;
    }

    protected override void updateMovement(float horizontal, float vertical)
    {

        if (isOnControlled)
            base.updateMovement(horizontal, vertical);
        else
        {
            Vector3 projectPositionToFloor = new Vector3(trans.position.x, 0, trans.position.z);
            if (Vector3.Distance(projectPositionToFloor, targetPoint) >= positioning * 3)
            {
                // Make the player run
                Anim.SetBool("isRunning", true);
                base.updateMovement(horizontal, vertical);
            }
            else
            {
                // do not run if near target point
                Anim.SetBool("isRunning", false);

                // make gk do side step
                if (inputY > 0)
                {
                    Anim.SetBool("isRightSidestep", true);
                    Anim.SetBool("isLeftSidestep", false);
                }
                else
                {
                    Anim.SetBool("isRightSidestep", false);
                    Anim.SetBool("isLeftSidestep", true);
                }

                // set velocitty
                float translationZ = Math.Max(Math.Abs(vertical), Math.Abs(horizontal)) * walkSpeed;
                Vector3 directionMove = new Vector3(inputX, 0, inputY);
                rigidBody.velocity = directionMove * translationZ;

                // Look at the ball
                trans.LookAt(new Vector3(ball.transform.position.x, trans.position.y, ball.transform.position.z));
            }
        }
    }

    public override void handleState()
    {
        base.handleState();
        
        switch (state)
        {
            case State.BodyBlock:
                doBlock();
                setState(State.WaitForNextState);
                break;

            case State.DivingSave:
                divingSave();
                setState(State.WaitForNextState);
                break;

            default:
                break;
        }
    }
}
