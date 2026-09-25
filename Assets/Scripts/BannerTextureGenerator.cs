using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[RequireComponent(typeof(Renderer))]
public class BannerTextureGenerator : MonoBehaviour
{
    private Texture2D _bannerTexture;
    private Renderer _bannerRenderer;
    private Font _msYaHeiFont;
    private int _texWidth;
    private int _texHeight;

    private void Awake()
    {
        // 加载系统微软雅黑字体，不存在则回退到Arial
        _msYaHeiFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 32);
        if (_msYaHeiFont == null)
        {
            _msYaHeiFont = Font.CreateDynamicFontFromOSFont("Arial", 32);
            Debug.LogWarning("未找到微软雅黑字体，已自动回退");
        }
    }

    public void Initialize(float worldWidth, float worldHeight)
    {
        _bannerRenderer = GetComponent<Renderer>();
        
        // 按世界尺寸比例计算纹理分辨率
        float aspect = worldWidth / worldHeight;
        _texHeight = 256;
        _texWidth = Mathf.RoundToInt(_texHeight * aspect);
        
        _bannerTexture = new Texture2D(_texWidth, _texHeight, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear
        };

        // 初始化红布材质
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.mainTexture = _bannerTexture;
        mat.color = new Color(0.78f, 0f, 0f, 1f); // 正红底色
        _bannerRenderer.material = mat;
    }

    public void RefreshText(string formattedText)
    {
        RenderTexture rt = RenderTexture.GetTemporary(_texWidth, _texHeight, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;
        
        // 填充红色底色
        GL.Clear(true, true, new Color(0.78f, 0f, 0f, 1f));

        // 字号 = 横幅高度的 2/3
        int fontSize = Mathf.RoundToInt(_texHeight * 0.66f);
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            font = _msYaHeiFont,
            fontSize = fontSize,
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        // 转换为Unity富文本格式（<color=FF0000> → <color=#FF0000>）
        string unityRichText = Regex.Replace(formattedText, 
            @"<color=([0-9A-Fa-f]{6})>", "<color=#$1>");

        GUI.Label(new Rect(0, 0, _texWidth, _texHeight), unityRichText, style);

        // 回写纹理
        _bannerTexture.ReadPixels(new Rect(0, 0, _texWidth, _texHeight), 0, 0);
        _bannerTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
    }
}
