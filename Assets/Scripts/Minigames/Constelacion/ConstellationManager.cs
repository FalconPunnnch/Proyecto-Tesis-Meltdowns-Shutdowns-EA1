using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public class ConstellationManager : MonoBehaviour
{
    [Header("Objetos existentes de la escena")]
    public SpriteRenderer fondo;
    public GameObject fondoBrillante;
    public Camera camaraJuego;

    [Header("Encuadre")]
    public bool ajustarCamaraAlFondo = true;

    [Header("Colores de los destellos")]
    public Color colorPendiente = new Color(0.65f, 0.8f, 1f, 0.85f);
    public Color colorEncendido = Color.white;

    [Header("Menú")]
    public string escenaMenu = "Menu";

    public bool Completada { get; private set; }

    private readonly Dictionary<Collider2D, SpriteRenderer> puntos =
        new Dictionary<Collider2D, SpriteRenderer>();

    private readonly HashSet<SpriteRenderer> encendidas =
        new HashSet<SpriteRenderer>();

    private readonly HashSet<SpriteRenderer> estrellas =
        new HashSet<SpriteRenderer>();

    private TextMeshProUGUI textoGuia;
    private TextMeshProUGUI textoContador;

    private Canvas canvasUI;

    private void Awake()
    {
        if (camaraJuego == null)
            camaraJuego = Camera.main;

        // Solo registra los sprites que ya existen dentro
        // de Constelacion_OsaMayor y tienen Collider2D.
        foreach (SpriteRenderer estrella
                 in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (estrella.gameObject == gameObject)
                continue;

            Collider2D[] colliders =
                estrella.GetComponents<Collider2D>();

            if (colliders.Length == 0)
                continue;

            estrellas.Add(estrella);

            estrella.gameObject.SetActive(true);
            estrella.enabled = true;
            // Conserva el color del sprite y establece 70 % de opacidad.
            Color color = estrella.color;
            color.a = 0.4f;
            estrella.color = color;

            foreach (Collider2D collider in colliders)
            {
                collider.isTrigger = true;
                collider.enabled = true;
                puntos[collider] = estrella;
            }
        }

        if (fondoBrillante != null)
            fondoBrillante.SetActive(false);
    }

    private void Start()
    {
        if (estrellas.Count == 0)
        {
            Debug.LogWarning(
                "No se encontraron destellos con SpriteRenderer " +
                "y Collider2D dentro de Constelacion_OsaMayor.",
                this
            );
        }

        AlinearFondos();

        if (ajustarCamaraAlFondo)
            AjustarCamara();

        ConstruirUI();
        ActualizarContador();
    }

    private void AlinearFondos()
    {
        if (fondo == null || fondo.sprite == null ||
            fondoBrillante == null)
            return;

        SpriteRenderer brillante =
            fondoBrillante.GetComponent<SpriteRenderer>();

        if (brillante == null || brillante.sprite == null)
            return;

        // Para esta escena los fondos deben estar sin rotación.
        // Ambos ocupan el mismo rectángulo del mundo.
        brillante.transform.rotation = fondo.transform.rotation;

        Vector3 escala = brillante.transform.localScale;

        Vector3 escalaMundoFondo = fondo.transform.lossyScale;

        Vector3 escalaPadre = brillante.transform.parent != null
            ? brillante.transform.parent.lossyScale
            : Vector3.one;

        Vector3 tamanoBase = fondo.sprite.bounds.size;
        Vector3 tamanoFinal = brillante.sprite.bounds.size;

        if (Mathf.Abs(escalaPadre.x) < 0.0001f ||
            Mathf.Abs(escalaPadre.y) < 0.0001f)
            return;

        escala.x =
            tamanoBase.x * escalaMundoFondo.x /
            (tamanoFinal.x * escalaPadre.x);

        escala.y =
            tamanoBase.y * escalaMundoFondo.y /
            (tamanoFinal.y * escalaPadre.y);

        brillante.transform.localScale = escala;

        // Compensa también posibles pivotes diferentes.
        Vector3 diferencia =
            fondo.bounds.center - brillante.bounds.center;

        brillante.transform.position +=
            new Vector3(diferencia.x, diferencia.y, 0f);
    }

    private void AjustarCamara()
    {
        if (camaraJuego == null ||
            !camaraJuego.orthographic ||
            fondo == null ||
            fondo.sprite == null)
            return;

        Bounds limites = fondo.bounds;

        Vector3 posicion = camaraJuego.transform.position;
        posicion.x = limites.center.x;
        posicion.y = limites.center.y;

        camaraJuego.transform.position = posicion;

        // Muestra el fondo completo sin deformarlo.
        camaraJuego.orthographicSize = Mathf.Max(
            limites.extents.y,
            limites.extents.x / camaraJuego.aspect
        );

        camaraJuego.clearFlags = CameraClearFlags.SolidColor;
        camaraJuego.backgroundColor =
            new Color(0.025f, 0.035f, 0.05f);
    }

    public bool RegistrarEstrellaEncendida(Collider2D collider)
    {
        if (Completada || collider == null)
            return false;

        if (!puntos.TryGetValue(collider, out SpriteRenderer estrella))
            return false;

        // HashSet evita sumar nuevamente la misma estrella.
        if (!encendidas.Add(estrella))
            return false;

        // Al tocar el destello, cambia solamente la opacidad a 100 %.
        Color color = estrella.color;
        color.a = 1f;
        estrella.color = color;

        foreach (Collider2D punto
                 in estrella.GetComponents<Collider2D>())
        {
            punto.enabled = false;
        }

        ActualizarContador();

        if (estrellas.Count > 0 &&
            encendidas.Count == estrellas.Count)
        {
            CompletarConstelacion();
        }

        return true;
    }

    private void ActualizarContador()
    {
        if (textoContador != null)
        {
            textoContador.text =
                encendidas.Count + " / " + estrellas.Count;
        }
    }

    private void CompletarConstelacion()
    {
        if (Completada)
            return;

        Completada = true;

        if (fondoBrillante != null)
            fondoBrillante.SetActive(true);

        if (textoGuia != null)
        {
            textoGuia.text =
                "Has iluminado toda la constelación.\n" +
                "Puedes observarla con calma.";
        }
    }

    private void ConstruirUI()
    {
        EventSystem eventos = FindAnyObjectByType<EventSystem>();

        if (eventos == null)
        {
            GameObject objeto = new GameObject(
                "EventSystem",
                typeof(EventSystem)
            );

            eventos = objeto.GetComponent<EventSystem>();
        }

        // UI compatible con el nuevo Input System.
        foreach (BaseInputModule modulo
                 in eventos.GetComponents<BaseInputModule>())
        {
            modulo.enabled = false;
        }

        InputSystemUIInputModule moduloUI =
            eventos.GetComponent<InputSystemUIInputModule>();

        if (moduloUI == null)
        {
            moduloUI =
                eventos.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        moduloUI.enabled = true;

        if (moduloUI.actionsAsset == null)
            moduloUI.AssignDefaultActions();

        GameObject objetoCanvas = new GameObject(
            "CanvasConstelacion",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasUI = objetoCanvas.GetComponent<Canvas>();
        canvasUI.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler =
            objetoCanvas.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Transform padre = objetoCanvas.transform;

        Image imagenBoton = CrearImagen(
            "Volver",
            padre,
            new Color(0.95f, 0.91f, 0.81f)
        );

        imagenBoton.raycastTarget = true;

        Anclar(
            imagenBoton.rectTransform,
            new Vector2(0, 1),
            new Vector2(0, 1),
            new Vector2(170, 65),
            new Vector2(25, -25)
        );

        Button boton = imagenBoton.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagenBoton;

        boton.onClick.AddListener(
            () => MenuManager.RegresarAMinijuegos(escenaMenu)
        );

        TextMeshProUGUI etiqueta = CrearTexto(
            "Texto",
            imagenBoton.transform,
            "Volver",
            28,
            TextAlignmentOptions.Center
        );

        etiqueta.color = new Color(0.23f, 0.20f, 0.15f);
        Estirar(etiqueta.rectTransform);

        textoGuia = CrearTexto(
            "TextoGuia",
            padre,
            "Acerca tu estrella a los destellos.\n" +
            "Usa WASD, flechas o mantén clic.",
            26,
            TextAlignmentOptions.Center
        );

        RectTransform guia = textoGuia.rectTransform;
        guia.anchorMin = new Vector2(0, 1);
        guia.anchorMax = new Vector2(1, 1);
        guia.pivot = new Vector2(0.5f, 1);
        guia.offsetMin = new Vector2(220, -110);
        guia.offsetMax = new Vector2(-220, -20);

        textoContador = CrearTexto(
            "TextoContador",
            padre,
            "",
            30,
            TextAlignmentOptions.Right
        );

        Anclar(
            textoContador.rectTransform,
            new Vector2(1, 1),
            new Vector2(1, 1),
            new Vector2(160, 65),
            new Vector2(-25, -25)
        );
    }

    private RectTransform CrearRect(string nombre, Transform padre)
    {
        GameObject objeto = new GameObject(
            nombre, typeof(RectTransform)
        );

        objeto.transform.SetParent(padre, false);

        return objeto.GetComponent<RectTransform>();
    }

    private Image CrearImagen(
        string nombre, Transform padre, Color color)
    {
        Image imagen =
            CrearRect(nombre, padre).gameObject.AddComponent<Image>();

        imagen.color = color;
        imagen.raycastTarget = false;

        return imagen;
    }

    private TextMeshProUGUI CrearTexto(
        string nombre,
        Transform padre,
        string contenido,
        float tamano,
        TextAlignmentOptions alineacion)
    {
        TextMeshProUGUI texto =
            CrearRect(nombre, padre).gameObject
                .AddComponent<TextMeshProUGUI>();

        texto.text = contenido;
        texto.fontSize = tamano;
        texto.color = Color.white;
        texto.alignment = alineacion;
        texto.raycastTarget = false;

        return texto;
    }

    private static void Estirar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Anclar(
        RectTransform rect,
        Vector2 ancla,
        Vector2 pivote,
        Vector2 tamano,
        Vector2 posicion)
    {
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.pivot = pivote;
        rect.sizeDelta = tamano;
        rect.anchoredPosition = posicion;
    }

    private void OnDestroy()
    {
        if (canvasUI != null)
            Destroy(canvasUI.gameObject);
    }
}