using UnityEngine;
using System.IO;
using System;
using System.Text.Json;

public class BannerManager : MonoBehaviour
{
    [Header("引用")]
    public Camera mainCamera;

    private BannerConfig _config;
    private GameObject _bannerObj;
    private float _bannerWidth;
    private float _bannerHeight;
    private DateTime _deadline;
    private float _updateTimer;
    private float _updateInterval = 3600f;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // 背景纯透明 + 强制关闭天空盒
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0, 0, 0, 0);
        RenderSettings.skybox = null;

        LoadConfig();
        if (_config == null) return;

        CreateBanner();
        CreateSpotLights();
        RefreshBannerText();
    }

    private void LoadConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "banner_config.json");

        if (!File.Exists(configPath))
        {
            Debug.LogError("未找到配置文件，请在 StreamingAssets 目录下创建 banner_config.json");
            _config = null;
            return;
        }

        string json = File.ReadAllText(configPath);

        // System.Text.Json 核心配置
        var jsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,       // 支持 // 和 /* */ 注释，自动跳过
            PropertyNameCaseInsensitive = true,                    // 字段名不区分大小写，容错更高
            AllowTrailingCommas = true,                           // 允许最后一个字段后带逗号
        };

        _config = JsonSerializer.Deserialize<BannerConfig>(json, jsonOptions);
        _deadline = DateTime.ParseExact(_config.deadline, "yyyy-MM-dd HH:mm:ss", null);
        _updateInterval = 3600f / _config.textureUpdateRateScale;
    }

    // 保留序列化方法，需要自动生成配置时可自行调用
    private void SaveConfig(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(_config, jsonOptions);
        File.WriteAllText(path, json);
    }

    private void CreateBanner()
    {
        float distance = 5f;
        float viewHeightRatio = 0.25f;
        _bannerHeight = 2 * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * viewHeightRatio;
        _bannerWidth = _bannerHeight * 4f;

        _bannerObj = new GameObject("Banner");
        _bannerObj.transform.SetParent(transform);

        Vector3 centerPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.75f, distance));
        _bannerObj.transform.position = centerPos;

        int xSeg = Mathf.RoundToInt(20 * _config.clothVertexDensity);
        int ySeg = Mathf.RoundToInt(5 * _config.clothVertexDensity);
        Mesh bannerMesh = GenPlaneMesh(_bannerWidth, _bannerHeight, xSeg, ySeg);

        MeshFilter mf = _bannerObj.AddComponent<MeshFilter>();
        mf.mesh = bannerMesh;
        _bannerObj.AddComponent<MeshRenderer>();

        // Unity 6 布料组件适配
        Cloth cloth = _bannerObj.AddComponent<Cloth>();
        cloth.useGravity = true;
        cloth.sleepThreshold = 0.005f;

        ClothSkinningCoefficient[] coeffs = cloth.coefficients;
        for (int i = 0; i < coeffs.Length; i++)
        {
            coeffs[i].maxDistance = 0.08f;
        }

        // 固定左右两个上角
        Vector3[] verts = bannerMesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            bool isTopLeft = Mathf.Approximately(verts[i].x, -_bannerWidth / 2f)
                           && Mathf.Approximately(verts[i].y, _bannerHeight / 2f);
            bool isTopRight = Mathf.Approximately(verts[i].x, _bannerWidth / 2f)
                            && Mathf.Approximately(verts[i].y, _bannerHeight / 2f);

            if (isTopLeft || isTopRight)
                coeffs[i].maxDistance = 0f;
        }
        cloth.coefficients = coeffs;

        // 挂载功能组件（已移除碰撞和交互）
        var texGen = _bannerObj.AddComponent<BannerTextureGenerator>();
        texGen.Initialize(_bannerWidth, _bannerHeight);

        var windCtrl = _bannerObj.AddComponent<BannerWindController>();
        windCtrl.Init(_config);
    }

    private Mesh GenPlaneMesh(float width, float height, int xSeg, int ySeg)
    {
        Mesh mesh = new Mesh { name = "BannerPlane" };

        Vector3[] vertices = new Vector3[(xSeg + 1) * (ySeg + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[xSeg * ySeg * 6];

        float xStep = width / xSeg;
        float yStep = height / ySeg;
        float xStart = -width / 2f;
        float yStart = -height / 2f;

        for (int y = 0; y <= ySeg; y++)
        {
            for (int x = 0; x <= xSeg; x++)
            {
                int idx = y * (xSeg + 1) + x;
                vertices[idx] = new Vector3(xStart + x * xStep, yStart + y * yStep, 0);
                uv[idx] = new Vector2((float)x / xSeg, (float)y / ySeg);
            }
        }

        int triIdx = 0;
        for (int y = 0; y < ySeg; y++)
        {
            for (int x = 0; x < xSeg; x++)
            {
                int a = y * (xSeg + 1) + x;
                int b = a + 1;
                int c = a + xSeg + 1;
                int d = c + 1;

                triangles[triIdx++] = a;
                triangles[triIdx++] = c;
                triangles[triIdx++] = b;
                triangles[triIdx++] = b;
                triangles[triIdx++] = c;
                triangles[triIdx++] = d;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void CreateSpotLights()
    {
        float[] pointX = new float[5];
        for (int i = 0; i < 5; i++)
            pointX[i] = -_bannerWidth / 2f + (_bannerWidth / 4f) * i;

        int[] lightIndexes = { 1, 3 };
        Color lightColor = HexToColor(_config.lightColor);

        foreach (int idx in lightIndexes)
        {
            GameObject lightObj = new GameObject($"BannerLight_{idx}");
            lightObj.transform.SetParent(transform);

            Vector3 localPos = new Vector3(
                pointX[idx],
                _bannerHeight * 0.5f,
                _bannerHeight * (Mathf.Sqrt(3) / 2f)
            );

            lightObj.transform.position = _bannerObj.transform.TransformPoint(localPos);
            lightObj.transform.LookAt(_bannerObj.transform.TransformPoint(new Vector3(pointX[idx], 0, 0)));

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.intensity = _config.lightIntensity;
            light.color = lightColor;
            light.spotAngle = 60f;
            light.range = 12f;
            light.shadows = _config.enableShadow ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = 0.8f;
        }
    }

    private void RefreshBannerText()
    {
        TimeSpan diff = _deadline - DateTime.Now;
        bool isExpired = diff.TotalSeconds < 0;

        if (isExpired)
            diff = DateTime.Now - _deadline;

        int days = Mathf.FloorToInt((float)diff.TotalDays);
        int hours = diff.Hours;

        string template = isExpired ? _config.afterText : _config.beforeText;
        string finalText = string.Format(template, days, hours);

        _bannerObj.GetComponent<BannerTextureGenerator>().RefreshText(finalText);
    }

    private void Update()
    {
        if (_config == null) return;

        _updateTimer += Time.deltaTime;
        if (_updateTimer >= _updateInterval)
        {
            _updateTimer = 0f;
            RefreshBannerText();
        }
    }

    private Color HexToColor(string hex)
    {
        if (hex.Length != 6) return Color.white;
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}
