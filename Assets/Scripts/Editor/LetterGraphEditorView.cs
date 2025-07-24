using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.IO;

public class LetterGraphEditorWindow : EditorWindow
{
    private LetterGraphView graphView;
    private string lettersPath = "Assets/Letters";
    private string compositionFile = "composition.json";
    private string letterFile = "letter.json";
    private string responsesFile = "responses.json";

    [MenuItem("Window/Letter Graph Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<LetterGraphEditorWindow>();
        var icon = EditorGUIUtility.IconContent("GraphModel Icon").image;
        window.titleContent = new GUIContent("Letter Graph", icon);
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
    * ConstructGraphView(): Initialize graph view and add to root.
    ***/
    private void ConstructGraphView()
    {
        graphView = new LetterGraphView { name = "Letter Graph View" };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    /***
    * ConstructConfigBoard(): Add configuration fields and save/load buttons in blackboard.
    ***/
    private void ConstructConfigBoard()
    {
        var board = new Blackboard(graphView) { title = "Configuration" };
        board.SetPosition(new Rect(10, 10, 240, 300));

        var pathField = new TextField("Base Path:") { value = lettersPath };
        pathField.RegisterValueChangedCallback(evt => lettersPath = evt.newValue);
        board.Add(pathField);

        var compField = new TextField("composition.json:") { value = compositionFile };
        compField.RegisterValueChangedCallback(evt => compositionFile = evt.newValue);
        board.Add(compField);

        var letterField = new TextField("letter.json:") { value = letterFile };
        letterField.RegisterValueChangedCallback(evt => letterFile = evt.newValue);
        board.Add(letterField);

        var respField = new TextField("responses.json:") { value = responsesFile };
        respField.RegisterValueChangedCallback(evt => responsesFile = evt.newValue);
        board.Add(respField);

        var btnContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };
        btnContainer.Add(new Button(() => LoadGraph()) { text = "Load" });
        btnContainer.Add(new Button(() => SaveGraph()) { text = "Save" });
        board.Add(btnContainer);

        rootVisualElement.Add(board);
    }

    /***
    * SaveGraph(): Serialize graphView to JSON files using current config.
    ***/
    public void SaveGraph()
    {
        var compPath = Path.Combine(lettersPath, compositionFile);
        var letterPath = Path.Combine(lettersPath, letterFile);
        var respPath = Path.Combine(lettersPath, responsesFile);
        Debug.Log($"Saving graph to:\n{compPath}\n{letterPath}\n{respPath}");
        // TODO: Implement serialization
    }

    /***
    * LoadGraph(): Read JSON files using current config and populate view.
    ***/
    public void LoadGraph()
    {
        var compPath = Path.Combine(lettersPath, compositionFile);
        Debug.Log($"Loading graph from: {compPath}");
        graphView.PopulateView(compPath);
    }
}

public class LetterGraphView : GraphView
{
    public LetterGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
    }

    /***
    * CreateNode(string title, Vector2 position): Instantiate a new node at given position.
    ***/
    public void CreateNode(string title, Vector2 position)
    {
        if (string.IsNullOrEmpty(title)) return;
        var node = new Node { title = title };
        var input = Port.Create<Edge>(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(float));
        var output = Port.Create<Edge>(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(float));
        node.inputContainer.Add(input);
        node.outputContainer.Add(output);
        node.RefreshExpandedState();
        node.RefreshPorts();
        AddElement(node);
        node.SetPosition(new Rect(position, new Vector2(150, 200)));
    }

    /***
    * Override context menu to add New Block option at mouse position.
    ***/
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        var localPos = evt.localMousePosition;
        evt.menu.AppendAction(
            "New Block",
            action => CreateNode("New Block", localPos),
            DropdownMenuAction.AlwaysEnabled);
    }

    /***
    * PopulateView(string jsonPath): Load nodes and edges from JSON.
    ***/
    public void PopulateView(string jsonPath)
    {
        // TODO: Implement JSON deserialization to recreate nodes and edges
    }
}
