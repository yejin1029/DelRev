using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    private Transform playerTransform;
    private float moveSpeed = 0.5f;
    private float detectionRange = 5f;
    private NavMeshAgent agent;

    void Start()
    {
        // Get NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        // Find player with "Player" tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Calculate distance between monster and player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Check if player is within detection range and has line of sight
            if (distanceToPlayer <= detectionRange && HasLineOfSight())
            {
                // Set destination for NavMeshAgent
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                // Stop moving if player is out of range or sight
                agent.ResetPath();
            }
        }
    }

    bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // Check if anything blocks the line of sight
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}