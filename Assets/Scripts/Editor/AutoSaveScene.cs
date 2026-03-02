using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Tự động save scene trước khi bấm Play.
/// Đặt trong thư mục Editor.
/// </summary>
[InitializeOnLoad]
public static class AutoSaveScene
{
    static AutoSaveScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // Save trước khi Enter Play mode
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("AutoSave: Đang lưu scene trước khi Play...");
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }
    }
}
 