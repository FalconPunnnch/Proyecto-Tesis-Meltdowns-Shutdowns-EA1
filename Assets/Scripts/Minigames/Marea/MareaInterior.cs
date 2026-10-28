using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MareaInterior : MonoBehaviour
{
    public string escenaMenu = "Menu";
    public float intervaloOnda = 0.09f;

    private Transform canvasT;
    private Sprite anillo;
    private float t;

    void Start()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        anillo = CrearAnillo(64);

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = canvasGO.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        canvasT = canvas.transform;

        var fondo = CrearImagen("Fondo", canvasT, Hex("#245A74"));
        Estirar(fondo.rectTransform);
        var hondo = CrearImagen("FondoHondo", canvasT, Hex("#153C52"));
        var rh = hondo.rectTransform; rh.anchorMin = new Vector2(0,0); rh.anchorMax = new Vector2(1,0.55f); rh.offsetMin = Vector2.zero; rh.offsetMax = Vector2.zero;

        var guia = CrearTexto("Guia", canvasT, "Toca o arrastra sobre el agua. También puedes solo observar.", 36, new Color(1,1,1,0.9f), TextAlignmentOptions.Center);
        Anclar(guia.rectTransform, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(1500,70), new Vector2(0,-70));

        var volver = CrearBoton("Volver", canvasT, "Volver", Hex("#F3E7CE"), Hex("#3A3326"));
        Anclar(volver.GetComponent<RectTransform>(), new Vector2(0,0), new Vector2(0,0), new Vector2(150,60), new Vector2(24,24));
        volver.onClick.AddListener(() => MenuManager.RegresarAMinijuegos(escenaMenu));
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
            CrearOnda(Mouse.current.position.ReadValue());
        else if (Mouse.current.leftButton.isPressed)
        {
            t -= Time.deltaTime;
            if (t <= 0f) { t = intervaloOnda; CrearOnda(Mouse.current.position.ReadValue()); }
        }
    }

    void CrearOnda(Vector2 screenPos)
    {
        var go = new GameObject("Onda", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(canvasT, false);
        var img = go.GetComponent<Image>();
        img.sprite = anillo; img.raycastTarget = false;
        img.color = new Color(0.78f, 0.92f, 1f, 0.7f);
        var rt = go.GetComponent<RectTransform>();
        rt.position = new Vector3(screenPos.x, screenPos.y, 0f);
        rt.sizeDelta = new Vector2(20, 20);
        StartCoroutine(AnimarOnda(rt, img));
    }

    IEnumerator AnimarOnda(RectTransform rt, Image img)
    {
        float dur = 1.6f, e = 0f;
        float maxS = Random.Range(120f, 220f);
        Color c0 = img.color;
        while (e < dur)
        {
            e += Time.deltaTime;
            float k = e / dur;
            float s = Mathf.Lerp(20f, maxS, k);
            rt.sizeDelta = new Vector2(s, s);
            img.color = new Color(c0.r, c0.g, c0.b, c0.a * (1f - k));
            yield return null;
        }
        Destroy(rt.gameObject);
    }

    // Genera una textura con un anillo de borde suave para las ondas
    Sprite CrearAnillo(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float c = (size - 1) / 2f, R = c - 3, thick = 3.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = Mathf.Clamp01(1f - Mathf.Abs(d - R) / thick);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    RectTransform Nuevo(string n, Transform p){ var g=new GameObject(n,typeof(RectTransform)); g.transform.SetParent(p,false); return g.GetComponent<RectTransform>(); }
    Image CrearImagen(string n, Transform p, Color c){ var i=Nuevo(n,p).gameObject.AddComponent<Image>(); i.color=c; return i; }
    TextMeshProUGUI CrearTexto(string n, Transform p, string t2, float s, Color c, TextAlignmentOptions a){ var x=Nuevo(n,p).gameObject.AddComponent<TextMeshProUGUI>(); x.text=t2; x.fontSize=s; x.color=c; x.alignment=a; return x; }
    Button CrearBoton(string n, Transform p, string et, Color fondo, Color txt){ var i=CrearImagen(n,p,fondo); var b=i.gameObject.AddComponent<Button>(); b.targetGraphic=i; var t2=CrearTexto("T",i.transform,et,30,txt,TextAlignmentOptions.Center); Estirar(t2.rectTransform); return b; }
    void Estirar(RectTransform rt){ rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; }
    void Anclar(RectTransform rt, Vector2 an, Vector2 pv, Vector2 sz, Vector2 po){ rt.anchorMin=an; rt.anchorMax=an; rt.pivot=pv; rt.sizeDelta=sz; rt.anchoredPosition=po; }
    static Color Hex(string h){ ColorUtility.TryParseHtmlString(h, out Color c); return c; }
}
