using UnityEditor;

public static class HeroActSceneBatchRefresh
{
    public static void RefreshGeneratedSceneContent()
    {
        SceneBackgroundMusicGenerator.RefreshSceneBackgroundMusicFromBatch();
        WorldZombieVictoryPanelGenerator.RefreshWorldZombieVictoryPanelFromBatch();
        AssetDatabase.SaveAssets();
    }
}
