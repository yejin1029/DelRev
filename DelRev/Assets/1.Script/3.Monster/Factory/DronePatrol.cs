using UnityEngine;

[DisallowMultipleComponent]
public class DronePatrol : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("월드 좌표로 입력되는 경유지들입니다. 드론은 0→1→...→N→...→1→0 식으로 왕복합니다.")]
    public Vector3[] waypoints;

    [Header("Movement")]
    public float moveSpeed = 3f;
    [Tooltip("목표 지점에 도착했다고 간주할 거리(허용 오차)")]
    public float arriveThreshold = 0.05f;
    [Tooltip("회전 보간 속도 (목표 방향으로 고개 돌림)")]
    public float turnLerp = 10f;

    private int _index = 0;
    private int _dir = 1; // +1 forward, -1 backward

    private void Reset()
    {
        waypoints = new Vector3[2] { transform.position, transform.position + transform.forward * 5f };
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 target = waypoints[_index];
        Vector3 to = target - transform.position;
        float dist = to.magnitude;

        if (dist <= arriveThreshold)
        {
            // 다음 인덱스로 진행(끝에서 방향 반전)
            _index += _dir;
            if (_index >= waypoints.Length)
            {
                _index = waypoints.Length - 2;
                _dir = -1;
            }
            else if (_index < 0)
            {
                _index = 1;
                _dir = 1;
            }
            return;
        }

        // 전진
        Vector3 move = to.normalized * moveSpeed * Time.deltaTime;
        if (move.magnitude > dist) move = to; // overshoot 방지
        transform.position += move;

        // 진행 방향으로 부드럽게 회전
        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(move.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnLerp);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawSphere(waypoints[i], 0.15f);
            if (i < waypoints.Length - 1)
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
        }
    }
#endif
}
