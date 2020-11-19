using UnityEngine;

public class transparentCamera : MonoBehaviour
{
    private RaycastHit vision;
    private Vector3 direction;
    private GameObject ball;
    private MeshRenderer fenceRender;
    private MeshRenderer columnRender;
    public GameObject fence;
    public GameObject column;


    // Start is called before the first frame update
    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        fenceRender = fence.GetComponent<MeshRenderer>();
        columnRender = column.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = ball.transform.position - transform.position;
            
        if (Physics.Raycast(transform.position, direction, out vision, direction.magnitude))
        {

            if (vision.collider == fence.GetComponent<BoxCollider>())
            {
                fenceRender.enabled = false;
                columnRender.enabled = false;

            }
            else
            {
                fenceRender.enabled = true;
                columnRender.enabled = true;
            }
        }
    }
}
