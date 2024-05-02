using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("�÷��̾� �̵� ���� ")]
    public GameObject groundPos;
    public LayerMask groundLayer;
    CharacterController playerController;
    int playerSpeed = 10;
    int originPlayerSpeed = 10;
    bool isGround = false;
    float gravity = 9.8f;
    Vector3 velocity = Vector3.zero;

    [Header("�÷��̾� ���� ����")]
    public float mouseSpeed = 100f;
    float xRotation = 0; 
    float yRotation = 0;

    [Header("�÷��̾� �ִϸ��̼�")]
    Animator playerAni;
    bool isRun = false;
    public bool isShot = false;

    [Header("�÷��̾� ������")]
    public GameObject crosshair;
    [Range(30, 50)]
    public float maxCross = 30.0f;

    [Header("�÷��̾� Shot ����")]
    public float maxDistance = 300f;
    RaycastHit hit;
    public LayerMask monsterLayer;
    public GameObject[] impactEffect;

    [Header("�÷��̾� �Ѿ� UI , �Ѿ˰���")]
    public Text bulletText;
    public int maxBullet = 0; // �� �Ѿ� ����
    public int nowBullet = 0; // ���� �Ѿȿ� �� �ִ� �Ѿ� ����
    int reloadBullet = 100; // ��ź �ִ�� 
    int semiReload;
    bool isReload = false;

    [Header("ź�� ����")]
    public GameObject bulletCasePrefab;
    public GameObject bulletPrefab;

    [Header("������ ����")]
    public LayerMask itemLayer;
    public float itemDistance = 10;
    public GameObject itemGetUI;
    bool getUI = false;
    bool isGrab = false;
    GameObject lookItem;

    [Header("ī�޶� ����")]
    public float shakePower = 0.01f; // ���� ����
    float shakeTime = 0.1f;

    [Header("���� ������Ʈ ����")]
    public GameObject prefabManager;

    [Header("Sound ����")]
    AudioSource playerAudio;
    AudioSource playerDamagedAudio;
    AudioSource enemyAudio;

    [Header("�� ���� �κ�")]
    public GameObject[] gunParts; // �� ���� ���� �ֿﶧ �����ؼ� �����°�

    [Header("�÷��̾� HP ����")]
    public Slider HpUI; // slider ��������
    public float maxHP = 100; // �ִ� ü�� ���߿� �־��ְ�
    public float Hp = 100; // ���� ü��
    public GameObject getDamaged;

    [Header("�� ��ȣ�ۿ�")]
    public LayerMask doorLayer;
    public LayerMask lockDoorLayer; // ��乮
    public bool isPlayerIn = false; // false�� �� �ٱ��ʿ� �ִ°� true �� �� ���ʿ� �ִ°�.
    public Text uiText; // E Ŭ�� UI
    public bool isKeyHave = false; // ���踦 ������ �ִ°�?
    bool isDoor = false;
   
    [Header("����Ʈ ����")]
    public GameObject flashLight;
    public bool isLightHave = false;
    bool isPress = false; // �� �����°�

    [Header("��ƼŬ ��")]
    public GameObject particle;

    [Header("Quest Item�� Ȱ��ȭ ���϶�")]
    bool isQuest = false;

    [Header("SmartPhone UI")]
    public GameObject SmartPhoneUi; // ����Ʈ�� UI
    public bool isSmartPhone = false; // �ڵ����� �ִ°� 
    public bool isGameClear = false;
    public Slider backUp; // ��� �ε� ����

    [Header("Book UI")]
    public GameObject bookUi;
    public bool isBook;

    [Header("Quest â�� ������")]
    public bool isQuestOff = true;

    [Header("Quest Clear")]
    public GameObject QuestClearIcon;


    Vector3 moveCamera = new Vector3(-0.093f, 1.63f, -0.119f);

    private void Awake()
    {
       instance = this;
    } // SingleTon

    void Start()
    {
        playerController = GetComponent<CharacterController>();
        playerAni = GetComponentInChildren<Animator>();
        HpUI.maxValue = maxHP;

        playerAudio = SoundManager.instance.playerSound;
        playerDamagedAudio = SoundManager.instance.playerDamagedSound;
        enemyAudio = SoundManager.instance.enemySound;

        bulletText.text = nowBullet + " / " + maxBullet; // �̰� �׳� �����Ҷ� �ʱ�ȭ
    }

    void Update()
    {
        // ���� �����ۿ� �־�����ҵ�
        HaveBook(); // å�� �������� 
        HaveSmartPhone(); // �ڵ����� ��������

        HpUI.value = Hp;

        if (!isQuest && !isQuestOff) 
        {
            ItemEffect(); // ������ ����
            PlayerMove(); // �÷��̾� ������ + �ִϸ��̼� (Ư�� Ű) 
            MoveLook(); // �÷��̾� ����
            ShotBullet(); // �ѽ�°� ����
        }
    }

    

    void PlayerMove()
    {
        // �̵����� 
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 newPos = transform.forward * moveZ + transform.right * moveX;

        playerController.Move(newPos * playerSpeed * Time.deltaTime);

        // �ִϸ��̼�
        playerAni.SetFloat("Speed", Mathf.Abs(moveX + moveZ));

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift))
        {

            isRun = !isRun;

            if (isRun)
                playerSpeed *= 2;
            else
                playerSpeed = originPlayerSpeed;

            playerAni.SetBool("IsRun", isRun);
        }

        if (!isReload)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (maxBullet != 0)
                {
                    isReload = true;
                    StartCoroutine(ReloadAni());
                    maxBullet++;
                    nowBullet--; // �Ѿ� ���� �ȸ¾Ƽ� ��������
                }
                else
                    StartCoroutine(IsEmptyAni());
            }
        }

        // ����Ʈ Ű�°� 
        if (Input.GetKeyDown(KeyCode.T) && isLightHave)
        {
            isPress = !isPress;
            flashLight.SetActive(isPress);
        }


        // ���� ���� ��� 

        // ������
        isGround = Physics.CheckSphere(groundPos.transform.position, 0.1f, groundLayer); // checksphere

        if (!isGround)
        {
            // Time.deltatime�� �ι� ����ϴ� ������ ���ؼ�
            velocity.y -= gravity * Time.deltaTime; 
            playerController.Move(velocity * Time.deltaTime);
        }

        #region
        //Vector3 inputVec;
        //inputVec.x = Input.GetAxis("Horizontal");
        //inputVec.z = Input.GetAxis("Vertical");
        //// Vector3 ������ �Ұ�� ȭ�鿡 ���� ��ȯ�� �ȵ�
        //// forward�� ���̶� right�� ���� �̿��ؼ� �÷��̾� �� �κ��� ���ϴµ� 
        //Vector3 newPos = new Vector3(transform.position.x + inputVec.x * playerSpeed * Time.deltaTime, 0, transform.position.z + inputVec.z * playerSpeed * Time.deltaTime);

        //playerController.Move(newPos);
        #endregion
    }

    void MoveLook() 
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSpeed * Time.deltaTime;

        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -45.0f, 45.0f);
        xRotation += mouseX;
        transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0);
    }

    void ShotBullet()
    {
        // Ÿ�� ���� Ȯ�� 
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * maxDistance, Color.red);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            isShot = true;
            // �Ѿ� �� ������ ���ε� ����
            if (nowBullet == 0)
            {
                isReload = true;
                bulletText.text = nowBullet + " / " + maxBullet; // ���� ���� ������ 0�� �Ǿ����� �� 0�� ������� 
                StartCoroutine(ReloadAni());
            }

            if (!isReload && nowBullet != 0) // ������ ���� �ƴϰ� ���� �Ѿ��� �Ѿȿ� �� ������
            {
                // �ִϸ��̼� 
                playerAni.SetTrigger("Shot");
                // ���� ���̾�
                if (Physics.Raycast(Ray(), out hit, maxDistance, monsterLayer))
                {
                    hit.collider.gameObject.GetComponent<Enemy>().zombieHp -= 25;
                    enemyAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Enemy_Pain]);
                    // ���Ŀ� ���� ������ ���� ����� ��
                }
                if (Physics.Raycast(Ray(), out hit, maxDistance, groundLayer))
                {
                    Debug.Log(hit.collider.name);
                }
                
            }
            
        }
        else
        {
            isShot = false;
        }
    }

    void ItemEffect() // �� , ������ �Ծ��� �� �ִ� ���� && UI ��� && �ִ��÷���
    {
        if (Physics.Raycast(Ray(), out hit, itemDistance, itemLayer) || Physics.Raycast(Ray(), out hit, itemDistance, doorLayer))
        {
            getUI = true;
            lookItem = hit.collider.gameObject; // �ٷ� �ֿ�� ���� �������� NullReference�� ���� �ѹ� ��������
            uiText.text = "������ ȹ��";
            if (Physics.Raycast(Ray(), out hit, itemDistance, doorLayer))
            {
                uiText.text = "���� ���� / �ݴ´�";
                isDoor = hit.collider.GetComponentInParent<DoorManager>().isDoor;
            }
        }
        else if (Physics.Raycast(Ray(), out hit, itemDistance, lockDoorLayer))
        {
            getUI = true;
            lookItem = hit.collider.gameObject;

            if (isKeyHave)
            {
                uiText.text = "Ű�� ����Ѵ�";
                isDoor = hit.collider.GetComponentInParent<DoorManager>().isDoor;
            }
            else
            {
                uiText.text = "Ű�� ���� ���� ���� ����";
                isDoor = hit.collider.GetComponentInParent<DoorManager>().isDoor;
            }
                
            
        }
        else
            getUI = false;

        itemGetUI.SetActive(getUI);

        if (getUI)
        {
            if (Physics.Raycast(Ray(), out hit, itemDistance, itemLayer))
            {
                if (Input.GetKeyDown(KeyCode.E) && !isGrab)
                {
                    isGrab = true;
                    StartCoroutine(PickUpAni());
                }
            }

            if (Physics.Raycast(Ray(), out hit, itemDistance, doorLayer))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isDoor = !isDoor;
                    lookItem.GetComponentInParent<DoorManager>().isDoor = !lookItem.GetComponentInParent<DoorManager>().isDoor;

                    StartCoroutine(DoorOpen());
                }
            }

            if (Physics.Raycast(Ray(), out hit, itemDistance, lockDoorLayer) && isKeyHave)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isDoor = !isDoor;
                    lookItem.GetComponentInParent<DoorManager>().isDoor = !lookItem.GetComponentInParent<DoorManager>().isDoor;

                    StartCoroutine(LockedDoorOpenAni());
                }
            }

            
        }
    }

    void PlayerHpUp(float effect) // �÷��̾� ü�� ȸ��
    {
        Hp += effect; // HPUI.value���� ����
    }

    void BulletUP(int bullet) // �÷��̾� ü�� ����
    {
        maxBullet += bullet;

    }

    void HaveBook()
    {
        if (isBook)
        {
            isQuest = true;
            bookUi.SetActive(true);
        }
    } // å�� �����°�?

    void HaveSmartPhone()
    {
        if (isSmartPhone)
        {
            StartCoroutine(SmartPhoneUI());
        }
    }

    IEnumerator SmartPhoneUI()
    {
        isQuest = true;
        SmartPhoneUi.SetActive(true);
        backUp.value += 0.2f * Time.deltaTime;

        yield return new WaitForSeconds(5f);
        QuestClearIcon.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        isGameClear = true;
        isSmartPhone = false;
        isQuest = false;
        SmartPhoneUi.SetActive(false);
    }

    IEnumerator LockedDoorOpenAni()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.DoorLockOpen]);
        yield return new WaitForSeconds(0.7f);
        StartCoroutine(DoorOpen());
    } // ��� �� ����

    IEnumerator DoorOpen() // �Ⱦ� �ִϸ��̼�
    {
        Vector3 pickupCamera = new Vector3(-0.093f, 1.73f, 0);
        moveCamera = new Vector3(-0.093f, 1.63f, -0.119f);
        Camera.main.transform.localPosition = pickupCamera;

        playerAni.SetTrigger("PickUp");
        for (int i = 0; i < gunParts.Length; i++)
        {
            gunParts[i].SetActive(false);
        }
        yield return new WaitForSeconds(1.7f);

        Camera.main.transform.localPosition = moveCamera;

        isDoor = false; 
        for (int i = 0; i < gunParts.Length; i++)
        {
            gunParts[i].SetActive(true);
        }
    }

    IEnumerator PickUpAni() // �Ⱦ� �ִϸ��̼�
    {
        Vector3 pickupCamera = new Vector3(-0.093f, 1.73f, 0);
        moveCamera = new Vector3(-0.093f, 1.63f, -0.119f);
        Camera.main.transform.localPosition = pickupCamera;
        playerAni.SetTrigger("PickUp");
        for (int i = 0; i < gunParts.Length; i++)
        {
            gunParts[i].SetActive(false);
        }

        yield return new WaitForSeconds(1.7f);

        Camera.main.transform.localPosition = moveCamera;
        switch (lookItem.GetComponent<Item>().itemType)
        {
            case ItemType.Heal: // ȸ�� ���� �������̶��
                PlayerHpUp(lookItem.GetComponent<Item>().Effect());
                break;

            case ItemType.Bullet: // �Ѿ� ���� �������̶��
                BulletUP(lookItem.GetComponent<Item>().Effect());
                bulletText.text = nowBullet  + " / " + maxBullet;
                break;

            case ItemType.ActiveItem:
                switch (lookItem.GetComponent<Item>().itemEffect)
                {
                   
                    case global::ItemEffect.Light:
                        isLightHave = true;
                        Debug.Log(isLightHave);

                        break;
                    case global::ItemEffect.Key:
                        isKeyHave = true;
                        // ����� ����� ��� UI ���
                        break;
                    case global::ItemEffect.Quest:
                        switch (lookItem.GetComponent<Item>().questItem)
                        {
                            case QuestItem.Book:
                                isBook = true;
                                break;
                            case QuestItem.Phone:
                                isSmartPhone = true;
                                break;
                        }
                        break;
                    default:
                        break;
                }
                
                break;
        }

        Destroy(lookItem);
        isGrab = false; // �ι� ���ݰ� ���� ����
        for (int i = 0; i < gunParts.Length; i++)
        {
            gunParts[i].SetActive(true);
        }
    }

    IEnumerator ReloadAni() // �Ѿ� ���ε� ����
    {
        if (maxBullet == 0 && nowBullet == 0)
        {
            StartCoroutine(IsEmptyAni()); // �Ѿ��� ���� �ִ� ��� 
        }
        else
        {
            playerAni.SetTrigger("IsReload"); // ���� ��찡 �ƴѰ�� ���ε� �ִϸ��̼� 

            if (nowBullet == 0) // ���� �ѿ� �Ѿ��� ������
            {
                if (maxBullet >= reloadBullet) // ��ź�� 20 �� ���� ���ٸ� 
                {
                    nowBullet = reloadBullet;
                    maxBullet -= reloadBullet;
                }
                else
                {
                    nowBullet = maxBullet;
                    maxBullet = 0;
                }
            }
            else // nowBullet != 0 ���� �Ѿȿ� ���� �� ������ 
            {
                if (maxBullet > reloadBullet)
                {
                    semiReload = reloadBullet - nowBullet; // ��ź������ ���� ��ź�� ���� ���
                    nowBullet = reloadBullet; // 20�� ����
                    maxBullet -= semiReload; // ������ ��ŭ ���
                }
                else // �Ѿ��� 20�� �Ʒ����
                {
                    semiReload = reloadBullet - nowBullet; // ���� ������ �ִ� ��ź�� ���� 3�̶�� ���� 20 - 3 = 17
                    if (semiReload >= maxBullet) // �����ؾ� �ϴ� ��ź ������ ���� ������ �ִ� ź�� ���� ���ٸ� 
                    {
                        semiReload = maxBullet; // �����ؾ� �ϴ� ��ź���� ������ maxbullet���� ��ȯ
                        maxBullet = 0;
                        nowBullet += semiReload + 2;
                        maxBullet--;

                    }
                    else // semiReload < maxBullet
                    {

                        maxBullet -= semiReload;
                        nowBullet = reloadBullet;
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(3.0f);

        // ���� �ٻ���ϸ� �ٽ� �Ѿ��� �ö󰡴� ���װ� �־ �̷��� ���� ���� ��
        if (maxBullet == 0 && nowBullet == 0)
        {
            maxBullet = 0;
            nowBullet = 0;
            bulletText.text = nowBullet + " / " + maxBullet;
        }
        else
        {
            nowBullet++;
            bulletText.text = nowBullet + " / " + maxBullet;
            isReload = false;
        }
        
    }

    IEnumerator IsEmptyAni() // �Ѿ� ������ �ִ�
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Dry_Fire]);

        playerAni.SetBool("IsEmpty", true);
        yield return new WaitForSeconds(2.0f);
        playerAni.SetBool("IsEmpty", false);
    }

    Ray Ray() // ���־��� ���̰� �׳� �Լ��� ����
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        return ray;
    }

    IEnumerator CameraShake() // ī�޶� ���°�
    {
        //�׳� �ڷ�ƾ���� �ϸ� �ȵǴ� ���� ��鸮�� ���� ��鸮�µ� �� �����
        moveCamera = new Vector3(-0.093f, 1.63f, -0.119f); // ī�޶��� ���� ������ �����;� �� �ƴϸ� �÷��̾ ���� ������ ����
        float camerashakeTime = 0;

        while (camerashakeTime < shakeTime)
        {
            float x = Random.Range(-1f, 1f) * shakePower;
            float y = Random.Range(-1f, 1f) * shakePower;
           // z���� ���� �ȿ����̴°� �� ����Ѱ� ���Ƽ� ��
           // float z = Random.RandomRange(-1f, 1f) * shakePower;

            Camera.main.transform.localPosition = new Vector3(moveCamera.x + x, moveCamera.y + y, moveCamera.z);
            camerashakeTime += Time.deltaTime;
            yield return null;
        }
        

        Camera.main.transform.localPosition = moveCamera;
    }

    IEnumerator GetDamaged()
    {
        getDamaged.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        getDamaged.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // �� ���°� ����
        if (other.CompareTag("InTrigger"))
        {
            isPlayerIn = true;
        }
        if (other.CompareTag("OutTrigger"))
        {
            isPlayerIn = false;
        }

        // ���� ����
        if (other.CompareTag("Attack"))
        {
            Debug.Log("�¾ҽ��ϴ�.");
            playerDamagedAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.BodyDamaged]);
            StartCoroutine(GetDamaged());
            Hp -= 10;
        }
    } 

    /// <summary>
    /// �̺�Ʈ �Լ� ����
    /// </summary>
    // �̺�Ʈ �Ѿ� �Լ�
    // ź�� ���� + �Ѿ�

    // ��ư �̺�Ʈ
    public void ExitButton() // å���� ������ ��ư
    {
        isQuest = false;
        isBook = false;
        bookUi.SetActive(false);
    }
    void BulletEffect() // �ִϸ��̼� �̺�Ʈ �Լ��� ���� �� ��ġ�� particle ����
    {
        if (nowBullet >= 0)
        {
            playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Gun_Shot]);
            GameObject impact;
            nowBullet -= 1;
            StartCoroutine(CameraShake());
            if (Physics.Raycast(Ray(), out hit, maxDistance, monsterLayer))
            {
                impact = Instantiate(impactEffect[2], hit.point, Quaternion.LookRotation(hit.normal));
                impact.transform.parent = prefabManager.transform;
            }
            if (Physics.Raycast(Ray(), out hit, maxDistance, groundLayer))
            {
                impact = Instantiate(impactEffect[1], hit.point, Quaternion.LookRotation(hit.normal));
                impact.transform.parent = prefabManager.transform;
            }

            // ź�� ���� ����
            GameObject instanceBullet = Instantiate(bulletPrefab, bulletCasePrefab.transform.position, bulletCasePrefab.transform.rotation);
            instanceBullet.transform.parent = prefabManager.transform;
            Rigidbody bulletRigid = instanceBullet.GetComponent<Rigidbody>();
            Vector3 caseVec = bulletCasePrefab.transform.right * Random.Range(0.5f, 1f) + Vector3.up * Random.Range(0.2f, 0.5f);

            bulletRigid.AddForce(caseVec, ForceMode.Impulse);
            bulletRigid.AddTorque(Vector3.up * 20, ForceMode.Impulse);

            Destroy(instanceBullet, 2.0f);

        }

        bulletText.text = (nowBullet + 1) + " / " + (maxBullet);
    }

    // �̺�Ʈ ���� ����
    void ReloadSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Gun_Reload]);
    }

    void GunUnReloadSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Gun_UnReload]);
    }

    void MagazineLoadSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Gun_MagazineLoad]);
    }

    void DryFireSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Dry_Fire]);
    }
    
    void RunSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Run]);
    }

    void WalkSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Walk]);
    }

    void PickUpSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.PickUp]);
    }

    void BulletCaseSound()
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.BulletCase]);
    }

    void ParticleOn()
    {
        particle.GetComponent<ParticleSystem>().Play();
    }
}
