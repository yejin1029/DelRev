using System;
using System.Collections;   // ← 이것 추가!
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class TurretSentinel : MonoBehaviour
{
    public static event Action<Vector3> GlobalAlertTurretPos;

    [Header("Rotate")]
    public float yawDegreesPerSecond = 72f;

    [Header("FOV")]
    public float viewRadius = 8f;
    [Range(0f, 360f)] public float viewAngle = 120f;
    public Transform eye;

    [Header("Detection")]
    public float requiredDetectTime = 0.5f;
    public float alertCooldown = 5f;

    [Header("Audio")]
    public AudioClip alertSfx;
    public float alertVolume = 0.8f;
    public float burstDelay = 0.1f;  // 4번 사이 딜레이
    public float restTime = 1f;      // 4번 끝난 뒤 쉬는 시간

    float _detectTimer = 0f;
    float _lastAlertSent = -999f;

    AudioSource _audio;
    Transform _player;

    bool _inAlertState = false;
    Coroutine _alertLoop;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        _player = PlayerController.Instance?.transform;
        if (eye == null) eye = transform;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, yawDegreesPerSecond * Time.deltaTime);

        if (_player == null) return;

        bool inSight = IsPlayerInSight();

        if (inSight)
        {
            _detectTimer += Time.deltaTime;

            if (_detectTimer >= requiredDetectTime)
            {
                if (!_inAlertState)
                {
                    _inAlertState = true;
                    _alertLoop = StartCoroutine(AlertBurstLoop());
                }

                // 쿨다운 이벤트
                if (Time.time - _lastAlertSent >= alertCooldown)
                {
                    _lastAlertSent = Time.time;
                    GlobalAlertTurretPos?.Invoke(transform.position);
                }
            }
        }
        else
        {
            ExitAlertState();
        }
    }

    IEnumerator AlertBurstLoop()
    {
        while (true) // 발견 상태일 동안 반복
        {
            // 🔥 4번 연속 재생
            for (int i = 0; i < 4; i++)
            {
                if (alertSfx != null)
                    _audio.PlayOneShot(alertSfx, alertVolume);

                yield return new WaitForSeconds(burstDelay);
            }

            // 🔥 1초 휴식
            yield return new WaitForSeconds(restTime);
        }
    }

    void ExitAlertState()
    {
        if (_inAlertState)
        {
            _inAlertState = false;
            _detectTimer = 0f;

            if (_alertLoop != null)
                StopCoroutine(_alertLoop);
        }
    }

    bool IsPlayerInSight()
    {
        Vector3 origin = eye.position;
        Vector3 toPlayer = _player.position - origin;

        if (toPlayer.magnitude > viewRadius) return false;
        if (Vector3.Angle(eye.forward, toPlayer.normalized) > viewAngle * 0.5f) return false;

        if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, viewRadius))
        {
            if (hit.transform == _player || hit.transform.IsChildOf(_player))
                return true;
        }

        return false;
    }
}
