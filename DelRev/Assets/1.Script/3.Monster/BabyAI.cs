using UnityEngine;
using UnityEngine.AI;

public class BabyAI : MonoBehaviour
{
    public float moveRadius = 3f;  // 아기가 돌아다닐 반경
    private NavMeshAgent agent;
    private Vector3 origin;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // ← 자동 회전 비활성화
        origin = transform.position;
        GoToRandomPosition();
        InvokeRepeating("GoToRandomPosition", 3f, 5f);  // 3초 후부터 5초마다 반복
    }

    
    void Update()
    {
        if (agent.velocity.sqrMagnitude > 0.1f) // 이동 중일 때만 회전
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void GoToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
        randomDirection += origin;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, moveRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
