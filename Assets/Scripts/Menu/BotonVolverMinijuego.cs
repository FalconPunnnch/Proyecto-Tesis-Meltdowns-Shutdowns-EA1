using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BotonVolverMinijuego : MonoBehaviour
{
    public string escenaMenu = "Menu";

    void Awake()
    {
        // Ensure EventSystem exists
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        CrearBotonVolver();
    }

    void CrearBotonVolver()
    {
        // Check if a Volver button already exists under this or child
        if (transform.Find("BotonVolver") != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform parentTransform = canvas != null ? canvas.transform : transform;

        var btnObj = new GameObject("BotonVolver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parentTransform, false);

        var rt = btnObj.GetComponent<RectTransform>();
        // Top-left anchor
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(170, 70);
        rt.anchoredPosition = new Vector2(30, -30);

        var img = btnObj.GetComponent<Image>();
#if UNITY_EDITOR
        var spriteBoton = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/boton_madera.png");
        if (spriteBoton != null)
        {
            img.sprite = spriteBoton;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
#endif
        {
            ColorUtility.TryParseHtmlString("#CC9A5C", out Color bgCol);
            img.color = bgCol;
        }

        var btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => MenuManager.RegresarAMinijuegos(escenaMenu));

        var txtObj = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);

        var txtRT = txtObj.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var tmp = txtObj.GetComponent<TextMeshProUGUI>();
#if UNITY_EDITOR
        var font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/Retro Gaming SDF.asset");
        if (font != null) tmp.font = font;
#endif
        tmp.text = "Volver";
        tmp.fontSize = 28;
        ColorUtility.TryParseHtmlString("#FBEFD8", out Color txtCol);
        tmp.color = txtCol;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        btnObj.transform.SetAsLastSibling();
    }
}
