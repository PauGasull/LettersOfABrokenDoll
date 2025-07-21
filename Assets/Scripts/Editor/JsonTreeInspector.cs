using UnityEngine;
using UnityEditor;
using SimpleJSON;
using System.Collections.Generic;

[CustomEditor(typeof(TextAsset))]
public class JsonTreeInspector : Editor
{
    private JSONNode rootNode;
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();
    private Vector2 scroll;

    public override void OnInspectorGUI()
    {
        // Live load colors
        Color keyColor     = LoadColor("JTI_KeyColor",     Color.gray);
        Color stringColor  = LoadColor("JTI_StringColor",  new Color(0.678f, 1f, 0.18f));
        Color numberColor  = LoadColor("JTI_NumberColor",  new Color(0f, 0.749f, 1f));
        Color booleanColor = LoadColor("JTI_BooleanColor", new Color(1f, 0.271f, 0f));
        Color nullColor    = LoadColor("JTI_NullColor",    new Color(0f, 0.808f, 0.820f));

        // Settings button
        GUILayout.Space(4);
        Rect settingsRect = GUILayoutUtility.GetRect(new GUIContent("⚙️ JSON Tree Settings"), GUI.skin.button);
        bool settingsClicked = GUI.Button(settingsRect, "⚙️ JSON Tree Settings");
if (!settingsClicked && Event.current.rawType == EventType.MouseDown && settingsRect.Contains(Event.current.mousePosition)) {
    settingsClicked = true;
    Event.current.Use();
}
        if (settingsClicked)
            JsonTreeSettingsWindow.ShowWindow();

        TextAsset asset = (TextAsset)target;
        string text = asset != null ? asset.text : string.Empty;
        string trimmed = text.TrimStart();
        bool isJson = asset != null && (asset.name.EndsWith(".json") || trimmed.StartsWith("{") || trimmed.StartsWith("["));

        if (!isJson)
        {
            // Plain text viewer
            EditorGUILayout.LabelField("File Content:", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(text, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            // Copy button
            Rect copyRect = GUILayoutUtility.GetRect(new GUIContent("📋 Copy to clipboard"), GUI.skin.button);
            bool copyClicked = GUI.Button(copyRect, "📋 Copy to clipboard");
if (!copyClicked && Event.current.rawType == EventType.MouseDown && copyRect.Contains(Event.current.mousePosition)) {
    copyClicked = true;
    Event.current.Use();
}
            if (copyClicked)
                EditorGUIUtility.systemCopyBuffer = text;

            return;
        }

        // Parse JSON fresh
        try
        {
            rootNode = JSON.Parse(text);
        }
        catch
        {
            EditorGUILayout.HelpBox("This JSON file is not valid.", MessageType.Error);
            return;
        }

        if (rootNode == null)
        {
            EditorGUILayout.HelpBox("Failed to parse JSON.", MessageType.Error);
            return;
        }

        // JSON tree viewer
        EditorGUILayout.LabelField("JSON Tree Viewer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawJsonNode("root", rootNode, 0, keyColor, stringColor, numberColor, booleanColor, nullColor);
        EditorGUILayout.EndScrollView();
    }

    private void DrawJsonNode(string key, JSONNode node, int indent, Color keyColor, Color strColor, Color numColor, Color boolColor, Color nullColor)
    {
        EditorGUI.indentLevel = indent;
        string displayKey = string.IsNullOrEmpty(key) ? "[root]" : key;

        if (node.IsObject || node.IsArray)
        {
            string foldoutKey = displayKey + node.GetHashCode();
            if (!foldouts.ContainsKey(foldoutKey))
                foldouts[foldoutKey] = true;

            string symbol = node.IsArray ? "[ ]" : "{ }";
            string countInfo = node.Count > 0 ? $" ({node.Count})" : string.Empty;
            foldouts[foldoutKey] = EditorGUILayout.Foldout(foldouts[foldoutKey], $"{displayKey} {symbol}{countInfo}");

            if (foldouts[foldoutKey])
            {
                foreach (var child in node)
                    DrawJsonNode(child.Key, child.Value, indent + 1, keyColor, strColor, numColor, boolColor, nullColor);
            }
        }
        else
        {
            string value = node.ToString().Replace("\n", "\\n");
            Color valueColor = node.IsNumber   ? numColor :
                                node.IsBoolean  ? boolColor :
                                node.IsNull     ? nullColor :
                                                  strColor;
            GUIStyle style = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = false };
            string hexKey   = ColorUtility.ToHtmlStringRGB(keyColor);
            string hexValue = ColorUtility.ToHtmlStringRGB(valueColor);
            EditorGUILayout.LabelField($"<color=#{hexKey}>{displayKey}:</color> <color=#{hexValue}>{value}</color>", style);
        }
    }

    private static Color LoadColor(string key, Color defaultColor)
    {
        string hex = EditorPrefs.GetString(key, ColorUtility.ToHtmlStringRGB(defaultColor));
        return ColorUtility.TryParseHtmlString("#" + hex, out var c) ? c : defaultColor;
    }
}

public class JsonTreeSettingsWindow : EditorWindow
{
    private Color keyColor;
    private Color stringColor;
    private Color numberColor;
    private Color booleanColor;
    private Color nullColor;

    public static void ShowWindow()
    {
        var window = GetWindow<JsonTreeSettingsWindow>("JSON Tree Settings");
        window.LoadPrefs();
        window.Show();
    }

    private void OnEnable() => LoadPrefs();

    private void OnGUI()
    {
        GUILayout.Label("Customize JSON Tree Colors (Live Preview)", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        keyColor = EditorGUILayout.ColorField("Key Color", keyColor);
        if (EditorGUI.EndChangeCheck()) ApplyColor("JTI_KeyColor", keyColor);

        EditorGUI.BeginChangeCheck();
        stringColor = EditorGUILayout.ColorField("String Color", stringColor);
        if (EditorGUI.EndChangeCheck()) ApplyColor("JTI_StringColor", stringColor);

        EditorGUI.BeginChangeCheck();
        numberColor = EditorGUILayout.ColorField("Number Color", numberColor);
        if (EditorGUI.EndChangeCheck()) ApplyColor("JTI_NumberColor", numberColor);

        EditorGUI.BeginChangeCheck();
        booleanColor = EditorGUILayout.ColorField("Boolean Color", booleanColor);
        if (EditorGUI.EndChangeCheck()) ApplyColor("JTI_BooleanColor", booleanColor);

        EditorGUI.BeginChangeCheck();
        nullColor = EditorGUILayout.ColorField("Null Color", nullColor);
        if (EditorGUI.EndChangeCheck()) ApplyColor("JTI_NullColor", nullColor);
    }

    private void LoadPrefs()
    {
        keyColor     = LoadColor("JTI_KeyColor",     Color.gray);
        stringColor  = LoadColor("JTI_StringColor",  new Color(0.678f, 1f, 0.18f));
        numberColor  = LoadColor("JTI_NumberColor",  new Color(0f, 0.749f, 1f));
        booleanColor = LoadColor("JTI_BooleanColor", new Color(1f, 0.271f, 0f));
        nullColor    = LoadColor("JTI_NullColor",    new Color(0f, 0.808f, 0.820f));
    }

    private void ApplyColor(string key, Color color)
    {
        EditorPrefs.SetString(key, ColorUtility.ToHtmlStringRGB(color));
        foreach (var win in Resources.FindObjectsOfTypeAll<EditorWindow>()) win.Repaint();
    }

    private static Color LoadColor(string key, Color defaultColor)
    {
        string hex = EditorPrefs.GetString(key, ColorUtility.ToHtmlStringRGB(defaultColor));
        return ColorUtility.TryParseHtmlString("#" + hex, out var c) ? c : defaultColor;
    }
}
