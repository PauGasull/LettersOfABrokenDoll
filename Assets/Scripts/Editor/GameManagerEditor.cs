using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "deleteSaveState");

        EditorGUILayout.Space();

        SerializedProperty prop = serializedObject.FindProperty("deleteSaveState");
        bool current = prop.boolValue;

        Color originalBg = GUI.backgroundColor;
        if (current && ColorUtility.TryParseHtmlString("#eb4949", out Color htmlRed))
            GUI.backgroundColor = htmlRed;

        bool toggled = GUILayout.Toggle(current, "Delete Save State", GUI.skin.button);

        GUI.backgroundColor = originalBg;
        if (toggled != current)
            prop.boolValue = toggled;

        serializedObject.ApplyModifiedProperties();
    }
}
