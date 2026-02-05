#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InstanceData {
    public string name;
    public float[] position;
    public float[] rotation;
    public float[] scale;
}

[ExecuteInEditMode]
public class MapObjectPlacement : MonoBehaviour {
    [Header("Map Data (JSON)")]
    public TextAsset jsonFile;

    [Header("Prefabs (match by name)")]
    public List<GameObject> prefabs = new();

    private Dictionary<string, GameObject> prefabMap;

    private void BuildPrefabMap() {
        prefabMap = new Dictionary<string, GameObject>();
        foreach (var prefab in prefabs) {
            if (prefab == null) continue;
            if (!prefabMap.ContainsKey(prefab.name))
                prefabMap.Add(prefab.name, prefab);
        }
    }

    public void PlaceObjects() {
        if (jsonFile == null) {
            Debug.LogWarning("Keine JSON-Datei referenziert!");
            return;
        }

        BuildPrefabMap();

        // DeletePlacedObjects();

        string json = jsonFile.text;
        InstanceData[] instances;
        try {
            instances = JsonHelper.FromJson<InstanceData>(json);
        } catch {
            Debug.LogError("Fehler beim Lesen der JSON-Datei!");
            return;
        }

        if (instances == null || instances.Length == 0) {
            Debug.LogWarning("Keine Objekte in der JSON gefunden.");
            return;
        }

        // "Objects" Container erstellen (auf gleicher Hierarchie-Ebene)
        GameObject parent = new("Objects");
        Undo.RegisterCreatedObjectUndo(parent, "Create Objects Container");
        // Auf gleicher Ebene platzieren
        if (transform.parent != null)
            parent.transform.parent = transform.parent;

        foreach (var inst in instances) {
            if (!prefabMap.TryGetValue(inst.name, out GameObject prefab)) {
                Debug.LogWarning($"Kein Prefab gefunden f�r '{inst.name}'.");
                continue;
            }
            
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = new Vector3(inst.position[0], inst.position[2], inst.position[1]);
            obj.transform.eulerAngles = new Vector3(-inst.rotation[0], -inst.rotation[2], inst.rotation[1]);
            obj.transform.localScale = new Vector3(inst.scale[0], inst.scale[2], inst.scale[1]);
            obj.transform.SetParent(parent.transform);
        }

        Debug.Log($"Platziert {instances.Length} Objekte unter '{parent.name}'.");
    }

    public void DeletePlacedObjects() {
        Transform existing = transform.parent
            ? transform.parent.Find("Objects")
            : GameObject.Find("Objects")?.transform;

        if (existing != null) {
            Undo.DestroyObjectImmediate(existing.gameObject);
            Debug.Log("Vorherige 'Objects'-Instanz gel�scht.");
        }
    }
}

// ------------------------------------------------------------
// JSON Helper f�r Arrays
// ------------------------------------------------------------
public static class JsonHelper {
    public static T[] FromJson<T>(string json) {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T> {
        public T[] array;
    }
}

// ------------------------------------------------------------
// Custom Editor
// ------------------------------------------------------------
[CustomEditor(typeof(MapObjectPlacement))]
public class MapPlacerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MapObjectPlacement placer = (MapObjectPlacement)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Place Objects")) {
            placer.PlaceObjects();
        }

        if (GUILayout.Button("Delete Placed Objects")) {
            placer.DeletePlacedObjects();
        }
    }
}
#endif
