using UnityEngine;
using UnityEngine.AI;

public class SmartKidAI : MonoBehaviour
{
    public float roamRadius = 10f;
    private NavMeshAgent agent;
    private bool isPlayerLocked = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Roam();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isPlayerLocked)
        {
            Roam();
        }
    }

    void Roam()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerLocked)
        {
            isPlayerLocked = true;
            agent.isStopped = true;
            ProblemManager.Instance.StartProblem(other.gameObject);
        }
    }

    public void ReleasePlayer()
    {
        isPlayerLocked = false;
        agent.isStopped = false;
        Roam();
    }
}
