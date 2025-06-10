using System;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Total : VisualElement
{
    public static Total instance;
    int timesProjectOpened = 0, timesProjectCompiled = 0, timesPlayModePressed = 0, totalScenesOpenedAmount = 0, totalRedoAmount = 0, totalUndoAmount = 0, crashAmount = 0;
    int normalLogAmount = 0, warningLogAmount = 0, errorLogAmount = 0, assertLogAmount = 0, exceptionLogAmount = 0;
    float totalTimeSpent = 0f, currentSessionLength = 0f;
    double totalDomainReloadTime = 0f, totalPlayModeTime = 0f, longestSession = 0f;

    private Label _totalSceneOpenedCountLabel;
    private Label _totalTimeSpentLabel;
    private Label _totalPlayModeCountLabel;
    private Label _totalCompiledCountLabel;
    private Label _totalRedoCountLabel;
    private Label _totalUndoCountLabel;
    private Label _totalNormalLogCountLabel;
    private Label _totalErrorLogCountLabel;
    private Label _totalWarningLogCountLabel;
    private Label _totalAssertLogCountLabel;
    private Label _totalExceptionLogCountLabel;
    private Label _totalDomainReloadTimeLabel;
    private Label _totalPlayModeTimeLabel;
    private Label _totalProjectOpenedLabel;
    private Label _totalCrashAmountLabel;
    private Label _totalLongestSessionLabel;

    public Total()
    {
        instance = this;
        GetTimes();

        VisualElement container = new VisualElement();
        container.style.width = 20000;
        container.style.height = 20000;
        style.backgroundColor = new Color(0, 0, 0, 0.45f);
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.humsteri.stats-monitor/Editor/styles.uss");
        Add(container);

        container.Add(NewTextLabel("[ Timers ]", new Vector2(45, 0), TextAnchor.MiddleLeft));
        container.Add(NewTimeLabel(ref _totalTimeSpentLabel, styleSheet, totalTimeSpent, new Vector2(50, 40), new Vector2(120, 30)));
        container.Add(NewTextLabel("Total project time", new Vector2(50, 60), TextAnchor.MiddleCenter));
        container.Add(NewTimeLabel(ref _totalLongestSessionLabel, styleSheet, longestSession, new Vector2(200, 40), new Vector2(120, 30)));
        container.Add(NewTextLabel("Longest session", new Vector2(200, 60), TextAnchor.MiddleCenter));
        container.Add(NewTimeLabel(ref _totalDomainReloadTimeLabel, styleSheet, totalDomainReloadTime, new Vector2(350, 40), new Vector2(120, 30)));
        container.Add(NewTextLabel("Total domain reload", new Vector2(350, 60), TextAnchor.MiddleCenter));
        container.Add(NewTimeLabel(ref _totalPlayModeTimeLabel, styleSheet, totalPlayModeTime, new Vector2(50, 100), new Vector2(120, 30)));
        container.Add(NewTextLabel("Total play mode", new Vector2(50, 120), TextAnchor.MiddleCenter));

        container.Add(NewTextLabel("[ Actions ]", new Vector2(45, 160), TextAnchor.MiddleLeft));
        container.Add(NewCountLabel(ref _totalProjectOpenedLabel, styleSheet, timesProjectOpened, new Vector2(50, 200)));
        container.Add(NewTextLabel("Project sessions", new Vector2(50, 220), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalCrashAmountLabel, styleSheet, crashAmount, new Vector2(200, 200)));
        container.Add(NewTextLabel("Crashesh", new Vector2(200, 220), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalSceneOpenedCountLabel, styleSheet, totalScenesOpenedAmount, new Vector2(350, 200)));
        container.Add(NewTextLabel("Scenes opened", new Vector2(350, 220), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalPlayModeCountLabel, styleSheet, timesPlayModePressed, new Vector2(50, 260)));
        container.Add(NewTextLabel("Play mode entered", new Vector2(50, 280), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalCompiledCountLabel, styleSheet, timesProjectCompiled, new Vector2(200, 260)));
        container.Add(NewTextLabel("Total compiled", new Vector2(200, 280), TextAnchor.MiddleCenter));

        container.Add(NewTextLabel("[ Edits ]", new Vector2(45, 320), TextAnchor.MiddleLeft));
        container.Add(NewCountLabel(ref _totalRedoCountLabel, styleSheet, totalRedoAmount, new Vector2(50, 360)));
        container.Add(NewTextLabel("Times Redo", new Vector2(50, 380), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalUndoCountLabel, styleSheet, totalUndoAmount, new Vector2(200, 360)));
        container.Add(NewTextLabel("Times Undo", new Vector2(200, 380), TextAnchor.MiddleCenter));

        container.Add(NewTextLabel("[ Logs ]", new Vector2(45, 420), TextAnchor.MiddleLeft));
        container.Add(NewCountLabel(ref _totalNormalLogCountLabel, styleSheet, normalLogAmount, new Vector2(50, 480)));
        container.Add(NewTextLabel("Normal log count", new Vector2(50, 500), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalWarningLogCountLabel, styleSheet, warningLogAmount, new Vector2(200, 480)));
        container.Add(NewTextLabel("Warning log count", new Vector2(200, 500), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalErrorLogCountLabel, styleSheet, errorLogAmount, new Vector2(350, 480)));
        container.Add(NewTextLabel("Error log count", new Vector2(350, 500), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalExceptionLogCountLabel, styleSheet, exceptionLogAmount, new Vector2(50, 540)));
        container.Add(NewTextLabel("Exception log count", new Vector2(50, 560), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _totalAssertLogCountLabel, styleSheet, assertLogAmount, new Vector2(200, 540)));
        container.Add(NewTextLabel("Assert log count", new Vector2(200, 560), TextAnchor.MiddleCenter));

        EditorApplication.update += UpdateTime;
        LoadManager.Instance.CompiledAction += SetCompiled;
        LoadManager.Instance.PlayModeAction += UpdatePlayModeEntered;
        LoadManager.Instance.PlayModeTimeAction += AddPlayModeTime;
        LoadManager.Instance.DomainReloadTimeAction += AddDomainReloadTime;
        LoadManager.Instance.SceneAction += UpdateScene;
        LoadManager.Instance.RedoAction += UpdateRedo;
        LoadManager.Instance.UndoAction += UpdateUndo;
        LoadManager.Instance.LogAction += UpdateLog;
    }
    #region Label Makers
    Label NewTextLabel(string txt, Vector2 pos, TextAnchor center)
    {
        Label label = new Label($"{txt}");
        label.style.width = 120;
        label.style.height = 45;
        label.style.unityTextAlign = center;
        label.style.justifyContent = Justify.Center;
        label.style.alignItems = Align.Center;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.position = Position.Absolute;
        label.style.left = pos.x;
        label.style.top = pos.y;
        return label;
    }
    Label NewTimeLabel(ref Label label, StyleSheet styleSheet, double time, Vector2 pos, Vector2 size)
    {
        label = new Label($"{TimeSpan.FromSeconds(time).Hours} H " +
            $"{TimeSpan.FromSeconds(time).Minutes} mm " +
            $"{TimeSpan.FromSeconds(time).Seconds} s ");
        label.style.width = size.x;
        label.style.height = size.y;
        label.styleSheets.Add(styleSheet);
        label.AddToClassList("rounded-box");
        label.style.backgroundColor = new Color(122f / 255f, 28f / 255f, 172f / 255f, 1f);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.justifyContent = Justify.Center;
        label.style.alignItems = Align.Center;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.position = Position.Absolute;
        label.style.left = pos.x;
        label.style.top = pos.y;
        return label;
    }
    Label NewCountLabel(ref Label label, StyleSheet styleSheet, int count, Vector2 pos)
    {
        label = new Label($"{count}");
        label.style.width = 120;
        label.style.height = 30;
        label.styleSheets.Add(styleSheet);
        label.AddToClassList("rounded-box");
        label.style.backgroundColor = new Color(122f / 255f, 28f / 255f, 172f / 255f, 1f);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.justifyContent = Justify.Center;
        label.style.alignItems = Align.Center;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.position = Position.Absolute;
        label.style.left = pos.x;
        label.style.top = pos.y;
        return label;
    }
    #endregion
    #region Label Updaters
    private void AddDomainReloadTime(double obj)
    {
        totalDomainReloadTime = obj;
        _totalDomainReloadTimeLabel.text = $"{TimeSpan.FromSeconds(totalDomainReloadTime).Hours} H " +
            $"{TimeSpan.FromSeconds(totalDomainReloadTime).Minutes} mm " +
            $"{TimeSpan.FromSeconds(totalDomainReloadTime).Seconds} ss ";
    }
    private void AddPlayModeTime(double obj)
    {
        _totalPlayModeTimeLabel.text = $"{TimeSpan.FromSeconds(obj).Hours} H " +
            $"{TimeSpan.FromSeconds(obj).Minutes} mm " +
            $"{TimeSpan.FromSeconds(obj).Seconds} ss ";
    }
    private void UpdatePlayModeEntered(int obj)
    {
        _totalPlayModeCountLabel.text = $"Total Times play mode entered: {obj}";
    }
    private void UpdateRedo(int obj)
    {
        _totalRedoCountLabel.text = $"Total Times redo: {obj}";
    }
    private void UpdateUndo(int arg2)
    {
        _totalUndoCountLabel.text = $"´Total Times undo: {arg2}";
    }
    private void UpdateLog(LogType type, int arg2)
    {
        switch (type)
        {
            case LogType.Error:
                _totalErrorLogCountLabel.text = $"Total Error log count: {arg2}";
                break;
            case LogType.Assert:
                _totalAssertLogCountLabel.text = $"Total Assert log count: {arg2}";
                break;
            case LogType.Warning:
                _totalWarningLogCountLabel.text = $"Total Warning log count: {arg2}";
                break;
            case LogType.Log:
                _totalNormalLogCountLabel.text = $"Total Normal log count: {arg2}";
                break;
            case LogType.Exception:
                _totalExceptionLogCountLabel.text = $"Total Exception log count: {arg2}";
                break;
            default:
                break;
        }
    }
    private void UpdateScene(int count)
    {
        _totalSceneOpenedCountLabel.text = $"Times scenes opened: {count}";
    }
    public void SetCompiled(int count)
    {
        _totalCompiledCountLabel.text = $"Times compiled: {count}";
    }
    void UpdateTime()
    {
        currentSessionLength = (float)(EditorApplication.timeSinceStartup);
        _totalTimeSpentLabel.text = $"{TimeSpan.FromSeconds(totalTimeSpent + currentSessionLength).Hours} H " +
            $"{TimeSpan.FromSeconds(totalTimeSpent + currentSessionLength).Minutes} mm " +
            $"{TimeSpan.FromSeconds(totalTimeSpent + currentSessionLength).Seconds} ss ";
    }
    #endregion
    void GetTimes()
    {
        totalTimeSpent = LoadManager.Instance.TotalTime();
        timesProjectCompiled = LoadManager.Instance.Compiled();
        timesPlayModePressed = LoadManager.Instance.PlayPressed();
        totalRedoAmount = LoadManager.Instance.Redo();
        totalUndoAmount = LoadManager.Instance.Undo();
        totalDomainReloadTime = LoadManager.Instance.TotalDomainReloadTime();
        longestSession = LoadManager.Instance.LongestSession();
        crashAmount = LoadManager.Instance.Crashesh();
        timesProjectOpened = LoadManager.Instance.Opened();
        totalPlayModeTime = LoadManager.Instance.TotalPlayModeTime();
        totalScenesOpenedAmount = LoadManager.Instance.ScenesOpened();
        normalLogAmount = LoadManager.Instance.GetLogType(LogType.Log);
        warningLogAmount = LoadManager.Instance.GetLogType(LogType.Warning);
        errorLogAmount = LoadManager.Instance.GetLogType(LogType.Error);
        exceptionLogAmount = LoadManager.Instance.GetLogType(LogType.Exception);
        assertLogAmount = LoadManager.Instance.GetLogType(LogType.Assert);
        currentSessionLength = (float)(EditorApplication.timeSinceStartup);
    }
    ~Total()
    {
        EditorApplication.update -= UpdateTime;
        LoadManager.Instance.CompiledAction -= SetCompiled;
        LoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        LoadManager.Instance.SceneAction -= UpdateScene;
        LoadManager.Instance.UndoAction -= UpdateRedo;
        LoadManager.Instance.RedoAction -= UpdateUndo;
        LoadManager.Instance.LogAction = UpdateLog;
        LoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        if (instance == this)
            instance = null;
    }
}
