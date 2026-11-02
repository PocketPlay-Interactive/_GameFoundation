using System.Collections;
using UnityEngine;

[ExtensionModuleNotice("Drag this component into Manager -> Extension Modules.")]
public sealed class SampleMonoExtensionModule : GameFoundationModuleBehaviour
{
    [SerializeField] private string message = "Mono extension module initialized.";

    public override IEnumerator Initialize()
    {
        LogSystem.LogSuccess(message);
        yield break;
    }
}
