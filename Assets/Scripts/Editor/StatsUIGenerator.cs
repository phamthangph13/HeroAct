using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor tool: Tạo UI bảng nâng chỉ số 1 lần trong Editor.
/// Chuột phải StatsPanel → "Generate Stats UI"
/// </summary>
public class StatsUIGenerator : Editor
{
    [MenuItem("GameObject/UI/Generate Stats UI", false, 10)]
    static void GenerateStatsUI()
    {
        GameObject panel = Selection.activeGameObject;
        if (panel == null)
        {
            Debug.LogError("Hãy chọn StatsPanel trước khi generate!");
            return;
        }

        // Xóa con cũ
        while (panel.transform.childCount > 0)
        {
            DestroyImmediate(panel.transform.GetChild(0).gameObject);
        }

        // Colors
        Color rowColor = new Color(0f, 0f, 0f, 0.35f);
        Color textColor = new Color(1f, 0.95f, 0.85f);
        Color btnColor = new Color(0.15f, 0.4f, 0.15f, 0.8f);
        Color coinColor = new Color(1f, 0.85f, 0f);

        // Setup panel
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(750, 380);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.localScale = Vector3.one;

        Image panelImg = panel.GetComponent<Image>();
        if (panelImg != null)
            panelImg.color = Color.white;

        // Vertical layout
        VerticalLayoutGroup mainLayout = panel.GetComponent<VerticalLayoutGroup>();
        if (mainLayout == null) mainLayout = panel.AddComponent<VerticalLayoutGroup>();
        mainLayout.padding = new RectOffset(105, 105, 100, 90);
        mainLayout.spacing = 4;
        mainLayout.childAlignment = TextAnchor.UpperCenter;
        mainLayout.childControlWidth = true;
        mainLayout.childControlHeight = false;
        mainLayout.childForceExpandWidth = true;
        mainLayout.childForceExpandHeight = false;

        // === Header ===
        GameObject header = CreateObj("Header", panel);
        header.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        HorizontalLayoutGroup hLayout = header.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;

        Text titleText = MakeText(header, "Title", "NÂNG CHỈ SỐ", 16, TextAnchor.MiddleCenter, coinColor);
        titleText.fontStyle = FontStyle.Bold;
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 0);

        Text coinText = MakeText(header, "CoinText", "💰 0", 14, TextAnchor.MiddleCenter, coinColor);
        coinText.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 0);

        // === Divider ===
        GameObject divider = CreateObj("Divider", panel);
        divider.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 2);
        divider.AddComponent<Image>().color = new Color(1f, 0.85f, 0f, 0.3f);

        // === Columns ===
        GameObject columns = CreateObj("Columns", panel);
        columns.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 130);
        HorizontalLayoutGroup colLayout = columns.AddComponent<HorizontalLayoutGroup>();
        colLayout.spacing = 10;
        colLayout.childAlignment = TextAnchor.UpperCenter;
        colLayout.childControlWidth = true;
        colLayout.childControlHeight = true;
        colLayout.childForceExpandWidth = true;
        colLayout.childForceExpandHeight = true;

        GameObject leftCol = CreateColumn(columns, "LeftColumn");
        GameObject rightCol = CreateColumn(columns, "RightColumn");

        // === Stat Rows ===
        string[] names = { "damage", "hp", "armor", "critRate", "critDamage", "goldDrop", "lifesteal" };
        string[] labels = { "Sát thương", "Máu", "Giáp", "Chí mạng", "ST chí mạng", "Rơi vàng", "Hút máu" };
        string[] icons = { "⚔", "❤", "🛡", "🎯", "💥", "💰", "♥" };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject parent = (i < 4) ? leftCol : rightCol;
            CreateStatRow(parent, names[i], $"{icons[i]} {labels[i]}", rowColor, textColor, btnColor, coinColor);
        }

        // === Close Button ===
        GameObject closeBtn = CreateObj("CloseBtn", panel);
        closeBtn.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 28);
        closeBtn.AddComponent<Image>().color = new Color(0.5f, 0.1f, 0.1f, 0.8f);
        closeBtn.AddComponent<Button>();
        Text closeText = MakeText(closeBtn, "Text", "ĐÓNG", 14, TextAnchor.MiddleCenter, Color.white);
        closeText.fontStyle = FontStyle.Bold;

        // Auto-link StatsUIManager
        StatsUIManager mgr = panel.GetComponent<StatsUIManager>();
        if (mgr == null) mgr = panel.AddComponent<StatsUIManager>();

        // Mark dirty
        EditorUtility.SetDirty(panel);
        Undo.RegisterCreatedObjectUndo(panel, "Generate Stats UI");

        Debug.Log("✅ Stats UI đã được tạo thành công! Nhớ Save Scene (Ctrl+S).");
    }

    static GameObject CreateColumn(GameObject parent, string name)
    {
        GameObject col = CreateObj(name, parent);
        col.AddComponent<RectTransform>();
        VerticalLayoutGroup vl = col.AddComponent<VerticalLayoutGroup>();
        vl.spacing = 4;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childControlWidth = true;
        vl.childControlHeight = false;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        return col;
    }

    static void CreateStatRow(GameObject parent, string statName, string label,
        Color rowColor, Color textColor, Color btnColor, Color coinColor)
    {
        GameObject row = CreateObj($"Row_{statName}", parent);
        row.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 32);
        row.AddComponent<Image>().color = rowColor;

        HorizontalLayoutGroup hl = row.AddComponent<HorizontalLayoutGroup>();
        hl.padding = new RectOffset(8, 8, 4, 4);
        hl.spacing = 5;
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.childControlWidth = false;
        hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;

        Text nameT = MakeText(row, "Name", label, 11, TextAnchor.MiddleLeft, textColor);
        nameT.GetComponent<RectTransform>().sizeDelta = new Vector2(110, 0);

        Text valueT = MakeText(row, "Value", "0", 12, TextAnchor.MiddleCenter, coinColor);
        valueT.fontStyle = FontStyle.Bold;
        valueT.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 0);

        Text levelT = MakeText(row, "Level", "Lv.0", 10, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f));
        levelT.GetComponent<RectTransform>().sizeDelta = new Vector2(35, 0);

        GameObject btn = CreateObj("UpgradeBtn", row);
        btn.AddComponent<RectTransform>().sizeDelta = new Vector2(60, 0);
        Image btnImg = btn.AddComponent<Image>();
        btnImg.color = btnColor;
        Button button = btn.AddComponent<Button>();
        ColorBlock cb = button.colors;
        cb.normalColor = btnColor;
        cb.highlightedColor = new Color(0.3f, 0.7f, 0.3f);
        cb.pressedColor = new Color(0.15f, 0.5f, 0.15f);
        cb.disabledColor = new Color(0.3f, 0.3f, 0.3f);
        button.colors = cb;

        MakeText(btn, "Cost", "💰10", 11, TextAnchor.MiddleCenter, Color.white);
    }

    static GameObject CreateObj(string name, GameObject parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        return obj;
    }

    static Text MakeText(GameObject parent, string name, string content, int size, TextAnchor anchor, Color color)
    {
        GameObject obj = CreateObj(name, parent);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
