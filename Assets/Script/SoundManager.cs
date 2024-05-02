using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundName
{
    Impact_Enemy,
    Impact_Grass,
    Gun_Reload,
    Main_Sound,
    Gun_Shot,
    Dry_Fire,
    Gun_UnReload,
    Gun_MagazineLoad,
    Walk,
    Run,
    PickUp,
    BulletCase,
    DoorSound,
    BodyDamaged,
    Enemy_Pain,
    DoorLockOpen
    
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("SoundEffect ����")]
    public AudioClip[] soundEffect;

    [Header("Sound ��� ���")]
    public AudioSource mainSoundPlayer;
    public AudioSource playerSound;
    public AudioSource doorSound;
    public AudioSource playerDamagedSound;
    public AudioSource enemySound;

    private void Awake()
    {
       instance = this;
    }

    private void Start()
    {
        mainSoundPlayer.clip = soundEffect[(int)SoundName.Main_Sound]; // ���� ���� ����
        mainSoundPlayer.Play();
    }

}
