using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TurretSentinel : MonoBehaviour
{
    // 포탑 위치를 전달하도록 이벤트 의미를 명확화
    public static event Action<Vector3> GlobalAlertTurretPos;

    [Header("Rotate")]
    public float yawDegreesPerSecond = 72f;  // 5초에 1바퀴

    [Header("FOV")]
    public float viewRadius = 8f;
    [Range(0f, 360f)] public float viewAngle = 120f;
    public Transform eye;
    public LayerMask obstructionMask;

    [Header("Detection")]
    public float requiredDetectTime = 0.5f;
    public float alertCooldown = 5f;

    [Header("Audio")]
    public AudioClip alertSfx;
    [Range(0f, 1f)] public float alertVolume = 0.8f;

    float _timer;
    float _lastAlertTime = -999f;
    AudioSource _audio;

    Transform _player;
    PlayerController _playerCtrl;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (eye == null) eye = transform;
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
    }

    void Update()
    {
        // 1) 회전
        transform.Rotate(Vector3.up, yawDegreesPerSecond * Time.deltaTime, Space.Self);

        // 2) 감시
        if (_player == null) return;

        bool inSight = IsPlayerInSight();

        if (inSight)
        {
            _timer += Time.deltaTime;

            if (_timer >= requiredDetectTime && Time.time - _lastAlertTime >= alertCooldown)
            {
                _lastAlertTime = Time.time;

                // 경고음
                if (alertSfx != null)
                    _audio.PlayOneShot(alertSfx, alertVolume);

                Debug.Log("포탑: 플레이어 감지 성공");

                // 포탑 '현재 위치'를 방송
                GlobalAlertTurretPos?.Invoke(transform.position);
            }
        }
        else
        {
            _timer = 0f;
        }
    }

    bool IsPlayerInSight()
    {
        Vector3 origin = eye.position;
        Vector3 toPlayer = (_player.position - origin);
        float dist = toPlayer.magnitude;
        if (dist > viewRadius) return false;

        Vector3 dir = toPlayer.normalized;
        float angle = Vector3.Angle(eye.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == _player || hit.transform.IsChildOf(_player))
                return true;

            if (((1 << hit.collider.gameObject.layer) & obstructionMask) != 0)
                return false;

            return false;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(eye ? eye.position : transform.position, viewRadius);

        Vector3 pos = eye ? eye.position : transform.position;
        Vector3 fwd = eye ? eye.forward : transform.forward;
        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * fwd;
        Vector3 right = Quaternion.Euler(0,  viewAngle * 0.5f, 0) * fwd;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + left * viewRadius);
        Gizmos.DrawLine(pos, pos + right * viewRadius);
    }
}
