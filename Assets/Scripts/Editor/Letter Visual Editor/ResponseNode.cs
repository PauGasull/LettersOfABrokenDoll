using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

public class ResponseNode : Node
{
    public string PathKey = "";
    public List<ResponseBlock> ResponseBlocks = new();

    private Foldout blocksFoldout;

    public ResponseNode()
    {
        title = "Response Node";

        var pathField = new TextField("Path Key") { value = "" };
        pathField.RegisterValueChangedCallback(evt => PathKey = evt.newValue);
        mainContainer.Add(pathField);

        blocksFoldout = new Foldout { text = "Response Blocks" };
        extensionContainer.Add(blocksFoldout);

        var addBlockButton = new Button(AddResponseBlock) { text = "Add Response Block" };
        extensionContainer.Add(addBlockButton);

        RefreshExpandedState();
        RefreshPorts();
    }

    private void AddResponseBlock()
    {
        var block = new ResponseBlock();
        ResponseBlocks.Add(block);

        var container = new VisualElement();
        container.style.marginBottom = 8;

        var idField = new TextField("Block ID") { value = block.ID };
        idField.RegisterValueChangedCallback(evt => block.ID = evt.newValue);
        container.Add(idField);

        var contentField = new TextField("Content") { multiline = true, value = block.Content };
        contentField.RegisterValueChangedCallback(evt => block.Content = evt.newValue);
        container.Add(contentField);

        blocksFoldout.Add(container);

        RefreshExpandedState();
    }

    public class ResponseBlock
    {
        public string ID = "";
        public string Content = "";
    }
}