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
        if (cam == null)
        {
            Debug.LogError("Main camera tag not found in scene!");
            return;
        }
        if (!cam.allowHDR) cam.allowHDR = true;
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        isAimingAtNavigation = false;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            crossHairStatus = 0;
            crosshairText = "";

            // 1) Lever (레버 우선 처리) -----------------------------------
            if (hit.transform.TryGetComponent<Lever>(out var lever))
            {
                crossHairStatus = 1;
                crosshairText = "(E) 레버 당기기";
                if (!interactionLocked && Input.GetKeyDown(KeyCode.E))
                {
                    // 조준 + E → 레버 트리거
                    // (Lever 내부에서 Player 카메라/거리 체크는 이미 완료됨)
                    // Lever가 public 메서드를 별도로 노출하지 않았다면 StartProcess만 호출해도 OK
                    if (lever.converter != null)
                        lever.converter.StartProcess();
                    else
                        Debug.LogWarning("[CrossHair] Lever에 converter가 연결되지 않았습니다.");
                }
            }
            // 2) 일반 문 ----------------------------------------------------
            else if (hit.transform.TryGetComponent<Door>(out var door))
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기";
                if (Input.GetKeyDown(KeyCode.E)) door.InteractWithThisDoor();
            }
            // 3) 시간제 문
            else if (hit.transform.TryGetComponent<SpecialDoor>(out var sdoor))
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기 (시간제)";
                if (Input.GetKeyDown(KeyCode.E)) sdoor.InteractWithThisDoor();
            }
            // 4) 열쇠 문
            else if (hit.transform.TryGetComponent<KeyDoor>(out var keyDoor))
            {
                crossHairStatus = 1;
                if (HasMatchingKeyFor(keyDoor))
                    crosshairText = "(E) 열쇠로 문 열기";
                else
                    crosshairText = "(E) 문 열기\n잠김: 맞는 열쇠 필요";

                if (Input.GetKeyDown(KeyCode.E)) keyDoor.InteractWithThisDoor();
            }
            // 5) 아이템
            else if (hit.transform.TryGetComponent<Item>(out var item))
            {
                crossHairStatus = 1;
                crosshairText = $"(E) {item.itemName} \n💰 {item.itemPrice} coin";
            }
            // 6) 네비게이션 (PlaneItemToCoin이 붙어있다면 제외)
            else if (hit.transform.CompareTag("Navigation") &&
                     !hit.transform.TryGetComponent<PlaneItemToCoin>(out _))
            {
                if (!interactionLocked)
                {
                    crossHairStatus = 1;
                    crosshairText = "(E) 네비게이션 열기";
                }
                else
                {
                    crossHairStatus = 0;
                    crosshairText = "";
                }
                isAimingAtNavigation = true;
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
        var inv = Inventory.Instance;
        if (inv == null) return false;
        var items = inv.GetInventoryItems();
        int idx = inv.GetCurrentIndex();
        if (idx < 0 || idx >= items.Count) return false;
        var current = items[idx];
        if (current == null) return false;
        var key = current as KeyItem;
        if (key == null) return false;
        return key.doorID == door.doorID;
    }

    void OnGUI()
    {
        switch (crossHairStatus)
        {
            case 0:
                if (crosshair != null)
                {
                    var r = new Rect(
                        (Screen.width - crosshair.width) / 2,
                        (Screen.height - crosshair.height) / 2,
                        crosshair.width, crosshair.height
                    );
                    GUI.DrawTexture(r, crosshair);
                }
                break;

            case 1:
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = crosshairFontSize,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 30, 200, 50),
                    crosshairText, style);
                break;
        }
    }
}
