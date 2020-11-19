using UnityEngine;
using System.Collections.Generic;

public class GoalDetection : MonoBehaviour
{
    public GameObject middle;
    public GameObject left;
    public GameObject right;
    public List<Vector3> frontGoal;
    public int density = 10;

    private void Start()
    {
        createFrontGoalArray();
    }

    public void createFrontGoalArray()
    {
        float deltaDistance = Vector3.Distance(left.transform.position, right.transform.position) / density;
        Vector3 linkedLine = left.transform.position - right.transform.position;

        for (int i = 0; i < density; i++)
        {
            Vector3 point = right.transform.position + middle.transform.forward * 4.0f + linkedLine.normalized * i * deltaDistance;
            point = new Vector3(point.x, 0, point.z);
            frontGoal.Add(point);

            //Instantiate(middle, frontGoal[i], Quaternion.Euler(0, 270, 0)).transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
