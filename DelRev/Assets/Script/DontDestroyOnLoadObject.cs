using UnityEngine;

public class DontDestroyOnLoadObject : MonoBehaviour
{
    private void Awake()
    {
        // 이미 존재하는 동일한 이름의 객체가 있다면 삭제
        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.tag);
        if (objs.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
