using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace StatsMonitor
{
    public class LoadManager
{
    #region Instance
    private static LoadManager instance;
    public static LoadManager Instance => instance ??= new LoadManager();
    #endregion

    double totalDomainReloadTime = 0, totalTimeSpentInPlayMode = 0, longestSession = 0;
    int timesProjectOpened = 0, timesCompiled = 0, timesPlayModePressed = 0, totalSceneOpenedAmount = 0, totalRedoAmount = 0, totalUndoAmount = 0, crashAmount = 0;
    int normalLogAmount = 0, warningLogAmount = 0, errorLogAmount = 0, assertLogAmount = 0, exceptionLogAmount = 0;
    bool openedIncremented = false;
    public Action<int> CompiledAction;
    public Action<int> CrashAction;
    public Action<int> ProjectOpenedAction;
    public Action<int> PlayModeAction;
    public Action<int> SceneAction;
    public Action<int> RedoAction;
    public Action<int> UndoAction;
    public Action<double> PlayModeTimeAction;
    public Action<double> DomainReloadTimeAction;
    public Action<LogType, int> LogAction;
    float totalTimeSpent = 0;
    string date = "";
    string saveFileName = Path.Combine(Application.persistentDataPath + "/spentTimeData.json");
    public LoadManager()
    {
        Load();
    }
    #region Saving and loading
    void Load()
    {
        if (File.Exists(saveFileName))
        {
            string jsonDataLoad = File.ReadAllText(saveFileName);
            SpendDataList dataLoaded = JsonUtility.FromJson<SpendDataList>(jsonDataLoad);

            if (dataLoaded.spendDatas.Count > 0)
            {
                var item = dataLoaded.spendDatas[0];
                openedIncremented = item.OpenedIncremented;
                date = item.Date;
                timesProjectOpened = item.TimesOpened;
                timesCompiled = item.TimesCompiled;
                timesPlayModePressed = item.TimesPlayModePressed;
                totalTimeSpent = item.TotalTimeProjectOpen;
                totalDomainReloadTime = item.TotalDomainReloadTime;
                totalTimeSpentInPlayMode = item.TotalPlayModeTime;
                longestSession = item.LongestSession;
                totalSceneOpenedAmount = item.TotalSceneOpenedAmount;
                totalRedoAmount = item.TotalRedoAmount;
                totalUndoAmount = item.TotalUndoAmount;
                crashAmount = item.CrashAmount;
                normalLogAmount = item.NormalLogAmount;
                errorLogAmount = item.ErrorLogAmount;
                warningLogAmount = item.WarningLogAmount;
                assertLogAmount = item.AssertLogAmount;
                exceptionLogAmount = item.ExceptionLogAmount;
            }
        }
    }
    public void Save()
    {
        SpendDataList dataToSave = new SpendDataList();
        dataToSave.spendDatas.Add(new SpendData
        {
            OpenedIncremented = openedIncremented,
            Date = date,
            TimesOpened = timesProjectOpened,
            TimesCompiled = timesCompiled,
            TimesPlayModePressed = timesPlayModePressed,
            TotalTimeProjectOpen = totalTimeSpent,
            TotalDomainReloadTime = totalDomainReloadTime,
            TotalPlayModeTime = totalTimeSpentInPlayMode,
            TotalSceneOpenedAmount = totalSceneOpenedAmount,
            LongestSession = longestSession,
            TotalRedoAmount = totalRedoAmount,
            TotalUndoAmount = totalUndoAmount,
            CrashAmount = crashAmount,
            ErrorLogAmount = errorLogAmount,
            NormalLogAmount = normalLogAmount,
            WarningLogAmount = warningLogAmount,
            AssertLogAmount = assertLogAmount,
            ExceptionLogAmount = exceptionLogAmount,
        });

        string json = JsonUtility.ToJson(dataToSave, true);
        File.WriteAllText(saveFileName, json);
    }
    #endregion
    #region Setters
    // Setters / Incrementers
    public void Opened(bool value)
    {
        openedIncremented = value;
    }
    public bool GetOpened() => openedIncremented;
    public void IncrementOpened()
    {
        timesProjectOpened++;
        ProjectOpenedAction?.Invoke(timesProjectOpened);
    }
    public void IncrementCompiled()
    {
        timesCompiled++;
        CompiledAction?.Invoke(timesCompiled);
    }
    public void IncrementPlayPressed()
    {
        timesPlayModePressed++;
        PlayModeAction?.Invoke(timesPlayModePressed);
    }
    public void IncrementUndo()
    {
        totalUndoAmount++;
        UndoAction?.Invoke(totalUndoAmount);
    }
    public void IncrementRedo()
    {
        totalRedoAmount++;
        RedoAction?.Invoke(totalRedoAmount);
    }
    public void IncrementLog(LogType type)
    {
        int _incremented = 0;
        switch (type)
        {
            case LogType.Error:
                errorLogAmount++;
                _incremented = errorLogAmount;
                break;
            case LogType.Assert:
                assertLogAmount++;
                _incremented = assertLogAmount;
                break;
            case LogType.Warning:
                warningLogAmount++;
                _incremented = warningLogAmount;
                break;
            case LogType.Log:
                normalLogAmount++;
                _incremented = normalLogAmount;
                break;
            case LogType.Exception:
                exceptionLogAmount++;
                _incremented = exceptionLogAmount;
                break;
            default:
                break;
        }
        LogAction?.Invoke(type, _incremented);
    }
    public void IncrementScenesOpened()
    {
        totalSceneOpenedAmount++;
        SceneAction?.Invoke(totalSceneOpenedAmount);
    }
    public void IncrementCrashesh() => crashAmount++;
    public void AddTime(float seconds) => totalTimeSpent += seconds;
    public void AddDomainReloadTime(double seconds)
    {
        totalDomainReloadTime += seconds;
        DomainReloadTimeAction?.Invoke(totalDomainReloadTime);
    }
    public void AddPlayModeTime(double seconds)
    {
        totalTimeSpentInPlayMode += seconds;
        PlayModeTimeAction?.Invoke(totalTimeSpentInPlayMode);
    }
    public void SetLongestSession(double seconds) => longestSession = seconds;
    public void SetDate(string _date)
    {
        if(date != _date)
        {
            date = _date;
        }
    }
    #endregion
    #region Getters
    // Getters
    public int Opened() => timesProjectOpened;
    public int ScenesOpened() => totalSceneOpenedAmount;
    public int Compiled() => timesCompiled;
    public int PlayPressed() => timesPlayModePressed;
    public int Redo() => totalRedoAmount;
    public int Undo() => totalUndoAmount;
    public int GetLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                return errorLogAmount;
            case LogType.Assert:
                return assertLogAmount;
            case LogType.Warning:
                return warningLogAmount;
            case LogType.Log:
                return normalLogAmount;
            case LogType.Exception:
                return exceptionLogAmount;
            default:
                return 0;
        }
    }
    public int Crashesh() => crashAmount;
    public string GetDate() => date;
    public float TotalTime() => totalTimeSpent;
    public double TotalDomainReloadTime() => totalDomainReloadTime;
    public double TotalPlayModeTime() => totalTimeSpentInPlayMode;
    public double LongestSession() => longestSession;
    #endregion
}
#region Save Data Total
[System.Serializable]
public class SpendData
{
    public bool OpenedIncremented;
    public string Date;
    public int TimesOpened;
    public int TimesCompiled;
    public int TimesPlayModePressed;
    public int TotalSceneOpenedAmount;
    public int TotalRedoAmount;
    public int TotalUndoAmount;
    public int CrashAmount;
    public int NormalLogAmount;
    public int WarningLogAmount;
    public int ErrorLogAmount;
    public int ExceptionLogAmount;
    public int AssertLogAmount;
    public int AssertLogAmountDaily;
    public float TotalTimeProjectOpen;
    public double TotalDomainReloadTime;
    public double TotalPlayModeTime;
    public double LongestSession;
}
[System.Serializable]
public class SpendDataList
{
    public List<SpendData> spendDatas = new List<SpendData>();
}
#endregion
}
