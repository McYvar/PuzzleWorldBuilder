using System.Collections;
using UnityEngine;

public interface ICommand
{
    void Execute();
    void Undo();
}
