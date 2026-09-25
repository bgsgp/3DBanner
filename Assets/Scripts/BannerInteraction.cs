using UnityEngine;
using System.Diagnostics;
using System.IO;

[RequireComponent(typeof(Collider))]
public class BannerInteraction : MonoBehaviour
{
    private int _clickCount;
    private float _lastClickTime;
    private const float ResetTimeout = 3f;
    private string _configPath;

    public void Init(string configPath)
    {
        _configPath = configPath;
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        
        // 超时重置计数
        if (Time.time - _lastClickTime > ResetTimeout)
            _clickCount = 0;

        _clickCount++;
        _lastClickTime = Time.time;

        if (_clickCount >= 8)
        {
            OpenConfigFile();
            _clickCount = 0;
        }
    }

    private void OpenConfigFile()
    {
        if (!File.Exists(_configPath))
        {
            Debug.LogError($"配置文件不存在：{_configPath}");
            return;
        }

        try
        {
            // Windows下调用资源管理器打开文件
            Process.Start("explorer.exe", $"\"{_configPath}\"");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"打开配置失败：{e.Message}");
        }
    }
}
