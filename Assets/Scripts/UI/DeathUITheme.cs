using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "HeroAct/UI/Death UI Theme", fileName = "DeathUITheme")]
public class DeathUITheme : ScriptableObject
{
    public TMP_FontAsset font;
    public Sprite panelSprite;
    public Sprite innerPanelSprite;
    public Sprite titleBannerSprite;
    public Sprite rowSprite;
    public Sprite primaryButtonSprite;
    public Sprite secondaryButtonSprite;
}
