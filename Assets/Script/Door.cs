using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // 내가 작성하는 코드
    float nowRotation;
    float doorRotation;
    Quaternion nowDoorRot;
    Quaternion oldDoorRot;
    
    private void Start()
    {
        doorRotation = -90f;
        nowRotation = doorRotation;
        nowDoorRot = transform.rotation;
        oldDoorRot = transform.rotation;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) 
        {
            // 점점 늘리는 transform을 점점늘리면 되는듯
            oldDoorRot = Quaternion.Euler(0, nowRotation, 0);
            transform.rotation = Quaternion.Slerp(nowDoorRot, oldDoorRot, 5f);
            if (nowRotation < 0)
            {
                nowRotation -= doorRotation;
            }
            else
            {
                nowRotation += doorRotation;
            }
            
        }
       
    }
}
