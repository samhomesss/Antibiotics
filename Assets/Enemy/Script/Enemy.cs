using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : MonoBehaviour
{
    [Header("�÷��̾�� ��ȣ�ۿ� �Ǵ� �� ����")]
    public int zombieHp = 100; // ����ü��
    public LayerMask isPlayer; // �÷��̾� ���̾�
    public float signalRange; // ����
    public float attackRange; // ����
    bool isPlayerInRange; // �÷��̾ ���� �ȿ� ���Դ°��� ���� ����
    bool isPlayerAttack; // �÷��̾ ���ݹ��� �ȿ� ��� �Դ°�?
    Transform target; // �÷��̾� ��ġ

    bool isLookTarget; //  Ÿ���� ã������ 
    
    [Header("��ǥ Ȥ�� AI ����")]
    public Transform patrolingPos; // ���� ������
    NavMeshAgent agent;
    Vector3 newPatrolPos;
    
    [Header("�� �ִϸ��̼�")]
    Animator enemyAni;
    bool isAttack = false;

    // ���� �����̰� �ϴ� Ʈ���� üũ 
    [Header("���� �����̰� �ϴ� Ʈ����")]
    public GameObject triggerPos; // trigger ��ġ �޾ƿ���
    public bool isMoveTrigger; // �ش� Ʈ������ ���� ���������� ���� �̵�
    float dir;
    int count = 0; // ó�� �ѹ��� ���� �������� �̵��� ���Ĵ� ��� �÷��̾� ����
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAni = GetComponent<Animator>();
        target = PlayerController.instance.transform;
        newPatrolPos = patrolingPos.position; // ó�� �������� �����ְ� 
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

        if (!isPlayerInRange) // �÷��̾� ã����
        {
            isLookTarget = false;
            // ������
            Patroling();
        }
        else
        {
            if (!isPlayerAttack)  // �پ ���󰡴°� 
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

        if (dir < 1) //���� ù ��Ʈ�Ѹ� �������� ���ٸ�
        {
            agent.speed = 0;
            count++;
           // float x = Random.Range(-10, 10); // Ư�� ������ ����
           // float z = Random.Range(-10, 10); // Ư�� ������ ����

           // newPatrolPos = new Vector3(target.position.x, target.position.y, target.position.z); // �÷��̾� ��ġ�� �ٽ� �̵�
        }
        agent.speed = 4f;

        agent.SetDestination(newPatrolPos);
    }

    void TargetMove()
    {
        isLookTarget = true;
        enemyAni.SetBool("IsRun", isLookTarget);
        // �÷��̾� ����
        agent.speed = 5.0f; // navmashagent �ȿ� ��� �ִ� Speed���� ����
                            //LookAt �Լ� -> �ȿ� ���� �Ű������� �������� �ٶ󺸰� �ϴ°�
        transform.LookAt(transform.position); // ��� ���� �ٶ�����ϴϱ�?
                                              // �ش� ��ġ�� �̵���Ű�°� 
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


    // Gizmo �׷��ִ� �Լ�
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // ����� ����
        Gizmos.DrawSphere(transform.position, signalRange);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}
