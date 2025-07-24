using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/***
 * LetterGraphEditorWindow: Main editor window for letter graphs
 * PRE: Unity editor environment
 * POST: Opens a window with graph view and configuration panel
 ***/
public class LetterGraphEditorWindow : EditorWindow
{
    private LetterGraphView graphView;
    private string baseFolder = "Assets/Letters";
    private string compositionFile = "composition.json";
    private string letterFile = "letter.json";
    private string responsesFile = "responses.json";

    [MenuItem("Window/Letter Graph Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<LetterGraphEditorWindow>();
        var icon = EditorGUIUtility.IconContent("GraphModel Icon").image;
        window.titleContent = new GUIContent("Letter Graph Editor", icon);
        window.minSize = new Vector2(600, 400);
    }

    private void OnEnable()
    {
        ConstructGraphView();
        ConstructConfigBoard();
    }

    private void OnDisable()
    {
        if (graphView != null)
            rootVisualElement.Remove(graphView);
    }

    /***
     * ConstructGraphView(): create and configure the main graph view
     * PRE: Window enabled
     * POST: Graph view added to root element
     ***/
    private void ConstructGraphView()
    {
        graphView = new LetterGraphView { name = "Letter Graph View" };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    /***
     * ConstructConfigBoard(): create the configuration blackboard
     * PRE: Window enabled
     * POST: Blackboard with fields and buttons added
     ***/
    private void ConstructConfigBoard()
    {
        var board = new Blackboard(graphView) { title = "Configuration" };
        board.SetPosition(new Rect(10, position.height - 320, 260, 300));

        var baseField = new TextField("Base folder path") { value = baseFolder };
        baseField.RegisterValueChangedCallback(e => baseFolder = e.newValue);
        board.Add(baseField);

        var compField = new TextField("composition.json") { value = compositionFile };
        compField.RegisterValueChangedCallback(e => compositionFile = e.newValue);
        board.Add(compField);

        var letterField = new TextField("letter.json") { value = letterFile };
        letterField.RegisterValueChangedCallback(e => letterFile = e.newValue);
        board.Add(letterField);

        var respField = new TextField("responses.json") { value = responsesFile };
        respField.RegisterValueChangedCallback(e => responsesFile = e.newValue);
        board.Add(respField);

        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.Add(new Button(() => LoadGraph()) { text = "Load" });
        row.Add(new Button(() => SaveGraph()) { text = "Save" });
        board.Add(row);

        rootVisualElement.Add(board);
    }

    /***
     * LoadGraph(): load json files into the graph view
     * PRE: Files exist on disk
     * POST: Graph view populated from json
     ***/
    public void LoadGraph()
    {
        string compPath = Path.Combine(baseFolder, compositionFile);
        string letterPath = Path.Combine(baseFolder, letterFile);
        string respPath = Path.Combine(baseFolder, responsesFile);
        GraphSaveUtility.LoadGraph(graphView, compPath, letterPath, respPath);
    }

    /***
     * SaveGraph(): serialize the graph view into json files
     * PRE: Graph view contains LetterNodes
     * POST: JSON files overwritten
     ***/
    public void SaveGraph()
    {
        string compPath = Path.Combine(baseFolder, compositionFile);
        string letterPath = Path.Combine(baseFolder, letterFile);
        string respPath = Path.Combine(baseFolder, responsesFile);
        GraphSaveUtility.SaveGraph(graphView, compPath, letterPath, respPath);
    }
}

/***
 * LetterGraphView: Custom graph view for editing letter nodes
 ***/
public class LetterGraphView : GraphView
{
    public LetterGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        style.flexGrow = 1;
    }

    /*** CreateNode(data, position): add a new LetterNode to the view ***/
    public LetterNode CreateNode(CompositionNodeData data, Vector2 position)
    {
        var node = new LetterNode(data);
        AddElement(node);
        node.SetPosition(new Rect(position, new Vector2(150, 260)));
        return node;
    }

    /*** ClearGraph(): remove all nodes and edges from the view ***/
    public void ClearGraph()
    {
        DeleteElements(graphElements.ToList());
    }

    /*** HighlightMissingConnections(): highlight nodes lacking connections ***/
    public void HighlightMissingConnections()
    {
        foreach (var n in nodes.ToList())
        {
            if (n is LetterNode ln)
            {
                bool missing = ln.InputPort.connected == false;
                foreach (var port in ln.OptionPorts.Values)
                    missing |= !port.connected;

                ln.titleContainer.style.backgroundColor = missing ? new Color(1f, 0f, 0f, 0.3f) : Color.clear;
            }
        }
    }

    /***
     * BuildContextualMenu(evt): add custom actions to context menu
     * PRE: Right click event
     * POST: Menu populated with actions
     ***/
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        Vector2 pos = evt.localMousePosition;
        evt.menu.AppendAction("New Block", a =>
        {
            var data = new CompositionNodeData
            {
                id = Guid.NewGuid().ToString(),
                shortText = "New Block",
                longText = string.Empty,
                delay = 0f,
                options = new Dictionary<string, NodeOptionData>
                {
                    {"A", new NodeOptionData{ shortText = "Option A", longText = string.Empty }},
                    {"B", new NodeOptionData{ shortText = "Option B", longText = string.Empty }}
                }
            };
            CreateNode(data, pos);
        }, DropdownMenuAction.AlwaysEnabled);

        if (evt.target is LetterNode node)
        {
            evt.menu.AppendAction("Delete Node", a => RemoveElement(node), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Center On Node", a =>
            {
                ClearSelection();
                AddToSelection(node);
                FrameSelection();
            }, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Edit Metadata", a => Debug.Log($"Edit metadata for {node.Id}"), DropdownMenuAction.AlwaysEnabled);
        }
    }
}

/***
 * LetterNode: GraphView node representing a composition block
 ***/
public class LetterNode : Node
{
    public CompositionNodeData Data { get; private set; }
    public string Id => Data.id;
    public Port InputPort { get; private set; }
    private Dictionary<string, Port> optionPorts = new();
    private TextField shortField;
    private TextField longField;
    private FloatField delayField;

    public LetterNode(CompositionNodeData data)
    {
        Data = data;
        title = data.id;

        shortField = new TextField { value = data.shortText };
        shortField.RegisterValueChangedCallback(e => Data.shortText = e.newValue);
        titleContainer.Add(shortField);

        longField = new TextField { value = data.longText, multiline = true };
        longField.RegisterValueChangedCallback(e => Data.longText = e.newValue);
        mainContainer.Add(longField);

        delayField = new FloatField("Delay") { value = data.delay };
        delayField.RegisterValueChangedCallback(e => Data.delay = e.newValue);
        mainContainer.Add(delayField);

        InputPort = Port.Create<Edge>(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(float));
        InputPort.portName = "In";
        inputContainer.Add(InputPort);

        foreach (var kv in Data.options)
        {
            CreateOptionPort(kv.Key, kv.Value);
        }

        var addBtn = new Button(AddOption) { text = "Add Option" };
        mainContainer.Add(addBtn);

        RefreshExpandedState();
        RefreshPorts();
    }

    /*** OptionPorts: dictionary of output ports by label ***/
    public Dictionary<string, Port> OptionPorts => optionPorts;

    /*** CreateOptionPort(label, data): add UI for an option ***/
    private void CreateOptionPort(string label, NodeOptionData optData)
    {
        var shortF = new TextField($"{label} Short") { value = optData.shortText };
        shortF.RegisterValueChangedCallback(e => optData.shortText = e.newValue);
        mainContainer.Add(shortF);

        var longF = new TextField($"{label} Long") { value = optData.longText, multiline = true };
        longF.RegisterValueChangedCallback(e => optData.longText = e.newValue);
        mainContainer.Add(longF);

        var port = Port.Create<Edge>(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(float));
        port.portName = label;
        optionPorts[label] = port;
        outputContainer.Add(port);
    }

    /*** AddOption(): create a new option labelled next letter ***/
    private void AddOption()
    {
        char c = (char)('A' + optionPorts.Count);
        string label = c.ToString();
        var opt = new NodeOptionData { shortText = $"Option {label}", longText = string.Empty };
        Data.options[label] = opt;
        CreateOptionPort(label, opt);
        RefreshPorts();
    }
}

/*** Serializable data classes used by the graph ***/
[Serializable]
public class CompositionRoot
{
    public List<CompositionNodeData> nodes = new();
}

[Serializable]
public class CompositionNodeData
{
    public string id;
    public string shortText;
    public string longText;
    public float delay;
    public Dictionary<string, NodeOptionData> options = new();
    public Dictionary<string, string> nextIds = new();
    public Vector2 position;
}

[Serializable]
public class NodeOptionData
{
    public string shortText;
    public string longText;
}

[Serializable]
public class LetterData
{
    public int delay;
    public string music;
    public string sender;
    public List<string> attachments;
}

[Serializable]
public class ResponseData
{
    public List<ResponseEntry> responses = new();
}

[Serializable]
public class ResponseEntry
{
    public string choiceId;
    public string content;
}

/*** GraphSaveUtility: load and save the graph view to disk ***/
public static class GraphSaveUtility
{
    /*** SaveGraph(view, paths): serialize graph to json files ***/
    public static void SaveGraph(LetterGraphView view, string compPath, string letterPath, string respPath)
    {
        SaveComposition(view, compPath);
        SaveDelays(view, letterPath);
        SaveResponses(view, respPath);
    }

    /*** LoadGraph(view, paths): populate graph from json files ***/
    public static void LoadGraph(LetterGraphView view, string compPath, string letterPath, string respPath)
    {
        LoadComposition(view, compPath);
        LoadDelays(view, letterPath);
        LoadResponses(view, respPath);
        view.HighlightMissingConnections();
    }

    /*** SaveComposition(view, path): write composition.json ***/
    public static void SaveComposition(LetterGraphView view, string path)
    {
        var root = new CompositionRoot();
        foreach (var node in view.nodes.ToList())
        {
            if (node is not LetterNode ln) continue;
            ln.Data.position = ln.GetPosition().position;
            ln.Data.nextIds.Clear();

            foreach (var kv in ln.OptionPorts)
            {
                foreach (var edge in kv.Value.connections)
                {
                    if (edge.input.node is LetterNode target)
                    {
                        ln.Data.nextIds[kv.Key] = target.Id;
                        break;
                    }
                }
            }

            root.nodes.Add(ln.Data);
        }

        string json = JsonConvert.SerializeObject(root, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log($"Composition saved to {path}");
    }

    /*** SaveDelays(view, path): write letter.json ***/
    public static void SaveDelays(LetterGraphView view, string path)
    {
        var map = new Dictionary<string, LetterData>();
        foreach (var node in view.nodes.ToList())
        {
            if (node is not LetterNode ln) continue;
            map[ln.Id] = new LetterData { delay = Mathf.RoundToInt(ln.Data.delay) };
        }

        string json = JsonConvert.SerializeObject(map, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log($"Letters saved to {path}");
    }

    /*** SaveResponses(view, path): placeholder export ***/
    public static void SaveResponses(LetterGraphView view, string path)
    {
        var map = new Dictionary<string, ResponseData>();
        string json = JsonConvert.SerializeObject(map, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    /*** LoadComposition(view, path): read composition.json ***/
    public static void LoadComposition(LetterGraphView view, string path)
    {
        view.ClearGraph();
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Composition file missing: {path}");
            return;
        }

        CompositionRoot root = null;
        try
        {
            root = JsonConvert.DeserializeObject<CompositionRoot>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse composition: {e.Message}");
            return;
        }

        if (root == null || root.nodes == null)
            return;

        var nodesById = new Dictionary<string, LetterNode>();
        foreach (var data in root.nodes)
        {
            var node = view.CreateNode(data, data.position);
            nodesById[data.id] = node;
        }

        foreach (var data in root.nodes)
        {
            if (!nodesById.TryGetValue(data.id, out var src)) continue;
            foreach (var kv in data.nextIds)
            {
                if (!nodesById.TryGetValue(kv.Value, out var dst)) continue;
                if (src.OptionPorts.TryGetValue(kv.Key, out var port))
                {
                    var edge = port.ConnectTo(dst.InputPort);
                    view.AddElement(edge);
                }
            }
        }
    }

    /*** LoadDelays(view, path): apply delay values ***/
    public static void LoadDelays(LetterGraphView view, string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Letter meta file missing: {path}");
            return;
        }
        Dictionary<string, LetterData> map = null;
        try
        {
            map = JsonConvert.DeserializeObject<Dictionary<string, LetterData>>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse letter data: {e.Message}");
            return;
        }

        foreach (var node in view.nodes.ToList())
        {
            if (node is not LetterNode ln) continue;
            if (map != null && map.TryGetValue(ln.Id, out var meta))
            {
                ln.Data.delay = meta.delay;
            }
        }
    }

    /*** LoadResponses(view, path): placeholder read ***/
    public static void LoadResponses(LetterGraphView view, string path) { }
}
