using System;
using System.Text;
using UnityEngine;

public class ErrorDebugGUI : MonoBehaviour
{
    [Range(0,1)]
    public float ErrorTimeScale;

    private string _errorMsg;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        Application.logMessageReceived += onLogMessageReceived;
        enabled = false;
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        Application.logMessageReceived -= onLogMessageReceived;
        
    }

    private void onLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if(enabled || type == LogType.Log || type == LogType.Warning) return;

        enabled = true;
        Time.timeScale = ErrorTimeScale;

        var sb = new StringBuilder();
        sb.AppendLine(condition);
        sb.AppendLine();
        sb.AppendLine(stackTrace);

        _errorMsg = sb.ToString();
    }

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// This function can be called multiple times per frame (one call per event).
    /// </summary>
    void OnGUI()
    {
        var safeView = Screen.safeArea;
        
        int fontSize = (int)(safeView.width*0.025f);
        var btnSize = safeView.width*0.15f;

        // 1. 全屏半透明黑色背景
        GUI.color = new Color(0, 0, 0, 0.5f); // 透明度 50%

        GUI.DrawTexture(safeView, Texture2D.whiteTexture);

        // 2. 显示文本
        GUI.color = Color.red;
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

        var labelRect = safeView;
        labelRect.height -= btnSize;

        GUI.Label(labelRect,_errorMsg);

        // 3. 两个按钮（水平排列）
        GUI.color = Color.yellow;
        GUI.skin.button.fontSize = fontSize*2;
        GUILayout.BeginArea(new Rect(0, labelRect.yMax, labelRect.width, btnSize));
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("复制到剪切板", GUILayout.Height(btnSize)))
        {
            // Debug.Log("点击了复制到剪切板");

            GUIUtility.systemCopyBuffer = _errorMsg;
        }

        if (GUILayout.Button("关闭", GUILayout.Height(btnSize)))
        {
            // Debug.Log("点击了关闭");

            enabled = false;
            Time.timeScale = 0;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

}
