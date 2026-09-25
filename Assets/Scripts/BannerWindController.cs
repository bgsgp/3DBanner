using UnityEngine;

[RequireComponent(typeof(Cloth))]
public class BannerWindController : MonoBehaviour
{
    private Cloth _cloth;
    private BannerConfig _config;

    private float _gustCooldown;
    private float _gustRemaining;
    private bool _isGusting;
    private Vector3 _currentWind;
    private Vector3 _targetWind;

    public void Init(BannerConfig config)
    {
        _config = config;
        _cloth = GetComponent<Cloth>();
        _currentWind = Vector3.zero;
        _gustCooldown = Random.Range(config.minGustInterval, config.maxGustInterval);
    }

    private void Update()
    {
        if (_config == null) return;

        if (_isGusting)
        {
            _gustRemaining -= Time.deltaTime;
            if (_gustRemaining <= 0f)
            {
                _targetWind = Vector3.zero;
                _isGusting = false;
                _gustCooldown = Random.Range(_config.minGustInterval, _config.maxGustInterval);
            }
        }
        else
        {
            _gustCooldown -= Time.deltaTime;
            if (_gustCooldown <= 0f)
            {
                TriggerRandomGust();
            }
        }

        _currentWind = Vector3.Lerp(_currentWind, _targetWind, Time.deltaTime * 2f);
        _cloth.externalAcceleration = _currentWind;
    }

    private void TriggerRandomGust()
    {
        _isGusting = true;

        float baseStrength = Random.Range(_config.minWindStrength, _config.maxWindStrength);
        float randomFactor = 1f + Random.Range(-_config.windRandomness, _config.windRandomness);
        float finalStrength = Mathf.Max(0.2f, baseStrength * randomFactor);

        // 优化：约束风向。让风主要朝向横幅吹(Z轴)，带有轻微的水平偏移(X)和垂直偏移(Y)
        // 而不是之前的 Random.insideUnitSphere.normalized（可能会反向吹或直接往天上吹）
        Vector3 windDir = new Vector3(
            Random.Range(-0.5f, 0.5f), // X轴轻微摇摆
            Random.Range(-0.2f, 0.2f), // Y轴极小浮动
            Random.Range(0.5f, 1f)     // Z轴主风力
        ).normalized;

        _targetWind = windDir * finalStrength;

        float baseDuration = Random.Range(_config.minGustDuration, _config.maxGustDuration);
        float durationFactor = 1f + Random.Range(-_config.windRandomness, _config.windRandomness);
        _gustRemaining = Mathf.Max(0.5f, baseDuration * durationFactor);
    }
}