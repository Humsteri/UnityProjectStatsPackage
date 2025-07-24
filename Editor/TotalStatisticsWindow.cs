using System.Diagnostics;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
static class OnStartUp
{
    private const string ReloadStartTimeKey = "\"DomainReloadProfiler.StartTime";
    private const string PlayModeTimeKey = "\"PlayMode.StartTime";
    static bool incrementedThisCompilation = false;
    //#if UNITY_EDITOR_WIN
    //    private static float interval = 5f;
    //    private static float time = 0;
    //    private static float mouseDistance = 0f;
    //    private static MousePoint _mousePoint;
    //    private struct MousePoint
    //    {
    //        public int x;
    //        public int y;
    //    }
    //    [DllImport("user32.dll")]
    //    private static extern int GetCursorPos(ref MousePoint lpPoint);
    //    public static Vector2 CurrentMousePosition => new Vector2(_mousePoint.x, _mousePoint.y);
    //#endif
    static OnStartUp()
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

        if (!SessionState.GetBool("FirstInitDone", false)) // Get the first startup
        {
            /* int exitedCleanly = PlayerPrefs.GetInt("ExitedCleanly", 1);
            if (exitedCleanly == 0)
            {
                LoadManager.Instance.IncrementCrashesh();
            } */
            if (!LoadManager.Instance.GetOpened())
            {
                LoadManager.Instance.IncrementOpened();
                LoadManager.Instance.Opened(true);
                LoadManager.Instance.Save();
            }
            
            
            //SessionState.SetBool("FirstInitDone", true);
        }
        //PlayerPrefs.SetInt("ExitedCleanly", 0);

        
    }

    #region Event subscribers
    //    private static void Update()
    //    {
    //#if UNITY_EDITOR_WIN
    //        GetCursorPos(ref _mousePoint);
    //        //time += Time.deltaTime;
    //        //UnityEngine.Debug.Log(time);
    //        //if (time > interval)
    //        //{
    //        //    UnityEngine.Debug.Log(Time.deltaTime);
    //        //    time = 0;
    //        //    //UnityEngine.Debug.Log("Inside");
    //        //}
    //        //UnityEngine.Debug.Log(CurrentMousePosition);
    //#endif
    //    }
    private static void OnCompilationStarted(object obj)
    {
        incrementedThisCompilation = false;
    }
    private static void OnCompilationFinished(object obj)
    {
        if (!incrementedThisCompilation)
        {
            LoadManager.Instance.IncrementCompiled();
            LoadManager.Instance.Save();
            incrementedThisCompilation = true;
        }
    }
    private static void HandleLog(string condition, string stackTrace, LogType type)
    {
        LoadManager.Instance.IncrementLog(type);
    }
    private static void OnUndoOrRedoPerformer(in UndoRedoInfo undo)
    {
        if (undo.isRedo) LoadManager.Instance.IncrementRedo();
        else LoadManager.Instance.IncrementUndo();
    }
    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        LoadManager.Instance.IncrementScenesOpened();
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
                LoadManager.Instance.AddDomainReloadTime(elapsedSeconds);
            }
            
            EditorPrefs.DeleteKey(ReloadStartTimeKey);
            LoadManager.Instance.Save();
        }
    }
    private static void OnQuit()
    {
        LoadManager.Instance.AddTime((float)(EditorApplication.timeSinceStartup));
        if(EditorApplication.timeSinceStartup > LoadManager.Instance.LongestSession())
            LoadManager.Instance.SetLongestSession(EditorApplication.timeSinceStartup);

        //PlayerPrefs.SetInt("ExitedCleanly", 1);
        LoadManager.Instance.Opened(false);
        LoadManager.Instance.Save();
    }
    private static void ModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            long playModestartTimestamp = Stopwatch.GetTimestamp();
            EditorPrefs.SetString(PlayModeTimeKey, playModestartTimestamp.ToString());
            LoadManager.Instance.IncrementCompiled();
        }
        if (change == PlayModeStateChange.ExitingPlayMode)
        {
            if (EditorPrefs.HasKey(PlayModeTimeKey))
            {
                long playModestartTimestamp = long.Parse(EditorPrefs.GetString(PlayModeTimeKey));
                long endTimestamp = Stopwatch.GetTimestamp();
                long elapsedTicks = endTimestamp - playModestartTimestamp;
                double elapsedSeconds = (double)elapsedTicks / Stopwatch.Frequency;

                LoadManager.Instance.AddPlayModeTime(elapsedSeconds);

                //UnityEngine.Debug.Log($"Play mode time: {elapsedSeconds:F2} seconds");

                EditorPrefs.DeleteKey(PlayModeTimeKey);
                LoadManager.Instance.Save();
            }
            else
            {
                //UnityEngine.Debug.LogWarning("Play mode start time was not found.");
            }
        }
        if (change == PlayModeStateChange.ExitingEditMode)
        {
            LoadManager.Instance.IncrementPlayPressed();
            //LoadManager.Instance.IncrementCompiled();
            LoadManager.Instance.Save();
        }
    }
    #endregion
}


