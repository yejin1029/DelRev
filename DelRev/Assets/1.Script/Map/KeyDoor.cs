using UnityEngine;
using System.Collections;

namespace SuburbanHouse
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshFilter))]
    public class KeyDoor : MonoBehaviour
    {
        public enum rotOrient
        {
            Y_Axis_Up,
            Z_Axis_Up,
            X_Axis_Up
        }

        public enum rotFixAxis
        {
            Y,
            Z
        }

        [Header("Rotation")]
        public rotOrient rotationOrientation;
        public bool applyRotationFix = false;
        public rotFixAxis rotationAxisFix;
        public float doorOpenAngle = 90.0f;
        [Range(1, 15)] public float speed = 8.0f;

        [Header("Audio")]
        public AudioClip doorOpenSound;
        public AudioClip doorCloseSound;

        [Header("Key Settings")]
        [Tooltip("이 문을 열 수 있는 열쇠의 ID (KeyItem.doorID와 일치해야 함)")]
        public string doorID;
        [Tooltip("처음에 잠겨 있을지 여부")]
        public bool startLocked = true;

        private Quaternion doorOpen = Quaternion.identity;
        private Quaternion doorClosed = Quaternion.identity;

        private bool doorStatus = false; // false: 닫힘, true: 열림
        private bool isLocked;           // true: 잠김

        void Start()
        {
            // 정적 오브젝트면 문 열림 로직 비활성화 (원본 Door.cs와 동일)
            if (this.gameObject.isStatic)
            {
                Debug.Log("This door has been set to static and won't be openable. Keyscripts has been removed.");
                Destroy(this);
                return;
            }

            isLocked = startLocked;

            // 회전 목표값 설정 (원본 Door.cs와 동일한 분기)
            switch (rotationOrientation)
            {
                case rotOrient.Z_Axis_Up:
                    doorOpen = Quaternion.Euler(
                        transform.localEulerAngles.x,
                        transform.localEulerAngles.y,
                        transform.localEulerAngles.z + doorOpenAngle
                    );
                    break;

                case rotOrient.Y_Axis_Up:
                    doorOpen = Quaternion.Euler(
                        transform.localEulerAngles.x,
                        transform.localEulerAngles.y + doorOpenAngle,
                        transform.localEulerAngles.z
                    );
                    break;

                case rotOrient.X_Axis_Up:
                    if (!applyRotationFix)
                    {
                        doorOpen = Quaternion.Euler(
                            transform.localEulerAngles.x + doorOpenAngle,
                            transform.localEulerAngles.y,
                            transform.localEulerAngles.z
                        );
                    }
                    else
                    {
                        if (rotationAxisFix.Equals(rotFixAxis.Y))
                        {
                            doorOpen = Quaternion.Euler(transform.localEulerAngles.x + 90f, 90f, 270f);
                        }
                        else if (rotationAxisFix.Equals(rotFixAxis.Z))
                        {
                            doorOpen = Quaternion.Euler(transform.localEulerAngles.x + 90f, 270f, 90f);
                        }
                    }
                    break;
            }

            doorClosed = Quaternion.Euler(
                transform.localEulerAngles.x,
                transform.localEulerAngles.y,
                transform.localEulerAngles.z
            );
        }

        /// <summary>
        /// 플레이어 상호작용 (E키)에서 호출.
        /// - 잠겨 있으면 현재 인벤토리 슬롯의 KeyItem 확인 후 일치 시 해제+오픈
        /// - 잠겨 있지 않으면 일반 문처럼 토글(열기/닫기)
        /// - 냉장고 문(Door_Top) 처리와 동일하게 라이트 토글 포함
        /// </summary>
        public void InteractWithThisDoor()
        {
            // 특수 케이스: 냉장고 문 라이트 토글 (원본 Door.cs와 동일 로직)
            if (transform.name.Equals("Door_Top"))
            {
                Light fridgeLight = transform.parent.GetComponentInChildren<Light>();
                if (fridgeLight != null)
                    fridgeLight.enabled = !doorStatus;
            }

            if (isLocked)
            {
                // 열쇠 검사: 현재 슬롯의 KeyItem이 있고 doorID가 일치하면 사용/소멸
                if (TryConsumeMatchingKey())
                {
                    Unlock();
                    // 해제 직후는 "열기" 동작 수행
                    StartCoroutine(this.moveDoor(doorOpen));
                    if (doorOpenSound != null)
                        AudioSource.PlayClipAtPoint(doorOpenSound, this.transform.position);
                }
                else
                {
                    Debug.Log("문이 잠겨 있습니다. 맞는 열쇠가 필요합니다.");
                }
                return;
            }

            // 잠겨 있지 않으면 원본처럼 토글
            if (doorStatus)
            {
                // 닫기
                StartCoroutine(this.moveDoor(doorClosed));
                if (doorCloseSound != null)
                    StartCoroutine(delayedCloseAudio(speed / 40f));
            }
            else
            {
                // 열기
                StartCoroutine(this.moveDoor(doorOpen));
                if (doorOpenSound != null)
                    AudioSource.PlayClipAtPoint(doorOpenSound, this.transform.position);
            }
        }

        /// <summary>
        /// 몬스터/스크립트 등이 강제로 열도록 할 때 사용(잠김은 유지됨).
        /// 원본 Door.cs의 OpenDoorForMonster와 유사.
        /// </summary>
        public void OpenDoorForMonster()
        {
            if (!doorStatus)
            {
                StartCoroutine(this.moveDoor(doorOpen));
                if (doorOpenSound != null)
                    AudioSource.PlayClipAtPoint(doorOpenSound, this.transform.position);
            }
        }

        /// <summary>외부에서 잠금 해제 + 즉시 열기</summary>
        public void UnlockAndOpen()
        {
            Unlock();
            StartCoroutine(this.moveDoor(doorOpen));
            if (doorOpenSound != null)
                AudioSource.PlayClipAtPoint(doorOpenSound, this.transform.position);
        }

        /// <summary>잠금만 해제</summary>
        public void Unlock()
        {
            isLocked = false;
        }

        IEnumerator delayedCloseAudio(float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(doorCloseSound, this.transform.position);
        }

        IEnumerator moveDoor(Quaternion target)
        {
            while (Quaternion.Angle(transform.localRotation, target) > 0.5f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * speed);
                yield return null;
            }
            doorStatus = !doorStatus;
            yield return null;
        }

        /// <summary>
        /// 현재 인벤토리 선택 슬롯의 KeyItem이 doorID와 일치하면
        /// - 문을 열 수 있도록 true 반환
        /// - 인벤토리에서 해당 키 즉시 삭제(드랍 없이 소멸)
        /// </summary>
        private bool TryConsumeMatchingKey()
        {
            var inv = Inventory.Instance;
            if (inv == null) return false;

            var items = inv.GetInventoryItems();
            int idx = inv.GetCurrentIndex();
            if (idx < 0 || idx >= items.Count) return false;

            var current = items[idx];
            if (current == null) return false;

            var key = current as KeyItem;
            if (key == null) return false;

            if (key.doorID != doorID) return false;

            // 맞는 열쇠 → 인벤토리에서 소멸
            inv.RemoveCurrentItemWithoutDrop();
            // 씬 상에 남아있을 수 있는 오브젝트 안전 제거
            if (key != null) Destroy(key.gameObject);
            return true;
        }
    }
}
