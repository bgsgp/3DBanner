using UnityEngine;
using System.IO;
using System.Text.Json;
using System;

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
    private float _updateInterval = 3600f; // 默认1小时更新一次

    private void Start()
    {
        if (mainCamera == null) 
            mainCamera = Camera.main;
        
        LoadConfig();
        CreateBanner();
        CreateSpotLights();
        RefreshBannerText();
    }

    private void LoadConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "banner_config.json");
        
        if (!File.Exists(configPath))
        {
            Debug.Log("未找到配置文件，已生成默认配置");
            _config = new BannerConfig();
            SaveConfig(configPath);
        }
        else
        {
            string json = File.ReadAllText(configPath);
            _config = JsonSerializer.Deserialize<BannerConfig>(json);
        }

        _deadline = DateTime.ParseExact(_config.Deadline, "yyyy-MM-dd HH:mm:ss", null);
        _updateInterval = 3600f / _config.TextureUpdateRateScale;
    }

    private void SaveConfig(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private void CreateBanner()
    {
        // 计算横幅尺寸：高度 = 屏幕高度的1/4，居上显示
        float distance = 5f;
        float viewHeightRatio = 0.25f;
        _bannerHeight = 2 * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * viewHeightRatio;
        _bannerWidth = _bannerHeight * 4f; // 标准横幅长宽比4:1

        // 创建横幅物体
        _bannerObj = new GameObject("Banner");
        _bannerObj.transform.SetParent(transform);
        
        // 位置：屏幕垂直方向 75% 处（居上）
        Vector3 centerPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.75f, distance));
        _bannerObj.transform.position = centerPos;

        // 生成自定义精度的平面网格
        int xSeg = Mathf.RoundToInt(20 * _config.ClothVertexDensity);
        int ySeg = Mathf.RoundToInt(5 * _config.ClothVertexDensity);
        Mesh bannerMesh = GenPlaneMesh(_bannerWidth, _bannerHeight, xSeg, ySeg);

        MeshFilter mf = _bannerObj.AddComponent<MeshFilter>();
        mf.mesh = bannerMesh;
        _bannerObj.AddComponent<MeshRenderer>();

        // 添加布料组件
        Cloth cloth = _bannerObj.AddComponent<Cloth>();
        cloth.useGravity = true;
        cloth.stiffness = 0.8f;
        cloth.damping = 0.1f;

        // 固定左右两个上角
        ClothSkinningCoefficient[] coeffs = cloth.coefficients;
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

        // 添加各功能组件
        var texGen = _bannerObj.AddComponent<BannerTextureGenerator>();
        texGen.Initialize(_bannerWidth, _bannerHeight);

        var windCtrl = _bannerObj.AddComponent<BannerWindController>();
        windCtrl.Init(_config);

        MeshCollider collider = _bannerObj.AddComponent<MeshCollider>();
        collider.sharedMesh = bannerMesh;

        var interaction = _bannerObj.AddComponent<BannerInteraction>();
        interaction.Init(Path.Combine(Application.streamingAssetsPath, "banner_config.json"));
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
        // 5个端点水平均匀分布
        float[] pointX = new float[5];
        for (int i = 0; i < 5; i++)
            pointX[i] = -_bannerWidth / 2f + (_bannerWidth / 4f) * i;

        // 第2、4个点（索引1、3）创建聚光灯
        int[] lightIndexes = { 1, 3 };
        Color lightColor = HexToColor(_config.LightColor);

        foreach (int idx in lightIndexes)
        {
            GameObject lightObj = new GameObject($"BannerLight_{idx}");
            lightObj.transform.SetParent(transform);

            // 位置：比横幅高1/2，前方距离 = 高度 * √3/2
            Vector3 localPos = new Vector3(
                pointX[idx],
                _bannerHeight * 0.5f, // 横幅中心 + 0.5倍高度
                _bannerHeight * (Mathf.Sqrt(3) / 2f)
            );
            
            lightObj.transform.position = _bannerObj.transform.TransformPoint(localPos);
            lightObj.transform.LookAt(_bannerObj.transform.TransformPoint(new Vector3(pointX[idx], 0, 0)));

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.intensity = _config.LightIntensity;
            light.color = lightColor;
            light.spotAngle = 60f;
            light.range = 12f;
            light.shadows = _config.EnableShadow ? LightShadows.Soft : LightShadows.None;
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

        string template = isExpired ? _config.AfterDeadlineText : _config.BeforeDeadlineText;
        string finalText = string.Format(template, days, hours);
        
        _bannerObj.GetComponent<BannerTextureGenerator>().RefreshText(finalText);
    }

    private void Update()
    {
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
