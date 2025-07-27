using UnityEngine;

public class TeacherManager : MonoBehaviour
{
    public Teacher[] teachers;

    void Start()
    {
        if (teachers.Length > 0)
        {
            int patrolIndex = Random.Range(0, teachers.Length);
            teachers[patrolIndex].SetAsPatrollingTeacher();
        }
    }
}
