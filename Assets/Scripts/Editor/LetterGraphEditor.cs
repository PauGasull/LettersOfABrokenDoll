using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class LetterFlowEditorWindow : EditorWindow
{
    private LetterGraphView _graphView;
    private VisualElement _toolbar;
    private string _compositionPath = "Assets/Letters/composition.json";
    private string _responsesPath = "Assets/Letters/responses.json";
    private string _lettersPath = "Assets/Letters/letter.json";

    /***
    * OnEnable(): Inicialitza l'EditorWindow
    * PRE: El Window està creat
    * POST: Crea el GraphView, la toolbar i carrega JSON
    ***/
    private void OnEnable()
    {
        ConstructUI();
        //LoadAllJson();
    }

    /***
    * OnDisable(): Allibera recursos en tancar
    * PRE: L'EditorWindow existeix
    * POST: Destrueix el GraphView
    ***/
    private void OnDisable()
    {
        if (_graphView != null)
            rootVisualElement.Remove(_graphView);
        if (_toolbar != null)
            rootVisualElement.Remove(_toolbar);
    }

    /***
    * ConstructUI(): Construeix la toolbar i el GraphView al rootVisualElement
    * PRE: rootVisualElement està disponible
    * POST: Afegeix controls i GraphView a la UI
    ***/
    private void ConstructUI()
    {
        // Només construïm el GraphView, la barra d'eines es fa amb OnGUI()
        if (_graphView != null)
            return;

        _graphView = new LetterGraphView { name = "Letter Flow Editor" };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    // Eliminem la variable _toolbar i qualsevol referència a Toolbar UIElements

    /***
    * LoadAllJson(): Carrega tots els JSON i crea nodes
    * PRE: Rutes a fitxers vàlides
    * POST: Omple el GraphView amb nodes i enllaços segons JSON
    ***/
    private void LoadAllJson()
    {
        if (!File.Exists(_compositionPath) || !File.Exists(_responsesPath) || !File.Exists(_lettersPath))
        {
            Debug.LogError("JSON files not found in Assets/Letters/");
            return;
        }

        var compText = File.ReadAllText(_compositionPath);
        var respText = File.ReadAllText(_responsesPath);
        var letText  = File.ReadAllText(_lettersPath);

        var compositions = JsonConvert.DeserializeObject<Dictionary<string, CompositionTemplate>>(compText);
        var responses    = JsonConvert.DeserializeObject<Dictionary<string, ResponseTemplate>>(respText);
        var lettersMeta  = JsonConvert.DeserializeObject<Dictionary<string, LetterMeta>>(letText);

        _graphView.PopulateGraph(compositions, responses, lettersMeta);
    }

    /***
    * SaveAllJson(): Serialitza l'estat actual del GraphView a JSON
    * PRE: GraphView existeix i conté dades
    * POST: Sobre-escriu fitxers JSON amb el nou contingut
    ***/
    private void SaveAllJson()
    {
        var data = _graphView.SerializeGraph();

        File.WriteAllText(_compositionPath, JsonConvert.SerializeObject(data.Item1, Formatting.Indented));
        File.WriteAllText(_responsesPath,   JsonConvert.SerializeObject(data.Item2, Formatting.Indented));
        File.WriteAllText(_lettersPath,     JsonConvert.SerializeObject(data.Item3, Formatting.Indented));
        AssetDatabase.Refresh();
    }

    [MenuItem("Window/Letter Flow Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<LetterFlowEditorWindow>();
        window.titleContent = new GUIContent("Letter Flow Editor");
    }
}

#region DataModels

[Serializable]
public class CompositionTemplate { public string root_block; public Dictionary<string, BlockData> blocks; }
[Serializable]
public class BlockData { public string prompt; public Dictionary<string, OptionData> options; }
[Serializable]
public class OptionData { public string short_text; public string long_text; public string next; }
[Serializable]
public class ResponseTemplate { public Dictionary<string, PathData> paths; public Dictionary<string, ResponseBlock> responses; }
[Serializable]
public class PathData { public List<string> blocks; }
[Serializable]
public class ResponseBlock { public string content; }
[Serializable]
public class LetterMeta { public int delay_seconds; }

#endregion

#region GraphView Implementation

public class LetterGraphView : GraphView
{
    /***
    * LetterGraphView(): Constructor del GraphView
    * PRE: ---
    * POST: Configura estils i interaccions bàsiques
    ***/
    public LetterGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
    }

    /***
    * PopulateGraph(): Crea nodes i connexions segons dades
    * PRE: Dades desserialitzades de JSON
    * POST: Afegeix nodes i enllaços a la vista
    ***/
    public void PopulateGraph(
        Dictionary<string, CompositionTemplate> comps,
        Dictionary<string, ResponseTemplate> resps,
        Dictionary<string, LetterMeta> metas)
    {
        // Neteja l'antic graf
        foreach (var element in graphElements.ToList())
            RemoveElement(element);

        // Crea nodes i connexions com abans...
        var nodes = new List<LetterNode>();
        foreach (var kvp in comps)
        {
            var node = new LetterNode(kvp.Key, kvp.Value);
            AddElement(node);
            nodes.Add(node);
        }
        foreach (var kvp in comps)
        {
            var source = nodes.First(n => n.TemplateID == kvp.Key);
            foreach (var block in kvp.Value.blocks)
                foreach (var opt in block.Value.options)
                {
                    var nextID = opt.Value.next;
                    if (string.IsNullOrEmpty(nextID) || !comps.ContainsKey(nextID)) continue;
                    var target = nodes.First(n => n.TemplateID == nextID);
                    var outPort = source.outputContainer.Children().OfType<Port>().First(p => p.portName == block.Key + "_" + opt.Key);
                    var inPort  = target.inputContainer.Children().OfType<Port>().First(p => p.portName == "In");
                    var edge = outPort.ConnectTo(inPort);
                    AddElement(edge);
                }
    }
}

    public Tuple<Dictionary<string, CompositionTemplate>, Dictionary<string, ResponseTemplate>, Dictionary<string, LetterMeta>> SerializeGraph()
    {
        // ... mateixa estructura
        return Tuple.Create(new Dictionary<string, CompositionTemplate>(), new Dictionary<string, ResponseTemplate>(), new Dictionary<string, LetterMeta>());
    }
}

public class LetterNode : Node
{
    public string TemplateID;
    public CompositionTemplate Data;

    /***
    * LetterNode(...): Crea node amb ports i prompt
    ***/
    public LetterNode(string templateID, CompositionTemplate data)
    {
        TemplateID = templateID;
        Data = data;
        title = templateID;

        // Input
        var inPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inPort.portName = "In";
        inputContainer.Add(inPort);

        // Outputs
        foreach (var block in data.blocks)
            foreach (var option in block.Value.options)
            {
                var port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                port.portName = block.Key + "_" + option.Key;
                outputContainer.Add(port);
            }

        var promptField = new TextField("Prompt:") { value = data.blocks[data.root_block].prompt };
        promptField.RegisterValueChangedCallback(evt => Data.blocks[Data.root_block].prompt = evt.newValue);
        extensionContainer.Add(promptField);

        RefreshExpandedState();
        RefreshPorts();
    }
}

#endregion
