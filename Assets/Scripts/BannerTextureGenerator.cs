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

        _rt = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGB32);
        _rt.filterMode = FilterMode.Bilinear;

        // 2. 创建离屏渲染相机
        GameObject camObj = new GameObject("BannerRenderCam");
        camObj.transform.position = new Vector3(0, -1000, 0); // 藏在视野外
        _renderCam = camObj.AddComponent<Camera>();
        _renderCam.orthographic = true;
        _renderCam.clearFlags = CameraClearFlags.SolidColor;
        _renderCam.backgroundColor = new Color(0.78f, 0f, 0f, 1f); // 正红底色
        _renderCam.enabled = false; // 禁用自动渲染，改为手动调用
        _renderCam.targetTexture = _rt;

        // 3. 创建 UGUI Canvas
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
        _uiText.fontSize = Mathf.RoundToInt(texHeight * 0.66f);
        _uiText.supportRichText = true;
        _uiText.color = Color.white;

        // 加载字体
        Font msYaHei = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", _uiText.fontSize);
        _uiText.font = msYaHei != null ? msYaHei : Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 5. 应用到横幅材质
        _bannerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        _bannerMat.mainTexture = _rt;
        _bannerMat.color = Color.white; // 基础色设为白，底色由相机背景控制
        GetComponent<Renderer>().material = _bannerMat;
    }

    public void RefreshText(string formattedText)
    {
        // 转换富文本格式
        _uiText.text = Regex.Replace(formattedText, @"<color=([0-9A-Fa-f]{6})>", "<color=#$1>");
        // 手动渲染一帧更新纹理
        _renderCam.Render();
    }

    private void OnDestroy()
    {
        if (_rt != null) _rt.Release();
        if (_renderCam != null) Destroy(_renderCam.gameObject);
        if (_bannerMat != null) Destroy(_bannerMat);
    }
}