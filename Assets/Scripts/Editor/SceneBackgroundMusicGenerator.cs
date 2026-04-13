using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures main project scenes have a serialized background music object.
/// </summary>
public class SceneBackgroundMusicGenerator : Editor
{
    private const string MusicObjectName = "BackgroundMusic";
    private const string MusicClipPath = "Assets/Audio/music.mp3";
    private const float DefaultVolume = 0.5f;

    private static readonly string[] ScenePaths =
    {
        "Assets/MainMenu.unity",
        "Assets/Hub.unity",
        "Assets/Lobby.unity",
        "Assets/World_Zombie.unity",
    };

    [DidReloadScripts]
    private static void RefreshScenesAfterScriptReload()
    {
        EditorApplication.delayCall += TryRefreshScenesInBackground;
    }

    [MenuItem("Tools/Audio/Refresh Scene Background Music")]
    private static void RefreshSceneBackgroundMusic()
    {
        RefreshAllScenes(false);
    }

    public static void RefreshSceneBackgroundMusicFromBatch()
    {
        RefreshAllScenes(true);
    }

    private static void RefreshAllScenes(bool keepLastOpen)
    {
        Scene previousActiveScene = SceneManager.GetActiveScene();

        foreach (string scenePath in ScenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ConfigureSceneMusic(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();

        if (!keepLastOpen && previousActiveScene.IsValid() && !string.IsNullOrEmpty(previousActiveScene.path))
        {
            EditorSceneManager.OpenScene(previousActiveScene.path, OpenSceneMode.Single);
        }
    }

    private static void TryRefreshScenesInBackground()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        const string sessionKey = "HeroAct.SceneBackgroundMusicRefreshed";
        if (SessionState.GetBool(sessionKey, false))
        {
            return;
        }

        Scene previousActiveScene = SceneManager.GetActiveScene();

        foreach (string scenePath in ScenePaths)
        {
            Scene scene = FindLoadedSceneByPath(scenePath);
            bool sceneWasAlreadyLoaded = scene.IsValid() && scene.isLoaded;

            if (!sceneWasAlreadyLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            if (scene.IsValid() && scene.isLoaded)
            {
                ConfigureSceneMusic(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (!sceneWasAlreadyLoaded && scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
        {
            SceneManager.SetActiveScene(previousActiveScene);
        }

        SessionState.SetBool(sessionKey, true);
    }

    private static void ConfigureSceneMusic(Scene scene)
    {
        AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicClipPath);
        if (musicClip == null)
        {
            Debug.LogError($"SceneBackgroundMusicGenerator: Missing music clip at {MusicClipPath}.");
            return;
        }

        GameObject musicObject = FindMusicObject(scene);
        if (musicObject == null)
        {
            musicObject = new GameObject(MusicObjectName);
            SceneManager.MoveGameObjectToScene(musicObject, scene);
        }

        musicObject.name = MusicObjectName;
        musicObject.transform.SetParent(null);
        musicObject.transform.localPosition = Vector3.zero;
        musicObject.transform.localRotation = Quaternion.identity;
        musicObject.transform.localScale = Vector3.one;

        AudioSource audioSource = musicObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = musicObject.AddComponent<AudioSource>();
        }

        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = DefaultVolume;
        audioSource.spatialBlend = 0f;

        BackgroundMusic backgroundMusic = musicObject.GetComponent<BackgroundMusic>();
        if (backgroundMusic == null)
        {
            backgroundMusic = musicObject.AddComponent<BackgroundMusic>();
        }

        backgroundMusic.musicClip = musicClip;
        backgroundMusic.volume = DefaultVolume;

        EditorUtility.SetDirty(musicObject);
    }

    private static GameObject FindMusicObject(Scene scene)
    {
        BackgroundMusic existingMusic = FindComponentInScene<BackgroundMusic>(scene);
        if (existingMusic != null)
        {
            return existingMusic.gameObject;
        }

        return FindGameObjectInScene(scene, MusicObjectName);
    }

    private static Scene FindLoadedSceneByPath(string path)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == path)
            {
                return scene;
            }
        }

        return default;
    }

    private static GameObject FindGameObjectInScene(Scene scene, string objectName)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return null;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            GameObject found = FindGameObjectRecursive(root.transform, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return null;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T found = root.GetComponentInChildren<T>(true);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static GameObject FindGameObjectRecursive(Transform parent, string objectName)
    {
        if (parent.name == objectName)
        {
            return parent.gameObject;
        }

        foreach (Transform child in parent)
        {
            GameObject found = FindGameObjectRecursive(child, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
