using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ItemDeliveryNotifier : MonoBehaviour
{
  [Header("References")]
  public MessageUI messageUI;
  public GameObject trailerObject;

  [Header("Settings")]
  public string itemTag = "Item";
  public string targetSceneName = "Company";

  private bool hasShownNextMissionMessage = false;
  private int lastItemCount = -1;
  private string previousSceneName = "";

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    if (scene.name == targetSceneName)
    {
      Debug.Log("[ItemDeliveryNotifier] Company 씬 로드 감지됨");
      hasShownNextMissionMessage = false;
      StartCoroutine(DelayedInitialCheck());
    }
  }

  private IEnumerator DelayedInitialCheck()
  {
    yield return new WaitForSeconds(0.3f); // 씬 로딩 후 참조 확보 대기

    if (trailerObject == null)
    {
      trailerObject = GameObject.Find("Trailer");
      if (trailerObject == null)
      {
        Debug.LogWarning("[ItemDeliveryNotifier] Trailer를 찾을 수 없습니다.");
        yield break;
      }
    }

    if (messageUI == null)
    {
      Debug.LogError("[ItemDeliveryNotifier] messageUI 참조가 필요합니다!");
      yield break;
    }

    int itemCount = CountTrailerItems();
    lastItemCount = itemCount;

    if (itemCount > 0)
    {
      messageUI.ShowMessage("회사에 아이템을 제출해 주세요.");
    }
    else
    {
      messageUI.ShowMessage("다음 맵으로 이동하여 임무를 수행해 주세요.");
      hasShownNextMissionMessage = true;
    }
  }

  private void Update()
  {
    if (SceneManager.GetActiveScene().name != targetSceneName) return;
    if (trailerObject == null) return;

    int itemCount = CountTrailerItems();

    // 아이템을 모두 제출한 시점 감지
    if (lastItemCount > 0 && itemCount == 0 && !hasShownNextMissionMessage)
    {
      messageUI.ShowMessage("다음 맵으로 이동하여 임무를 수행해 주세요.");
      hasShownNextMissionMessage = true;
    }

    lastItemCount = itemCount;
  }

  private int CountTrailerItems()
  {
    int count = 0;
    foreach (Transform child in trailerObject.transform)
    {
      if (child != null && child.CompareTag(itemTag))
        count++;
    }
    return count;
  }
}
