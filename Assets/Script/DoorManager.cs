using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [Header("���� ���� ����")]
    public bool isDoor = false; // false �� ���� , true �� ���� -> �÷��̾��� ��ġ���� �ٲ���
    bool isIn; // ��� �������� -> false �� / true ��
    bool isOpen = false; // ���� ���ȴ��� �Ǵ�
    bool isInOut; // �ش� �÷��̾ ������

    Animator doorAni;

    private void Start()
    {
        doorAni = GetComponent<Animator>();
        isInOut = PlayerController.instance.isPlayerIn; // false�� �� �ٱ��� true�� ������ ���� �ִϸ��̼��� false
        isIn = false;
    }

    void Update()
    {
        isInOut = PlayerController.instance.isPlayerIn;

        if (!isOpen && isDoor) // ���� �ִ� ��Ȳ����
        {
            if (!isInOut)
            {
                doorAni.SetBool("isOpen", true);
                isIn = false; // �ۿ��� �����ٸ�
                isOpen = true;
            }
            else
            {
                doorAni.SetBool("isOpenIn", true);
                isIn = true;
                isOpen = true;
            }
        }
        else if (isOpen && !isDoor) // �����ִ� ��Ȳ����
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
