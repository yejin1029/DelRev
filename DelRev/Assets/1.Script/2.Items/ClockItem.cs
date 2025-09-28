using UnityEngine;

public class ClockItem : Item, IInventoryEffect
{
    private GameObject clockInstance;

    public void OnAdd(PlayerController player)
    {
        if (player == null) return;

        if (clockInstance == null)
        {
            var prefab = Resources.Load<GameObject>("StoreItems/Clock");
            if (prefab != null)
            {
                // 📌 Main Camera 찾기
                Camera cam = Camera.main;
                if (cam != null)
                {
                    clockInstance = GameObject.Instantiate(prefab, cam.transform);

                    // 🔥 카메라 기준으로 고정 위치/회전/스케일 적용
                    clockInstance.transform.localPosition = new Vector3(0.66f, -0.33f, 0.87f);
                    clockInstance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    clockInstance.transform.localScale    = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
        }
    }

    public void OnRemove(PlayerController player)
    {
        if (clockInstance != null)
        {
            GameObject.Destroy(clockInstance);
            clockInstance = null;
        }
    }
}
