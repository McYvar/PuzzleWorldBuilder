using System.Collections.Generic;

public class ClipBoard : EditorBase
{
    public static List<ClipBoard> clipboard = new List<ClipBoard>();
    public string normalName;

    public void AddToClipBoard()
    {
        clipboard.Add(this);
        normalName = name;
        name = name + " : OnClipboard";
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        RemoveFromClipBoard();
    }

    public void RemoveFromClipBoard()
    {
        clipboard.Remove(this);
    }
}