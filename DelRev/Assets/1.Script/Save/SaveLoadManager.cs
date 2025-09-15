using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    // ========= 아이템 프리팹 자동 탐색 루트 =========
    private static readonly string[] ITEM_ROOTS = new[]
    {
        "Items",
        "HouseItems",
        "StoreItems"
        // 필요 시 "Prefabs/Items" 등 추가
    };

    // ========= Trailer 탐색 규칙 =========
    private const string TRAILER_TAG  = "Car";      // ← 현재 프로젝트 태그
    private const string TRAILER_NAME_FALLBACK = "Trailer";

    // 이름 -> 프리팹 캐시 (프리팹 파일명, Item.itemName, 정규화 이름까지 키 등록)
    private static Dictionary<string, GameObject> _itemLookup; // key: name/itemName/normalized

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace(" ", "").Replace("_", "").ToLowerInvariant();
    }

    private static string StripClone(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("(Clone)", "").Trim();
    }

    private static void BuildItemLookup()
    {
        _itemLookup = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        int added = 0;

        foreach (var root in ITEM_ROOTS)
        {
            var gos = Resources.LoadAll<GameObject>(root);
            foreach (var go in gos)
            {
                var item = go.GetComponent<Item>();
                if (item == null) continue;

                void AddKey(string k)
                {
                    if (!string.IsNullOrEmpty(k) && !_itemLookup.ContainsKey(k))
                        _itemLookup[k] = go;
                }

                AddKey(go.name);
                AddKey(Normalize(go.name));
                AddKey(item.itemName);
                AddKey(Normalize(item.itemName));
                added++;
            }
        }

        Debug.Log($"[SaveLoadManager] 아이템 프리팹 인덱싱 완료: {added}개 / 루트: {string.Join(", ", ITEM_ROOTS)}");
    }

    private static GameObject ResolveItemPrefab(string name)
    {
        if (_itemLookup == null) BuildItemLookup();
        if (string.IsNullOrEmpty(name)) return null;

        // 1) 원문
        if (_itemLookup.TryGetValue(name, out var prefab)) return prefab;

        // 2) 정규화
        var norm = Normalize(name);
        if (_itemLookup.TryGetValue(norm, out prefab)) return prefab;

        // 3) (Clone) 제거 후 재시도
        var stripped = StripClone(name);
        if (!string.Equals(stripped, name, StringComparison.Ordinal))
        {
            if (_itemLookup.TryGetValue(stripped, out prefab)) return prefab;
            var strippedNorm = Normalize(stripped);
            if (_itemLookup.TryGetValue(strippedNorm, out prefab)) return prefab;
        }

        return null;
    }

    // ========= 저장 파일 유틸 =========
    public static string GetSavePath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    public static bool HasSave(int slot) => File.Exists(GetSavePath(slot));

    // ========= Trailer 찾기/도우미 =========
    private static Transform FindTrailerRoot()
    {
        GameObject go = null;

        // 태그로 탐색
        try { go = GameObject.FindWithTag(TRAILER_TAG); } catch { /* 태그 없으면 무시 */ }
        if (go != null) return go.transform;

        go = GameObject.Find(TRAILER_NAME_FALLBACK);
        if (go != null) return go.transform;

        return null;
    }

    private static IEnumerable<Transform> EnumerateTrailerItemChildren(Transform trailerRoot)
    {
        if (trailerRoot == null) yield break;
        foreach (Transform child in trailerRoot)
        {
            if (child.GetComponent<Item>() != null)
                yield return child;
        }
    }

    // ========= 저장 =========
    public static void SaveGame(PlayerController player, Inventory inventory, int slot)
    {
        if (player == null || inventory == null)
        {
            Debug.LogWarning("SaveGame 실패: Player 또는 Inventory 참조가 없습니다.");
            return;
        }

        var data = new SaveData();

        // 기본 스탯/상태
        data.health = player.health;
        try { data.stamina = player.stamina; } catch { data.stamina = 0f; }
        data.coinCount = player.coinCount;
        data.playerPosition = player.transform.position;
        data.sceneName = SceneManager.GetActiveScene().name;

        // 현재 일자(MapTracker)
        try
        {
            var tracker = MapTracker.Instance;
            if (tracker != null)
                data.currentDay = tracker.currentDay;
        }
        catch { data.currentDay = 0; }

        // 인벤토리 4칸 저장(이름 기반)
        data.inventoryItemNames = new List<string>(4);
        var items = inventory.GetInventoryItems();
        for (int i = 0; i < 4; i++)
        {
            string name = null;
            if (items != null && i < items.Count && items[i] != null)
            {
                try { name = items[i].itemName; } catch { name = items[i].name; }
            }
            data.inventoryItemNames.Add(name);
        }

        // Trailer 내부 아이템 저장
        data.trailerItems = new List<SaveData.TrailerItemData>();
        var trailer = FindTrailerRoot();
        if (trailer != null)
        {
            foreach (var t in EnumerateTrailerItemChildren(trailer))
            {
                var item = t.GetComponent<Item>();
                if (item == null) continue;

                var entry = new SaveData.TrailerItemData
                {
                    key           = !string.IsNullOrEmpty(item.itemName) ? item.itemName : StripClone(t.name),
                    localPosition = t.localPosition,
                    localRotation = t.localRotation,
                    localScale    = t.localScale
                };
                data.trailerItems.Add(entry);
            }
        }

        // 직렬화 & 저장
        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(slot);
        File.WriteAllText(path, json);

        PlayerPrefs.SetInt("last_slot", slot);
        PlayerPrefs.Save();

        Debug.Log(
            $"저장 완료 - 슬롯 {slot}: {path}\n" +
            $"씬={data.sceneName}, 위치={data.playerPosition}, 코인={data.coinCount}, 일자={data.currentDay}, " +
            $"TrailerItems={data.trailerItems?.Count ?? 0}"
        );
    }

    // ========= 로드 (씬 이동은 외부 Continue 흐름) =========
    public static void LoadGame(PlayerController player, Inventory inventory, int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"LoadGame 실패: 파일 없음 - {path}");
            return;
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveData>(json);

        // 플레이어 복원
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            bool wasEnabled = false;
            if (cc != null) { wasEnabled = cc.enabled; cc.enabled = false; }
            player.transform.position = data.playerPosition;
            if (cc != null) cc.enabled = wasEnabled;

            player.health = data.health;
            try { player.stamina = data.stamina; } catch { }
            player.coinCount = data.coinCount;
        }

        // 인벤토리 복원
        if (inventory != null)
        {
            for (int i = 0; i < 4; i++)
            {
                string savedName = (data.inventoryItemNames != null && i < data.inventoryItemNames.Count)
                                   ? data.inventoryItemNames[i] : null;

                if (!string.IsNullOrEmpty(savedName))
                {
                    var prefab = ResolveItemPrefab(savedName);
                    if (prefab != null)
                    {
                        var go = GameObject.Instantiate(prefab);   // 부모/표시는 Inventory가 관리
                        var item = go.GetComponent<Item>();
                        inventory.SetItemAt(i, item);
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveLoadManager] 인벤토리 복원 실패: '{savedName}' 프리팹을 찾지 못했습니다. " +
                                         $"검색 루트: {string.Join(", ", ITEM_ROOTS)}");
                        inventory.SetItemAt(i, null);
                    }
                }
                else
                {
                    inventory.SetItemAt(i, null);
                }
            }
        }

        // Trailer 내부 아이템 복원
        var trailer = FindTrailerRoot();
        if (trailer != null)
        {
            // 기존 '아이템' 자식만 제거 (Item 없는 장식은 유지)
            var toRemove = new List<GameObject>();
            foreach (var t in EnumerateTrailerItemChildren(trailer))
                toRemove.Add(t.gameObject);
            foreach (var go in toRemove)
                GameObject.Destroy(go);

            int restored = 0;
            if (data.trailerItems != null)
            {
                foreach (var entry in data.trailerItems)
                {
                    if (string.IsNullOrEmpty(entry.key)) continue;

                    var prefab = ResolveItemPrefab(entry.key);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"[SaveLoadManager] Trailer 복원 실패: '{entry.key}' 프리팹을 찾지 못했습니다.");
                        continue;
                    }

                    var go = GameObject.Instantiate(prefab, trailer); // 부모 먼저 지정
                    var tr = go.transform;
                    tr.localPosition = entry.localPosition;
                    tr.localRotation = entry.localRotation;
                    tr.localScale    = entry.localScale;
                    restored++;
                }
            }

            Debug.Log($"[SaveLoadManager] Trailer 복원 완료: {restored}/{data.trailerItems?.Count ?? 0}");
        }
        else
        {
            if (data.trailerItems != null && data.trailerItems.Count > 0)
                Debug.LogWarning("[SaveLoadManager] Trailer 루트를 찾지 못해 Trailer 아이템을 복원하지 못했습니다. 태그/이름 확인 요망.");
        }

        // MapTracker 일자 복원
        try
        {
            var tracker = MapTracker.Instance;
            if (tracker != null)
                tracker.currentDay = data.currentDay;
        }
        catch { }

        PlayerPrefs.SetInt("last_slot", slot);
        PlayerPrefs.Save();

        Debug.Log($"로드 완료 - 슬롯 {slot}: {path}");
    }
}
