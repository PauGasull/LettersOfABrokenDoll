using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

public class GraphEditorWindow : EditorWindow
{
    private CompositionGraphView _graphView;

    [MenuItem("Tools/Letter Composition Editor")]
    public static void Open()
    {
        var window = GetWindow<GraphEditorWindow>();
        window.titleContent = new GUIContent("Letter Graph Editor");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    private void ConstructGraphView()
    {
        _graphView = new CompositionGraphView { name = "Letter Graph" };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    public void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var nodeMenu = new ToolbarMenu { text = "Create Node" };
        nodeMenu.menu.AppendAction("Start", _ => _graphView.CreateStartNode(new Vector2(100, 200)));
        nodeMenu.menu.AppendAction("Response", _ => _graphView.CreateResponseNode(new Vector2(400, 200)));
        nodeMenu.menu.AppendAction("Generic", _ => _graphView.CreateGenericNode(new Vector2(700, 200)));
        toolbar.Add(nodeMenu);

        var pathButton = new Button(() => _graphView.GenerateAllPaths()) { text = "Generate Paths" };
        toolbar.Add(pathButton);

        rootVisualElement.Add(toolbar);
    }
}