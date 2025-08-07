using UnityEngine;
using SuburbanHouse;

public class CrossHair : MonoBehaviour
{
    Camera cam;

    [Range(1, 5)]
    public float rayDistance = 2f;

    public Texture2D crosshair;
    public int crosshairFontSize = 20; // 텍스트 크기 조절 변수 (Inspector에서 설정 가능)

    int crossHairStatus = 0;
    string crosshairText = ""; // 표시할 텍스트 저장

    public bool isAimingAtNavigation = false; // 네비게이션 조준 여부
    public bool interactionLocked = false;    // 네비게이션 화면 열린 여부

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("Main camera tag not found in scene!");
            return;
        }

        if (!cam.allowHDR)
        {
            cam.allowHDR = true;
        }
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        isAimingAtNavigation = false;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // 문 (Door)
            if (hit.transform.GetComponent<Door>())
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.transform.GetComponent<Door>().InteractWithThisDoor();
                }
            }
            // ✅ 스페셜 도어 (SpecialDoor)
            else if (hit.transform.GetComponent<SpecialDoor>())
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.transform.GetComponent<SpecialDoor>().InteractWithThisDoor();
                }
            }
            // 차고 문 (GarageDoor)
            else if (hit.transform.GetComponent<GarageDoor>())
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.transform.GetComponent<GarageDoor>().ToggleDoor();
                }
            }
            // 아이템 (Item)
            else if (hit.transform.GetComponent<Item>())
            {
                Item item = hit.transform.GetComponent<Item>();
                crossHairStatus = 1;
                crosshairText = $"(E) {item.itemName} \n💰 {item.itemPrice}coin";
            }
            // 네비게이션
            else if (hit.transform.CompareTag("Navigation"))
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

    void OnGUI()
    {
        switch (crossHairStatus)
        {
            case 0:
                if (crosshair != null)
                {
                    Rect crosshairRect = new Rect(
                        (Screen.width - crosshair.width) / 2,
                        (Screen.height - crosshair.height) / 2,
                        crosshair.width,
                        crosshair.height);

                    GUI.DrawTexture(crosshairRect, crosshair);
                }
                break;

            case 1:
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = crosshairFontSize;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;

                GUI.Label(
                    new Rect(Screen.width / 2 - 100, Screen.height / 2 + 30, 200, 50),
                    crosshairText,
                    style);
                break;
        }
    }
}
