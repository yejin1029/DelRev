using UnityEngine;
using System.Collections;
using SuburbanHouse;

public class CrossHair : MonoBehaviour
{
    Camera cam;

    [Range(1, 5)]
    public float rayDistance = 2f;

    public Texture2D crosshair; // 크로스헤어 이미지

    int crossHairStatus = 0;

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
        // 카메라 중심에서 정면으로 Ray 쏘기
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.transform.GetComponent<SuburbanHouse.Door>())
            {
                crossHairStatus = 1;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Door d = hit.transform.GetComponent<Door>();
                    d.InteractWithThisDoor();
                }
            }
            else if (hit.transform.GetComponent<GarageDoor>())
            {
                crossHairStatus = 1;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    GarageDoor gd = hit.transform.GetComponent<GarageDoor>();
                    gd.ToggleDoor();
                }
            }
            else
            {
                crossHairStatus = 0;
            }
        }
        else
        {
            crossHairStatus = 0;
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
                // "Press E" 텍스트 표시
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 20;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;

                GUI.Label(
                    new Rect(Screen.width / 2 - 75, Screen.height / 2 + 30, 150, 30),
                    "Press E",
                    style);
                break;
        }
    }
}
