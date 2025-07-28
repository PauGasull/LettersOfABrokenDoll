using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;
using Codice.Client.Commands;
using UnityEditor.PackageManager.UI;

#pragma warning disable UNT0011
#pragma warning disable IDE0090
public class QuickActionsWindow : EditorWindow
{
	public string KeyToDelete;

    public string editorWindowText = "Choose a file name: ";
    public string editorWindowTermination = "Choose a file extension: ";
    public string newFileName = "New File";
    public string newFileTermination = ".txt";

    public static string iconPath = "Assets/Gizmos/GameControllerGizmo.png";

    [MenuItem("Window/Quick Actions")]
	public static void ShowWindow()
	{
        QuickActionsWindow window = GetWindow<QuickActionsWindow>("Quick Actions");

        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        if (icon != null)
            window.titleContent = new GUIContent("Quick Actions", icon);
        else
            Debug.LogWarning("Icon not found at path: " + iconPath);
    }

    static void Create(string name, string term)
    {
        string copyPath = "Assets/" + name + term;
        //Debug.Log("Creating Classfile: " + copyPath);

        if (File.Exists(copyPath) == false) // do not overwrite
            using (StreamWriter outfile = new StreamWriter(copyPath))
                outfile.WriteLine("/* " + System.DateTime.Now + " */"); // File written

        AssetDatabase.Refresh();
    }

    private void OnGUI()
	{
		if (GUILayout.Button("Delete ALL PlayerPrefs"))
		{
			PlayerPrefs.DeleteAll();
			Debug.Log("<size=15><color=green>Deleted <b>All</b> PlayerPrefs</color></size>");			
		}

        GUILayout.BeginHorizontal();
        KeyToDelete = GUILayout.TextField(KeyToDelete, GUILayout.Width(150));
		if (GUILayout.Button("Delete Specific PlayerPref"))
		{
			if (KeyToDelete == "")
			{
				Debug.Log("<color=red>Add a valid PlayerPref<b> Key </b></color>");
			}
			else
			{
				PlayerPrefs.DeleteKey(KeyToDelete);
				Debug.Log("<color=green>Deleted <b>" + KeyToDelete + "</b> PlayerPref</color>");
			}
		}
        GUILayout.EndHorizontal();
		GUILayout.Space(10f);

		newFileName = EditorGUILayout.TextField(editorWindowText, newFileName);
        newFileTermination = EditorGUILayout.TextField(editorWindowTermination, newFileTermination);

        if (GUILayout.Button("Create new file")) 
        {
            Create(newFileName, newFileTermination);
            Close();
        }

        if (GUILayout.Button("Cancel"))
            Close();

        //this.Repaint();

    }
}
