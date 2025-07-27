using UnityEngine;
using UnityEngine.AI;

public class DollMonsterAI : MonoBehaviour
{
    public float detectionRange = 15f;
    public float moveSpeed = 2.2f;
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    public float attackCooldown = 2f;

    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private Animator animator;

    private PlayerController playerController;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (playerController == null) return;

        bool isPlayerCrouching = Input.GetKey(playerController.crouchKey);  // 직접 키 입력 확인

        if (distance <= detectionRange && !isPlayerCrouching)
        {
            agent.SetDestination(player.position);
            animator?.SetBool("isMoving", true);

            if (distance <= attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            agent.ResetPath();
            animator?.SetBool("isMoving", false);
        }
    }

    void AttackPlayer()
    {
        Debug.Log("인형탈이 플레이어를 공격!");
        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
        }
        animator?.SetTrigger("attack");
    }
}
