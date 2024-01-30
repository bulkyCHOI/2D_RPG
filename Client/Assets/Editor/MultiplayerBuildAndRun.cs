using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiplayerBuildAndRun
{
    [MenuItem("Tools/Build and Run/2 Players %#&2")]

    static void PerformWin64Build2()
    {
        PerformWin64Build(2);
    }
    [MenuItem("Tools/Build and Run/3 Players %#&3")]
    static void PerformWin64Build3()
    {
        PerformWin64Build(3);
    }
    [MenuItem("Tools/Build and Run/4 Players %#&4")]
    static void PerformWin64Build4()
    {
        PerformWin64Build(4);
    }

    static void PerformWin64Build(int playerCount) 
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        for(int i = 0; i < playerCount; ++i)
        {
            BuildPipeline.BuildPlayer(GetScenePaths(), 
                "Builds/Win64/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe", 
                BuildTarget.StandaloneWindows64, 
                BuildOptions.AutoRunPlayer);
        }
    }

    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for(int i = 0; i < scenes.Length; ++i)
            scenes[i] = EditorBuildSettings.scenes[i].path; 
        return scenes;
    }
}
