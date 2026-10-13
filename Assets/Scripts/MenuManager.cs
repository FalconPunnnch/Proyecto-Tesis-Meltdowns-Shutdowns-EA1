using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Tooltip("Opcional. Si lo dejas vacío, usa la fuente por defecto.")]
    public TMP_FontAsset fuentePixel;

    // ================= PALETA (colores del mockup) =================
    Color cieloArriba   = Hex("#A9DCEA");
    Color cieloAbajo    = Hex("#F8D7A8");
    Color maderaLetrero = Hex("#7B5230");
    Color cremaTexto    = Hex("#FBEFD8");
    Color parchemino    = Hex("#F3E7CE");
    Color textoOscuro   = Hex("#3A3326");
    Color botonMadera   = Hex("#CC9A5C");
    Color acento        = Hex("#E0896B");

    // Colores por dominio
    Color colSensorial  = Hex("#AEC6E8"); // azul
    Color colEmocional  = Hex("#F3C3CC"); // rosa
    Color colCognitivo  = Hex("#BEE3B2"); // verde

    // ================= Estado interno =================
    Sprite spriteRedondo;
    Sprite spritePanel;

    GameObject panelBienvenida, panelPrincipal, panelMinijuegos, panelConfig;
    GameObject pantallaAnterior;
    Image overlayBrillo;
    Button botonContinuar;

    List<DatosMinijuego> minijuegos;

    class DatosMinijuego
    {
        public string nombre;
        public string descripcion;
        public Color color;
        public bool desbloqueado; // false = "Próximamente"
        public bool completado;   // muestra el check ✓
        public string escena;     // nombre de la escena a abrir (vacío = ninguna)
        public DatosMinijuego(string n, string d, Color c, bool desb, bool comp, string esc)
        { nombre = n; descripcion = d; color = c; desbloqueado = desb; completado = comp; escena = esc; }
    }

    void Start()
    {
        CargarRecursos();
        AsegurarEventSystem();
        Canvas canvas = CrearCanvas();
        CrearFondo(canvas.transform);
        DefinirMinijuegos();

        panelBienvenida = ConstruirBienvenida(canvas.transform);
        panelPrincipal  = ConstruirPrincipal(canvas.transform);
        panelMinijuegos = ConstruirMinijuegos(canvas.transform);
        panelConfig     = ConstruirConfig(canvas.transform);

        CrearOverlayBrillo(canvas.transform);
        CargarPreferencias();
        MostrarPantalla(panelBienvenida);
    }

    void CargarRecursos()
    {
        spriteRedondo = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        spritePanel   = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
    }

    void AsegurarEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    Canvas CrearCanvas()
    {
        var go = new GameObject("CanvasMenu", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        return c;
    }

    void CrearFondo(Transform padre)
    {
        var fondo = CrearPanel("Fondo", padre, Color.white, false);
        Estirar(fondo.rectTransform);
        var tex = new Texture2D(1, 2);
        tex.SetPixel(0, 0, cieloAbajo);
        tex.SetPixel(0, 1, cieloArriba);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        fondo.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f));
        fondo.type = Image.Type.Simple;
    }

    void DefinirMinijuegos()
    {
        // Los 8 minijuegos del documento. Solo 2 están desarrollados (desbloqueado = true)
        // y abren su escena; los demás aparecen como "Próximamente".
        minijuegos = new List<DatosMinijuego>
        {
            // Sensorial (azul)
            new DatosMinijuego("LIMPIAR PANTALLA",        "Despeja la niebla sensorial con clics y arrastre.", colSensorial, true,  true,  "LimpiarPantalla"),
            new DatosMinijuego("SANTUARIO DE FRECUENCIAS","Atenúa o silencia los estímulos a tu gusto.",       colSensorial, false, false, ""),
            // Emocional (rosa)
            new DatosMinijuego("INFLAR UN GLOBO",         "Respiraciones voluntarias y predecibles a tu ritmo.", colEmocional, true,  true,  "InflarGlobo"),
            new DatosMinijuego("REGAR PLANTITAS",         "Acciones pequeñas que devuelven color y vida.",      colEmocional, false, false, ""),
            new DatosMinijuego("LA MAREA INTERIOR",       "Genera ondas suaves que se calman solas.",           colEmocional, false, false, ""),
            new DatosMinijuego("PEQUEÑOS FAROS",          "Explora emociones y necesidades sin presión.",       colEmocional, false, false, ""),
            // Cognitivo (verde)
            new DatosMinijuego("5-4-3-2-1 GROUNDING",     "Orienta tu atención hacia el entorno inmediato.",    colCognitivo, false, false, ""),
            new DatosMinijuego("RUTAS ALTERNATIVAS",      "Explora nuevas posibilidades cuando un plan falla.", colCognitivo, false, false, ""),
        };
    }

    // ================= PANTALLA: Bienvenida =================
    GameObject ConstruirBienvenida(Transform padre)
    {
        var raiz = CrearPantalla("Bienvenida", padre);

        var titulo = CrearTexto("Titulo", raiz.transform, "A mi ritmo", 110, textoOscuro,
                                TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(titulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(1200, 150), new Vector2(0, 150));

        var sub = CrearTexto("Subtitulo", raiz.transform, "Cada paso también cuenta", 40, textoOscuro,
                             TextAlignmentOptions.Center);
        Anclar(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(1000, 60), new Vector2(0, 50));

        var comenzar = CrearBoton("Comenzar", raiz.transform, "▶  Comenzar", botonMadera, cremaTexto, 40,
                                  () => MostrarPantalla(panelPrincipal));
        Anclar(comenzar.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(520, 100), new Vector2(0, -120));

        var ajustes = CrearBoton("Ajustes", raiz.transform, "⚙  Ajustes sensoriales", parchemino, textoOscuro, 28,
                                 () => AbrirConfig(panelBienvenida));
        Anclar(ajustes.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(420, 72), new Vector2(0, -250));

        return raiz;
    }

    // ================= PANTALLA: Menú principal =================
    GameObject ConstruirPrincipal(Transform padre)
    {
        var raiz = CrearPantalla("Principal", padre);

        var titulo = CrearTexto("Titulo", raiz.transform, "A mi ritmo", 80, textoOscuro,
                                TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
               new Vector2(1000, 120), new Vector2(0, -90));

        string[] etiquetas = { "Nueva partida", "Continuar partida", "Minijuegos", "Configuración", "Salir del juego" };
        UnityAction[] acciones =
        {
            NuevaPartida,
            Continuar,
            () => MostrarPantalla(panelMinijuegos),
            () => AbrirConfig(panelPrincipal),
            SalirDelJuego
        };

        for (int i = 0; i < etiquetas.Length; i++)
        {
            var b = CrearBoton("Btn_" + i, raiz.transform, etiquetas[i], botonMadera, cremaTexto, 36, acciones[i]);
            Anclar(b.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2(540, 86), new Vector2(0, 216 - i * 108));
            if (i == 1) botonContinuar = b;
        }
        return raiz;
    }

    // ================= PANTALLA: Minijuegos =================
    GameObject ConstruirMinijuegos(Transform padre)
    {
        var raiz = CrearPantalla("Minijuegos", padre);

        var atras = CrearBoton("Atras", raiz.transform, "←  Atrás", parchemino, textoOscuro, 28,
                               () => MostrarPantalla(panelPrincipal));
        Anclar(atras.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1),
               new Vector2(170, 72), new Vector2(30, -30));

        var ajustes = CrearBoton("Ajustes", raiz.transform, "⚙  Ajustes", parchemino, textoOscuro, 28,
                                 () => AbrirConfig(panelMinijuegos));
        Anclar(ajustes.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1),
               new Vector2(190, 72), new Vector2(-30, -30));

        var letrero = CrearPanel("Letrero", raiz.transform, maderaLetrero, true);
        Anclar(letrero.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
               new Vector2(760, 120), new Vector2(0, -25));
        var tit = CrearTexto("Titulo", letrero.transform, "MIS MINIJUEGOS", 58, cremaTexto,
                             TextAlignmentOptions.Center, FontStyles.Bold);
        Estirar(tit.rectTransform);

        var sub = CrearTexto("Subtitulo", raiz.transform, "EXPLORA Y REGULA A TU PROPIO RITMO", 28, textoOscuro,
                             TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(sub.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
               new Vector2(1000, 44), new Vector2(0, -160));

        var cont = NuevoRect("Cuadricula", raiz.transform);
        Anclar(cont, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(1700, 520), new Vector2(0, -230));
        var grid = cont.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(540, 150);
        grid.spacing = new Vector2(40, 28);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperCenter;

        foreach (var d in minijuegos)
            CrearTarjeta(cont, d);

        var volumen = CrearBoton("Volumen", raiz.transform, "🔊  Volumen", parchemino, textoOscuro, 26,
                                 () => AbrirConfig(panelMinijuegos));
        Anclar(volumen.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0),
               new Vector2(180, 70), new Vector2(-215, 30));

        var guardar = CrearBoton("Guardar", raiz.transform, "💾  Guardar", parchemino, textoOscuro, 26,
                                 GuardarPreferencias);
        Anclar(guardar.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0),
               new Vector2(180, 70), new Vector2(-30, 30));

        var atras2 = CrearBoton("Atras2", raiz.transform, "←  Atrás", parchemino, textoOscuro, 26,
                                () => MostrarPantalla(panelPrincipal));
        Anclar(atras2.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 0),
               new Vector2(160, 70), new Vector2(30, 30));

        return raiz;
    }

    void CrearTarjeta(Transform padre, DatosMinijuego d)
    {
        Color baseColor = d.desbloqueado ? d.color : Hex("#CFC9BC");
        var tarjeta = CrearPanel("Tarjeta_" + d.nombre, padre, baseColor, true);
        var btn = tarjeta.gameObject.AddComponent<Button>();
        btn.targetGraphic = tarjeta;
        btn.interactable = d.desbloqueado;
        if (d.desbloqueado) btn.onClick.AddListener(() => AbrirMinijuego(d));

        var icono = CrearPanel("Icono", tarjeta.transform, Oscurecer(baseColor, 0.18f), true);
        Anclar(icono.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
               new Vector2(96, 96), new Vector2(68, 0));

        var tit = CrearTexto("Titulo", tarjeta.transform, d.nombre, 28, textoOscuro,
                             TextAlignmentOptions.TopLeft, FontStyles.Bold);
        Anclar(tit.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
               new Vector2(380, 44), new Vector2(130, -18));

        var desc = CrearTexto("Desc", tarjeta.transform, d.descripcion, 21, Oscurecer(textoOscuro, -0.1f),
                              TextAlignmentOptions.TopLeft);
        Anclar(desc.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
               new Vector2(380, 90), new Vector2(130, -62));

        if (d.completado)
        {
            var check = CrearTexto("Check", tarjeta.transform, "✓", 34, Oscurecer(baseColor, 0.4f),
                                   TextAlignmentOptions.Center, FontStyles.Bold);
            Anclar(check.rectTransform, new Vector2(1, 1), new Vector2(1, 1),
                   new Vector2(44, 44), new Vector2(-14, -12));
        }

        if (!d.desbloqueado)
        {
            var prox = CrearTexto("Prox", tarjeta.transform, "Próximamente", 24, Hex("#6B6555"),
                                  TextAlignmentOptions.BottomRight, FontStyles.Italic);
            Anclar(prox.rectTransform, new Vector2(1, 0), new Vector2(1, 0),
                   new Vector2(240, 36), new Vector2(-16, 12));
        }
    }

    // ================= PANTALLA: Configuración =================
    GameObject ConstruirConfig(Transform padre)
    {
        var raiz = CrearPantalla("Config", padre);

        var panel = CrearPanel("Panel", raiz.transform, parchemino, true);
        Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(900, 720), Vector2.zero);

        var tit = CrearTexto("Titulo", panel.transform, "Configuración sensorial", 48, textoOscuro,
                             TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(tit.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
               new Vector2(800, 70), new Vector2(0, -40));

        CrearFilaSlider(panel.transform, "Volumen", 0f, 1f, AudioListener.volume, -30, v =>
        { AudioListener.volume = v; PlayerPrefs.SetFloat("volumen", v); });

        CrearFilaSlider(panel.transform, "Brillo", 0.2f, 1f, 1f, -180, v =>
        { AplicarBrillo(v); PlayerPrefs.SetFloat("brillo", v); });

        CrearFilaSlider(panel.transform, "Nitidez", 0f, 1f, 1f, -330, v =>
        { PlayerPrefs.SetFloat("nitidez", v); });

        var nota = CrearTexto("Nota", panel.transform,
                              "Aquí irán los demás ajustes sensoriales (movimiento, texto, etc.).",
                              22, Hex("#6B6555"), TextAlignmentOptions.Center, FontStyles.Italic);
        Anclar(nota.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(820, 40), new Vector2(0, -210));

        var volver = CrearBoton("Volver", panel.transform, "Volver", botonMadera, cremaTexto, 34,
                                () => MostrarPantalla(pantallaAnterior));
        Anclar(volver.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
               new Vector2(300, 80), new Vector2(0, 40));

        return raiz;
    }

    void CrearFilaSlider(Transform panel, string etiqueta, float min, float max, float valor, float y, UnityAction<float> alCambiar)
    {
        var lbl = CrearTexto("Lbl_" + etiqueta, panel, etiqueta, 30, textoOscuro,
                             TextAlignmentOptions.Left, FontStyles.Bold);
        Anclar(lbl.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(650, 40), new Vector2(0, y + 45));
        CrearSlider(panel, min, max, valor, new Vector2(0, y), alCambiar);
    }

    // ================= Acciones =================
    void NuevaPartida()
    {
        PlayerPrefs.SetInt("hayGuardado", 1);
        PlayerPrefs.Save();
        if (botonContinuar) botonContinuar.interactable = true;
        Debug.Log("Nueva partida: aquí luego cargas la escena de la historia (prólogo).");
        MostrarPantalla(panelMinijuegos);
    }

    void Continuar()
    {
        Debug.Log("Continuar: aquí luego cargas el perfil guardado y el último episodio.");
    }

    void AbrirMinijuego(DatosMinijuego d)
    {
        if (!string.IsNullOrEmpty(d.escena))
            SceneManager.LoadScene(d.escena); // abre la escena del minijuego
        else
            Debug.Log("Minijuego sin escena asignada: " + d.nombre);
    }

    void AbrirConfig(GameObject origen)
    {
        pantallaAnterior = origen;
        MostrarPantalla(panelConfig);
    }

    void GuardarPreferencias()
    {
        PlayerPrefs.Save();
        Debug.Log("Preferencias guardadas.");
    }

    void CargarPreferencias()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("volumen", 1f);
        AplicarBrillo(PlayerPrefs.GetFloat("brillo", 1f));
        if (botonContinuar) botonContinuar.interactable = PlayerPrefs.GetInt("hayGuardado", 0) == 1;
    }

    void AplicarBrillo(float v)
    {
        if (overlayBrillo) overlayBrillo.color = new Color(0, 0, 0, Mathf.Clamp01(1f - v));
    }

    void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void MostrarPantalla(GameObject p)
    {
        panelBienvenida.SetActive(p == panelBienvenida);
        panelPrincipal.SetActive(p == panelPrincipal);
        panelMinijuegos.SetActive(p == panelMinijuegos);
        panelConfig.SetActive(p == panelConfig);
    }

    void CrearOverlayBrillo(Transform padre)
    {
        overlayBrillo = CrearPanel("OverlayBrillo", padre, new Color(0, 0, 0, 0), false);
        Estirar(overlayBrillo.rectTransform);
        overlayBrillo.raycastTarget = false;
        overlayBrillo.transform.SetAsLastSibling();
    }

    // ================= Helpers de UI =================
    GameObject CrearPantalla(string nombre, Transform padre)
    {
        var rt = NuevoRect("Pantalla_" + nombre, padre);
        Estirar(rt);
        return rt.gameObject;
    }

    RectTransform NuevoRect(string nombre, Transform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go.GetComponent<RectTransform>();
    }

    Image CrearPanel(string nombre, Transform padre, Color color, bool redondeado = true)
    {
        var rt = NuevoRect(nombre, padre);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        if (redondeado && spritePanel != null) { img.sprite = spritePanel; img.type = Image.Type.Sliced; }
        return img;
    }

    TextMeshProUGUI CrearTexto(string nombre, Transform padre, string texto, float tam, Color color,
                               TextAlignmentOptions align, FontStyles estilo = FontStyles.Normal)
    {
        var rt = NuevoRect(nombre, padre);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (fuentePixel != null) t.font = fuentePixel;
        t.text = texto; t.fontSize = tam; t.color = color;
        t.alignment = align; t.fontStyle = estilo;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    Button CrearBoton(string nombre, Transform padre, string etiqueta, Color colorFondo, Color colorTexto,
                      float tamTexto, UnityAction alClic)
    {
        var img = CrearPanel(nombre, padre, colorFondo, true);
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        cb.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(1f, 1f, 1f, 0.5f);
        b.colors = cb;

        var t = CrearTexto("Texto", img.transform, etiqueta, tamTexto, colorTexto,
                           TextAlignmentOptions.Center, FontStyles.Bold);
        Estirar(t.rectTransform, 12, 12, 6, 6);

        if (alClic != null) b.onClick.AddListener(alClic);
        return b;
    }

    void CrearSlider(Transform padre, float min, float max, float valor, Vector2 pos, UnityAction<float> alCambiar)
    {
        var rt = NuevoRect("Slider", padre);
        Anclar(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640, 36), pos);
        var s = rt.gameObject.AddComponent<Slider>();

        var fondo = CrearPanel("Fondo", rt, new Color(0, 0, 0, 0.15f), true);
        Estirar(fondo.rectTransform);

        var areaRelleno = NuevoRect("AreaRelleno", rt);
        Estirar(areaRelleno, 8, 8, 0, 0);
        var relleno = NuevoRect("Relleno", areaRelleno);
        relleno.anchorMin = new Vector2(0, 0); relleno.anchorMax = new Vector2(1, 1);
        relleno.sizeDelta = new Vector2(10, 0);
        var rellenoImg = relleno.gameObject.AddComponent<Image>();
        rellenoImg.sprite = spriteRedondo; rellenoImg.type = Image.Type.Sliced; rellenoImg.color = acento;

        var areaHandle = NuevoRect("AreaHandle", rt);
        Estirar(areaHandle, 12, 12, 0, 0);
        var handle = NuevoRect("Handle", areaHandle);
        handle.sizeDelta = new Vector2(34, 34);
        var handleImg = handle.gameObject.AddComponent<Image>();
        handleImg.sprite = spriteRedondo; handleImg.type = Image.Type.Sliced; handleImg.color = Color.white;

        s.fillRect = relleno;
        s.handleRect = handle;
        s.targetGraphic = handleImg;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = min; s.maxValue = max; s.value = valor;
        if (alCambiar != null) s.onValueChanged.AddListener(alCambiar);
    }

    void Estirar(RectTransform rt, float izq = 0, float der = 0, float arr = 0, float ab = 0)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(izq, ab); rt.offsetMax = new Vector2(-der, -arr);
    }

    void Anclar(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
    }

    static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out Color c); return c; }

    static Color Oscurecer(Color c, float cantidad)
    {
        return new Color(Mathf.Clamp01(c.r - cantidad), Mathf.Clamp01(c.g - cantidad),
                         Mathf.Clamp01(c.b - cantidad), c.a);
    }
}