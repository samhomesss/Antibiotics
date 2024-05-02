using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("플레이어 이동 관련 ")]
    public GameObject groundPos;
    public LayerMask groundLayer;
    CharacterController playerController;
    int playerSpeed = 10;
    int originPlayerSpeed = 10;
    bool isGround = false;
    float gravity = 9.8f;
    Vector3 velocity = Vector3.zero;

    [Header("플레이어 시점 관련")]
    public float mouseSpeed = 100f;
    float xRotation = 0; 
    float yRotation = 0;

    [Header("플레이어 애니메이션")]
    Animator playerAni;
    bool isRun = false;
    public bool isShot = false;

    [Header("플레이어 조준점")]
    public GameObject crosshair;
    [Range(30, 50)]
    public float maxCross = 30.0f;

    [Header("플레이어 Shot 관련")]
    public float maxDistance = 300f;
    RaycastHit hit;
    public LayerMask monsterLayer;
    public GameObject[] impactEffect;

    [Header("플레이어 총알 UI , 총알갯수")]
    public Text bulletText;
    public int maxBullet = 0; // 총 총알 갯수
    public int nowBullet = 0; // 현재 총안에 들어가 있는 총알 갯수
    int reloadBullet = 100; // 장탄 최대수 
    int semiReload;
    bool isReload = false;

    [Header("탄피 관련")]
    public GameObject bulletCasePrefab;
    public GameObject bulletPrefab;

    [Header("아이템 관련")]
    public LayerMask itemLayer;
    public float itemDistance = 10;
    public GameObject itemGetUI;
    bool getUI = false;
    bool isGrab = false;
    GameObject lookItem;

    [Header("카메라 관련")]
    public float shakePower = 0.01f; // 흔드는 정도
    float shakeTime = 0.1f;

    [Header("생성 오브젝트 관리")]
    public GameObject prefabManager;

    [Header("Sound 관련")]
    AudioSource playerAudio;
    AudioSource playerDamagedAudio;
    AudioSource enemyAudio;

    [Header("총 파츠 부분")]
    public GameObject[] gunParts; // 총 파츠 무기 주울때 꺼야해서 들고오는거

    [Header("플레이어 HP 관련")]
    public Slider HpUI; // slider 가져오고
    public float maxHP = 100; // 최대 체력 나중에 넣어주고
    public float Hp = 100; // 현재 체력
    public GameObject getDamaged;

    [Header("문 상호작용")]
    public LayerMask doorLayer;
    public LayerMask lockDoorLayer; // 잠긴문
    public bool isPlayerIn = false; // false가 문 바깥쪽에 있는거 true 가 문 안쪽에 있는거.
    public Text uiText; // E 클릭 UI
    public bool isKeyHave = false; // 열쇠를 가지고 있는가?
    bool isDoor = false;
   
    [Header("라이트 관련")]
    public GameObject flashLight;
    public bool isLightHave = false;
    bool isPress = false; // 빛 켜지는거

    [Header("파티클 총")]
    public GameObject particle;

    [Header("Quest Item이 활성화 중일때")]
    bool isQuest = false;

    [Header("SmartPhone UI")]
    public GameObject SmartPhoneUi; // 스마트폰 UI
    public bool isSmartPhone = false; // 핸드폰이 있는가 
    public bool isGameClear = false;
    public Slider backUp; // 백업 로딩 길이

    [Header("Book UI")]
    public GameObject bookUi;
    public bool isBook;

    [Header("Quest 창이 닫히면")]
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

        bulletText.text = nowBullet + " / " + maxBullet; // 이건 그냥 시작할때 초기화
    }

    void Update()
    {
        // 추후 아이템에 넣어줘야할듯
        HaveBook(); // 책을 가졌을때 
        HaveSmartPhone(); // 핸드폰을 가졌을때

        HpUI.value = Hp;

        if (!isQuest && !isQuestOff) 
        {
            ItemEffect(); // 아이템 관련
            PlayerMove(); // 플레이어 움직임 + 애니메이션 (특정 키) 
            MoveLook(); // 플레이어 시점
            ShotBullet(); // 총쏘는거 관련
        }
    }

    

    void PlayerMove()
    {
        // 이동관련 
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 newPos = transform.forward * moveZ + transform.right * moveX;

        playerController.Move(newPos * playerSpeed * Time.deltaTime);

        // 애니메이션
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
                    nowBullet--; // 총알 갯수 안맞아서 임의조정
                }
                else
                    StartCoroutine(IsEmptyAni());
            }
        }

        // 라이트 키는거 
        if (Input.GetKeyDown(KeyCode.T) && isLightHave)
        {
            isPress = !isPress;
            flashLight.SetActive(isPress);
        }


        // 무기 스왑 방식 

        // 땅판정
        isGround = Physics.CheckSphere(groundPos.transform.position, 0.1f, groundLayer); // checksphere

        if (!isGround)
        {
            // Time.deltatime을 두번 사용하는 이유에 대해서
            velocity.y -= gravity * Time.deltaTime; 
            playerController.Move(velocity * Time.deltaTime);
        }

        #region
        //Vector3 inputVec;
        //inputVec.x = Input.GetAxis("Horizontal");
        //inputVec.z = Input.GetAxis("Vertical");
        //// Vector3 값으로 할경우 화면에 따른 전환이 안됨
        //// forward의 값이랑 right의 값을 이용해서 플레이어 앞 부분을 정하는듯 
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
        // 타겟 판정 확인 
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * maxDistance, Color.red);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            isShot = true;
            // 총알 다 썻을때 리로드 관련
            if (nowBullet == 0)
            {
                isReload = true;
                bulletText.text = nowBullet + " / " + maxBullet; // 따로 넣은 이유는 0이 되었을때 딱 0에 멈출려고 
                StartCoroutine(ReloadAni());
            }

            if (!isReload && nowBullet != 0) // 재장전 중이 아니고 현재 총알이 총안에 들어가 있을때
            {
                // 애니메이션 
                playerAni.SetTrigger("Shot");
                // 적용 레이어
                if (Physics.Raycast(Ray(), out hit, maxDistance, monsterLayer))
                {
                    hit.collider.gameObject.GetComponent<Enemy>().zombieHp -= 25;
                    enemyAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Enemy_Pain]);
                    // 추후에 몬스터 데미지 들어가게 만들면 됨
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

    void ItemEffect() // 문 , 아이템 먹었을 수 있는 판정 && UI 출력 && 애니플레이
    {
        if (Physics.Raycast(Ray(), out hit, itemDistance, itemLayer) || Physics.Raycast(Ray(), out hit, itemDistance, doorLayer))
        {
            getUI = true;
            lookItem = hit.collider.gameObject; // 바로 주우면 고개를 돌렸을때 NullReference가 떠서 한번 저장해줌
            uiText.text = "아이템 획득";
            if (Physics.Raycast(Ray(), out hit, itemDistance, doorLayer))
            {
                uiText.text = "문을 연다 / 닫는다";
                isDoor = hit.collider.GetComponentInParent<DoorManager>().isDoor;
            }
        }
        else if (Physics.Raycast(Ray(), out hit, itemDistance, lockDoorLayer))
        {
            getUI = true;
            lookItem = hit.collider.gameObject;

            if (isKeyHave)
            {
                uiText.text = "키를 사용한다";
                isDoor = hit.collider.GetComponentInParent<DoorManager>().isDoor;
            }
            else
            {
                uiText.text = "키가 없어 문을 열수 없다";
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

    void PlayerHpUp(float effect) // 플레이어 체력 회복
    {
        Hp += effect; // HPUI.value에서 변경
    }

    void BulletUP(int bullet) // 플레이어 체력 증가
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
    } // 책을 열었는가?

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
    } // 잠긴 문 열기

    IEnumerator DoorOpen() // 픽업 애니메이션
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

    IEnumerator PickUpAni() // 픽업 애니메이션
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
            case ItemType.Heal: // 회복 관련 아이템이라면
                PlayerHpUp(lookItem.GetComponent<Item>().Effect());
                break;

            case ItemType.Bullet: // 총알 관련 아이템이라면
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
                        // 어딘가의 열쇠다 라는 UI 출력
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
        isGrab = false; // 두번 안줍게 변수 선언
        for (int i = 0; i < gunParts.Length; i++)
        {
            gunParts[i].SetActive(true);
        }
    }

    IEnumerator ReloadAni() // 총알 리로드 관련
    {
        if (maxBullet == 0 && nowBullet == 0)
        {
            StartCoroutine(IsEmptyAni()); // 총알이 없는 애니 출력 
        }
        else
        {
            playerAni.SetTrigger("IsReload"); // 위의 경우가 아닌경우 리로드 애니메이션 

            if (nowBullet == 0) // 현재 총에 총알이 없을때
            {
                if (maxBullet >= reloadBullet) // 총탄이 20 발 보다 많다면 
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
            else // nowBullet != 0 현재 총안에 총이 들어가 있을때 
            {
                if (maxBullet > reloadBullet)
                {
                    semiReload = reloadBullet - nowBullet; // 장탄수에서 없는 장탄수 빼서 계산
                    nowBullet = reloadBullet; // 20발 장전
                    maxBullet -= semiReload; // 빠진거 만큼 계산
                }
                else // 총알이 20발 아래라면
                {
                    semiReload = reloadBullet - nowBullet; // 지금 가지고 있는 총탄의 수가 3이라고 가정 20 - 3 = 17
                    if (semiReload >= maxBullet) // 장전해야 하는 총탄 수보다 내가 가지고 있는 탄의 수가 적다면 
                    {
                        semiReload = maxBullet; // 장전해야 하는 총탄수의 갯수를 maxbullet으로 변환
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

        // 총을 다사용하면 다시 총알이 올라가는 버그가 있어서 이렇게 적음 이유 모름
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

    IEnumerator IsEmptyAni() // 총알 없을때 애니
    {
        playerAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.Dry_Fire]);

        playerAni.SetBool("IsEmpty", true);
        yield return new WaitForSeconds(2.0f);
        playerAni.SetBool("IsEmpty", false);
    }

    Ray Ray() // 자주쓰는 레이값 그냥 함수로 정의
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        return ray;
    }

    IEnumerator CameraShake() // 카메라 흔드는거
    {
        //그냥 코루틴으로 하면 안되는 이유 흔들리지 않음 흔들리는데 좀 어색함
        moveCamera = new Vector3(-0.093f, 1.63f, -0.119f); // 카메라의 로컬 포지션 가져와야 됨 아니면 플레이어가 따라서 움직여 버림
        float camerashakeTime = 0;

        while (camerashakeTime < shakeTime)
        {
            float x = Random.Range(-1f, 1f) * shakePower;
            float y = Random.Range(-1f, 1f) * shakePower;
           // z축을 같이 안움직이는게 덜 어색한거 같아서 뺌
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
        // 문 여는거 판정
        if (other.CompareTag("InTrigger"))
        {
            isPlayerIn = true;
        }
        if (other.CompareTag("OutTrigger"))
        {
            isPlayerIn = false;
        }

        // 맞은 판정
        if (other.CompareTag("Attack"))
        {
            Debug.Log("맞았습니다.");
            playerDamagedAudio.PlayOneShot(SoundManager.instance.soundEffect[(int)SoundName.BodyDamaged]);
            StartCoroutine(GetDamaged());
            Hp -= 10;
        }
    } 

    /// <summary>
    /// 이벤트 함수 모음
    /// </summary>
    // 이벤트 총알 함수
    // 탄피 배출 + 총알

    // 버튼 이벤트
    public void ExitButton() // 책에서 나가는 버튼
    {
        isQuest = false;
        isBook = false;
        bookUi.SetActive(false);
    }
    void BulletEffect() // 애니메이션 이벤트 함수로 적용 그 위치에 particle 생성
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

            // 탄피 배출 관련
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

    // 이벤트 사운드 모음
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
