using UnityEngine;

public class PetCCTV : MonoBehaviour
{
    public float rotationSpeed = 360f / 5f; // 360도 / 5초
    public float viewDistance = 3f;
    public float viewAngle = 120f;
    public Transform player;
    public AudioSource warningAudio;
    public AudioSource songAudio;
    public GameObject dog;
    public float detectionTimeThreshold = 3f;
    private float detectionTimer = 0f;
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
        if (IsPlayerInSight())
        {
            detectionTimer += Time.deltaTime;

            if (detectionTimer >= detectionTimeThreshold && !hasTriggered)
            {
                TriggerAlarm();
                hasTriggered = true;
            }
        }
        else
        {
            detectionTimer = 0f;
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

        // Optional: raycast check for obstacles
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
                dogScript.MoveToCCTV(transform.position); // CCTV 자신의 위치로 개를 호출
            }
        }
    }
}
