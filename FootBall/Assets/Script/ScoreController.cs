using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public FootballTeam belongTo;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Ball"))
        {
            belongTo.Goal();
        }
    }
}
