using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class GuardRobot : MonoBehaviour
{
    [Header("Refs")]
    public Transform eye;
    public LayerMask obstructionMask;
    NavMeshAgent _agent;
    AudioSource _audio;  // 🔊 추가됨

    Transform _player;
    PlayerController _playerCtrl;

    [Header("Movement")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 3f;
    public float stopDistance = 2f;

    [Header("Patrol Area")]
    public float patrolRadius = 6f;
    public float waypointTolerance = 0.5f;
    public int patrolSampleTries = 12;

    [Header("Detection")]
    public float detectRadius = 6f;
    [Range(0f, 360f)] public float detectAngle = 110f;

    [Header("Combat")]
    public float attackRange = 1.6f;
    public float attackDamage = 20f;
    public float attackInterval = 1.0f;

    [Header("Combat Audio")]
    public AudioClip attackSfx;        // 🔊 공격 소리
    [Range(0f, 1f)] public float attackVolume = 0.8f;

    [Header("Return / Idle")]
    public Transform homePoint;

    enum State { Idle, MovingToTurret, Patrolling, Chasing }
    State _state = State.Idle;

    Vector3 _spawnPos;
    Vector3 _patrolCenter;
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
        _audio = GetComponent<AudioSource>();  // 🔊 초기화
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
                if (!IsMoving()) MoveTo(Home());
                break;

            case State.MovingToTurret:
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

    // ====== 시야 / 공격 ======

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

            // 🔥 플레이어 체력 감소
            _playerCtrl.TakeDamage(attackDamage);

            // 🔊 공격음 재생
            if (attackSfx != null)
                _audio.PlayOneShot(attackSfx, attackVolume);
        }
    }

    void SetNextPatrolPoint()
    {
        Vector3 center = (_state == State.Patrolling || _state == State.MovingToTurret) ? _patrolCenter : Home();
        if (TryGetRandomPointOnNavmesh(center, patrolRadius, out Vector3 point))
        {
            _currentPatrolTarget = point;
            MoveTo(point);
        }
        else
        {
            MoveTo(Home());
        }
    }

    Vector3 Home() => homePoint ? homePoint.position : _spawnPos;

    void MoveTo(Vector3 pos)
    {
        _agent.isStopped = false;
        _agent.SetDestination(pos);
    }

    bool IsMoving() => _agent.pathPending || _agent.remainingDistance > stopDistance;

    bool TryGetRandomPointOnNavmesh(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < patrolSampleTries; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * radius;
            Vector3 sample = new Vector3(center.x + rnd.x, center.y + 2f, center.z + rnd.y);

            if (Physics.Raycast(sample, Vector3.down, out RaycastHit hit, 10f))
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
