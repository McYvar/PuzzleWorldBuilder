using System.Collections;
using UnityEngine;

public interface ICommand
{
    bool addToUndo { get; set; }
    void Execute();
    void Undo();
    void Redo();
    void ClearFirstUndo();
    void ClearRedo();
}
