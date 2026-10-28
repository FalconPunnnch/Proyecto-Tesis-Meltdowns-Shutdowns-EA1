using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

// Un solo componente. Carga los PNG incluidos en Resources/Grounding54321.
// Conserva las claves de guardado del Grounding54321 anterior.
public class Grounding54321 : MonoBehaviour
{
    public string escenaMenu = "Menu";
    [Header("Imágenes opcionales: si están vacías se cargan las incluidas")]
    public Sprite fondo, ojo, tacto, oido, olfato, gusto, mano;
    public Sprite botonVolver, botonMadera;
    [Header("Sonidos opcionales, nunca se reproducen automáticamente")]
    public AudioClip aves, aguaFuente;
    [UnityEngine.Serialization.FormerlySerializedAs("hojas")]
    public AudioClip campanillas;
    [Range(0, 1)] public float volumen = 0.25f;

    private readonly string[] nombres = { "Vista", "Tacto", "Oído", "Olfato", "Gusto" };
    private readonly int[] metas = { 5, 4, 3, 2, 1 };
    private readonly string[] instrucciones = {
        "Observa 5 cosas que puedas ver.",
        "Nota 4 sensaciones de contacto cómodas para ti.",
        "Escucha 3 sonidos que puedas identificar.",
        "Nota 2 olores de tu entorno, si es cómodo.",
        "Nota 1 sabor que ya esté presente, si deseas hacerlo."
    };
    private readonly string[] sugerenciasTacto = {
        "Puedes notar la tela de tu ropa.", "Puedes notar el apoyo de la silla.",
        "Puedes notar los pies apoyados.", "Puedes notar una superficie cercana que sea cómoda."
    };
    private readonly string[] objetos = { "Banco", "Árbol", "Fuente", "Flores", "Infusión" };
    // Coordenadas normalizadas, origen abajo a la izquierda. Ajustables si cambias el fondo.
    [Header("Ubicación de los elementos sobre el fondo")]
    public Vector2[] posiciones = {
        new Vector2(0.16f, 0.44f), new Vector2(0.26f, 0.65f),
        new Vector2(0.66f, 0.50f), new Vector2(0.85f, 0.36f), new Vector2(0.32f, 0.47f)
    };
    private char[] estado;
    private int indice;
    private bool terminado;
    private string prefijo;
    private GameObject canvasActual;
    private RectTransform escenario;
    private TextMeshProUGUI textoPaso, textoProgreso, resumen, ayuda, conteo;
    private Button siguiente, omitir, registrar;
    private Image iconoActual, manoActual;
    private Image[] placas;
    private TextMeshProUGUI[] etiquetasEstado;
    private Sprite[] iconos;
    private Button[] puntos;
    private readonly bool[,] elegidos = new bool[5, 5];
    private readonly int[] contados = new int[5];
    private AudioSource sonido;
    private readonly List<UnityEngine.Object> temporales = new List<UnityEngine.Object>();
    private readonly Dictionary<Sprite, Sprite> recortes = new Dictionary<Sprite, Sprite>();
    private readonly Color crema = new Color(1, 0.96f, 0.86f);

    private void Start()
    {
        string id = PlayerPrefs.GetString("Tesis.PerfilActivo", "local");
        if (string.IsNullOrWhiteSpace(id)) id = "local";
        prefijo = "Tesis." + Uri.EscapeDataString(id) + ".grounding.";
        string guardado = PlayerPrefs.GetString(prefijo + "sesionActual", "-----");
        if (guardado.Length != 5) guardado = "-----";
        estado = guardado.ToCharArray();
        for (int i = 0; i < estado.Length; i++)
            if (estado[i] != 'C' && estado[i] != 'O') estado[i] = '-';
        BuscarPendiente();
        CargarImagenes();
        sonido = gameObject.AddComponent<AudioSource>();
        sonido.playOnAwake = false;
        sonido.spatialBlend = 0;
        CrearEscena();
        ActualizarPantalla();
    }

    private void CargarImagenes()
    {
        fondo = Cargar(fondo, "fondo", false);
        ojo = Cargar(ojo, "ojo", true);
        tacto = Cargar(tacto, "tacto", true);
        oido = Cargar(oido, "oido", true);
        olfato = Cargar(olfato, "olfato", true);
        gusto = Cargar(gusto, "gusto", true);
        mano = Cargar(mano, "hand", true);
        botonVolver = Cargar(botonVolver, "botonVolver", true);
        botonMadera = Cargar(botonMadera, "botonMadera", true);
        iconos = new[] { ojo, tacto, oido, olfato, gusto };
    }

    private Sprite Cargar(Sprite asignado, string nombre, bool recortar)
    {
        Sprite s = asignado;
        if (s == null) s = Resources.Load<Sprite>("Grounding54321/" + nombre);
        if (s == null)
        {
            Texture2D t = Resources.Load<Texture2D>("Grounding54321/" + nombre);
            if (t != null)
            {
                t.filterMode = FilterMode.Point;
                s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
                temporales.Add(s);
            }
        }
        if (s == null) Debug.LogWarning("Falta la imagen " + nombre, this);
        return s != null && recortar ? Recortar(s) : s;
    }

    // Tus iconos tienen mucho espacio transparente y posiciones distintas en su PNG.
    // Se recorta el rectángulo de dibujo en memoria, sin alterar el archivo original.
    private Sprite Recortar(Sprite original)
    {
        if (recortes.TryGetValue(original, out Sprite previo)) return previo;
        if (original.packed) return original;
        RenderTexture anterior = RenderTexture.active;
        RenderTexture rt = null;
        Texture2D copia = null;
        Sprite resultado = original;
        try
        {
            Texture2D t = original.texture;
            Color32[] pixeles;
            if (t.isReadable) pixeles = t.GetPixels32();
            else
            {
                rt = RenderTexture.GetTemporary(t.width, t.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(t, rt);
                RenderTexture.active = rt;
                copia = new Texture2D(t.width, t.height, TextureFormat.RGBA32, false);
                copia.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
                copia.Apply();
                pixeles = copia.GetPixels32();
            }
            Rect r = original.rect;
            int ox = Mathf.RoundToInt(r.x), oy = Mathf.RoundToInt(r.y);
            int w = Mathf.RoundToInt(r.width), h = Mathf.RoundToInt(r.height);
            int x0 = w, y0 = h, x1 = -1, y1 = -1;
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                if (pixeles[(oy + y) * t.width + ox + x].a < 32) continue;
                x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x);
                y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y);
            }
            if (x1 >= x0 && y1 >= y0)
            {
                resultado = Sprite.Create(t, new Rect(ox + x0, oy + y0, x1 - x0 + 1, y1 - y0 + 1), new Vector2(0.5f, 0.5f), original.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                temporales.Add(resultado);
            }
        }
        catch (Exception e) { Debug.LogWarning("No se pudo recortar " + original.name + ": " + e.Message, this); }
        finally
        {
            RenderTexture.active = anterior;
            if (rt != null) RenderTexture.ReleaseTemporary(rt);
            if (copia != null) Destroy(copia);
        }
        recortes[original] = resultado;
        return resultado;
    }

    private void CrearEscena()
    {
        EventSystem eventos = FindAnyObjectByType<EventSystem>();
        if (eventos == null) eventos = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        foreach (BaseInputModule m in eventos.GetComponents<BaseInputModule>()) m.enabled = false;
        InputSystemUIInputModule ui = eventos.GetComponent<InputSystemUIInputModule>();
        if (ui == null) ui = eventos.gameObject.AddComponent<InputSystemUIInputModule>();
        ui.enabled = true;
        if (ui.actionsAsset == null) ui.AssignDefaultActions();

        canvasActual = new GameObject("CanvasGrounding", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasActual.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasActual.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Image exterior = Imagen("Franjas", canvasActual.transform, null, new Color(0.10f, 0.14f, 0.11f), Vector2.zero, Vector2.zero);
        Estirar(exterior.rectTransform);
        RectTransform marco = Nuevo("Encuadre", canvasActual.transform);
        Estirar(marco);
        AspectRatioFitter fit = marco.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fit.aspectRatio = 16f / 9f;
        escenario = Nuevo("Escenario", marco);
        escenario.sizeDelta = new Vector2(1920, 1080);
        Image fondoUI = Imagen("Fondo", escenario, fondo, Color.white, new Vector2(1920, 1080), Vector2.zero);
        fondoUI.preserveAspect = true;

        placas = new Image[5]; etiquetasEstado = new TextMeshProUGUI[5];
        for (int i = 0; i < 5; i++)
        {
            placas[i] = Imagen(nombres[i], escenario, null, new Color(0.12f, 0.17f, 0.12f, 0.88f), new Vector2(118, 116), new Vector2((i - 2) * 134, 447));
            Imagen("Icono", placas[i].transform, iconos[i], Color.white, new Vector2(49, 49), new Vector2(0, 21));
            etiquetasEstado[i] = Texto("Estado", placas[i].transform, metas[i] + " · " + nombres[i], 19, new Vector2(115, 43), new Vector2(0, -30));
        }
        textoProgreso = Texto("Progreso", escenario, "", 25, new Vector2(800, 48), new Vector2(0, 350));
        textoProgreso.outlineColor = new Color(0.12f, 0.14f, 0.10f);
        textoProgreso.outlineWidth = 0.22f;
        Boton("Volver", escenario, botonVolver, botonVolver != null ? "" : "Volver", new Vector2(120, 85), new Vector2(-850, 455), Volver);
        Boton("Reiniciar", escenario, botonMadera, "Nueva práctica", new Vector2(290, 80), new Vector2(760, 455), Reiniciar);

        puntos = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            int k = i;
            Vector2 p = posiciones != null && i < posiciones.Length ? posiciones[i] : new Vector2(0.15f + i * 0.17f, 0.5f);
            puntos[i] = Boton("Elemento_" + objetos[i], escenario, null, objetos[i], new Vector2(160, 48), new Vector2((p.x - 0.5f) * 1920, (p.y - 0.5f) * 1080), () => ElegirElemento(k));
        }
        Image panel = Imagen("PanelGuia", escenario, null, new Color(0.10f, 0.16f, 0.12f, 0.93f), new Vector2(1760, 258), new Vector2(0, -395));
        iconoActual = Imagen("SentidoActual", panel.transform, null, Color.white, new Vector2(74, 74), new Vector2(-782, 50));
        manoActual = Imagen("ManoDeApoyo", panel.transform, mano, Color.white, new Vector2(76, 76), new Vector2(780, 52));
        textoPaso = Texto("Instruccion", panel.transform, "", 34, new Vector2(1380, 63), new Vector2(0, 85));
        ayuda = Texto("Ayuda", panel.transform, "", 24, new Vector2(1400, 77), new Vector2(0, 16));
        conteo = Texto("Conteo", panel.transform, "", 21, new Vector2(490, 38), new Vector2(-570, -81));
        registrar = Boton("Registrar", panel.transform, null, "Noté una sensación", new Vector2(350, 60), new Vector2(-80, -81), RegistrarSensacion);
        omitir = Boton("Omitir", panel.transform, botonMadera, "Omitir", new Vector2(190, 60), new Vector2(235, -81), () => ResolverPaso('O'));
        siguiente = Boton("Siguiente", panel.transform, botonMadera, "Lo hice / Siguiente", new Vector2(350, 60), new Vector2(560, -81), () => ResolverPaso('C'));
        resumen = Texto("Resumen", escenario, "", 19, new Vector2(1000, 35), new Vector2(0, -523));
        AjustarEncuadre();
    }

    private void LateUpdate() { AjustarEncuadre(); }
    private void AjustarEncuadre()
    {
        if (escenario == null) return;
        RectTransform marco = (RectTransform)escenario.parent;
        escenario.localScale = Vector3.one * Mathf.Min(marco.rect.width / 1920, marco.rect.height / 1080);
    }
    private void BuscarPendiente()
    {
        indice = 0;
        while (indice < 5 && estado[indice] != '-') indice++;
        terminado = indice == 5;
    }
    private AudioClip Clip(int k) { return k == 1 ? aves : k == 2 ? aguaFuente : k == 4 ? campanillas : null; }
    private void ElegirElemento(int k)
    {
        if (terminado || (indice != 0 && indice != 2)) return;
        if (indice == 2)
        {
            AudioClip clip = Clip(k);
            if (clip == null) return;
            sonido.Stop(); sonido.clip = clip; sonido.volume = volumen; sonido.Play();
        }
        if (!elegidos[indice, k]) { elegidos[indice, k] = true; contados[indice]++; }
        ActualizarPantalla();
    }
    private void RegistrarSensacion()
    {
        if (terminado || indice == 0 || indice == 2) return;
        contados[indice] = Mathf.Min(metas[indice], contados[indice] + 1);
        ActualizarPantalla();
    }
    private void ResolverPaso(char respuesta)
    {
        if (terminado) return;
        sonido.Stop();
        estado[indice] = respuesta;
        BuscarPendiente();
        PlayerPrefs.SetString(prefijo + "sesionActual", new string(estado));
        PlayerPrefs.SetInt(prefijo + "ultimoIndice", indice);
        if (terminado)
        {
            int hechos = 0, omitidos = 0;
            foreach (char c in estado) { if (c == 'C') hechos++; if (c == 'O') omitidos++; }
            PlayerPrefs.SetInt(prefijo + "sesionesCompletadas", PlayerPrefs.GetInt(prefijo + "sesionesCompletadas", 0) + 1);
            PlayerPrefs.SetInt(prefijo + "pasosRealizadosAcumulados", PlayerPrefs.GetInt(prefijo + "pasosRealizadosAcumulados", 0) + hechos);
            PlayerPrefs.SetInt(prefijo + "pasosOmitidosAcumulados", PlayerPrefs.GetInt(prefijo + "pasosOmitidosAcumulados", 0) + omitidos);
            PlayerPrefs.SetString(prefijo + "ultimaSesionResultados", new string(estado));
            PlayerPrefs.SetString(prefijo + "ultimaFinalizacionUTC", DateTime.UtcNow.ToString("o"));
        }
        PlayerPrefs.Save();
        ActualizarPantalla();
    }
    private void ActualizarPantalla()
    {
        int hechos = 0, omitidos = 0;
        for (int i = 0; i < 5; i++)
        {
            if (estado[i] == 'C') hechos++;
            if (estado[i] == 'O') omitidos++;
            placas[i].color = !terminado && i == indice ? new Color(0.36f, 0.42f, 0.22f, 0.96f) : new Color(0.12f, 0.17f, 0.12f, 0.88f);
            etiquetasEstado[i].text = metas[i] + " · " + nombres[i] + (estado[i] == 'C' ? "\nHecho" : estado[i] == 'O' ? "\nOmitido" : "");
        }
        resumen.text = "Realizados: " + hechos + "  ·  Omitidos: " + omitidos + "  ·  Puedes omitir cualquier sentido.";
        siguiente.gameObject.SetActive(!terminado); omitir.gameObject.SetActive(!terminado);
        registrar.gameObject.SetActive(!terminado && indice != 0 && indice != 2);
        manoActual.gameObject.SetActive(!terminado && indice == 1);
        iconoActual.gameObject.SetActive(!terminado);
        for (int k = 0; k < 5; k++)
        {
            bool mostrar = !terminado && (indice == 0 || (indice == 2 && Clip(k) != null));
            puntos[k].gameObject.SetActive(mostrar);
            if (!mostrar) continue;
            string etiqueta = objetos[k];
            Vector2 posicion = posiciones != null && k < posiciones.Length ? posiciones[k] : new Vector2(0.15f + k * 0.17f, 0.5f);
            if (indice == 2)
            {
                etiqueta = k == 1 ? "Oír aves" : k == 2 ? "Oír agua" : "Oír campanillas";
                posicion = k == 1 ? new Vector2(0.33f, 0.73f) : k == 2 ? new Vector2(0.66f, 0.50f) : new Vector2(0.91f, 0.70f);
            }
            puntos[k].GetComponent<RectTransform>().anchoredPosition = new Vector2((posicion.x - 0.5f) * 1920, (posicion.y - 0.5f) * 1080);
            puntos[k].GetComponentInChildren<TextMeshProUGUI>().text = etiqueta + (elegidos[indice, k] ? " ✓" : "");
            puntos[k].GetComponent<Image>().color = elegidos[indice, k] ? new Color(0.65f, 0.77f, 0.43f) : new Color(0.92f, 0.82f, 0.60f);
        }
        if (terminado)
        {
            textoPaso.text = "Terminaste a tu manera.";
            textoProgreso.text = "Práctica finalizada";
            ayuda.text = "Puedes observar el parque, volver o iniciar una nueva práctica.";
            conteo.text = "Sin prisa.";
            return;
        }
        textoPaso.text = instrucciones[indice];
        textoProgreso.text = (indice + 1) + " / 5 · " + nombres[indice];
        iconoActual.sprite = iconos[indice];
        conteo.text = "Registrados: " + contados[indice] + " / " + metas[indice];
        if (indice == 0) ayuda.text = "Puedes elegir cinco elementos del parque o mirar tu entorno real. Observa su forma y su color.";
        if (indice == 1) ayuda.text = contados[1] < 4 ? sugerenciasTacto[contados[1]] + " Solo si resulta agradable." : "Ya registraste cuatro sensaciones. Continúa cuando quieras.";
        if (indice == 2) ayuda.text = "Escucha tu entorno real. Si hay audios asignados, también puedes activarlos en el parque. No hay sonido automático.";
        if (indice == 3) ayuda.text = "Presta atención a olores que ya estén presentes cerca de ti. La pantalla no produce aromas.";
        if (indice == 4) ayuda.text = "Observa el sabor que ya percibes en tu boca. No necesitas comer ni beber nada.";
        registrar.interactable = contados[indice] < metas[indice];
        registrar.GetComponentInChildren<TextMeshProUGUI>().text = indice == 1 ? "Noté una sensación" : indice == 3 ? "Noté un olor" : "Noté un sabor";
    }
    private void Reiniciar()
    {
        sonido.Stop(); estado = "-----".ToCharArray();
        Array.Clear(elegidos, 0, elegidos.Length); Array.Clear(contados, 0, contados.Length);
        BuscarPendiente();
        PlayerPrefs.SetString(prefijo + "sesionActual", "-----");
        PlayerPrefs.SetInt(prefijo + "ultimoIndice", 0); PlayerPrefs.Save(); ActualizarPantalla();
    }
    private void Volver() { sonido.Stop(); PlayerPrefs.Save(); MenuManager.RegresarAMinijuegos(escenaMenu); }

    private static RectTransform Nuevo(string nombre, Transform padre)
    {
        GameObject g = new GameObject(nombre, typeof(RectTransform)); g.transform.SetParent(padre, false);
        RectTransform r = g.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f); return r;
    }
    private static Image Imagen(string nombre, Transform padre, Sprite sprite, Color color, Vector2 tam, Vector2 pos)
    {
        Image img = Nuevo(nombre, padre).gameObject.AddComponent<Image>(); img.sprite = sprite; img.color = color;
        img.rectTransform.sizeDelta = tam; img.rectTransform.anchoredPosition = pos; img.preserveAspect = sprite != null; img.raycastTarget = false; return img;
    }
    private TextMeshProUGUI Texto(string nombre, Transform padre, string texto, float tamano, Vector2 tam, Vector2 pos)
    {
        TextMeshProUGUI t = Nuevo(nombre, padre).gameObject.AddComponent<TextMeshProUGUI>(); t.text = texto; t.fontSize = tamano;
        t.alignment = TextAlignmentOptions.Center; t.color = crema; t.raycastTarget = false;
        t.rectTransform.sizeDelta = tam; t.rectTransform.anchoredPosition = pos; return t;
    }
    private Button Boton(string nombre, Transform padre, Sprite sprite, string texto, Vector2 tam, Vector2 pos, Action accion)
    {
        if (sprite != null) { float s = Mathf.Min(tam.x / sprite.rect.width, tam.y / sprite.rect.height); tam = sprite.rect.size * s; }
        Image img = Imagen(nombre, padre, sprite, sprite != null ? Color.white : new Color(0.92f, 0.82f, 0.60f), tam, pos);
        img.raycastTarget = true;
        Button b = img.gameObject.AddComponent<Button>(); b.targetGraphic = img; b.onClick.AddListener(() => accion());
        if (!string.IsNullOrEmpty(texto))
        {
            TextMeshProUGUI label = Texto("Etiqueta", img.transform, texto, 24, Vector2.zero, Vector2.zero);
            label.color = new Color(0.20f, 0.14f, 0.08f); label.enableAutoSizing = true; label.fontSizeMin = 12; label.fontSizeMax = 24;
            label.rectTransform.anchorMin = new Vector2(0.12f, 0.15f); label.rectTransform.anchorMax = new Vector2(0.88f, 0.85f);
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        }
        return b;
    }
    private static void Estirar(RectTransform r, float margen = 0)
    { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.one * margen; r.offsetMax = -Vector2.one * margen; }
    private void OnDestroy()
    {
        if (sonido != null) { sonido.Stop(); Destroy(sonido); }
        if (canvasActual != null) Destroy(canvasActual);
        foreach (UnityEngine.Object o in temporales) if (o != null) Destroy(o);
    }
}
