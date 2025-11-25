using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class DroneAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("총구 Transform (Z+가 발사 방향)")]
    public Transform muzzle;
    [Tooltip("트레이서용 LineRenderer (PositionCount=2, UseWorldSpace=ON 추천)")]
    public LineRenderer tracer;
    [Tooltip("총구 파티클 (선택)")]
    public ParticleSystem muzzleFlash;
    [Tooltip("임팩트 파티클 프리팹 (월드 공간 재생 추천)")]
    public ParticleSystem impactEffectPrefab;
    [Tooltip("기관총 사운드 (loop=ON)")]
    public AudioSource gunAudio;
    [Tooltip("시야 검사용 시작점(선택). 비워두면 드론 Transform 사용")]
    public Transform eye;

    [Header("Detection (정면 전용)")]
    [Tooltip("감지/사격 가능 거리")]
    public float detectRange = 25f;
    [Tooltip("정면 시야각(도). 이 각도 안에 플레이어가 있을 때만 교전")]
    public float fovAngle = 60f;
    [Tooltip("시야 차단용 레이어 마스크(벽/지형 등)")]
    public LayerMask obstacleMask = ~0;
    [Tooltip("피격 판정용 레이어(플레이어 포함 필수)")]
    public LayerMask hittableMask = ~0;

    [Header("Gun")]
    [Tooltip("초당 대미지(DPS)")]
    public float damagePerSecond = 15f;
    [Tooltip("초당 판정 횟수(샘플링). 높을수록 촘촘한 명중/트레이서")]
    public float fireSamplesPerSecond = 12f;
    [Tooltip("트레이서 유지 시간(초)")]
    public float tracerDuration = 0.04f;
    [Tooltip("정면 사격에도 약간의 탄 퍼짐을 줄지(도)")]
    public float aimSpread = 0.5f;

    [Header("Behaviour")]
    [Tooltip("시야를 잃은 뒤 사격 지속 시간(초)")]
    public float loseSightTime = 0.5f;
    public bool drawDebug = false;

    private Transform _player;
    private float _lastSeenTime = -999f;
    private Coroutine _fireLoopCo;
    private ParticleSystem _pooledImpact;

    private void Awake()
    {
        if (tracer != null)
        {
            tracer.positionCount = 2;
            tracer.enabled = false;
            // Inspector 권장: Use World Space=ON, Width=0.02~0.05, Unlit/Additive 머티리얼
        }

        if (impactEffectPrefab != null)
        {
            _pooledImpact = Instantiate(impactEffectPrefab);
            _pooledImpact.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (gunAudio != null) gunAudio.loop = true;
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
            _player = PlayerController.Instance.transform;
        else
        {
            var pc = FindObjectOfType<PlayerController>();
            _player = pc ? pc.transform : null;
        }
    }

    private void OnDisable()
    {
        if (_fireLoopCo != null) StopCoroutine(_fireLoopCo);
        _fireLoopCo = null;
        if (gunAudio != null) gunAudio.Stop();
        if (tracer != null) tracer.enabled = false;
        if (muzzleFlash != null) muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void Update()
    {
        if (_player == null) return;

        bool canEngage = PlayerInFrontFOVAndVisible();
        if (canEngage) _lastSeenTime = Time.time;

        bool shouldFire = Time.time - _lastSeenTime <= loseSightTime;

        // 총구는 플레이어 쪽으로 회전하지 않습니다(정면 고정).
        // 드론의 정면은 이동 스크립트(DronePatrol)가 결정한다고 가정.

        if (shouldFire && _fireLoopCo == null)
        {
            _fireLoopCo = StartCoroutine(FireLoop());
            if (gunAudio != null) gunAudio.Play();
            if (muzzleFlash != null) muzzleFlash.Play();
        }
        else if (!shouldFire && _fireLoopCo != null)
        {
            StopCoroutine(_fireLoopCo);
            _fireLoopCo = null;
            if (gunAudio != null) gunAudio.Stop();
            if (tracer != null) tracer.enabled = false;
            if (muzzleFlash != null) muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    // ========= 정면 FOV + 가시성 체크 =========
    bool PlayerInFrontFOVAndVisible()
    {
        if (_player == null) return false;

        Vector3 origin = eye ? eye.position : transform.position;
        Vector3 forward = muzzle ? muzzle.forward : transform.forward;
        Vector3 toPlayer = _player.position + Vector3.up * 0.9f - origin; // 상체 높이 기준
        float dist = toPlayer.magnitude;
        if (dist > detectRange) return false;

        // 정면 시야각 안에 있는지
        float angle = Vector3.Angle(forward, toPlayer);
        if (angle > fovAngle * 0.5f) return false;

        // 시야 차단 체크(플레이어 쪽으로)
        if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit blockHit, detectRange, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (!IsHitBelongsToPlayer(blockHit.collider.transform))
                return false;
        }

        if (drawDebug)
        {
            Debug.DrawRay(origin, forward * Mathf.Min(dist, 2f), Color.cyan, 0.05f); // 정면
            Debug.DrawLine(origin, _player.position + Vector3.up * 0.9f, Color.yellow, 0.05f); // 시야
        }
        return true;
    }

    // ========= 사격 루프 =========
    IEnumerator FireLoop()
    {
        float interval = 1f / Mathf.Max(1f, fireSamplesPerSecond);

        while (true)
        {
        Vector3 start = muzzle ? muzzle.position : transform.position;

        // 플레이어 조준점으로 방향 설정
        Vector3 aimPoint = GetAimPoint(); // (카메라 있으면 머리, 없으면 상체)
        Vector3 dir = (aimPoint - start).normalized;

        // 약간의 퍼짐
        if (aimSpread > 0f)
        {
            dir = Quaternion.Euler(
                Random.Range(-aimSpread, aimSpread),
                Random.Range(-aimSpread, aimSpread),
                0f) * dir;
        }

            if (drawDebug)
                Debug.DrawRay(start, dir * detectRange, Color.red, 0.05f);

            Vector3 end = start + dir * detectRange;

            if (Physics.Raycast(start, dir, out RaycastHit hit, detectRange, hittableMask, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;

                if (IsHitBelongsToPlayer(hit.collider.transform) && PlayerController.Instance != null)
                {
                    float dmg = damagePerSecond * interval;
                    PlayerController.Instance.TakeDamage(dmg);
                }

                if (_pooledImpact != null)
                {
                    _pooledImpact.transform.position = end;
                    _pooledImpact.transform.rotation = Quaternion.LookRotation(hit.normal);
                    _pooledImpact.Play();
                }
            }

            if (tracer != null)
            {
                tracer.SetPosition(0, start);
                tracer.SetPosition(1, end);
                tracer.enabled = true;
                yield return new WaitForSeconds(tracerDuration);
                tracer.enabled = false;
            }

            yield return new WaitForSeconds(Mathf.Max(0f, interval - tracerDuration));
        }
    }

    // ========= 유틸 =========
    bool IsHitBelongsToPlayer(Transform hitTransform)
    {
        if (_player == null || hitTransform == null) return false;
        if (hitTransform.CompareTag("Player")) return true;
        if (hitTransform.GetComponentInParent<PlayerController>() != null) return true;
        if (hitTransform == _player || hitTransform.IsChildOf(_player)) return true;
        if (hitTransform.root.GetComponent<PlayerController>() != null) return true;
        return false;
    }

    Vector3 GetAimPoint()
    {
        if (_player == null)
            return transform.position + transform.forward * 10f; // fallback

        // PlayerController의 카메라 Transform 우선
        var pc = PlayerController.Instance;
        if (pc != null && pc.cameraTransform != null)
            return pc.cameraTransform.position;

        // 없으면 플레이어 중심 + 상체 높이
        return _player.position + Vector3.up * 0.9f;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawDebug) return;

        // 정면 FOV 시각화
        Vector3 origin = transform.position;
        Vector3 fwd = (muzzle ? muzzle.forward : transform.forward);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, detectRange);

        if (fovAngle < 360f)
        {
            Quaternion left = Quaternion.AngleAxis(-fovAngle * 0.5f, Vector3.up);
            Quaternion right = Quaternion.AngleAxis(fovAngle * 0.5f, Vector3.up);
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(origin, left * fwd * detectRange);
            Gizmos.DrawRay(origin, right * fwd * detectRange);
        }
    }
#endif
}

