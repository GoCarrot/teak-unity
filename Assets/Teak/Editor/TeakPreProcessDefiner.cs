using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

using System.Collections.Generic;

class TeakPreProcessDefiner :
#if UNITY_2018_1_OR_NEWER
    IPreprocessBuildWithReport
#else
    IPreprocessBuild
#endif
{
    public int callbackOrder { get { return 0; } }
    public static readonly string[] TeakDefines = new string[] {
        "TEAK_2_0_OR_NEWER",
        "TEAK_2_1_OR_NEWER"
    };

#if UNITY_2018_1_OR_NEWER
    public void OnPreprocessBuild(BuildReport report) {
        SetTeakPreprocesorDefines(report.summary.platformGroup);
    }
#else

    public void OnPreprocessBuild(BuildTarget target, string path) {
        SetTeakPreprocesorDefines(BuildPipeline.GetBuildTargetGroup(target));
    }
#endif

    private void SetTeakPreprocesorDefines(BuildTargetGroup targetGroup) {
        string[] existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';');

        HashSet<string> updatedDefines = new HashSet<string>(existingDefines);
        updatedDefines.RemoveWhere(define => define.StartsWith("TEAK_") && define.EndsWith("_OR_NEWER"));
        updatedDefines.UnionWith(TeakDefines);

        string[] defines = new string[updatedDefines.Count];
        updatedDefines.CopyTo(defines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defines));
    }
}
