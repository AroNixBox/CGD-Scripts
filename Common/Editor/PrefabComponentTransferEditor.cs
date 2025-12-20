using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

[CustomEditor(typeof(PrefabComponentTransfer))]
public class PrefabComponentTransferEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Transfer Mesh Components from Child")) {
            Transfer();
        }
    }

    void Transfer() {
        var parent = ((PrefabComponentTransfer)target).gameObject;

        // Child mit gleichem Namen suchen
        Transform child = null;
        foreach (Transform t in parent.transform) {
            if (t.name == parent.name) {
                child = t;
                break;
            }
        }

        if (child == null) {
            Debug.LogWarning("Kein Child mit gleichem Namen gefunden.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(parent, "Transfer Mesh Components");

        MoveComponent<MeshCollider>(child.gameObject, parent);
        MoveComponent<MeshRenderer>(child.gameObject, parent);
        MoveComponent<MeshFilter>(child.gameObject, parent);
        
        Undo.DestroyObjectImmediate(child.gameObject);

        Debug.Log("Mesh Components erfolgreich transferiert.");
    }

    void MoveComponent<T>(GameObject from, GameObject to) where T : Component {
        var source = from.GetComponent<T>();
        if (!source) return;

        // Kopieren
        ComponentUtility.CopyComponent(source);
        ComponentUtility.PasteComponentAsNew(to);

        // Neu hinzugefügte Komponente nach oben schieben
        var newComp = to.GetComponent<T>();
        for (int i = 0; i < 10; i++)
            ComponentUtility.MoveComponentUp(newComp);

        // Original entfernen
        Undo.DestroyObjectImmediate(source);
    }
}
