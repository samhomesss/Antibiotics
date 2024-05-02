using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : MonoBehaviour
{
    [Header("플레이어와 상호작용 되는 적 정보")]
    public int zombieHp = 100; // 좀비체력
    public LayerMask isPlayer; // 플레이어 레이어
    public float signalRange; // 범위
    public float attackRange; // 범위
    bool isPlayerInRange; // 플레이어가 범위 안에 들어왔는가에 대한 판정
    bool isPlayerAttack; // 플레이어가 공격범위 안에 들어 왔는가?
    Transform target; // 플레이어 위치

    bool isLookTarget; //  타겟을 찾았을때 
    
    [Header("좌표 혹은 AI 관련")]
    public Transform patrolingPos; // 순찰 포지션
    NavMeshAgent agent;
    Vector3 newPatrolPos;
    
    [Header("적 애니메이션")]
    Animator enemyAni;
    bool isAttack = false;

    // 적을 움직이게 하는 트리거 체크 
    [Header("적을 움직이게 하는 트리거")]
    public GameObject triggerPos; // trigger 위치 받아오고
    public bool isMoveTrigger; // 해당 트리거의 값이 무엇인지에 따라 이동
    float dir;
    int count = 0; // 처음 한번만 지정 방향으로 이동후 이후는 계속 플레이어 방향
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAni = GetComponent<Animator>();
        target = PlayerController.instance.transform;
        newPatrolPos = patrolingPos.position; // 처음 포지션을 정해주고 
        dir = Vector3.Distance(transform.position, newPatrolPos);
    }

    void Update()
    {
        if (dir < 1 && count == 0)
        {
            newPatrolPos = patrolingPos.position;
        }
        else
        {
            newPatrolPos = new Vector3(target.position.x, target.position.y, target.position.z);
        }
        isMoveTrigger = triggerPos.GetComponent<EnemyMoveTrigger>().isEnemyMove;

        if (!isMoveTrigger) return;

        isPlayerInRange = Physics.CheckSphere(transform.position, signalRange, isPlayer);
        isPlayerAttack = Physics.CheckSphere(transform.position, attackRange, isPlayer);

        if (!isPlayerInRange) // 플레이어 찾는중
        {
            isLookTarget = false;
            // 순찰중
            Patroling();
        }
        else
        {
            if (!isPlayerAttack)  // 뛰어서 따라가는거 
            {
                TargetMove();
            }
            else
            {
               if (!isAttack)
                StartCoroutine(AttackAni());
            }
        }

        if (zombieHp <= 0)
        {
            StartCoroutine(DeadAni());
        }
       
    }

    IEnumerator DeadAni()
    {
        enemyAni.SetBool("IsDead", true);
        yield return new WaitForSeconds(1.2f);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    void Patroling()
    {
        enemyAni.SetFloat("Speed", agent.speed);
        transform.LookAt(transform.position);

        if (dir < 1) //만약 첫 패트롤링 지역으로 간다면
        {
            agent.speed = 0;
            count++;
           // float x = Random.Range(-10, 10); // 특정 범위를 지정
           // float z = Random.Range(-10, 10); // 특정 범위를 지정

           // newPatrolPos = new Vector3(target.position.x, target.position.y, target.position.z); // 플레이어 위치로 다시 이동
        }
        agent.speed = 4f;

        agent.SetDestination(newPatrolPos);
    }

    void TargetMove()
    {
        isLookTarget = true;
        enemyAni.SetBool("IsRun", isLookTarget);
        // 플레이어 추적
        agent.speed = 5.0f; // navmashagent 안에 들어 있는 Speed값을 설정
                            //LookAt 함수 -> 안에 적은 매개변수의 방향으로 바라보게 하는것
        transform.LookAt(transform.position); // 계속 앞을 바라봐야하니까?
                                              // 해당 위치로 이동시키는거 
        agent.SetDestination(target.position);
    }

    IEnumerator AttackAni()
    {
        isLookTarget = false;
        
        agent.speed = 0.0f;
        enemyAni.SetTrigger("IsAttack");
        isAttack = true;
       
        yield return new WaitForSeconds(1f);
        agent.speed = 5.0f;
        isAttack = false;
    }


    // Gizmo 그려주는 함수
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // 기즈모 색깔
        Gizmos.DrawSphere(transform.position, signalRange);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}
