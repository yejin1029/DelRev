using UnityEngine;

public class PetCCTV : MonoBehaviour
{
    public float rotationSpeed = 360f / 15f; // 360도 / 15초
    public float viewDistance = 5f;
    public float viewAngle = 120f;
    public Transform player;
    public AudioSource warningAudio;
    public AudioSource songAudio;
    public GameObject dog;
    private bool hasTriggered = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        dog = GameObject.FindGameObjectWithTag("Dog");
    }

    void Update()
    {
        RotateCCTV();
        DetectPlayer();
    }

    void RotateCCTV()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void DetectPlayer()
    {
        if (IsPlayerInSight() && !hasTriggered)
        {
            TriggerAlarm();
            hasTriggered = true;
        }
        else if (!IsPlayerInSight())
        {
            hasTriggered = false; // 다시 시야에서 벗어나면 재탐지 가능
        }
    }

    bool IsPlayerInSight()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Ignore vertical angle
        float distance = directionToPlayer.magnitude;

        if (distance > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2)
            return false;

        // Raycast to check for obstacles
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f; // CCTV 눈높이 기준
        Vector3 rayDirection = (player.position - rayOrigin).normalized;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, viewDistance))
        {
            if (hit.transform != player)
            {
                // 장애물이 있음
                return false;
            }
        }

        return true;
    }

    void TriggerAlarm()
    {
        if (warningAudio) warningAudio.Play();
        if (songAudio) songAudio.Play();

        if (dog != null)
        {
            Dog dogScript = dog.GetComponent<Dog>();
            if (dogScript != null)
            {
                dogScript.MoveToCCTV(transform.position); // CCTV 위치로 개 이동
            }
        }
    }
}
