using System;
using UnityEngine;
using System.Text.Json.Serialization;

[Serializable]
public class BannerConfig
{
    [Header("倒计时与文案")]
    [JsonPropertyName("deadline")]
    public string deadline;

    [JsonPropertyName("beforeText")]
    public string beforeText;

    [JsonPropertyName("afterText")]
    public string afterText;

    [Header("灯光配置")]
    [JsonPropertyName("lightIntensity")]
    public float lightIntensity;

    [JsonPropertyName("lightColor")]
    public string lightColor;

    [JsonPropertyName("enableShadow")]
    public bool enableShadow;

    [Header("风力配置")]
    [JsonPropertyName("minWindStrength")]
    public float minWindStrength;

    [JsonPropertyName("maxWindStrength")]
    public float maxWindStrength;

    [JsonPropertyName("minGustInterval")]
    public float minGustInterval;

    [JsonPropertyName("maxGustInterval")]
    public float maxGustInterval;

    [JsonPropertyName("minGustDuration")]
    public float minGustDuration;

    [JsonPropertyName("maxGustDuration")]
    public float maxGustDuration;

    [JsonPropertyName("windRandomness")]
    [Range(0f, 1f)]
    public float windRandomness;

    [Header("画质配置")]
    [JsonPropertyName("clothVertexDensity")]
    [Range(0.1f, 1f)]
    public float clothVertexDensity;

    [JsonPropertyName("textureUpdateRateScale")]
    [Range(0.1f, 2f)]
    public float textureUpdateRateScale;
}
