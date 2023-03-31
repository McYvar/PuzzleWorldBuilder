using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class ExecuteOnlyIf : MonoBehaviour
{
    [SerializeField] UnityEvent ifTrueEvents;
    [SerializeField] UnityEvent ifFalseEvents;
    [SerializeField] UnityEvent invokeIfInvalid;

    public void InvokeIfSaved()
    {
        if (DataPersistenceManager.instance.GetSavedState())
            ifTrueEvents.Invoke();

        else ifFalseEvents.Invoke();
    }

    // file extention is "puzzlebuildtool"
    public void InvokeIfFileDoesntExists()
    {
        string currentFileName = DataPersistenceManager.instance.GetFileName();
        if (currentFileName == "")
        {
            invokeIfInvalid.Invoke();
            return;
        }

        if (File.Exists(Path.Combine(Application.persistentDataPath, currentFileName + ".puzzlebuildtool")))
            ifFalseEvents.Invoke();

        else ifTrueEvents.Invoke();
    }
}
