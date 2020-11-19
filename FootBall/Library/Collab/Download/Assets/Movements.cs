using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movements : MonoBehaviour
{

    private Animator Anim;
    // Start is called before the first frame update
    void Start()
    {
        Anim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per fram
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Anim.SetBool("isWalking", true);
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            Anim.SetBool("isWalking", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Anim.SetBool("isRunning", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Anim.SetBool("isRunning", false);
        }
    }
}

