using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardRobot : MonoBehaviour
{
    [Header("Refs")]
    public Transform eye;
    public LayerMask obstructionMask;
    NavMeshAgent _agent;

    Transform _player;
    PlayerController _playerCtrl;

    [Header("Movement")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 3f;
    public float stopDistance = 2f;

    [Header("Patrol Area")]
    [Tooltip("포탑 위치를 중심으로 순찰할 반경")]
    public float patrolRadius = 6f;
    [Tooltip("순찰 지점에 거의 도달했다고 보려는 거리")]
    public float waypointTolerance = 0.5f;
    [Tooltip("새 웨이포인트를 시도할 최대 반복(내비 샘플 실패 대비)")]
    public int patrolSampleTries = 12;

    [Header("Detection (자체 시야)")]
    public float detectRadius = 6f;
    [Range(0f, 360f)] public float detectAngle = 110f;

    [Header("Combat")]
    public float attackRange = 1.6f;
    public float attackDamage = 20f;
    public float attackInterval = 1.0f;

    [Header("Return / Idle")]
    public Transform homePoint;  // 호출 전 대기 위치(없으면 시작 위치)

    enum State { Idle, MovingToTurret, Patrolling, Chasing }
    State _state = State.Idle;

    Vector3 _spawnPos;
    Vector3 _patrolCenter;   // 포탑 위치
    Vector3 _currentPatrolTarget;
    float _nextAttackTime;

    void OnEnable()
    {
        TurretSentinel.GlobalAlertTurretPos += OnTurretAlert;
    }
    void OnDisable()
    {
        TurretSentinel.GlobalAlertTurretPos -= OnTurretAlert;
    }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _spawnPos = transform.position;
    }

    void Start()
    {
        if (PlayerController.Instance != null)
        {
            _playerCtrl = PlayerController.Instance;
            _player = _playerCtrl.transform;
        }
        else
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) { _playerCtrl = pc; _player = pc.transform; }
        }

        _agent.stoppingDistance = Mathf.Min(attackRange * 0.8f, 1.0f);
        ToIdle();
    }

    void Update()
    {
        switch (_state)
        {
            case State.Idle:
                // 호출 없이는 공격/추적하지 않음
                if (!IsMoving()) MoveTo(Home());
                break;

            case State.MovingToTurret:
                // 도착하면 순찰 시작
                if (!IsMoving()) ToPatrol();
                TryDetectAndChase();
                break;

            case State.Patrolling:
                if (!IsMoving() || Vector3.Distance(transform.position, _currentPatrolTarget) <= waypointTolerance)
                    SetNextPatrolPoint();
                TryDetectAndChase();
                break;

            case State.Chasing:
                if (_player != null)
                {
                    _agent.speed = chaseSpeed;
                    _agent.isStopped = false;
                    _agent.SetDestination(_player.position);

                    float dist = Vector3.Distance(transform.position, _player.position);
                    if (dist <= attackRange)
                        TryAttack();

                    // 플레이어를 잃어버리면(가림막/각도/거리) 순찰로 복귀
                    if (!CanSeePlayer())
                        ToPatrol();
                }
                else
                {
                    ToPatrol();
                }
                break;
        }
    }

    // ====== 상태 전이 ======
    void OnTurretAlert(Vector3 turretPos)
    {
        _patrolCenter = turretPos;
        _agent.speed = patrolSpeed;
        _agent.isStopped = false;
        _state = State.MovingToTurret;
        MoveTo(_patrolCenter);
    }

    void ToIdle()
    {
        _agent.speed = patrolSpeed;
        _state = State.Idle;
        MoveTo(Home());
    }

    void ToPatrol()
    {
        _agent.speed = patrolSpeed;
        _state = State.Patrolling;
        SetNextPatrolPoint();
    }

    void ToChase()
    {
        _state = State.Chasing;
        _agent.speed = chaseSpeed;
    }

    // ====== 동작 유틸 ======
    void TryDetectAndChase()
    {
        if (_player == null) return;
        if (CanSeePlayer())
            ToChase();
    }

    bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 origin = (eye ? eye.position : transform.position);
        Vector3 toPlayer = _player.position - origin;
        float dist = toPlayer.magnitude;
        if (dist > detectRadius) return false;

        Vector3 dir = toPlayer.normalized;
        float angle = Vector3.Angle((eye ? eye.forward : transform.forward), dir);
        if (angle > detectAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, detectRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == _player || hit.transform.IsChildOf(_player))
                return true;

            if (((1 << hit.collider.gameObject.layer) & obstructionMask) != 0)
                return false;

            return false;
        }
        return false;
    }

    void TryAttack()
    {
        if (_playerCtrl == null) return;

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + attackInterval;
            _playerCtrl.TakeDamage(attackDamage); // 플레이어 체력 감소
        }
    }

    void SetNextPatrolPoint()
    {
        // 포탑 알림을 받은 뒤에만 순찰 중심 사용
        Vector3 center = (_state == State.Patrolling || _state == State.MovingToTurret) ? _patrolCenter : Home();
        if (TryGetRandomPointOnNavmesh(center, patrolRadius, out Vector3 point))
        {
            _currentPatrolTarget = point;
            MoveTo(point);
        }
        else
        {
            // 샘플 실패 시 홈으로 복귀
            MoveTo(Home());
        }
    }

    Vector3 Home()
    {
        return homePoint ? homePoint.position : _spawnPos;
    }

    void MoveTo(Vector3 pos)
    {
        _agent.isStopped = false;
        _agent.SetDestination(pos);
    }

    bool IsMoving()
    {
        return _agent.pathPending || _agent.remainingDistance > stopDistance;
    }

    bool TryGetRandomPointOnNavmesh(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < patrolSampleTries; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * radius;
            Vector3 sample = new Vector3(center.x + rnd.x, center.y + 2f, center.z + rnd.y); // 약간 위에서 드롭
            if (Physics.Raycast(sample, Vector3.down, out RaycastHit hit, 10f, ~0, QueryTriggerInteraction.Ignore))
            {
                Vector3 candidate = hit.point;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 1.5f, NavMesh.AllAreas))
                {
                    result = navHit.position;
                    return true;
                }
            }
        }
        result = center;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((_state == State.Patrolling) ? _patrolCenter : transform.position, patrolRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(eye ? eye.position : transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
