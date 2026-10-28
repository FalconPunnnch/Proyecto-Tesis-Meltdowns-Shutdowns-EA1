using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Pequeños Faros: autoobservación guiada de necesidades.
/// Usa el fondo apagado y cuatro overlays PNG RGBA.
/// </summary>
public class PequenosFaros : MonoBehaviour
{
    [Header("Recursos visuales")]
    public Sprite fondoApagado;

    // Orden: Olas, Brote, Viento, Brújula.
    public Sprite[] iconosFaros = new Sprite[4];

    // IMPORTANTE: asignar los PNG Overlay de 1672x941.
    public Sprite[] farosEncendidos = new Sprite[4];

    // Opcional: panel pixel art de tu interfaz.
    public Sprite marcoVentana;

    [Header("Navegación y accesibilidad")]
    public string escenaMenu = "Menu";
    public bool guardarSoloProgresoLocal = true;
    public bool animacionSuave = true;

    const float ANCHO = 1672f;
    const float ALTO = 941f;
    const string PREFIJO = "PequenosFaros_V1_Faro_";

    [Serializable]
    class Pregunta
    {
        public string texto;
        public string[] opciones;
        public string[] apoyos;

        public Pregunta(string texto, string[] opciones, string[] apoyos)
        {
            this.texto = texto;
            this.opciones = opciones;
            this.apoyos = apoyos;
        }
    }

    readonly bool[] encendidos = new bool[4];
    readonly Coroutine[] animaciones = new Coroutine[4];

    readonly Image[] capasEncendidas = new Image[4];
    readonly Button[] botonesFaro = new Button[4];
    readonly Button[] botonesOpciones = new Button[4];

    readonly TextMeshProUGUI[] textosOpciones = new TextMeshProUGUI[4];

    Pregunta[][] banco;
    Pregunta[] preguntasDeSesion = new Pregunta[4];

    RectTransform area;
    GameObject capaModal;

    TextMeshProUGUI tituloModal;
    TextMeshProUGUI textoPregunta;
    TextMeshProUGUI textoEstado;
    TextMeshProUGUI textoMensaje;

    int faroPendiente = -1;
    bool modalActivo;

    // =========================================================
    // INICIALIZACIÓN
    // =========================================================

    void Start()
    {
        CrearBancoDePreguntas();

        for (int i = 0; i < 4; i++)
        {
            preguntasDeSesion[i] = banco[i][UnityEngine.Random.Range(0, banco[i].Length)];
        }

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject canvasGO = new GameObject(
            "CanvasPequenosFaros",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler escalador = canvasGO.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(ANCHO, ALTO);
        escalador.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        escalador.matchWidthOrHeight = 0.5f;

        area = Nuevo("AreaArte_16_9", canvas.transform);
        Estirar(area);

        AspectRatioFitter ajuste = area.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        ajuste.aspectRatio = ANCHO / ALTO;

        Image fondo = CrearImagen("Fondo_FarosApagados", area, Color.white, fondoApagado);
        Estirar(fondo.rectTransform);
        fondo.raycastTarget = false;

        if (fondoApagado == null)
        {
            fondo.color = Hex("#15294D");
            Debug.LogWarning("Asigna el fondo apagado en el Inspector.");
        }

        for (int i = 0; i < 4; i++) CrearOverlayEncendido(i);
        for (int i = 0; i < 4; i++) CrearMarcadorInteractivo(i);

        CrearInterfaz();
        CrearVentanaModal();

        for (int i = 0; i < 4; i++)
        {
            encendidos[i] = guardarSoloProgresoLocal && PlayerPrefs.GetInt(PREFIJO + i, 0) == 1;
            AplicarLuz(i, encendidos[i] ? 1f : 0f);
        }

        ActualizarEstado();
    }

    void Update()
    {
        if (modalActivo && Input.GetKeyDown(KeyCode.Escape))
            Posponer();
    }

    // =========================================================
    // FAROS Y SUS SPRITES
    // =========================================================

    void CrearOverlayEncendido(int i)
    {
        Sprite sprite = farosEncendidos != null && i < farosEncendidos.Length ? farosEncendidos[i] : null;

        Image imagen = CrearImagen("FaroEncendidoOverlay_" + i, area, new Color(1f, 1f, 1f, 0f), sprite);

        if (sprite != null)
        {
            // ¡Esto obliga a la imagen a tomar su tamaño real en píxeles (1672x941)!
            imagen.SetNativeSize();
        }
        else
        {
            Debug.LogWarning("Falta el Overlay del faro " + (i + 1));
        }

        // Anclamos exactamente al centro de la pantalla
        imagen.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        imagen.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        imagen.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        imagen.rectTransform.anchoredPosition = Vector2.zero;

        // Aseguramos que la escala sea 1 por si algo la estaba comprimiendo
        imagen.rectTransform.localScale = Vector3.one;
         
        imagen.raycastTarget = false;
        capasEncendidas[i] = imagen;
    }

    void CrearMarcadorInteractivo(int i)
    {
        Vector2[] marcadores =
        {
            new Vector2(-509, 251),  // Olas 
            new Vector2(-146, 171),  // Brote 
            new Vector2(206, 133),   // Viento 
            new Vector2(644, 160)    // Brújula 
        };

        Sprite sprite = iconosFaros != null && i < iconosFaros.Length ? iconosFaros[i] : null;

        Image imagen = CrearImagen("MarcadorInteractivo_" + i, area, sprite != null ? Color.white : Hex("#F1D59D"), sprite);
        imagen.preserveAspect = true;

        // Asignamos el tamaño base
        Anclar(imagen.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(64, 70), marcadores[i]);

        // ¡Aquí aplicamos la escala en 2 a todos los botones de los faros!
        imagen.rectTransform.localScale = new Vector3(2.5f, 2.5f, 2f);

        Button boton = imagen.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;

        // Script de flotación en el eje Y
        imagen.gameObject.AddComponent<AnimacionHoverBoton>();

        int indice = i;
        boton.onClick.AddListener(() => AbrirPregunta(indice));

        botonesFaro[i] = boton;
    }

    // =========================================================
    // INTERFAZ
    // =========================================================

    void CrearInterfaz()
    {
        Image guiaFondo = CrearImagen("FondoGuia", area, new Color(0.04f, 0.10f, 0.23f, 0.78f));
        Anclar(guiaFondo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1050, 60), new Vector2(0, -41));
        guiaFondo.raycastTarget = false;

        var guia = CrearTexto("Guia", guiaFondo.transform, "Elige un faro y descubre qué necesitas. No tienes que encenderlos todos.", 25, Hex("#FFF3D8"));
        Estirar(guia.rectTransform);

        Button volver = CrearBoton("Volver", area, "< Volver", new Vector2(-739, 410), new Vector2(152, 58), Hex("#F5E7CF"), Hex("#302B32"), 25);
        volver.onClick.AddListener(() =>
        {
            // MenuManager.RegresarAMinijuegos(escenaMenu); 
            Debug.Log("Volviendo al menú..."); // Descomenta tu línea de menú real aquí
        });

        textoEstado = CrearTexto("Estado", area, "", 25, Hex("#FFF3DC"));
        Anclar(textoEstado.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(800, 42), new Vector2(0, 48));

        textoMensaje = CrearTexto("MensajeDeApoyo", area, "Cada pequeña elección cuenta. Puedes detenerte cuando lo necesites.", 24, Hex("#FFECC1"));
        Anclar(textoMensaje.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1180, 66), new Vector2(0, 100));

        Button reiniciar = CrearBoton("Reiniciar", area, "Reiniciar luces", new Vector2(706, -412), new Vector2(205, 55), Hex("#F5E7CF"), Hex("#302B32"), 22);
        reiniciar.onClick.AddListener(Reiniciar);
    }

    // =========================================================
    // VENTANA EMERGENTE
    // =========================================================

    void CrearVentanaModal()
    {
        capaModal = Nuevo("CapaModal_BloqueaFondo", area).gameObject;
        Estirar(capaModal.GetComponent<RectTransform>());

        Image oscurecer = capaModal.AddComponent<Image>();
        oscurecer.color = new Color(0.02f, 0.04f, 0.13f, 0.75f);
        oscurecer.raycastTarget = true;

        Image ventana = CrearImagen("VentanaPregunta", capaModal.transform, marcoVentana != null ? Color.white : Hex("#FFF0D6"), marcoVentana);
        ventana.preserveAspect = false;
        Anclar(ventana.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(920, 580), Vector2.zero);
        ventana.raycastTarget = true;

        tituloModal = CrearTexto("TituloModal", ventana.transform, "", 28, Hex("#78472F"));
        Anclar(tituloModal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(790, 46), new Vector2(0, 245));

        textoPregunta = CrearTexto("Pregunta", ventana.transform, "", 36, Hex("#302C3E"));
        Anclar(textoPregunta.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800, 112), new Vector2(0, 160));

        for (int i = 0; i < 4; i++)
        {
            Button boton = CrearBoton("Respuesta_" + i, ventana.transform, "", new Vector2(0, 26 - i * 77), new Vector2(790, 63), Hex("#E5BB83"), Hex("#302C3E"), 25);
            int opcion = i;
            boton.onClick.AddListener(() => Responder(opcion));
            botonesOpciones[i] = boton;
            textosOpciones[i] = boton.GetComponentInChildren<TextMeshProUGUI>();
        }

        Button posponer = CrearBoton("Posponer", ventana.transform, "Ahora no", new Vector2(330, -264), new Vector2(168, 43), Hex("#F4DFC1"), Hex("#5B514B"), 22);
        posponer.onClick.AddListener(Posponer);

        capaModal.SetActive(false);
    }

    void AbrirPregunta(int indice)
    {
        if (modalActivo || indice < 0 || indice >= 4) return;

        modalActivo = true;
        faroPendiente = indice;

        // Se cambiaron los puntos medios por guiones para evitar errores de fuente
        string[] nombres = { "OLAS - Señales", "BROTE - Energía", "VIENTO - Entorno", "BRÚJULA - Apoyos" };

        tituloModal.text = nombres[indice];
        Pregunta pregunta = preguntasDeSesion[indice];
        textoPregunta.text = pregunta.texto;

        for (int i = 0; i < 4; i++)
        {
            textosOpciones[i].text = pregunta.opciones[i];
            botonesOpciones[i].interactable = true;
        }

        capaModal.SetActive(true);
        capaModal.transform.SetAsLastSibling();
    }

    // =========================================================
    // RESPUESTAS Y BLOQUEO
    // =========================================================

    void Responder(int opcion)
    {
        if (!modalActivo || faroPendiente < 0 || opcion < 0 || opcion >= 4) return;

        int indice = faroPendiente;
        Pregunta pregunta = preguntasDeSesion[indice];
        bool estabaEncendido = encendidos[indice];

        encendidos[indice] = true;
        modalActivo = false;
        faroPendiente = -1;
        capaModal.SetActive(false);

        textoMensaje.text = pregunta.apoyos[opcion];

        if (guardarSoloProgresoLocal)
        {
            PlayerPrefs.SetInt(PREFIJO + indice, 1);
            PlayerPrefs.Save();
        }

        if (animaciones[indice] != null)
        {
            StopCoroutine(animaciones[indice]);
            animaciones[indice] = null;
        }

        if (!estabaEncendido)
            animaciones[indice] = StartCoroutine(AnimarEncendido(indice));
        else
            AplicarLuz(indice, 1f);

        ActualizarEstado();
    }

    void Posponer()
    {
        if (!modalActivo) return;

        modalActivo = false;
        faroPendiente = -1;
        capaModal.SetActive(false);
        textoMensaje.text = "Está bien posponerlo. Este faro seguirá aquí cuando quieras volver.";
    }

    // =========================================================
    // ANIMACIÓN Y OPACIDAD
    // =========================================================

    IEnumerator AnimarEncendido(int indice)
    {
        float duracion = animacionSuave ? 1.0f : 0f;

        if (duracion <= 0f)
        {
            AplicarLuz(indice, 1f);
            animaciones[indice] = null;
            yield break;
        }

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            t = t * t * (3f - 2f * t);
            AplicarLuz(indice, t);
            yield return null;
        }

        AplicarLuz(indice, 1f);
        animaciones[indice] = null;
    }

    void AplicarLuz(int i, float intensidad)
    {
        // Se cambió a 1.0f para que el PNG aproveche su propia transparencia y no se vea gris/apagado.
        DefinirAlpha(capasEncendidas[i], intensidad * 1.0f);
    }

    // =========================================================
    // ESTADO Y REINICIO
    // =========================================================

    void ActualizarEstado()
    {
        int cantidad = 0;
        for (int i = 0; i < 4; i++)
        {
            if (encendidos[i]) cantidad++;
        }

        textoEstado.text = "Faros encendidos: " + cantidad + "/4 - Puedes parar cuando quieras";
    }

    void Reiniciar()
    {
        if (modalActivo) return;

        StopAllCoroutines();
        for (int i = 0; i < 4; i++)
        {
            encendidos[i] = false;
            animaciones[i] = null;
            AplicarLuz(i, 0f);

            if (guardarSoloProgresoLocal)
                PlayerPrefs.DeleteKey(PREFIJO + i);

            preguntasDeSesion[i] = banco[i][UnityEngine.Random.Range(0, banco[i].Length)];
        }

        if (guardarSoloProgresoLocal) PlayerPrefs.Save();

        textoMensaje.text = "Nuevo recorrido. Puedes comenzar por cualquier faro.";
        ActualizarEstado();
    }

    // =========================================================
    // BANCO DE PREGUNTAS
    // =========================================================

    void CrearBancoDePreguntas()
    {
        banco = new Pregunta[4][];

        banco[0] = new[]
        {
            new Pregunta("¿Qué señal podría indicar que necesitas una pausa?", new[] { "Los sonidos o las luces me molestan más", "Me cuesta organizar lo que pienso", "Noto tensión o necesidad de moverme", "Aún no identifico una señal" }, new[] { "Puedes probar un lugar con menos estímulos.", "Dar espacio a tus pensamientos también es cuidarte.", "Moverte o hacer stimming puede ser una forma válida de regularte.", "No saberlo todavía está bien. Observar sin presionarte ya cuenta." }),
            new Pregunta("Cuando empiezas a saturarte, ¿qué notas primero?", new[] { "Cambios en mi cuerpo", "Irritación o emociones intensas", "Necesito menos interacción", "Todavía no puedo distinguirlo" }, new[] { "Tu cuerpo puede ofrecer pistas; no hace falta interpretarlas perfectamente.", "Tus emociones pueden señalar una necesidad de pausa.", "Pedir silencio o menos conversación es una opción válida.", "Puedes reconocer la incertidumbre sin forzarte a definirla." }),
            new Pregunta("Si el día ha sido intenso, ¿qué podría ayudarte ahora?", new[] { "Silencio o audífonos", "Tomar distancia de una tarea", "Un movimiento repetitivo cómodo", "No lo tengo claro" }, new[] { "Puedes ajustar los sonidos según tu comodidad.", "Dejar una tarea en pausa también es una elección.", "La autorregulación puede incluir movimientos elegidos por ti.", "No tienes que encontrar una solución de inmediato." })
        };

        banco[1] = new[]
        {
            new Pregunta("¿Cómo sientes tu energía disponible en este momento?", new[] { "Baja: quiero descansar", "Media: prefiero ir despacio", "Alta: me siento con energía", "No puedo estimarla todavía" }, new[] { "Descansar es una necesidad válida, no un fracaso.", "Ajustar el ritmo a tu energía es una decisión útil.", "Está bien disfrutar de tu energía sin exigirte más.", "Puedes dejar esa pregunta abierta por ahora." }),
            new Pregunta("¿Qué forma de descanso te resulta más cómoda?", new[] { "Estar a solas", "Hacer algo repetitivo y agradable", "Compartir espacio sin conversar", "Todavía no sé qué necesito" }, new[] { "Tener tiempo a solas puede ayudarte a recuperarte.", "Una actividad familiar y agradable también puede ser descanso.", "La compañía sin exigencias de hablar es una opción válida.", "No necesitas justificar que aún no lo sabes." }),
            new Pregunta("¿Qué necesitas permitirte hoy?", new[] { "Hacer menos cosas", "Avanzar a mi ritmo", "Disfrutar de mi interés personal", "No estoy seguro de qué" }, new[] { "Reducir demandas puede proteger tu energía.", "Tu propio ritmo merece respeto.", "Tus intereses también pueden ser una fuente de bienestar.", "Puedes darte tiempo para descubrirlo." })
        };

        banco[2] = new[]
        {
            new Pregunta("¿Qué aspecto de tu entorno ajustarías primero?", new[] { "El volumen de los sonidos", "El brillo o tipo de luz", "La temperatura, olores o texturas", "No necesito cambiar nada ahora" }, new[] { "Ajustar el sonido puede hacer tu espacio más cómodo.", "Elegir una luz agradable es una adaptación válida.", "Tu comodidad sensorial merece ser tenida en cuenta.", "También es válido estar cómodo como estás." }),
            new Pregunta("¿Qué te ayudaría si hay demasiados estímulos?", new[] { "Ir a un espacio tranquilo", "Reducir una fuente de ruido", "Usar un objeto que me reconforte", "No sé qué me ayudaría" }, new[] { "Buscar tranquilidad es una decisión de autocuidado.", "Puedes cambiar el entorno en vez de exigirte tolerarlo.", "Tus apoyos sensoriales son herramientas legítimas.", "Puedes explorar opciones poco a poco." }),
            new Pregunta("¿Qué nivel de interacción te apetece ahora?", new[] { "Ninguna por un rato", "Compañía en silencio", "Hablar brevemente", "Aún no lo tengo claro" }, new[] { "Puedes solicitar un momento a solas.", "No toda compañía requiere conversación.", "Puedes comunicarte brevemente y a tu manera.", "Está bien reservarte la decisión." })
        };

        banco[3] = new[]
        {
            new Pregunta("¿Cómo preferirías comunicar una necesidad importante?", new[] { "Con un mensaje escrito", "Diciéndolo con pocas palabras", "Usando una señal acordada", "Todavía no sé qué método elegir" }, new[] { "La comunicación escrita es tan válida como la oral.", "Una frase corta puede ser suficiente.", "Puedes acordar señales con personas de confianza.", "Puedes probar métodos distintos sin comprometerte con uno." }),
            new Pregunta("¿Qué apoyo quisieras tener disponible ante una sobrecarga?", new[] { "Un lugar donde retirarme", "Una persona que respete mi silencio", "Una forma de pedir una pausa", "Aún no sé qué apoyo elegir" }, new[] { "Preparar un espacio seguro puede darte opciones.", "Puedes pedir presencia sin necesidad de hablar.", "Una señal de pausa puede reducir el esfuerzo de explicarte.", "No es necesario tener un plan perfecto hoy." }),
            new Pregunta("Si tus planes cambian de repente, ¿qué podría ayudarte?", new[] { "Conocer el nuevo plan con tiempo", "Tener una alternativa sencilla", "Disponer de unos minutos para adaptarme", "No estoy seguro todavía" }, new[] { "Tener información anticipada puede ayudarte a decidir.", "Una alternativa preparada amplía tus posibilidades.", "Pedir tiempo para adaptarte es válido.", "Puedes necesitar más tiempo para reconocer qué te ayuda." })
        };
    }

    // =========================================================
    // FUNCIONES AUXILIARES DE UNITY UI
    // =========================================================

    static void DefinirAlpha(Image imagen, float a)
    {
        if (imagen == null) return;
        Color c = imagen.color;
        c.a = Mathf.Clamp01(a);
        imagen.color = c;
    }

    static RectTransform Nuevo(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go.GetComponent<RectTransform>();
    }

    static Image CrearImagen(string nombre, Transform padre, Color color, Sprite sprite = null)
    {
        Image imagen = Nuevo(nombre, padre).gameObject.AddComponent<Image>();
        imagen.sprite = sprite;
        imagen.color = color;
        return imagen;
    }

    static TextMeshProUGUI CrearTexto(string nombre, Transform padre, string texto, float tam, Color color)
    {
        TextMeshProUGUI tmp = Nuevo(nombre, padre).gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tam;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button CrearBoton(string nombre, Transform padre, string texto, Vector2 pos, Vector2 dimensiones, Color fondo, Color colorTexto, float tamTexto)
    {
        Image imagen = CrearImagen(nombre, padre, fondo);
        Anclar(imagen.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), dimensiones, pos);

        Button boton = imagen.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;

        TextMeshProUGUI tmp = CrearTexto("Texto", imagen.transform, texto, tamTexto, colorTexto);
        Estirar(tmp.rectTransform);
        tmp.margin = new Vector4(14, 2, 14, 2);

        return boton;
    }

    static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Estas dos líneas evitan que Unity colapse la imagen
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    static void Anclar(RectTransform rt, Vector2 anchorMinMax, Vector2 pivot, Vector2 tam, Vector2 pos)
    {
        rt.anchorMin = anchorMinMax;
        rt.anchorMax = anchorMinMax;
        rt.pivot = pivot;
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;
    }

    static Color Hex(string valor)
    {
        ColorUtility.TryParseHtmlString(valor, out Color color);
        return color;
    }
}

/// <summary>
/// Script auxiliar que añade un efecto de flotación (movimiento en Y)
/// cuando el mouse pasa sobre los iconos de los faros.
/// </summary>
public class AnimacionHoverBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector2 posOriginal;
    RectTransform rt;
    bool hover = false;
    float elevacion = 15f; // Cuántos píxeles sube

    void Start()
    {
        rt = GetComponent<RectTransform>();
        posOriginal = rt.anchoredPosition;
    }

    void Update()
    {
        float targetY = posOriginal.y + (hover ? elevacion : 0f);
        rt.anchoredPosition = Vector2.Lerp(
            rt.anchoredPosition,
            new Vector2(posOriginal.x, targetY),
            Time.deltaTime * 12f
        );
    }

    public void OnPointerEnter(PointerEventData e) { hover = true; }
    public void OnPointerExit(PointerEventData e) { hover = false; }
}