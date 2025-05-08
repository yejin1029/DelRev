using UnityEngine;
using System.Linq;  // LINQ 사용

public class DontDestroyOnLoadByName : MonoBehaviour
{
    [Tooltip("false로 하면 이 스크립트가 아예 동작하지 않습니다.")]
    public bool persistenceEnabled = true;

    private void Awake()
    {
        if (!persistenceEnabled)
            return;  // 비활성화 상태면 로직 중단

        // 같은 이름을 가진 모든 오브젝트를 찾아서
        var objs = FindObjectsOfType<GameObject>()
                   .Where(go => go.name == gameObject.name)
                   .ToArray();

        // 둘 이상이면 자신을 파괴
        if (objs.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // 유일할 때만 퍼시스트 처리
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 런타임 중에 켜고 끌 때 호출할 수 있습니다.
    /// </summary>
    public void SetPersistenceEnabled(bool enabled)
    {
        persistenceEnabled = enabled;
    }
}
