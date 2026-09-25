using System;
using UnityEngine;

[Serializable]
public class BannerConfig
{
    [Header("倒计时与文案")]
    public string deadline;
    public string beforeText;
    public string afterText;

    [Header("灯光配置")]
    public float lightIntensity;
    public string lightColor;
    public bool enableShadow;

    [Header("风力配置")]
    public float minWindStrength;
    public float maxWindStrength;
    public float minGustInterval;
    public float maxGustInterval;
    public float minGustDuration;
    public float maxGustDuration;
    [Range(0f, 1f)]
    public float windRandomness;

    [Header("画质配置")]
    [Range(0.1f, 1f)]
    public float clothVertexDensity;
    [Range(0.1f, 2f)]
    public float textureUpdateRateScale;
}
