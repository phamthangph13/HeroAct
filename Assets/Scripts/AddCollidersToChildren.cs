using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Gắn script này vào GameObject cha (props, props2, OnTopOfTheFloor...).
/// Bấm nút "Thêm Collider Cho Tất Cả Con" trong Inspector để tự động add BoxCollider2D.
/// Xóa script sau khi dùng xong.
/// </summary>
public class AddCollidersToChildren : MonoBehaviour
{
    [Header("Tự động thêm BoxCollider2D cho tất cả object con")]
    public bool includeInactive = false;

    public void AddColliders()
    {
        int count = 0;
        foreach (Transform child in transform)
        {
            if (!includeInactive && !child.gameObject.activeInHierarchy) continue;

            // Chỉ thêm nếu chưa có collider
            if (child.GetComponent<Collider2D>() == null)
            {
                child.gameObject.AddComponent<BoxCollider2D>();
                count++;
            }
        }
        Debug.Log($"AddCollidersToChildren: Đã thêm BoxCollider2D cho {count} object con.");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AddCollidersToChildren))]
public class AddCollidersToChildrenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AddCollidersToChildren script = (AddCollidersToChildren)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Thêm Collider Cho Tất Cả Con", GUILayout.Height(40)))
        {
            script.AddColliders();
        }
    }
}
#endif
