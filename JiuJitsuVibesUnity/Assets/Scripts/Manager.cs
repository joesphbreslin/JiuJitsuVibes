using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

    int state = 0;
    public Animator redAnim, blueAnim;
  
	void Update () {
        if(state >= 0)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                state--;
                redAnim.SetInteger("state", state);
                blueAnim.SetInteger("state", state);
            }
         
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            state++;
            redAnim.SetInteger("state", state);
            blueAnim.SetInteger("state", state);
        }


    }
}
