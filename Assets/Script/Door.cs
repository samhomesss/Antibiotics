using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // ���� �ۼ��ϴ� �ڵ�
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
            // ���� �ø��� transform�� �����ø��� �Ǵµ�
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
