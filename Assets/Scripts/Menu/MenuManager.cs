/*
 ============================================================================
  MenuManager.cs  —  "Videojuego Tesis"
  Construye POR CÓDIGO todos los menús: Bienvenida, Principal, Minijuegos y
  Configuración, y abre la escena de cada minijuego jugable.
 ============================================================================
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Tooltip("Opcional. Si lo dejas vacío, usa la fuente por defecto.")]
    public TMP_FontAsset fuentePixel;

    [Header("Audio (Opcional)")]
    [Tooltip("Asigna tu AudioMixer si utilizas canales independientes por Mezclador.")]
    public AudioMixer mainAudioMixer;

    // ================= PALETA (colores del mockup) =================
    Color cieloArriba = Hex("#A9DCEA");
    Color cieloAbajo = Hex("#F8D7A8");
    Color maderaLetrero = Hex("#7B5230");
    Color cremaTexto = Hex("#FBEFD8");
    Color parchemino = Hex("#F3E7CE");
    Color textoOscuro = Hex("#3A3326");
    Color botonMadera = Hex("#CC9A5C");
    Color acento = Hex("#E0896B");

    // Colores según el cuadro de dominios
    Color colSensorial = Hex("#AEC6E8"); // azul
    Color colEmocional = Hex("#F3C3CC"); // rosa
    Color colCognitivo = Hex("#BEE3B2"); // verde

    // ================= Estado interno =================
    public static bool regresarAMinijuegos = false;

    public static void RegresarAMinijuegos(string nombreEscenaMenu = "Menu")
    {
        regresarAMinijuegos = true;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/" + nombreEscenaMenu + ".unity");
            return;
        }
#endif
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    Sprite spriteRedondo;
    Sprite spritePanel;

    // Sprites de Menú y Tarjetas
    public Sprite tarjetaAzul;
    public Sprite tarjetaRosa;
    public Sprite tarjetaVerde;
    public Sprite spriteBotonMadera;
    public Sprite spriteLetrero;
    public Sprite spriteFondoMenu;
    public Sprite spritePanelConfig;

    // Iconos de Minijuegos
    public Sprite iconoLimpieza;
    public Sprite iconoSantuario;
    public Sprite iconoGlobo;
    public Sprite iconoPlantitas;
    public Sprite iconoMarea;
    public Sprite iconoFaros;
    public Sprite iconoArbol;
    public Sprite iconoGrounding;
    public Sprite iconoConstelacion;
    public Sprite iconoNube;
    public Sprite iconoRutas;
    public Sprite iconoPantallaAdmin;

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
        public bool desbloqueado;
        public string escena;
        public Sprite spriteTarjeta;
        public Sprite spriteIcono;
        public DatosMinijuego(string n, string d, Color c, bool desb, string esc, Sprite tarjeta = null, Sprite icono = null)
        {
            nombre = n; descripcion = d; color = c; desbloqueado = desb; escena = esc;
            spriteTarjeta = tarjeta; spriteIcono = icono;
        }
    }

    void Start()
    {
        CargarRecursos();
        AsegurarEventSystem();
        Canvas canvas = CrearCanvas();
        CrearFondo(canvas.transform);
        DefinirMinijuegos();

        panelBienvenida = ConstruirBienvenida(canvas.transform);
        panelPrincipal = ConstruirPrincipal(canvas.transform);
        panelMinijuegos = ConstruirMinijuegos(canvas.transform);
        panelConfig = ConstruirConfig(canvas.transform);

        CrearOverlayBrillo(canvas.transform);
        CargarPreferencias();

        if (regresarAMinijuegos)
        {
            regresarAMinijuegos = false;
            MostrarPantalla(panelMinijuegos);
        }
        else
        {
            MostrarPantalla(panelBienvenida);
        }
    }

    void CargarRecursos()
    {
        spriteRedondo = null;
        spritePanel = null;

#if UNITY_EDITOR
        if (tarjetaAzul == null) tarjetaAzul = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/tarjeta_azul.png");
        if (tarjetaRosa == null) tarjetaRosa = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/tarjeta_rosa.png");
        if (tarjetaVerde == null) tarjetaVerde = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/tarjeta_verde.png");
        if (spriteBotonMadera == null) spriteBotonMadera = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/boton_madera.png");
        if (spriteLetrero == null) spriteLetrero = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/letrero.png");
        if (spriteFondoMenu == null) spriteFondoMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/fondo.png");
        if (spritePanelConfig == null) spritePanelConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/05_panel_floral_vertical_32bits.png");

        if (iconoLimpieza == null) iconoLimpieza = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/limpiar pantallas icon.png");
        if (iconoSantuario == null) iconoSantuario = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/santuario de frecuencias icon.png");
        if (iconoGlobo == null) iconoGlobo = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/globo icon.png");
        if (iconoPlantitas == null) iconoPlantitas = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/regar plantitas icon.png");
        if (iconoMarea == null) iconoMarea = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/marea interior icon.png");
        if (iconoFaros == null) iconoFaros = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/pequeños faros icon.png");
        if (iconoArbol == null) iconoArbol = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/arbol icon.png");
        if (iconoGrounding == null) iconoGrounding = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/54321 icon.png");
        if (iconoConstelacion == null) iconoConstelacion = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/Constelacion icon.png");
        if (iconoNube == null) iconoNube = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/nube icon.png");
        if (iconoRutas == null) iconoRutas = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/ic_mente.png");
        if (iconoPantallaAdmin == null) iconoPantallaAdmin = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/menu/ic_mente.png");
#endif
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
        fondo.raycastTarget = false;

        if (spriteFondoMenu != null)
        {
            fondo.sprite = spriteFondoMenu;
            fondo.type = Image.Type.Simple;
        }
        else
        {
            var tex = new Texture2D(1, 2);
            tex.SetPixel(0, 0, cieloAbajo);
            tex.SetPixel(0, 1, cieloArriba);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply();
            fondo.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f));
            fondo.type = Image.Type.Simple;
        }
    }

    void DefinirMinijuegos()
    {
        minijuegos = new List<DatosMinijuego>
        {
            new DatosMinijuego("LIMPIAR PANTALLA",          "Despeja la niebla sensorial con clics y arrastre.", colSensorial, true,  "LimpiarPantalla",  tarjetaAzul,  iconoLimpieza),
            new DatosMinijuego("REGAR PLANTITAS",           "Acciones pequeñas que devuelven color y vida.",      colSensorial, true,  "RegarPlantitas",   tarjetaAzul,  iconoPlantitas),
            new DatosMinijuego("SANTUARIO DE FRECUENCIAS",  "Atenúa o silencia los estímulos a tu gusto.",        colSensorial, false, "",                 tarjetaAzul,  iconoSantuario),
            new DatosMinijuego("EL ÁRBOL",                  "Observa las hojas caer y ajusta el viento.",         colSensorial, true,  "Arbol",            tarjetaAzul,  iconoArbol),

            new DatosMinijuego("INFLAR UN GLOBO",           "Respiraciones voluntarias y predecibles a tu ritmo.", colEmocional, true, "Globito",          tarjetaRosa,  iconoGlobo),
            new DatosMinijuego("LA MAREA INTERIOR",         "Genera ondas suaves que se calman solas.",           colEmocional, true, "MareaInterior",    tarjetaRosa,  iconoMarea),
            new DatosMinijuego("PEQUEÑOS FAROS",            "Explora emociones y necesidades sin presión.",       colEmocional, true, "PequeñosFaros",    tarjetaRosa,  iconoFaros),
            new DatosMinijuego("NUBECITA",                  "Despeja tu mente mientras contemplas las nubes.",    colEmocional, true, "Nubecita",         tarjetaRosa,  iconoNube),

            new DatosMinijuego("5-4-3-2-1 GROUNDING",       "Orienta tu atención hacia el entorno inmediato.",    colCognitivo, true, "Grounding54321",   tarjetaVerde, iconoGrounding),
            new DatosMinijuego("RUTAS ALTERNATIVAS",        "Explora nuevas posibilidades cuando un plan falla.", colCognitivo, false,"",                 tarjetaVerde, iconoRutas),
            new DatosMinijuego("CONSTELACIÓN",              "Conecta las estrellas a tu propio ritmo.",           colCognitivo, true, "Constelacion",     tarjetaVerde, iconoConstelacion),
            new DatosMinijuego("PANTALLA ADMINISTRATIVA",   "Organiza y reduce estímulos visuales con orden.",     colCognitivo, true, "PantallaAdmin",    tarjetaVerde, iconoPantallaAdmin != null ? iconoPantallaAdmin : iconoRutas),
        };
    }

    // ================= PANTALLA: Bienvenida =================
    GameObject ConstruirBienvenida(Transform padre)
    {
        var raiz = CrearPantalla("Bienvenida", padre);

        var titulo = CrearTexto("Titulo", raiz.transform, "Bienvenido", 110, textoOscuro,
                                TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(titulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(1200, 150), new Vector2(0, 150));

        var sub = CrearTexto("Subtitulo", raiz.transform, "Cada paso también cuenta", 40, textoOscuro,
                           TextAlignmentOptions.Center);
        Anclar(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(1000, 60), new Vector2(0, 50));

        var comenzar = CrearBoton("Comenzar", raiz.transform, "Comenzar", botonMadera, cremaTexto, 40,
                                 () => MostrarPantalla(panelPrincipal));
        Anclar(comenzar.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(580, 150), new Vector2(0, -90));

        // Botón Ajustes más ancho (600) y más alto (120)
        var ajustes = CrearBoton("Ajustes", raiz.transform, "Ajustes sensoriales", botonMadera, cremaTexto, 28,
                                 () => AbrirConfig(panelBienvenida));
        Anclar(ajustes.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(600, 130), new Vector2(0, -235));

        return raiz;
    }

    // ================= PANTALLA: Menú principal =================
    GameObject ConstruirPrincipal(Transform padre)
    {
        var raiz = CrearPantalla("Principal", padre);

        var titulo = CrearTexto("Titulo", raiz.transform, "Bienvenido", 80, textoOscuro,
                                TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
               new Vector2(1000, 100), new Vector2(0, -80));

        string[] etiquetas = { "Nueva partida", "Continuar partida", "Minijuegos", "Configuración", "Salir del juego" };
        UnityAction[] acciones =
        {
            NuevaPartida,
            Continuar,
            () => MostrarPantalla(panelMinijuegos),
            () => AbrirConfig(panelPrincipal),
            SalirDelJuego
        };

        // Altura a 130 y multiplicador de separación bajó de 145 a 140 (menos espacio vacío entre botones)
        for (int i = 0; i < etiquetas.Length; i++)
        {
            var b = CrearBoton("Btn_" + i, raiz.transform, etiquetas[i], botonMadera, cremaTexto, 34, acciones[i]);
            Anclar(b.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2(600, 130), new Vector2(0, 200 - i * 140));
            if (i == 1) botonContinuar = b;
        }
        return raiz;
    }

    // ================= PANTALLA: Minijuegos =================
    GameObject ConstruirMinijuegos(Transform padre)
    {
        var raiz = CrearPantalla("Minijuegos", padre);

        // Botones superiores crecieron a 250x110
        var atras = CrearBoton("Atras", raiz.transform, "Atrás", botonMadera, cremaTexto, 26,
                               () => MostrarPantalla(panelPrincipal));
        Anclar(atras.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1),
               new Vector2(250, 110), new Vector2(35, -35));

        var ajustes = CrearBoton("Ajustes", raiz.transform, "Ajustes", botonMadera, cremaTexto, 26,
                                 () => AbrirConfig(panelMinijuegos));
        Anclar(ajustes.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1),
               new Vector2(250, 110), new Vector2(-35, -35));

        // Letrero creció a 1000x180 y bajó un poco en Y (-50)
        var letrero = CrearPanel("Letrero", raiz.transform, Color.white, true);
        letrero.raycastTarget = false;
        if (spriteLetrero != null)
        {
            letrero.sprite = spriteLetrero;
            letrero.type = Image.Type.Sliced;
            letrero.color = Color.white;
        }
        else
        {
            letrero.color = maderaLetrero;
        }
        Anclar(letrero.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
               new Vector2(1000, 210), new Vector2(0, -30));
        var tit = CrearTexto("Titulo", letrero.transform, "MIS MINIJUEGOS", 50, textoOscuro,
                           TextAlignmentOptions.Center, FontStyles.Bold);
        Estirar(tit.rectTransform);

        var sub = CrearTexto("Subtitulo", raiz.transform, "EXPLORA Y REGULA A TU PROPIO RITMO", 26, textoOscuro,
                           TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(sub.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
               new Vector2(1000, 40), new Vector2(0, -230));

        var cont = NuevoRect("Cuadricula", raiz.transform);
        Anclar(cont, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1700, 560), new Vector2(0, -50));
        var grid = cont.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(540, 140);
        grid.spacing = new Vector2(35, 18);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.MiddleCenter;

        foreach (var d in minijuegos)
            CrearTarjeta(cont, d);

        return raiz;
    }

    void CrearTarjeta(Transform padre, DatosMinijuego d)
    {
        Color baseColor = d.desbloqueado ? d.color : Hex("#CFC9BC");
        var tarjeta = CrearPanel("Tarjeta_" + d.nombre, padre, Color.white, true);
        tarjeta.raycastTarget = true;

        if (d.spriteTarjeta != null)
        {
            tarjeta.sprite = d.spriteTarjeta;
            tarjeta.type = Image.Type.Sliced;
            tarjeta.color = d.desbloqueado ? Color.white : new Color(0.85f, 0.85f, 0.85f, 0.9f);
        }
        else
        {
            tarjeta.color = baseColor;
        }

        var btn = tarjeta.gameObject.AddComponent<Button>();
        btn.targetGraphic = tarjeta;
        btn.interactable = d.desbloqueado;
        if (d.desbloqueado) btn.onClick.AddListener(() => AbrirMinijuego(d));

        if (d.spriteIcono != null)
        {
            var iconoGO = NuevoRect("IconoSprite", tarjeta.transform);
            Anclar(iconoGO, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2(90, 90), new Vector2(79f, -2.5f));
            var imgIcono = iconoGO.gameObject.AddComponent<Image>();
            imgIcono.sprite = d.spriteIcono;
            imgIcono.preserveAspect = true;
            imgIcono.raycastTarget = false;
            imgIcono.color = d.desbloqueado ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        }

        var tit = CrearTexto("Titulo", tarjeta.transform, d.nombre, 22, textoOscuro,
                           TextAlignmentOptions.TopLeft, FontStyles.Bold);
        tit.enableAutoSizing = true;
        tit.fontSizeMin = 15;
        tit.fontSizeMax = 22;
        tit.textWrappingMode = TextWrappingModes.NoWrap;
        Anclar(tit.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
               new Vector2(380, 34), new Vector2(138, -18));

        var desc = CrearTexto("Desc", tarjeta.transform, d.descripcion, 18, Oscurecer(textoOscuro, -0.05f),
                            TextAlignmentOptions.TopLeft);
        Anclar(desc.rectTransform, new Vector2(0, 1), new Vector2(0, 1),
               new Vector2(380, 80), new Vector2(138, -54));

        if (!d.desbloqueado)
        {
            var prox = CrearTexto("Prox", tarjeta.transform, "Próximamente", 20, Hex("#5A5244"),
                                TextAlignmentOptions.BottomRight, FontStyles.Italic);
            Anclar(prox.rectTransform, new Vector2(1, 0), new Vector2(1, 0),
                   new Vector2(220, 32), new Vector2(-16, 12));
        }
    }

    // ================= PANTALLA: Configuración =================
    GameObject ConstruirConfig(Transform padre)
    {
        var raiz = CrearPantalla("Config", padre);

        var sombra = CrearPanel("SombraFondo", raiz.transform, new Color(0, 0, 0, 0.4f), false);
        sombra.raycastTarget = true;
        Estirar(sombra.rectTransform);

        var panel = CrearPanel("Panel", raiz.transform, Color.white, false);
        panel.raycastTarget = true;

        if (spritePanelConfig != null)
        {
            panel.sprite = spritePanelConfig;
            panel.type = Image.Type.Simple;
            panel.color = Color.white;
        }
        else
        {
            panel.color = parchemino;
        }

        // Panel agrandado a 900x900 para garantizar que nada choque con los bordes
        Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(900, 900), Vector2.zero);

        // Título desplazado dramáticamente hacia abajo (-230) para salir del borde floral superior
        var tit = CrearTexto("Titulo", panel.transform, "Configuración sensorial", 42, textoOscuro,
                           TextAlignmentOptions.Center, FontStyles.Bold);
        Anclar(tit.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
               new Vector2(700, 25), new Vector2(0, -230));

        // --- CANALES DE AUDIO SEPARADOS ---
        float volMusica = PlayerPrefs.GetFloat("volumenMusica", 1f);
        float volSFX = PlayerPrefs.GetFloat("volumenSFX", 1f);

        // Bajaron las posiciones 'y' drásticamente para encajar debajo del título corregido
        CrearFilaSlider(panel.transform, "Música", 0f, 1f, volMusica, 30, v =>
        {
            AplicarVolumenMusica(v);
            PlayerPrefs.SetFloat("volumenMusica", v);
        });

        CrearFilaSlider(panel.transform, "Efectos (SFX)", 0f, 1f, volSFX, -80, v =>
        {
            AplicarVolumenSFX(v);
            PlayerPrefs.SetFloat("volumenSFX", v);
        });

        // --- AJUSTE VISUAL (Brillo) ---
        float brilloVal = PlayerPrefs.GetFloat("brillo", 1f);
        CrearFilaSlider(panel.transform, "Brillo", 0.2f, 1f, brilloVal, -190, v =>
        {
            AplicarBrillo(v);
            PlayerPrefs.SetFloat("brillo", v);
        });

        var nota = CrearTexto("Nota", panel.transform,
                            "Ajusta los canales de audio y brillo a tu nivel de confort sensorial.",
                            18, Hex("#6B6555"), TextAlignmentOptions.Center, FontStyles.Italic);
        Anclar(nota.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(600, 36), new Vector2(0, -265));

        // Botón volver más alto y centrado sobre el borde inferior
        var volver = CrearBoton("Volver", panel.transform, "Volver", botonMadera, cremaTexto, 32,
                                () => CerrarConfig());
        Anclar(volver.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
               new Vector2(300, 120), new Vector2(0, 50));

        return raiz;
    }

    void CrearFilaSlider(Transform panel, string etiqueta, float min, float max, float valor, float y, UnityAction<float> alCambiar)
    {
        var lbl = CrearTexto("Lbl_" + etiqueta, panel, etiqueta, 26, textoOscuro,
                           TextAlignmentOptions.Left, FontStyles.Bold);
        // Desplazamiento Y extra de +50 para separar la etiqueta del slider visualmente
        Anclar(lbl.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
               new Vector2(500, 30), new Vector2(-10, y + 50));
        CrearSlider(panel, min, max, valor, new Vector2(-10, y), alCambiar);
    }

    // ================= Métodos de Ajuste de Audio =================
    void AplicarVolumenMusica(float v)
    {
        if (mainAudioMixer != null)
        {
            float db = v > 0.0001f ? Mathf.Log10(v) * 20 : -80f;
            mainAudioMixer.SetFloat("MusicaVol", db);
        }
    }

    void AplicarVolumenSFX(float v)
    {
        if (mainAudioMixer != null)
        {
            float db = v > 0.0001f ? Mathf.Log10(v) * 20 : -80f;
            mainAudioMixer.SetFloat("SFXVol", db);
        }
    }

    // ================= Acciones =================
    void NuevaPartida()
    {
        PlayerPrefs.SetInt("hayGuardado", 1);
        PlayerPrefs.Save();
        if (botonContinuar) botonContinuar.interactable = true;
        MostrarPantalla(panelMinijuegos);
    }

    void Continuar()
    {
        Debug.Log("Continuar partida...");
    }

    void AbrirMinijuego(DatosMinijuego d)
    {
        if (!string.IsNullOrEmpty(d.escena))
        {
            if (Application.CanStreamedLevelBeLoaded(d.escena))
            {
                SceneManager.LoadScene(d.escena);
            }
            else if (d.escena == "Globito" && Application.CanStreamedLevelBeLoaded("InflarGlobo"))
            {
                SceneManager.LoadScene("InflarGlobo");
            }
            else if (d.escena == "InflarGlobo" && Application.CanStreamedLevelBeLoaded("Globito"))
            {
                SceneManager.LoadScene("Globito");
            }
        }
    }

    void AbrirConfig(GameObject origen)
    {
        pantallaAnterior = origen;
        panelConfig.SetActive(true);
    }

    void CerrarConfig()
    {
        panelConfig.SetActive(false);
    }

    void GuardarPreferencias()
    {
        PlayerPrefs.Save();
    }

    void CargarPreferencias()
    {
        AplicarVolumenMusica(PlayerPrefs.GetFloat("volumenMusica", 1f));
        AplicarVolumenSFX(PlayerPrefs.GetFloat("volumenSFX", 1f));
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
        panelConfig.SetActive(false);
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
        img.raycastTarget = false;
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
        t.raycastTarget = false;
        return t;
    }

    Button CrearBoton(string nombre, Transform padre, string etiqueta, Color colorFondo, Color colorTexto,
                      float tamTexto, UnityAction alClic)
    {
        var img = CrearPanel(nombre, padre, colorFondo, true);
        img.raycastTarget = true;

        if (spriteBotonMadera != null)
        {
            img.sprite = spriteBotonMadera;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

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
        Anclar(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 32), pos);
        var s = rt.gameObject.AddComponent<Slider>();

        var fondo = CrearPanel("Fondo", rt, new Color(0, 0, 0, 0.15f), true);
        Estirar(fondo.rectTransform);

        var areaRelleno = NuevoRect("AreaRelleno", rt);
        Estirar(areaRelleno, 8, 8, 0, 0);
        var relleno = NuevoRect("Relleno", areaRelleno);
        relleno.anchorMin = new Vector2(0, 0); relleno.anchorMax = new Vector2(1, 1);
        relleno.sizeDelta = new Vector2(10, 0);
        var rellenoImg = relleno.gameObject.AddComponent<Image>();
        rellenoImg.raycastTarget = false;
        rellenoImg.sprite = spriteRedondo; rellenoImg.type = Image.Type.Sliced; rellenoImg.color = acento;

        var areaHandle = NuevoRect("AreaHandle", rt);
        Estirar(areaHandle, 12, 12, 0, 0);
        var handle = NuevoRect("Handle", areaHandle);
        handle.sizeDelta = new Vector2(30, 30);
        var handleImg = handle.gameObject.AddComponent<Image>();
        handleImg.raycastTarget = true;
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