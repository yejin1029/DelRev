using UnityEngine;
using SuburbanHouse;

public class CrossHair : MonoBehaviour
{
    Camera cam;

    [Range(1, 5)]
    public float rayDistance = 2f;

    public Texture2D crosshair;

    int crossHairStatus = 0;
    string crosshairText = ""; // 표시할 텍스트 저장

    void Start()
    {
        cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("Main camera tag not found in scene!");
            Destroy(this.gameObject);
        }

        if (!cam.allowHDR)
        {
            cam.allowHDR = true;
        }
    }

    void Update()
    {
        // 시야 중심으로 레이 쏘기
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // 문
            if (hit.transform.GetComponent<Door>())
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.transform.GetComponent<Door>().InteractWithThisDoor();
                }
            }
            // 차고 문
            else if (hit.transform.GetComponent<GarageDoor>())
            {
                crossHairStatus = 1;
                crosshairText = "(E) 문 열기";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.transform.GetComponent<GarageDoor>().ToggleDoor();
                }
            }
            // 아이템
            else if (hit.transform.GetComponent<Item>())
            {
                Item item = hit.transform.GetComponent<Item>();
                crossHairStatus = 1;
                crosshairText = $"(E) {item.itemName} \n💰 {item.itemPrice}coin";
            }
            else
            {
                crossHairStatus = 0;
                crosshairText = "";
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
                // 기본 십자선 표시
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
                // 텍스트 표시 (문, 차고, 아이템 등)
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 20;
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
