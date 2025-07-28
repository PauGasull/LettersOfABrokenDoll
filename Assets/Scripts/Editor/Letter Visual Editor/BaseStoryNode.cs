using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;

public class OptionData
{
    public string ID;
    public TextField ShortText;
    public TextField LongText;
    public Port OutputPort;
}

public class BaseStoryNode : Node
{
    public string GUID;
    public bool EntryPoint = false;
    public string BlockID;
    private Foldout optionFoldout;
    private int optionCounter = 0;

    private List<OptionData> options = new();

    public BaseStoryNode(string nodeName)
    {
        title = nodeName;
        BlockID = nodeName;

        var blockIDField = new TextField("Block ID") { value = BlockID };
        blockIDField.RegisterValueChangedCallback(evt => BlockID = evt.newValue);
        mainContainer.Add(blockIDField);

        var promptField = new TextField("Prompt") { multiline = true };
        extensionContainer.Add(promptField);

        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);

        optionFoldout = new Foldout { text = "Options" };
        extensionContainer.Add(optionFoldout);

        var addOptionButton = new Button(() => AddOption()) { text = "Add Option" };
        extensionContainer.Add(addOptionButton);

        // Option A and B
        AddOption(); AddOption();

        RefreshExpandedState();
        RefreshPorts();
    }

    private void AddOption()
    {
        if (optionCounter < 3)
        {
            string optionID = ((char)('A' + optionCounter)).ToString();
            optionCounter++;

            var container = new VisualElement();

            var shortText = new TextField($"Short ({optionID})") { multiline = false };
            container.Add(shortText);

            var longText = new TextField($"Long ({optionID})") { multiline = true };
            container.Add(longText);

            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = optionID;
            outputContainer.Add(outputPort);

            optionFoldout.Add(container);

            var spacer = new VisualElement();
            spacer.style.height = 8;
            spacer.style.marginBottom = 4;
            optionFoldout.Add(spacer);

            options.Add(new OptionData
            {
                ID = optionID,
                ShortText = shortText,
                LongText = longText,
                OutputPort = outputPort
            });


            if (optionCounter >= 3)
            {
                foreach (var child in extensionContainer.Children())
                {
                    if (child is Button button && button.text == "Add Option")
                    {
                        button.style.display = DisplayStyle.None;
                        break;
                    }
                }
            }

            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public List<OptionData> GetOptions()
    {
        return options;
    }
}
