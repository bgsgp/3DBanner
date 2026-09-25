using System;
using UnityEngine;

[Serializable]
public class BannerConfig
{
    [Header("倒计时与文案")]
    public string deadline = "2027-06-07 09:00:00";
    public string beforeText = "距高考还剩 <color=FFD700>{0} 天 {1} 时</color>";
    public string afterText = "距高考开始过去 <color=FF6347>{0} 天 {1} 时</color>";

    [Header("灯光配置")]
    public float lightIntensity = 2.5f;
    public string lightColor = "FFFFFF";
    public bool enableShadow = true;

    [Header("风力配置")]
    public float minWindStrength = 0.8f;
    public float maxWindStrength = 3.5f;
    public float minGustInterval = 3f;
    public float maxGustInterval = 10f;
    public float minGustDuration = 1.5f;
    public float maxGustDuration = 5f;
    [Range(0f, 1f)]
    public float windRandomness = 0.7f;

    [Header("画质配置")]
    [Range(0.1f, 1f)]
    public float clothVertexDensity = 0.6f;
    [Range(0.1f, 2f)]
    public float textureUpdateRateScale = 1f;
}
