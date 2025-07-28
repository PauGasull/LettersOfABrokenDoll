using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class CompositionGraphView : GraphView
{
    public CompositionGraphView()
    {
        style.flexGrow = 1;
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p =>
            startPort != p &&
            startPort.node != p.node &&
            startPort.direction != p.direction).ToList();
    }

    public void CreateStartNode(Vector2 position)
    {
        var node = new BaseStoryNode("Start")
        {
            GUID = Guid.NewGuid().ToString(),
            EntryPoint = true
        };
        node.SetPosition(new Rect(position, new Vector2(250, 300)));
        AddElement(node);
    }

    public void CreateGenericNode(Vector2 position)
    {
        var node = new BaseStoryNode("Node")
        {
            GUID = Guid.NewGuid().ToString(),
            EntryPoint = false
        };
        node.SetPosition(new Rect(position, new Vector2(250, 300)));
        AddElement(node);
    }

    public void CreateResponseNode(Vector2 position)
    {
        var node = new ResponseNode();
        node.SetPosition(new Rect(position, new Vector2(300, 200)));
        AddElement(node);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 mousePosition = evt.localMousePosition;

        evt.menu.AppendAction("Create Node/Start", _ => CreateStartNode(mousePosition));
        evt.menu.AppendAction("Create Node/Response", _ => CreateResponseNode(mousePosition));
    }

    public List<string> GeneratePaths()
    {
        var startNode = nodes.OfType<BaseStoryNode>().FirstOrDefault(n => n.EntryPoint);
        if (startNode == null)
        {
            Debug.LogWarning("No entry point found.");
            return new List<string>();
        }

        var paths = new List<string>();
        TraverseNode(startNode, "", paths);
        return paths;
    }

    public void GenerateAllPaths()
    {
        var paths = GeneratePaths();
        var responseMap = GetResponseNodeMap();

        foreach (var path in paths)
        {
            if (responseMap.ContainsKey(path))
                Debug.Log($"✔ Response found for path: {path}");
            else
                Debug.LogWarning($"❌ No response node found for path: {path}");
        }
    }

    private void TraverseNode(BaseStoryNode node, string currentPath, List<string> paths)
    {
        foreach (var option in node.GetOptions())
        {
            var output = option.OutputPort.connections.FirstOrDefault();
            if (output == null)
            {
                paths.Add(currentPath + option.ID);
                continue;
            }

            if (output.input.node is BaseStoryNode nextNode)
            {
                TraverseNode(nextNode, currentPath + option.ID + "_", paths);
            }
        }
    }
    
    public Dictionary<string, ResponseNode> GetResponseNodeMap()
    {
        return nodes.OfType<ResponseNode>()
                    .Where(n => !string.IsNullOrEmpty(n.PathKey))
                    .ToDictionary(n => n.PathKey, n => n);
    }
}