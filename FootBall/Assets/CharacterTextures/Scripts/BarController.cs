using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    public GameObject chargeBar;
    public GameObject Cursor;
    Camera dynamicCamera;
    private void Start()
    {
        dynamicCamera = GameObject.FindGameObjectWithTag("DynamicCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    { 
        Vector3 camPose = dynamicCamera.WorldToScreenPoint(transform.position);
        chargeBar.transform.position = camPose;
        Cursor.transform.position = camPose;
    }
}