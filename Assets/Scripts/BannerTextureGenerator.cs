using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[RequireComponent(typeof(Renderer))]
public class BannerTextureGenerator : MonoBehaviour
{
    private Camera _renderCam;
    private Text _uiText;
    private RenderTexture _rt;
    private Material _bannerMat;

    public void Initialize(float worldWidth, float worldHeight)
    {
        // 1. 计算纹理尺寸
        float aspect = worldWidth / worldHeight;
        int texHeight = 256;
        int texWidth = Mathf.RoundToInt(texHeight * aspect);

        _rt = new RenderTexture(texWidth, texHeight, 24, RenderTextureFormat.ARGB32);
        _rt.filterMode = FilterMode.Bilinear;

        // 2. 创建离屏渲染相机
        GameObject camObj = new GameObject("BannerRenderCam");
        camObj.transform.position = new Vector3(0, -1000, 0); // 放在视线外
        _renderCam = camObj.AddComponent<Camera>();
        _renderCam.orthographic = true;
        _renderCam.clearFlags = CameraClearFlags.SolidColor;
        _renderCam.backgroundColor = new Color(0.78f, 0f, 0f, 1f); // 强行填充正红底色
        _renderCam.enabled = false;
        _renderCam.targetTexture = _rt;

        // 3. 创建 UI Canvas
        GameObject canvasObj = new GameObject("BannerCanvas");
        canvasObj.transform.SetParent(camObj.transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = _renderCam;

        // 4. 创建 UI Text
        GameObject textObj = new GameObject("BannerText");
        textObj.transform.SetParent(canvasObj.transform, false);
        _uiText = textObj.AddComponent<Text>();

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _uiText.alignment = TextAnchor.MiddleCenter;
        _uiText.fontSize = Mathf.RoundToInt(texHeight * 0.65f);
        _uiText.supportRichText = true;
        _uiText.color = Color.yellow; // 设置默认金黄色文字，增加对比度
        _uiText.fontStyle = FontStyle.Bold; // 新增：强制字体加粗

        // 字体自动容错
        Font msYaHei = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", _uiText.fontSize);
        _uiText.font = msYaHei != null ? msYaHei : Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 5. 自动兼容 Shader (解决粉紫色问题)
        Shader bannerShader = Shader.Find("Universal Render Pipeline/Lit");
        if (bannerShader == null) bannerShader = Shader.Find("Standard");
        if (bannerShader == null) bannerShader = Shader.Find("Unlit/Texture");

        _bannerMat = new Material(bannerShader);

        // 双向兼容 URP(_BaseMap) 与 标准管线(_MainTex)
        if (_bannerMat.HasProperty("_BaseMap")) _bannerMat.SetTexture("_BaseMap", _rt);
        if (_bannerMat.HasProperty("_MainTex")) _bannerMat.SetTexture("_MainTex", _rt);

        GetComponent<Renderer>().material = _bannerMat;
    }

    public void RefreshText(string formattedText)
    {
        if (_uiText == null || _renderCam == null) return;

        // 转换格式
        _uiText.text = Regex.Replace(formattedText, @"<color=([0-9A-Fa-f]{6})>", "<color=#$1>");

        // 关键步骤：强制 UGUI 立即计算网格与排版，防止渲染出空白图
        Canvas.ForceUpdateCanvases();

        // 手动拍一张照更新 RenderTexture
        _renderCam.Render();
    }

    private void OnDestroy()
    {
        if (_rt != null) _rt.Release();
        if (_renderCam != null) Destroy(_renderCam.gameObject);
        if (_bannerMat != null) Destroy(_bannerMat);
    }
}