using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrentSession : VisualElement
{
    public static CurrentSession instance;
    int timesProjectCompiled = 0, timesPlayModePressed = 0, totalScenesOpenedAmount = 0, totalRedoAmount = 0, totalUndoAmount = 0;
    int normalLogAmount = 0, warningLogAmount = 0, errorLogAmount = 0, assertLogAmount = 0, exceptionLogAmount = 0;
    float currentSessionLength = 0f;
    double totalDomainReloadTime = 0f, totalPlayModeTime = 0f;

    private Label _sessionLengthLabel;
    private Label _sessionSceneOpenedCountLabel;
    private Label _sessionPlayModeCountLabel;
    private Label _sessionCompiledCountLabel;
    private Label _sessionRedoCountLabel;
    private Label _sessionUndoCountLabel;
    private Label _sessionNormalLogCountLabel;
    private Label _sessionErrorLogCountLabel;
    private Label _sessionWarningLogCountLabel;
    private Label _sessionAssertLogCountLabel;
    private Label _sessionExceptionLogCountLabel;
    private Label _sessionDomainReloadTimeLabel;
    private Label _sessionPlayModeTimeLabel;

    public CurrentSession()
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
        container.Add(NewTimeLabel(ref _sessionLengthLabel, styleSheet, currentSessionLength, new Vector2(50,40), new Vector2(120, 30)));
        container.Add(NewTextLabel("Current", new Vector2(50, 60), TextAnchor.MiddleCenter));
        container.Add(NewTimeLabel(ref _sessionDomainReloadTimeLabel, styleSheet, totalDomainReloadTime, new Vector2(200,40), new Vector2(120, 30)));
        container.Add(NewTextLabel("Domain Reload", new Vector2(200, 60), TextAnchor.MiddleCenter));
        container.Add(NewTimeLabel(ref _sessionPlayModeTimeLabel, styleSheet, totalPlayModeTime, new Vector2(350,40), new Vector2(120, 30)));
        container.Add(NewTextLabel("Play time", new Vector2(350,60), TextAnchor.MiddleCenter));


        container.Add(NewTextLabel("[ Actions ]", new Vector2(45, 100), TextAnchor.MiddleLeft));
        container.Add(NewCountLabel(ref _sessionSceneOpenedCountLabel, styleSheet, totalScenesOpenedAmount, new Vector2(50, 140)));
        container.Add(NewTextLabel("Scenes opened", new Vector2(50, 160), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _sessionCompiledCountLabel, styleSheet, timesProjectCompiled, new Vector2(200, 140)));
        container.Add(NewTextLabel("Times compiled", new Vector2(200, 160), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _sessionPlayModeCountLabel, styleSheet, timesPlayModePressed, new Vector2(350, 140)));
        container.Add(NewTextLabel("Play mode entered" , new Vector2(350, 160), TextAnchor.MiddleCenter));


        container.Add(NewTextLabel("[ Edits ]", new Vector2(45, 200), TextAnchor.MiddleLeft));
        container.Add(NewCountLabel(ref _sessionRedoCountLabel, styleSheet, totalRedoAmount, new Vector2(50, 240)));
        container.Add(NewTextLabel("Times Redo", new Vector2(50, 260), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _sessionUndoCountLabel, styleSheet, totalUndoAmount, new Vector2(200, 240)));
        container.Add(NewTextLabel("Times Undo", new Vector2(200, 260), TextAnchor.MiddleCenter));


        container.Add(NewTextLabel("[ Logs ]", new Vector2(45, 300), TextAnchor.MiddleLeft));
        container.Add(NewCountLabel(ref _sessionNormalLogCountLabel, styleSheet, normalLogAmount, new Vector2(50, 340)));
        container.Add(NewTextLabel("Normal log count", new Vector2(50, 360), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _sessionWarningLogCountLabel, styleSheet, warningLogAmount, new Vector2(200, 340)));
        container.Add(NewTextLabel("Warning log count", new Vector2(200, 360), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _sessionErrorLogCountLabel, styleSheet, errorLogAmount, new Vector2(350, 340)));
        container.Add(NewTextLabel("Error log count", new Vector2(350, 360), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _sessionExceptionLogCountLabel, styleSheet, exceptionLogAmount, new Vector2(50, 400)));
        container.Add(NewTextLabel("Exception log count", new Vector2(50, 420), TextAnchor.MiddleCenter));
        container.Add(NewCountLabel(ref _sessionAssertLogCountLabel, styleSheet, assertLogAmount, new Vector2(200, 400)));
        container.Add(NewTextLabel("Assert log count", new Vector2(200, 420), TextAnchor.MiddleCenter));


        EditorApplication.update += UpdateTime;
        DailyLoadManager.Instance.CompiledAction += SetCompiled;
        DailyLoadManager.Instance.PlayModeAction += UpdatePlayModeEntered;
        DailyLoadManager.Instance.PlayModeTimeAction += AddPlayModeTime;
        DailyLoadManager.Instance.DomainReloadTimeAction += AddDomainReloadTime;
        DailyLoadManager.Instance.SceneAction += UpdateScene;
        DailyLoadManager.Instance.RedoAction += UpdateRedo;
        DailyLoadManager.Instance.UndoAction += UpdateUndo;
        DailyLoadManager.Instance.LogAction += UpdateLog;
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
    #region Label updaters
    void UpdateTime()
    {
        currentSessionLength = (float)(EditorApplication.timeSinceStartup);
        _sessionLengthLabel.text = $"{TimeSpan.FromSeconds(currentSessionLength).Hours} H " +
            $"{TimeSpan.FromSeconds(currentSessionLength).Minutes} mm " +
            $"{TimeSpan.FromSeconds(currentSessionLength).Seconds} ss";
    }
    private void AddDomainReloadTime(double obj)
    {
        totalDomainReloadTime = obj;
        _sessionDomainReloadTimeLabel.text = $"{TimeSpan.FromSeconds(totalDomainReloadTime).Hours} H " +
            $"{TimeSpan.FromSeconds(totalDomainReloadTime).Minutes} mm " +
            $"{TimeSpan.FromSeconds(totalDomainReloadTime).Seconds} ss ";
    }
    private void AddPlayModeTime(double obj)
    {
        _sessionPlayModeTimeLabel.text = $"{TimeSpan.FromSeconds(obj).Hours} H " +
            $"{TimeSpan.FromSeconds(obj).Minutes} mm " +
            $"{TimeSpan.FromSeconds(obj).Seconds} ss ";
    }
    private void UpdatePlayModeEntered(int obj)
    {
        _sessionPlayModeCountLabel.text = $"{obj}";
    }
    private void UpdateRedo(int obj)
    {
        _sessionRedoCountLabel.text = $"{obj}";
    }
    private void UpdateUndo(int arg2)
    {
        _sessionUndoCountLabel.text = $"{arg2}";
    }
    private void UpdateLog(LogType type, int arg2)
    {
        switch (type)
        {
            case LogType.Error:
                _sessionErrorLogCountLabel.text = $"{arg2}";
                break;
            case LogType.Assert:
                _sessionAssertLogCountLabel.text = $"{arg2}";
                break;
            case LogType.Warning:
                _sessionWarningLogCountLabel.text = $"{arg2}";
                break;
            case LogType.Log:
                _sessionNormalLogCountLabel.text = $"{arg2}";
                break;
            case LogType.Exception:
                _sessionExceptionLogCountLabel.text = $"{arg2}";
                break;
            default:
                break;
        }
    }
    private void UpdateScene(int count)
    {
        _sessionSceneOpenedCountLabel.text = $"{count}";
    }
    public void SetCompiled(int count)
    {
        _sessionCompiledCountLabel.text = $"{count}";
    }
    #endregion
    void GetTimes()
    {
        timesProjectCompiled = DailyLoadManager.Instance.Compiled();
        timesPlayModePressed = DailyLoadManager.Instance.PlayPressed();
        totalRedoAmount = DailyLoadManager.Instance.Redo();
        totalUndoAmount = DailyLoadManager.Instance.Undo();
        totalDomainReloadTime = DailyLoadManager.Instance.TotalDomainReloadTime();
        totalPlayModeTime = DailyLoadManager.Instance.TotalPlayModeTime();
        totalScenesOpenedAmount = DailyLoadManager.Instance.ScenesOpened();
        normalLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Log);
        warningLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Warning);
        errorLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Error);
        exceptionLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Exception);
        assertLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Assert);
        currentSessionLength = (float)(EditorApplication.timeSinceStartup);
    }
    ~CurrentSession()
    {
        EditorApplication.update -= UpdateTime;
        DailyLoadManager.Instance.CompiledAction -= SetCompiled;
        DailyLoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        DailyLoadManager.Instance.SceneAction -= UpdateScene;
        DailyLoadManager.Instance.UndoAction -= UpdateRedo;
        DailyLoadManager.Instance.RedoAction -= UpdateUndo;
        DailyLoadManager.Instance.LogAction -= UpdateLog;
        DailyLoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        if (instance == this)
            instance = null;
    }
}
