using UnityEngine;
using System.Collections;

namespace SuburbanHouse
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshFilter))]
    public class SpecialDoor : MonoBehaviour
    {
        [Header("Time Settings")]
        public Clock clockScript;
        public int openStartHour = 8;
        public int openEndHour = 18;

        [Header("Entry Direction Control")]
        [Tooltip("0: 기본 방향 (transform.forward), 1: 반대 방향 (-transform.forward)")]
        public int entryDirectionState = 0; // 입장 방향 제어용 (트리거나 인스펙터에서 설정)
        private Vector3 entryDirection;

        public enum rotOrient { Y_Axis_Up, Z_Axis_Up, X_Axis_Up }

        [Header("Door Settings")]
        public rotOrient rotationOrientation = rotOrient.Y_Axis_Up;
        public float doorOpenAngle = 90.0f;
        [Range(1, 15)] public float speed = 8.0f;

        [Header("Sound Settings")]
        public AudioClip doorOpenSound;
        public AudioClip doorCloseSound;

        private Quaternion doorOpen = Quaternion.identity;
        private Quaternion doorClosed = Quaternion.identity;
        private bool doorStatus = false;
        private bool autoClosing = false;

        void Start()
        {
            if (gameObject.isStatic)
            {
                Debug.Log("This door has been set to static and won't be openable. SpecialDoor script has been removed.");
                Destroy(this);
                return;
            }

            // 회전 초기화
            Vector3 angles = transform.localEulerAngles;
            switch (rotationOrientation)
            {
                case rotOrient.Z_Axis_Up:
                    doorOpen = Quaternion.Euler(angles.x, angles.y, angles.z + doorOpenAngle);
                    break;
                case rotOrient.Y_Axis_Up:
                    doorOpen = Quaternion.Euler(angles.x, angles.y + doorOpenAngle, angles.z);
                    break;
                case rotOrient.X_Axis_Up:
                    doorOpen = Quaternion.Euler(angles.x + doorOpenAngle, angles.y, angles.z);
                    break;
            }
            doorClosed = Quaternion.Euler(angles);

            // 입장 방향 설정
            entryDirection = (entryDirectionState == 0) ? transform.forward : -transform.forward;
        }

        void Update()
        {
            if (doorStatus && clockScript.realTime && !autoClosing)
            {
                int currentHour = clockScript.hour;
                bool isEntryAllowed = (currentHour >= openStartHour && currentHour < openEndHour);

                if (!isEntryAllowed)
                {
                    Debug.Log("⏰ 제한 시간 감지: 자동으로 문을 닫습니다.");
                    StartCoroutine(AutoCloseAfterDelay(3f));
                }
            }
        }

        public void InteractWithThisDoor()
        {
            Vector3 toPlayer = (Camera.main.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(entryDirection, toPlayer);
            bool isPlayerEntering = dot > 0;

            int currentHour = clockScript.hour;

            if (isPlayerEntering)
            {
                if (clockScript.realTime && (currentHour < openStartHour || currentHour >= openEndHour))
                {
                    Debug.Log("⛔ 입장 불가 시간입니다.");
                    return;
                }
            }

            if (doorStatus)
            {
                StartCoroutine(MoveDoor(doorClosed));
                if (doorCloseSound != null)
                    StartCoroutine(DelayedCloseAudio(speed / 40f));
            }
            else
            {
                StartCoroutine(MoveDoor(doorOpen));
                if (doorOpenSound != null)
                    AudioSource.PlayClipAtPoint(doorOpenSound, transform.position);
            }
        }

        IEnumerator AutoCloseAfterDelay(float seconds)
        {
            autoClosing = true;
            yield return new WaitForSeconds(seconds);

            int currentHour = clockScript.hour;
            bool isEntryAllowed = !clockScript.realTime || (currentHour >= openStartHour && currentHour < openEndHour);

            if (doorStatus && !isEntryAllowed)
            {
                StartCoroutine(MoveDoor(doorClosed));
                if (doorCloseSound != null)
                    StartCoroutine(DelayedCloseAudio(speed / 40f));
                Debug.Log("🚪 제한 시간 - 문 자동 닫힘 완료");
            }

            autoClosing = false;
        }

        IEnumerator DelayedCloseAudio(float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(doorCloseSound, transform.position);
        }

        IEnumerator MoveDoor(Quaternion target)
        {
            while (Quaternion.Angle(transform.localRotation, target) > 0.5f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * speed);
                yield return null;
            }
            doorStatus = !doorStatus;
        }

        // 트리거를 통해 방향 갱신하고 싶다면 이렇게도 가능
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // 예: 트리거가 문 안쪽일 경우 entryDirectionState = 0
                // 예: 트리거가 문 바깥쪽일 경우 entryDirectionState = 1

                // 필요시 entryDirectionState 를 변경하고 entryDirection 재설정
                // entryDirectionState = 0 or 1; (외부에서 설정 가능)
                entryDirection = (entryDirectionState == 0) ? transform.forward : -transform.forward;
            }
        }
    }
}
