using System;
using System.Collections;
using UnityEngine;
namespace StatsMonitor
{
    public class FpsCounter : MonoBehaviour
{
    #region Instance
    public static FpsCounter instance;
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
    private float count;
    [SerializeField] public bool lockFps = false;
    [SerializeField] int targetFps = 60;
    [Range(0.0f, 3.0f)]
    [SerializeField] float refreshRate = 0.1f;
    [SerializeField] int fontSize = 20;
    [SerializeField] Color fontColor = new Color(1, 1, 1, 1);
    [SerializeField] Color bgColor = Color.black;
    private event Action<Color> OnColorChanged;
    private event Action<int> OnFpsChanged;
    Vector2 counterPos;
    [SerializeField] Position currPos;
    [SerializeField] Vector2 boxSize = new Vector2(100, 100);
    [SerializeField] Vector2 fpsTextPlacement = new Vector2(10, 10);
    GUIStyle boxStyle = null;
    readonly GUIStyle debugGuiStyle = new();
    enum Position
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }
    private IEnumerator Start()
    {
        SetFps(targetFps);
        GUI.depth = 2;
        OnColorChanged += ChangebgColor;
        OnFpsChanged += SetFps;
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(refreshRate);
        }
    }
    public void SetFps(int _targetFps)
    {
        if (lockFps)
        {
            QualitySettings.vSyncCount = 0;
            if (_targetFps <= 0)
                Application.targetFrameRate = targetFps;
            else
                Application.targetFrameRate = _targetFps;
        }
        else
        {
            Application.targetFrameRate = -1;
        }
    }
    void OnValidate()
    {
        OnColorChanged?.Invoke(bgColor);
        OnFpsChanged?.Invoke(targetFps);
    }

    private void OnDestroy()
    {
        OnColorChanged -= ChangebgColor;
        OnFpsChanged -= SetFps;
    }
    private void ChangebgColor(Color newColor)
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex((int)boxSize.x, (int)boxSize.y, bgColor);
    }

    private void OnGUI()
    {
        InitStyles();
        debugGuiStyle.fontSize = fontSize;
        debugGuiStyle.normal.textColor = fontColor;
        SetBox();
        SetLabels();

    }
    private void InitStyles()
    {
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex((int)boxSize.x, (int)boxSize.y, bgColor);
        }
    }
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    void SetBox()
    {

        switch (currPos)
        {
            case Position.TopRight:
                counterPos = new Vector2(Screen.width - boxSize.x, 0);
                break;
            case Position.TopLeft:
                counterPos = new Vector2(0, 0);
                break;
            case Position.BottomRight:
                counterPos = new Vector2(Screen.width - boxSize.x, Screen.height - boxSize.y);
                break;
            case Position.BottomLeft:
                counterPos = new Vector2(0, Screen.height - boxSize.y);
                break;
            default:
                break;
        }
        GUI.Box(new Rect(counterPos.x, counterPos.y, boxSize.x, boxSize.y), "", boxStyle);
    }
    void SetLabels()
    {
        float x = counterPos.x + fpsTextPlacement.x;
        float y = counterPos.y + fpsTextPlacement.y;
        GUI.Label(new Rect(x, y + (15 * 1), 200, 50), $"Fps {Mathf.Round(count)}", this.debugGuiStyle);
        GUI.Label(new Rect(x, y + (15 * 2), 200, 50), $"", this.debugGuiStyle);
        GUI.Label(new Rect(x, y + (15 * 3), 200, 50), $"", this.debugGuiStyle);
    }



}
}


