using UnityEngine;

public class UIManager : MonoBehaviour
{
    public AssemblyManager assemblyManager;

    public void OnNextClicked()
    {
        if(assemblyManager != null) assemblyManager.NextStep();
    }

    public void OnPrevClicked()
    {
        if(assemblyManager != null) assemblyManager.PrevStep();
    }
}