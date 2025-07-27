using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Principal : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;

    private enum State { Patrol, Alert, FinalBlocker }
    private State currentState = State.Patrol;

    [Header("Player Detection")]
    public Transform player;
    public float attackRange = 1.5f;
    public float attackDamage = 40f;
    public float attackInterval = 1f;
    private float attackTimer = 0f;

    [Header("Final Blocker Mode")]
    public float dangerSpeed = 5f;
    public float normalSpeed = 1f;
    public float dangerAttack = 150f;
    private bool isFinalBlocker = false;

    [Header("Rotation")]
    public float rotationSpeed = 5f;

    [Header("Audio")]
    public AudioSource alertSound;
    public AudioSource attackSound;

    private Vector3 alertTarget;

    private PlayerController playerController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        GoToNextPoint();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }
    }

    void Update()
    {
        RotateInPlace();

        switch (currentState)
        {
            case State.Patrol:
                if (!agent.pathPending && agent.remainingDistance < 0.3f)
                    GoToNextPoint();
                break;

            case State.Alert:
                agent.SetDestination(alertTarget);
                if (Vector3.Distance(transform.position, alertTarget) < 0.5f)
                    currentState = State.Patrol;
                break;

            case State.FinalBlocker:
                if (player != null)
                    agent.SetDestination(player.position);
                break;
        }

        // 공격 처리
        if (player != null && Vector3.Distance(transform.position, player.position) < attackRange)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                if (playerController != null)
                    playerController.health -= attackDamage;

                attackSound?.Play();
            }
        }
        else
        {
            attackTimer = 0f;
        }
    }

    private void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // 회전만 수행 (한 방향)
    void RotateInPlace()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    // 충돌 or CCTV 감지 시 호출
    public void AlertToPosition(Vector3 position)
    {
        if (currentState == State.FinalBlocker) return;

        alertTarget = position;
        currentState = State.Alert;
        alertSound?.Play();
    }

    // 위험 게이지 최대치 도달 시 호출
    public void OnDangerGaugeMaxed()
    {
        isFinalBlocker = true;
        currentState = State.FinalBlocker;
        agent.speed = dangerSpeed;
        attackDamage = dangerAttack;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AlertToPosition(other.transform.position);
        }
    }

    // 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
