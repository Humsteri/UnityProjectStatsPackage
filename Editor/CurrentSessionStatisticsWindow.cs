using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace StatsMonitor
{
   public class CurrentSessionStatisticsWindow : EditorWindow
{
    #region Instance
    public static CurrentSessionStatisticsWindow instance;
    private void OnEnable()
    {
        instance = this;
    }
    private void OnDisable()
    {
        if (instance == this)
            instance = null;
    }
    #endregion
}

[InitializeOnLoad]
static class OnStartUpDaily
{
    private const string ReloadStartTimeKey = "\"DomainReloadProfilerCurrent.StartTime";
    private const string PlayModeTimeKey = "\"PlayModeCurrent.StartTime";
    static bool incrementedThisCompilation = false;
    static OnStartUpDaily()
    {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyRelaod;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyRelaod;

        CompilationPipeline.compilationStarted += OnCompilationStarted;
        CompilationPipeline.compilationFinished += OnCompilationFinished;

        EditorApplication.playModeStateChanged += ModeStateChanged;
        EditorApplication.quitting += OnQuit;

        EditorSceneManager.sceneOpened += OnSceneOpened;

        Undo.undoRedoEvent += OnUndoOrRedoPerformer;

        Application.logMessageReceived += HandleLog;

        DailyLoadManager.Instance.Save();
    }
    #region Event subscribers
    private static void OnCompilationStarted(object obj)
    {
        incrementedThisCompilation = false;
    }

    private static void OnCompilationFinished(object obj)
    {
        if (!incrementedThisCompilation)
        {
            DailyLoadManager.Instance.IncrementCompiled();
            DailyLoadManager.Instance.Save();
            incrementedThisCompilation = true;
        }
    }
    private static void HandleLog(string condition, string stackTrace, LogType type)
    {
        DailyLoadManager.Instance.IncrementLog(type);
    }
    private static void OnUndoOrRedoPerformer(in UndoRedoInfo undo)
    {
        if (undo.isRedo) DailyLoadManager.Instance.IncrementRedo();
        else DailyLoadManager.Instance.IncrementUndo();
    }
    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        DailyLoadManager.Instance.IncrementScenesOpened();
        DailyLoadManager.Instance.Save();
    }
    private static void OnBeforeAssemblyRelaod()
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        EditorPrefs.SetString(ReloadStartTimeKey, startTimestamp.ToString());
    }
    private static void OnAfterAssemblyRelaod()
    {
        if (EditorPrefs.HasKey(ReloadStartTimeKey))
        {
            long startTimestamp = long.Parse(EditorPrefs.GetString(ReloadStartTimeKey));
            long endTimestamp = Stopwatch.GetTimestamp();
            long elapsedTicks = endTimestamp - startTimestamp;
            double elapsedSeconds = (double)elapsedTicks / Stopwatch.Frequency;
            if (elapsedSeconds >= 0)
            {
                DailyLoadManager.Instance.AddDomainReloadTime(elapsedSeconds);
            }

            EditorPrefs.DeleteKey(ReloadStartTimeKey);
            DailyLoadManager.Instance.Save();
        }
    }
    private static void OnQuit()
    {
        DailyLoadManager.Instance.Reset();
        DailyLoadManager.Instance.Save();
    }
    private static void ModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            long playModestartTimestamp = Stopwatch.GetTimestamp();
            EditorPrefs.SetString(PlayModeTimeKey, playModestartTimestamp.ToString());
            DailyLoadManager.Instance.IncrementCompiled();
        }
        if (change == PlayModeStateChange.ExitingPlayMode)
        {
            if (EditorPrefs.HasKey(PlayModeTimeKey))
            {
                long playModestartTimestamp = long.Parse(EditorPrefs.GetString(PlayModeTimeKey));
                long endTimestamp = Stopwatch.GetTimestamp();
                long elapsedTicks = endTimestamp - playModestartTimestamp;
                double elapsedSeconds = (double)elapsedTicks / Stopwatch.Frequency;

                DailyLoadManager.Instance.AddPlayModeTime(elapsedSeconds);
                
                EditorPrefs.DeleteKey(PlayModeTimeKey);
                DailyLoadManager.Instance.Save();
            }
        }
        if (change == PlayModeStateChange.ExitingEditMode)
        {
            DailyLoadManager.Instance.IncrementPlayPressed();
            DailyLoadManager.Instance.Save();
        }
    }
    #endregion
} 
}
