using System;
using System.Collections.Generic;

[Serializable]
public class CompositionTemplate
{
    public string root_block;
    public Dictionary<string, CompositionBlock> blocks;
}

[Serializable]
public class CompositionBlock
{
    public string prompt;
    public Dictionary<string, CompositionOption> options;
}

[Serializable]
public class CompositionOption
{
    public string short_text;
    public string long_text;
    public string next;
}

[Serializable]
public class LetterMeta
{
    public int delay_seconds;
}

[Serializable]
public class ResponseTemplate
{
    public Dictionary<string, ResponsePath> paths;
    public Dictionary<string, ResponseContent> responses;
}

[Serializable]
public class ResponsePath
{
    public List<string> blocks;
}

[Serializable]
public class ResponseContent
{
    public string content;
}
