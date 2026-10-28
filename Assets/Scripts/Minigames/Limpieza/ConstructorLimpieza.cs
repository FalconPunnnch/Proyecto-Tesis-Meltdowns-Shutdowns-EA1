/*
 * ConstructorLimpieza.cs
 * Construye la escena y contiene la lógica en un solo archivo.
 *
 * Asigna la imagen del jardín al campo "Fondo Ventana"
 * del componente, desde el Inspector.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

public class ConstructorLimpieza : MonoBehaviour
{
    [Header("Fondo de la ventana")]
    public Sprite fondoVentana;

    [Tooltip("Color de las franjas si la proporción de pantalla difiere del fondo.")]
    public Color colorExterior = new Color(0.12f, 0.10f, 0.08f);

    [Header("Ajustes")]
    public string escenaMenu = "Menu";

    [Min(1f)]
    public float metaLimpieza = 1000f;

    [Min(0f)]
    public float velocidadLimpiezaPorMovimiento = 1f;

    private CanvasGroup vaporCG;
    private GameObject capaVapor;
    private Slider barraProgreso;
    private TextMeshProUGUI textoGuia;
    private GameObject panelRecompensa;

    private Canvas canvasUI;
    private RectTransform areaVentana;

    private float progresoLimpieza;
    private bool minijuegoCompletado;

    private void Start()
    {
        metaLimpieza = Mathf.Max(1f, metaLimpieza);

        ConfigurarEventos();

        // ---------- Canvas ----------
        GameObject canvasGO = new GameObject(
            "CanvasLimpieza",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasUI = canvasGO.GetComponent<Canvas>();
        canvasUI.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Transform raiz = canvasGO.transform;

        // ---------- Fondo ----------
        Image exterior = CrearImagen(
            "FondoExterior",
            raiz,
            colorExterior
        );
        Estirar(exterior.rectTransform);

        // El fondo y el vapor comparten el mismo rectángulo.
        // Así el vapor no cubre las posibles franjas exteriores.
        areaVentana = Nuevo("AreaVentana", raiz);
        Estirar(areaVentana);

        if (fondoVentana != null)
        {
            AspectRatioFitter ajuste =
                areaVentana.gameObject.AddComponent<AspectRatioFitter>();

            ajuste.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            ajuste.aspectRatio =
                fondoVentana.rect.width / fondoVentana.rect.height;
        }

        Image fondo = CrearImagen(
            "ImagenJardin",
            areaVentana,
            fondoVentana != null ? Color.white : Col("#8FB98F")
        );

        Estirar(fondo.rectTransform);

        fondo.sprite = fondoVentana;
        fondo.type = Image.Type.Simple;
        fondo.preserveAspect = true;

        if (fondoVentana == null)
        {
            Debug.LogWarning(
                "Asigna la imagen del jardín al campo Fondo Ventana " +
                "de ConstructorLimpieza.",
                this
            );
        }

        // ---------- Vapor ----------
        Image vapor = CrearImagen(
            "CapaVapor",
            areaVentana,
            Color.white
        );

        Estirar(vapor.rectTransform);

        capaVapor = vapor.gameObject;
        vaporCG = capaVapor.AddComponent<CanvasGroup>();
        vaporCG.alpha = 1f;
        vaporCG.blocksRaycasts = false;
        vaporCG.interactable = false;

        // ---------- Texto guía ----------
        textoGuia = CrearTexto(
            "TextoGuia",
            raiz,
            "Haz clic y arrastra sobre la ventana para limpiar el vapor.",
            34,
            Col("#3A3326"),
            TextAlignmentOptions.Center,
            FontStyles.Bold
        );

        // Contorno para mantener legibilidad sobre el jardín.
        textoGuia.outlineWidth = 0f;

        Anclar(
            textoGuia.rectTransform,
            new Vector2(0.5f, 1),
            new Vector2(0.5f, 1),
            new Vector2(1400, 100),
            new Vector2(0, -110)
        );

        // ---------- Progreso ----------
        barraProgreso = CrearSlider(raiz);

        Anclar(
            barraProgreso.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0),
            new Vector2(700, 36),
            new Vector2(0, 80)
        );

        barraProgreso.minValue = 0f;
        barraProgreso.maxValue = metaLimpieza;
        barraProgreso.value = 0f;

        // ---------- Panel final ----------
        Image recompensa = CrearImagen(
            "PanelRecompensa",
            raiz,
            new Color(0, 0, 0, 0.65f)
        );

        Estirar(recompensa.rectTransform);
        recompensa.raycastTarget = true;

        TextMeshProUGUI textoFinal = CrearTexto(
            "TextoFinal",
            recompensa.transform,
            "La ventana está despejada.\nPuedes observar el jardín con calma.",
            44,
            Color.white,
            TextAlignmentOptions.Center,
            FontStyles.Bold
        );

        Anclar(
            textoFinal.rectTransform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(1200, 160),
            new Vector2(0, 70)
        );

        Button botonFinal = CrearBoton(
            "ObservarJardin",
            recompensa.transform,
            "Observar el jardín",
            Col("#CC9A5C"),
            Col("#FBEFD8")
        );

        Anclar(
            botonFinal.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(360, 80),
            new Vector2(0, -90)
        );

        botonFinal.onClick.AddListener(ObservarJardin);

        panelRecompensa = recompensa.gameObject;
        panelRecompensa.SetActive(false);

        // ---------- Botón volver ----------
        Button volver = CrearBoton(
            "Volver",
            raiz,
            "Volver",
            Col("#CC9A5C"),
            Col("#FBEFD8")
        );

        Anclar(
            volver.GetComponent<RectTransform>(),
            new Vector2(0, 1),
            new Vector2(0, 1),
            new Vector2(170, 70),
            new Vector2(30, -30)
        );

        volver.onClick.AddListener(
            () => MenuManager.RegresarAMinijuegos(escenaMenu)
        );

        progresoLimpieza = 0f;
        minijuegoCompletado = false;
    }

    private void ObservarJardin()
    {
        panelRecompensa.SetActive(false);

        // Despeja la vista para observar el jardín.
        textoGuia.gameObject.SetActive(false);
        barraProgreso.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (minijuegoCompletado || Mouse.current == null)
            return;

        Mouse mouse = Mouse.current;

        if (!mouse.leftButton.isPressed)
            return;

        // No limpiar mientras se interactúa con botones.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        // Solo cuenta movimiento dentro de la imagen de la ventana.
        if (!RectTransformUtility.RectangleContainsScreenPoint(
                areaVentana,
                mouse.position.ReadValue(),
                null))
            return;

        Vector2 delta = mouse.delta.ReadValue();

        float movX = Mathf.Abs(delta.x);
        float movY = Mathf.Abs(delta.y);

        if (movX <= 0.1f && movY <= 0.1f)
            return;

        progresoLimpieza = Mathf.Min(
            metaLimpieza,
            progresoLimpieza +
            (movX + movY) * velocidadLimpiezaPorMovimiento * 0.1f
        );

        barraProgreso.value = progresoLimpieza;

        vaporCG.alpha = Mathf.Clamp01(
            1f - progresoLimpieza / metaLimpieza
        );
        
        // El texto cambia de su color inicial a blanco al limpiar.
        textoGuia.color = Color.Lerp(
            Col("#3A3326"),
            Color.white,
            Mathf.Clamp01(progresoLimpieza / metaLimpieza)
        );

        if (progresoLimpieza >= metaLimpieza)
            CompletarLimpieza();
    }
        private void CompletarLimpieza()
    {
        if (minijuegoCompletado)
            return;

        minijuegoCompletado = true;

        PlayerPrefs.SetInt("completado_LimpiarPantalla", 1);
        PlayerPrefs.Save();

        capaVapor.SetActive(false);
        panelRecompensa.SetActive(true);

        textoGuia.text = "La ventana está despejada.";
        textoGuia.color = Color.white;

        // Se dibuja delante del panel oscuro y conserva su posición.
        textoGuia.transform.SetAsLastSibling();
    }

    private void ConfigurarEventos()
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

        // Evita mezclar el módulo antiguo con el nuevo Input System.
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
    }

    // ---------- Construcción de UI ----------

    private RectTransform Nuevo(string nombre, Transform padre)
    {
        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform)
        );

        objeto.transform.SetParent(padre, false);

        return objeto.GetComponent<RectTransform>();
    }

    private Image CrearImagen(
        string nombre,
        Transform padre,
        Color color)
    {
        Image imagen =
            Nuevo(nombre, padre).gameObject.AddComponent<Image>();

        imagen.color = color;
        imagen.raycastTarget = false;

        return imagen;
    }

    private TextMeshProUGUI CrearTexto(
        string nombre,
        Transform padre,
        string contenido,
        float tamano,
        Color color,
        TextAlignmentOptions alineacion,
        FontStyles estilo = FontStyles.Normal)
    {
        TextMeshProUGUI texto =
            Nuevo(nombre, padre).gameObject
                .AddComponent<TextMeshProUGUI>();

        texto.text = contenido;
        texto.fontSize = tamano;
        texto.color = color;
        texto.alignment = alineacion;
        texto.fontStyle = estilo;
        texto.raycastTarget = false;

        return texto;
    }

    private Button CrearBoton(
        string nombre,
        Transform padre,
        string etiqueta,
        Color colorFondo,
        Color colorTexto)
    {
        Image imagen = CrearImagen(nombre, padre, colorFondo);
        imagen.raycastTarget = true;

        Button boton = imagen.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;

        TextMeshProUGUI texto = CrearTexto(
            "Texto",
            imagen.transform,
            etiqueta,
            26,
            colorTexto,
            TextAlignmentOptions.Center,
            FontStyles.Bold
        );

        Estirar(texto.rectTransform);

        return boton;
    }

    private Slider CrearSlider(Transform padre)
    {
        RectTransform rect = Nuevo("BarraProgreso", padre);
        Slider slider = rect.gameObject.AddComponent<Slider>();

        // Es un indicador: el jugador no puede arrastrarlo.
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;

        Image fondo = CrearImagen(
            "Fondo",
            rect,
            new Color(0, 0, 0, 0.45f)
        );

        Estirar(fondo.rectTransform);

        RectTransform area = Nuevo("AreaRelleno", rect);
        Estirar(area, 5, 5, 5, 5);

        Image relleno = CrearImagen(
            "Relleno",
            area,
            Col("#E0896B")
        );

        Estirar(relleno.rectTransform);

        slider.fillRect = relleno.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private void Estirar(
        RectTransform rect,
        float izquierda = 0,
        float derecha = 0,
        float arriba = 0,
        float abajo = 0)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(izquierda, abajo);
        rect.offsetMax = new Vector2(-derecha, -arriba);
    }

    private void Anclar(
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

    private static Color Col(string hexadecimal)
    {
        ColorUtility.TryParseHtmlString(hexadecimal, out Color color);
        return color;
    }

    private void OnDestroy()
    {
        if (canvasUI != null)
            Destroy(canvasUI.gameObject);
    }
}