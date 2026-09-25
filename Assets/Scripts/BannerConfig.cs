using System;
using System.Text.Json.Serialization;
using UnityEngine;

[Serializable]
public class BannerConfig
{
    [Header("倒计时与文案")]
    [JsonPropertyName("deadline")]
    public string Deadline { get; set; } = "2027-06-07 09:00:00";
    
    [JsonPropertyName("beforeText")]
    public string BeforeDeadlineText { get; set; } = "距高考还剩 <color=FF0000>{0} 天 {1} 时</color>";
    
    [JsonPropertyName("afterText")]
    public string AfterDeadlineText { get; set; } = "距高考开始过去 <color=FF0000>{0} 天 {1} 时</color>";

    [Header("灯光配置")]
    [JsonPropertyName("lightIntensity")]
    public float LightIntensity { get; set; } = 2.5f;
    
    [JsonPropertyName("lightColor")]
    public string LightColor { get; set; } = "FFFFFF";
    
    [JsonPropertyName("enableShadow")]
    public bool EnableShadow { get; set; } = true;

    [Header("风力配置")]
    [JsonPropertyName("minWindStrength")]
    public float MinWindStrength { get; set; } = 0.8f;
    
    [JsonPropertyName("maxWindStrength")]
    public float MaxWindStrength { get; set; } = 3.5f;
    
    [JsonPropertyName("minGustInterval")]
    public float MinGustInterval { get; set; } = 3f;
    
    [JsonPropertyName("maxGustInterval")]
    public float MaxGustInterval { get; set; } = 10f;
    
    [JsonPropertyName("minGustDuration")]
    public float MinGustDuration { get; set; } = 1.5f;
    
    [JsonPropertyName("maxGustDuration")]
    public float MaxGustDuration { get; set; } = 5f;
    
    [JsonPropertyName("windRandomness")]
    [Range(0f, 1f)]
    public float WindRandomness { get; set; } = 0.7f;

    [Header("画质配置")]
    [JsonPropertyName("clothVertexDensity")]
    [Range(0.1f, 1f)]
    public float ClothVertexDensity { get; set; } = 0.6f;
    
    [JsonPropertyName("textureUpdateRateScale")]
    [Range(0.1f, 2f)]
    public float TextureUpdateRateScale { get; set; } = 1f;
}
