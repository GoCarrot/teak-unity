using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

class TeakPreProcessDefiner :
#if UNITY_2018_1_OR_NEWER
    IPreprocessBuildWithReport
#else
    IPreprocessBuild
#endif
{
    public int callbackOrder { get { return 0; } }
    public static readonly string[] TeakDefines = new string[] { "TEAK_2_0_OR_NEWER" };

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
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        if (!defines.EndsWith(";")) defines += ";";
        defines += string.Join(";", TeakDefines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
    }
}
