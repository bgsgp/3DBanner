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
                // 阵风结束，恢复无风
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

        // 平滑过渡风力，避免突变
        _currentWind = Vector3.Lerp(_currentWind, _targetWind, Time.deltaTime * 2f);
        _cloth.externalAcceleration = _currentWind;
    }

    private void TriggerRandomGust()
    {
        _isGusting = true;

        // 基础风力 + 随机度波动
        float baseStrength = Random.Range(_config.minWindStrength, _config.maxWindStrength);
        float randomFactor = 1f + Random.Range(-_config.windRandomness, _config.windRandomness);
        float finalStrength = Mathf.Max(0.2f, baseStrength * randomFactor);

        // 三维完全随机风向
        Vector3 windDir = Random.insideUnitSphere.normalized;
        _targetWind = windDir * finalStrength;

        // 阵风持续时间
        float baseDuration = Random.Range(_config.minGustDuration, _config.maxGustDuration);
        float durationFactor = 1f + Random.Range(-_config.windRandomness, _config.windRandomness);
        _gustRemaining = Mathf.Max(0.5f, baseDuration * durationFactor);
    }
}
