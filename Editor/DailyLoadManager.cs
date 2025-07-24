using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace StatsMonitor
{
    public class DailyLoadManager
{
    #region Instance
    private static DailyLoadManager instance;
    public static DailyLoadManager Instance => instance ??= new DailyLoadManager();
    #endregion

    double totalDomainReloadTime = 0, totalTimeSpentInPlayMode = 0;
    int timesCompiled = 0, timesPlayModePressed = 0, totalSceneOpenedAmount = 0, totalRedoAmount = 0, totalUndoAmount = 0;
    int normalLogAmount = 0, warningLogAmount = 0, errorLogAmount = 0, assertLogAmount = 0, exceptionLogAmount = 0;
    public Action<int> CompiledAction;
    public Action<int> PlayModeAction;
    public Action<int> SceneAction;
    public Action<int> RedoAction;
    public Action<int> UndoAction;
    public Action<double> PlayModeTimeAction;
    public Action<double> DomainReloadTimeAction;
    public Action<LogType,int> LogAction;
    float totalTimeSpent = 0;
    string date = "";
    string saveFileName = Path.Combine(Application.persistentDataPath + "/currentSessionSpentTimeData.json");
    private DailyLoadManager()
    {
        Load();
    }
    #region Saving and loading
    void Load()
    {
        if (File.Exists(saveFileName))
        {
            string jsonDataLoad = File.ReadAllText(saveFileName);
            CurrentSessionDataList dataLoaded = JsonUtility.FromJson<CurrentSessionDataList>(jsonDataLoad);

            if (dataLoaded.spendDatas.Count > 0)
            {
                var item = dataLoaded.spendDatas[0];
                date = item.Date;
                timesCompiled = item.TimesCompiled;
                timesPlayModePressed = item.TimesPlayModePressed;
                totalTimeSpent = item.TotalTimeProjectOpen;
                totalDomainReloadTime = item.TotalDomainReloadTime;
                totalTimeSpentInPlayMode = item.TotalPlayModeTime;
                totalSceneOpenedAmount = item.TotalSceneOpenedAmount;
                totalRedoAmount = item.TotalRedoAmount;
                totalUndoAmount = item.TotalUndoAmount;
                normalLogAmount = item.NormalLogAmount;
                errorLogAmount = item.ErrorLogAmount;
                warningLogAmount = item.WarningLogAmount;
                assertLogAmount = item.AssertLogAmount;
                exceptionLogAmount = item.ExceptionLogAmount;
            }
        }
    }
    public void Reset()
    {
        totalDomainReloadTime = 0; 
        totalTimeSpentInPlayMode = 0;
        timesCompiled = 0;
        timesPlayModePressed = 0;
        totalSceneOpenedAmount = 0;
        totalRedoAmount = 0;
        totalUndoAmount = 0;

        normalLogAmount = 0;
        warningLogAmount = 0;
        errorLogAmount = 0;
        assertLogAmount = 0;
        exceptionLogAmount = 0;
    }
    public void Save()
    {
        CurrentSessionDataList dataToSave = new CurrentSessionDataList();
        dataToSave.spendDatas.Add(new CurrentSessionData
        {
            Date = date,
            TimesCompiled = timesCompiled,
            TimesPlayModePressed = timesPlayModePressed,
            TotalTimeProjectOpen = totalTimeSpent,
            TotalDomainReloadTime = totalDomainReloadTime,
            TotalPlayModeTime = totalTimeSpentInPlayMode,
            TotalSceneOpenedAmount = totalSceneOpenedAmount,
            TotalRedoAmount = totalRedoAmount,
            TotalUndoAmount = totalUndoAmount,
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
    public void IncrementCompiled()
    {
        timesCompiled++;
        CompiledAction?.Invoke(timesCompiled);
    }
    public void IncrementPlayPressed()
    {
        timesPlayModePressed++;
        PlayModeAction?.Invoke(timesPlayModePressed);
        Save();
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
        LogAction?.Invoke(type , _incremented);
    }
    public void IncrementScenesOpened()
    {
        totalSceneOpenedAmount++;
        SceneAction?.Invoke(totalSceneOpenedAmount);
    }
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
    public string GetDate() => date;
    public float TotalTime() => totalTimeSpent;
    public double TotalDomainReloadTime() => totalDomainReloadTime;
    public double TotalPlayModeTime() => totalTimeSpentInPlayMode;
    #endregion
}
#region Save Data Total
[System.Serializable]
public class CurrentSessionData
{
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
public class CurrentSessionDataList
{
    public List<CurrentSessionData> spendDatas = new List<CurrentSessionData>();
}
#endregion
}
