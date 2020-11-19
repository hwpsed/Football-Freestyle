using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameObject belongTo = null;
    private Transform trans;
    private double minY = double.NegativeInfinity;
    public bool touchable = true;
    public GameObject parentCamera;
    public GameObject onWaitingForPass = null;
    public GameObject lastTouch = null;
    private float maxLeftX = -54;
    private float maxRightX = 10;
    private float distanceControl = 2.5f;
    

    // Start is called before the first frame update
    void Start()
    {
        trans = gameObject.transform;
        parentCamera = GameObject.FindGameObjectWithTag("CameraSupporter");
    }

    // Update is called once per frame
    void Update()
    {
        // Make object never below the floor
        if (trans.localPosition.y < minY)
            trans.localPosition = new Vector3(trans.localPosition.x, (float)minY, trans.localPosition.z);

        // Make the ball move with the player
        if (belongTo != null)
        {
            Vector3 temp = belongTo.transform.localPosition + belongTo.transform.forward * 0.52f;
            trans.localPosition = new Vector3(temp.x, trans.localPosition.y, temp.z);

            Vector3 playerPos = belongTo.transform.position;
            // set the parent camera
            if (playerPos.x >= maxLeftX && playerPos.x <= maxRightX)
                parentCamera.transform.position = new Vector3(playerPos.x, parentCamera.transform.position.y, parentCamera.transform.position.z);

            Vector3 playerVel = belongTo.GetComponent<Rigidbody>().velocity;

            gameObject.GetComponent<Rigidbody>().velocity = new Vector3(playerVel.x, gameObject.GetComponent<Rigidbody>().velocity.y, playerVel.z);

            // check the height
            if (trans.localPosition.y > distanceControl)
                belongTo = null;

            if (belongTo != null)
                lastTouch = belongTo;
        }
        else
            // set the parent camera
            if (trans.position.x >= maxLeftX && trans.position.x <= maxRightX)
                parentCamera.transform.position = new Vector3(trans.position.x, parentCamera.transform.position.y, parentCamera.transform.position.z);

        // remove on waiting for pass
        if (belongTo != null && onWaitingForPass != null)
        {
            onWaitingForPass.GetComponent<Movements>().isOnControlled = false;
            onWaitingForPass = null;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Fence"))
        {
            if (belongTo != null)
            {
                Vector3 reflectForce = (trans.position - belongTo.GetComponent<Transform>().position) * 2;
                gameObject.GetComponent<Rigidbody>().AddForce(reflectForce);
                belongTo = null;
                onWaitingForPass = null;
            }
        }
        
        if (collision.gameObject.tag.Equals("Floor"))
        {
            if (double.IsNegativeInfinity(minY))
            {
                minY = collision.gameObject.transform.localPosition.y;
            }
        }

        if (collision.gameObject.tag.Equals("Footballer") && !collision.gameObject.Equals(belongTo))
        {
            belongTo = null;
            onWaitingForPass = null;
            lastTouch = collision.gameObject;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Fence"))
        {
            if (belongTo != null)
            {
                belongTo = null;
                touchable = false;
            }
            trans.localPosition += new Vector3(-trans.localPosition.x, trans.localPosition.y, -trans.localPosition.z) * Time.deltaTime * Time.deltaTime;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Fence"))
        {
            touchable = true;
        }
    }
}
