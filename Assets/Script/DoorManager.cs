using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [Header("문에 대한 정보")]
    public bool isDoor = false; // false 면 닫힘 , true 면 열림 -> 플레이어의 위치에서 바꿔줌
    bool isIn; // 어디서 열었는지 -> false 밖 / true 안
    bool isOpen = false; // 문이 열렸는지 판단
    bool isInOut; // 해당 플레이어가 밖인지

    Animator doorAni;

    private void Start()
    {
        doorAni = GetComponent<Animator>();
        isInOut = PlayerController.instance.isPlayerIn; // false는 문 바깥쪽 true는 문안쪽 원래 애니메이션은 false
        isIn = false;
    }

    void Update()
    {
        isInOut = PlayerController.instance.isPlayerIn;

        if (!isOpen && isDoor) // 닫혀 있는 상황에서
        {
            if (!isInOut)
            {
                doorAni.SetBool("isOpen", true);
                isIn = false; // 밖에서 열었다면
                isOpen = true;
            }
            else
            {
                doorAni.SetBool("isOpenIn", true);
                isIn = true;
                isOpen = true;
            }
        }
        else if (isOpen && !isDoor) // 열려있는 상황에선
        {
            if ((!isIn))
                 doorAni.SetBool("isOpen", false);
            else
                 doorAni.SetBool("isOpenIn", false);
                
            isOpen = false;
        }
    }

    public void DoorSound()
    {
        SoundManager.instance.doorSound.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.DoorSound]);
    }

}
