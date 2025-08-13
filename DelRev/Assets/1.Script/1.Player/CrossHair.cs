using UnityEngine;
using SuburbanHouse;

public class CrossHair : MonoBehaviour
{
    Camera cam;

    [Range(1, 5)] public float rayDistance = 3.5f;
    public Texture2D crosshair;
    public int crosshairFontSize = 20;

    int crossHairStatus = 0;
    string crosshairText = "";
    public bool isAimingAtNavigation = false;
    public bool interactionLocked = false;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        if (cam == null) { Debug.LogError("Main camera tag not found in scene!"); return; }
        if (!cam.allowHDR) cam.allowHDR = true;
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        isAimingAtNavigation = false;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // 일반 문
            if (hit.transform.GetComponent<Door>())
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기";
                if (Input.GetKeyDown(KeyCode.E))
                    hit.transform.GetComponent<Door>().InteractWithThisDoor();
            }
            // 시간제 문 (SpecialDoor)
            else if (hit.transform.GetComponent<SpecialDoor>())
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기 (시간제)";
                if (Input.GetKeyDown(KeyCode.E))
                    hit.transform.GetComponent<SpecialDoor>().InteractWithThisDoor();
            }
            // 열쇠 문 (KeyDoor)
            else if (hit.transform.GetComponent<KeyDoor>())
            {
                var keyDoor = hit.transform.GetComponent<KeyDoor>();
                crossHairStatus = 1;

                if (HasMatchingKeyFor(keyDoor))
                    crosshairText = "(E) 열쇠로 문 열기";
                else
                    crosshairText = "(E) 문 열기\n잠김: 맞는 열쇠 필요";

                if (Input.GetKeyDown(KeyCode.E))
                    keyDoor.InteractWithThisDoor();
            }
            // 아이템
            else if (hit.transform.GetComponent<Item>())
            {
                Item item = hit.transform.GetComponent<Item>();
                crossHairStatus = 1;
                crosshairText = $"(E) {item.itemName} \n💰 {item.itemPrice}coin";
            }
            // 네비게이션
            else if (hit.transform.CompareTag("Navigation"))
            {
                if (!interactionLocked) { crossHairStatus = 1; crosshairText = "(E) 네비게이션 열기"; }
                else { crossHairStatus = 0; crosshairText = ""; }
                isAimingAtNavigation = true;
            }
            else
            {
                crossHairStatus = 0;
                crosshairText = "";
                isAimingAtNavigation = false;
            }
        }
        else
        {
            crossHairStatus = 0;
            crosshairText = "";
        }
    }

    bool HasMatchingKeyFor(KeyDoor door)
    {
        if (door == null) return false;
        var inv = Inventory.Instance; if (inv == null) return false; // 싱글톤 인벤토리:contentReference[oaicite:0]{index=0}
        var items = inv.GetInventoryItems();                          // 공개 게터 사용:contentReference[oaicite:1]{index=1}
        int idx = inv.GetCurrentIndex();                               // 현재 슬롯:contentReference[oaicite:2]{index=2}
        if (idx < 0 || idx >= items.Count) return false;
        var current = items[idx]; if (current == null) return false;
        var key = current as KeyItem; if (key == null) return false;
        return key.doorID == door.doorID;
    }

    void OnGUI()
    {
        switch (crossHairStatus)
        {
            case 0:
                if (crosshair != null)
                {
                    var r = new Rect((Screen.width - crosshair.width)/2, (Screen.height - crosshair.height)/2, crosshair.width, crosshair.height);
                    GUI.DrawTexture(r, crosshair);
                }
                break;
            case 1:
                GUIStyle style = new GUIStyle(GUI.skin.label) {
                    fontSize = crosshairFontSize, fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }, alignment = TextAnchor.MiddleCenter
                };
                GUI.Label(new Rect(Screen.width/2 - 100, Screen.height/2 + 30, 200, 50), crosshairText, style);
                break;
        }
    }
}
