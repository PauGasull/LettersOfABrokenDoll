using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(LetterDataLoader))]
public class LetterDataLoaderEditor : Editor
{
    private LetterDataLoader loader;

    private Dictionary<string, bool> compositionFoldouts = new();
    private Dictionary<string, Dictionary<string, bool>> blockFoldouts = new();

    private Dictionary<string, bool> responseFoldouts = new();
    private Dictionary<string, bool> pathFoldouts = new();

    private bool showComposition = true;
    private bool showLetterMeta = true;
    private bool showResponses = true;

    void OnEnable()
    {
        loader = (LetterDataLoader)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("🔍 Runtime Data Viewer", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("  This viewer only works on Runtime.", MessageType.Info);
            return;
        }

        // --- COMPOSITION ---
        showComposition = EditorGUILayout.Foldout(showComposition, $"📬 Compositions ({loader.compositionData?.Count ?? 0})", true);
        if (showComposition && loader.compositionData != null)
        {
            EditorGUI.indentLevel++;
            foreach (var kv in loader.compositionData)
            {
                string compId = kv.Key;
                var comp = kv.Value;

                if (!compositionFoldouts.ContainsKey(compId))
                    compositionFoldouts[compId] = false;

                compositionFoldouts[compId] = EditorGUILayout.Foldout(compositionFoldouts[compId], $"📄 {compId} (root: {comp.root_block})", true);

                if (compositionFoldouts[compId])
                {
                    EditorGUI.indentLevel++;
                    foreach (var blockKv in comp.blocks)
                    {
                        string blockId = blockKv.Key;
                        var block = blockKv.Value;

                        if (!blockFoldouts.ContainsKey(compId))
                            blockFoldouts[compId] = new();

                        if (!blockFoldouts[compId].ContainsKey(blockId))
                            blockFoldouts[compId][blockId] = false;

                        blockFoldouts[compId][blockId] = EditorGUILayout.Foldout(blockFoldouts[compId][blockId], $"🔹 Block {blockId}: {block.prompt}", true);

                        if (blockFoldouts[compId][blockId])
                        {
                            EditorGUI.indentLevel++;
                            foreach (var opt in block.options)
                            {
                                EditorGUILayout.LabelField($"▶ {opt.Key}: {opt.Value.short_text}");
                                EditorGUILayout.LabelField("   ↪ long_text:", opt.Value.long_text.Trim());
                                EditorGUILayout.LabelField("   ↪ next:", opt.Value.next ?? "null");
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        // --- LETTER METADATA ---
        showLetterMeta = EditorGUILayout.Foldout(showLetterMeta, $"🕓 Letter Metadata ({loader.letterMetaData?.Count ?? 0})", true);
        if (showLetterMeta && loader.letterMetaData != null)
        {
            EditorGUI.indentLevel++;
            foreach (var kv in loader.letterMetaData)
            {
                EditorGUILayout.LabelField($"✉️ {kv.Key} → {kv.Value.delay_seconds} s");
            }
            EditorGUI.indentLevel--;
        }

        // --- RESPONSES ---
        showResponses = EditorGUILayout.Foldout(showResponses, $"💬 Responses ({loader.responseData?.Count ?? 0})", true);
        if (showResponses && loader.responseData != null)
        {
            EditorGUI.indentLevel++;
            foreach (var kv in loader.responseData)
            {
                string respId = kv.Key;
                var resp = kv.Value;

                if (!responseFoldouts.ContainsKey(respId))
                    responseFoldouts[respId] = false;

                responseFoldouts[respId] = EditorGUILayout.Foldout(responseFoldouts[respId], $"🧾 {respId}", true);

                if (responseFoldouts[respId])
                {
                    // PATHS
                    EditorGUILayout.LabelField("Paths:");
                    EditorGUI.indentLevel++;
                    foreach (var pathKv in resp.paths)
                    {
                        string path = pathKv.Key;

                        if (!pathFoldouts.ContainsKey(path))
                            pathFoldouts[path] = false;

                        pathFoldouts[path] = EditorGUILayout.Foldout(pathFoldouts[path], $"🔸 {path}", true);

                        if (pathFoldouts[path])
                        {
                            foreach (var blockId in pathKv.Value.blocks)
                            {
                                if (resp.responses.ContainsKey(blockId))
                                {
                                    string content = resp.responses[blockId].content;
                                    EditorGUILayout.TextArea($"📝 {blockId}: {content}", GUILayout.Height(40));
                                }
                            }
                        }
                    }
                    EditorGUI.indentLevel--;

                    // RESPONSES sense path
                    EditorGUILayout.LabelField("Other Response Blocks:");
                    EditorGUI.indentLevel++;
                    foreach (var r in resp.responses)
                    {
                        if (!IsResponseInAnyPath(resp.paths, r.Key))
                        {
                            EditorGUILayout.TextArea($"📝 {r.Key}: {r.Value.content}", GUILayout.Height(40));
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }
    }

    private bool IsResponseInAnyPath(Dictionary<string, ResponsePath> paths, string responseKey)
    {
        foreach (var path in paths.Values)
        {
            if (path.blocks.Contains(responseKey))
                return true;
        }
        return false;
    }
}
