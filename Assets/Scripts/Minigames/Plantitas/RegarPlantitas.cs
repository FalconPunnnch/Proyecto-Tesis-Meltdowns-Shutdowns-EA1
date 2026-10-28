using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

// Unity 6. Sustituye RegarPlantitas.cs, no agregues otro constructor.
// Los sprites de plantas deben incluir la maceta y compartir lienzo y escala.
public class RegarPlantitas : MonoBehaviour
{
    [Header("Imágenes: arrastrar desde Assets")]
    public Sprite fondo;
    public Sprite plantaMarchita;
    public Sprite plantaViva;
    [Tooltip("Opcional. Si falta, el estado final usa Planta Viva.")]
    public Sprite plantaConFlor;
    public Sprite gota;
    public Sprite regadera;

    [Header("Regadera durante el riego")]
    public Vector2 tamanoRegadera = new Vector2(190, 155);
    public Vector2 desplazamientoRegadera = new Vector2(92, 42);
    [Tooltip("Opcional: botón cuadrado de flecha, sin texto adicional.")]
    public Sprite botonVolver;
    [Tooltip("Opcional: madera sin texto para Volver a empezar.")]
    public Sprite botonMadera;

    [Header("Configuración")]
    [Range(1, 7)] public int numeroPlantas = 5;
    public string escenaMenu = "Menu";
    [Range(0.3f, 5f)] public float duracionRiego = 2.4f;
    [Tooltip("Activa si prefieres que terminen verdes, sin flor.")]
    public bool terminarSinFlor;
    [Tooltip("Desactiva para evitar crecimiento y balanceo; conserva el cambio de estado.")]
    public bool animarPlantas = true;

    [Header("Composición sobre el fondo (1920 × 1080)")]
    [Range(0.08f, 0.4f)] public float alturaBaseMacetas = 0.17f;
    [Range(0.35f, 0.8f)] public float anchoFila = 0.66f;
    [Range(160f, 380f)] public float altoPlantas = 285f;
    public Color colorFranjas = new Color(0.10f, 0.13f, 0.10f);

    private GameObject canvasActual;
    private RectTransform escenario;
    private TextMeshProUGUI guia;
    private TextMeshProUGUI contador;
    private bool[] ocupadas;
    private int regadas;
    private int cantidadSesion;
    private string prefijo;
    private readonly List<UnityEngine.Object> recursos = new List<UnityEngine.Object>();
    private Sprite suplenteMarchita, suplenteViva;
    private class MedidaSprite
    {
        public Sprite recortado;
        public float anchoMaceta;
        public float centroMaceta;
    }
    private readonly Dictionary<Sprite, MedidaSprite> medidas = new Dictionary<Sprite, MedidaSprite>();

    private void Start()
    {
        string id = PlayerPrefs.GetString("Tesis.PerfilActivo", "local");
        prefijo = "Tesis." + (string.IsNullOrWhiteSpace(id) ? "local" : Uri.EscapeDataString(id)) + ".plantitas.";
        CrearEscena();
    }

    private void CrearEscena()
    {
        ConfigurarEventos();
        cantidadSesion = Mathf.Clamp(numeroPlantas, 1, 7);
        ocupadas = new bool[cantidadSesion];
        canvasActual = new GameObject("CanvasPlantitas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasActual.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasActual.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Image exterior = Imagen("Franjas", canvasActual.transform, null, colorFranjas);
        Estirar(exterior.rectTransform);
        RectTransform marco = Nuevo("Encuadre", canvasActual.transform);
        Estirar(marco);
        AspectRatioFitter ajuste = marco.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        ajuste.aspectRatio = 16f / 9f;

        // Escenario de coordenadas estables, también al redimensionar la ventana.
        escenario = Nuevo("Escenario", marco);
        escenario.sizeDelta = new Vector2(1920, 1080);
        Image imagenFondo = Imagen("Fondo", escenario, fondo, fondo != null ? Color.white : new Color(0.26f, 0.34f, 0.28f));
        Estirar(imagenFondo.rectTransform);
        imagenFondo.preserveAspect = true;
        if (fondo == null) Debug.LogWarning("Asigna Fondo en RegarPlantitas. No necesitas un objeto Fondo adicional.", this);

        float espacio = 1920f * anchoFila / cantidadSesion;
        float ancho = Mathf.Min(245f, espacio * 0.88f);
        for (int i = 0; i < cantidadSesion; i++)
        {
            float x = (i - (cantidadSesion - 1) / 2f) * espacio;
            CrearPlanta(i, new Vector2(x, -540f + 1080f * alturaBaseMacetas), ancho);
        }

        // Paneles locales: no oscurecen todo el fondo.
        Image placa = Imagen("PlacaGuia", escenario, null, new Color(0.10f, 0.14f, 0.10f, 0.78f));
        Colocar(placa.rectTransform, new Vector2(1250, 104), new Vector2(0, 362));
        guia = Texto("Guia", placa.transform, "Riega las plantas que quieras, a tu ritmo.", 32);
        Estirar(guia.rectTransform, 20);
        Image estado = Imagen("PlacaProgreso", escenario, null, new Color(0.10f, 0.14f, 0.10f, 0.78f));
        Colocar(estado.rectTransform, new Vector2(350, 54), new Vector2(0, -478));
        contador = Texto("Progreso", estado.transform, "0 / " + cantidadSesion + " plantas cuidadas", 24);
        Estirar(contador.rectTransform, 4);

        Boton("Volver", escenario, botonVolver, botonVolver == null ? "Volver" : "", new Vector2(112, 96), new Vector2(-850, 450), Regresar);
        Boton("Reiniciar", escenario, botonMadera, "Volver a empezar", new Vector2(340, 88), new Vector2(0, -45), Reiniciar);
        ActualizarEncuadre();
    }

    private void LateUpdate() { ActualizarEncuadre(); }

    private void ActualizarEncuadre()
    {
        if (escenario == null) return;
        RectTransform marco = (RectTransform)escenario.parent;
        float escala = Mathf.Min(marco.rect.width / 1920f, marco.rect.height / 1080f);
        escenario.localScale = Vector3.one * escala;
    }

    private void CrearPlanta(int indice, Vector2 baseMaceta, float ancho)
    {
        RectTransform grupo = Nuevo("Planta_" + indice, escenario);
        grupo.pivot = new Vector2(0.5f, 0);
        grupo.anchoredPosition = baseMaceta;
        grupo.sizeDelta = new Vector2(ancho, altoPlantas);

        Sprite inicial = plantaMarchita != null ? plantaMarchita : Suplente(false);
        Sprite verde = plantaViva != null ? plantaViva : Suplente(true);
        Sprite flor = !terminarSinFlor && plantaConFlor != null ? plantaConFlor : verde;
        MedidaSprite medidaInicial = MedirSprite(inicial);
        MedidaSprite medidaVerde = MedirSprite(verde);
        MedidaSprite medidaFlor = MedirSprite(flor);
        Image antes = Imagen("Marchita", grupo, medidaInicial.recortado, Color.white);
        Image intermedia = Imagen("Verde", grupo, medidaVerde.recortado, new Color(1, 1, 1, 0));
        Image despues = Imagen("ConFlor", grupo, medidaFlor.recortado, new Color(1, 1, 1, 0));
        // Conserva la escala de la marchita y usa su MACETA como referencia.
        float escalaInicial = Mathf.Min(ancho / inicial.rect.width, altoPlantas / inicial.rect.height);
        float anchoMacetaReferencia = medidaInicial.anchoMaceta * escalaInicial;
        AjustarSprite(antes, medidaInicial, escalaInicial);
        AjustarSprite(intermedia, medidaVerde, anchoMacetaReferencia / medidaVerde.anchoMaceta);
        AjustarSprite(despues, medidaFlor, anchoMacetaReferencia / medidaFlor.anchoMaceta);

        // Zona amplia y estable: no depende del tamaño de las hojas.
        Image zona = Imagen("ZonaInteractiva", grupo, null, Color.clear);
        Estirar(zona.rectTransform);
        zona.raycastTarget = true;
        Button boton = zona.gameObject.AddComponent<Button>();
        boton.targetGraphic = zona;
        boton.transition = Selectable.Transition.None;
        Navigation navegacion = boton.navigation;
        navegacion.mode = Navigation.Mode.None;
        boton.navigation = navegacion;
        boton.onClick.AddListener(() => StartCoroutine(Regar(indice, boton, grupo, antes, intermedia, despues)));
    }

    private IEnumerator Regar(int indice, Button boton, RectTransform grupo, Image antes, Image intermedia, Image despues)
    {
        if (ocupadas[indice]) yield break;
        ocupadas[indice] = true;
        boton.interactable = false;
        float duracion = Mathf.Max(0.3f, duracionRiego);

        // La regadera se muestra solo junto a la planta que recibe agua, sin mano.
        Image herramienta = null;
        Vector2 posicionRegadera = grupo.anchoredPosition +
            new Vector2(desplazamientoRegadera.x, altoPlantas + desplazamientoRegadera.y);
        if (regadera != null)
        {
            herramienta = Imagen("Regadera_" + indice, escenario, regadera, new Color(1, 1, 1, 0));
            herramienta.preserveAspect = true;
            herramienta.rectTransform.sizeDelta = tamanoRegadera;
            herramienta.rectTransform.anchoredPosition = posicionRegadera;
        }

        Image[] gotas = new Image[5];
        for (int i = 0; i < gotas.Length; i++)
        {
            // Color blanco para conservar los colores originales del sprite de la gota.
            gotas[i] = Imagen("Gota_" + i, grupo, gota, new Color(1, 1, 1, 0));
            gotas[i].rectTransform.anchorMin = gotas[i].rectTransform.anchorMax = new Vector2(0.5f, 0);
            gotas[i].rectTransform.sizeDelta = gota != null ? new Vector2(13, 22) : new Vector2(6, 14);
            gotas[i].preserveAspect = true;
        }

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float p = Mathf.Clamp01(tiempo / duracion);
            // 0-15 %: aparece y se inclina la regadera.
            // 15-50 %: marchita -> verde. 50-65 %: se aprecia el verde.
            // 65-95 %: verde -> flor. Al terminar, se retira la regadera.
            float aVerde = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.15f, 0.50f, p));
            float aFlor = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.65f, 0.95f, p));
            antes.color = new Color(1, 1, 1, 1 - aVerde);
            intermedia.color = new Color(1, 1, 1, aVerde * (1 - aFlor));
            despues.color = new Color(1, 1, 1, aFlor);

            if (animarPlantas)
            {
                grupo.localScale = Vector3.one * (1 + 0.025f * Mathf.Sin(p * Mathf.PI));
                grupo.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(p * Mathf.PI * 2) * 1.2f);
            }
            float entrada = Mathf.SmoothStep(0, 1, Mathf.Clamp01(p / 0.15f));
            float salida = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.88f, 1f, p));
            float visible = entrada * (1 - salida);
            if (herramienta != null)
            {
                herramienta.color = new Color(1, 1, 1, visible);
                herramienta.rectTransform.anchoredPosition = posicionRegadera + Vector2.up * (1 - visible) * 20;
                herramienta.rectTransform.localRotation = Quaternion.Euler(0, 0, 22 * visible);
            }
            for (int i = 0; i < gotas.Length; i++)
            {
                float fase = Mathf.Repeat(p * 3f + i * 0.2f, 1f);
                gotas[i].rectTransform.anchoredPosition = new Vector2((i - 2) * 12, Mathf.Lerp(altoPlantas + 35, altoPlantas * 0.63f, fase));
                gotas[i].color = new Color(1, 1, 1, Mathf.Sin(fase * Mathf.PI) * visible);
            }
            yield return null;
        }
        grupo.localScale = Vector3.one;
        grupo.localRotation = Quaternion.identity;
        antes.color = Color.clear;
        intermedia.color = Color.clear;
        despues.color = Color.white;
        if (herramienta != null) Destroy(herramienta.gameObject);
        foreach (Image imagen in gotas) if (imagen != null) Destroy(imagen.gameObject);
        regadas++;
        contador.text = regadas + " / " + cantidadSesion + " plantas cuidadas";
        guia.text = regadas == cantidadSesion ? "Todas recibieron agua. Puedes observarlas con calma." : "Has cuidado una planta. Continúa cuando quieras.";
        GuardarRiego();
    }

    private void GuardarRiego()
    {
        PlayerPrefs.SetInt(prefijo + "ultimaSesionRegadas", regadas);
        PlayerPrefs.SetInt(prefijo + "riegosAcumulados", PlayerPrefs.GetInt(prefijo + "riegosAcumulados", 0) + 1);
        if (regadas == cantidadSesion)
        {
            PlayerPrefs.SetInt(prefijo + "sesionesCompletadas", PlayerPrefs.GetInt(prefijo + "sesionesCompletadas", 0) + 1);
            PlayerPrefs.SetString(prefijo + "ultimaFinalizacionUTC", DateTime.UtcNow.ToString("o"));
        }
        PlayerPrefs.Save();
    }

    private void Reiniciar()
    {
        PlayerPrefs.SetInt(prefijo + "ultimaSesionRegadas", regadas);
        PlayerPrefs.Save();
        StopAllCoroutines();
        if (canvasActual != null) { canvasActual.SetActive(false); Destroy(canvasActual); }
        regadas = 0;
        CrearEscena();
    }

    private void Regresar()
    {
        PlayerPrefs.SetInt(prefijo + "ultimaSesionRegadas", regadas);
        PlayerPrefs.Save();
        MenuManager.RegresarAMinijuegos(escenaMenu);
    }

    private void ConfigurarEventos()
    {
        EventSystem eventos = FindAnyObjectByType<EventSystem>();
        if (eventos == null) eventos = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        foreach (BaseInputModule modulo in eventos.GetComponents<BaseInputModule>()) modulo.enabled = false;
        InputSystemUIInputModule ui = eventos.GetComponent<InputSystemUIInputModule>();
        if (ui == null) ui = eventos.gameObject.AddComponent<InputSystemUIInputModule>();
        ui.enabled = true;
        if (ui.actionsAsset == null) ui.AssignDefaultActions();
    }

    private static RectTransform Nuevo(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        return rect;
    }
    private static Image Imagen(string nombre, Transform padre, Sprite sprite, Color color)
    {
        Image imagen = Nuevo(nombre, padre).gameObject.AddComponent<Image>();
        imagen.sprite = sprite;
        imagen.color = color;
        imagen.raycastTarget = false;
        return imagen;
    }
    private static void AjustarSprite(Image imagen, MedidaSprite medida, float escala)
    {
        RectTransform r = imagen.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0);
        r.sizeDelta = imagen.sprite.rect.size * escala;
        // Centra la maceta, no la copa: evita desplazamientos al florecer.
        r.anchoredPosition = new Vector2((imagen.sprite.rect.width * 0.5f - medida.centroMaceta) * escala, 0);
    }

    // Analiza el alfa una sola vez por sprite. No requiere Read/Write en el importador.
    // Los sprites deben estar sin rotar ni empaquetar en un atlas.
    private MedidaSprite MedirSprite(Sprite original)
    {
        if (medidas.TryGetValue(original, out MedidaSprite existente)) return existente;
        MedidaSprite resultado = new MedidaSprite
        {
            recortado = original,
            anchoMaceta = original.rect.width,
            centroMaceta = original.rect.width * 0.5f
        };
        if (original.packed)
        {
            Debug.LogWarning("Para medir " + original.name + ", usa el PNG sin Sprite Atlas.", this);
            medidas[original] = resultado;
            return resultado;
        }
        Texture2D copia = null;
        RenderTexture temporal = null;
        RenderTexture anterior = RenderTexture.active;
        try
        {
            Texture2D textura = original.texture;
            Color32[] pixels;
            if (textura.isReadable) pixels = textura.GetPixels32();
            else
            {
                temporal = RenderTexture.GetTemporary(textura.width, textura.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(textura, temporal);
                RenderTexture.active = temporal;
                copia = new Texture2D(textura.width, textura.height, TextureFormat.RGBA32, false);
                copia.ReadPixels(new Rect(0, 0, textura.width, textura.height), 0, 0);
                copia.Apply();
                pixels = copia.GetPixels32();
            }
            Rect rect = original.rect;
            int origenX = Mathf.RoundToInt(rect.x), origenY = Mathf.RoundToInt(rect.y);
            int w = Mathf.RoundToInt(rect.width), h = Mathf.RoundToInt(rect.height);
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                if (pixels[(origenY + y) * textura.width + origenX + x].a < 40) continue;
                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
            }
            if (maxX < minX || maxY < minY) return resultado;
            int anchoVisible = maxX - minX + 1, altoVisible = maxY - minY + 1;
            int anchoBase = 1;
            float centro = anchoVisible * 0.5f;
            // Busca el ancho máximo en el tercio inferior: maceta, no hojas.
            int limite = minY + Mathf.Max(1, Mathf.RoundToInt(altoVisible * 0.32f));
            for (int y = minY; y < limite; y++)
            {
                int izquierda = maxX + 1, derecha = -1;
                for (int x = minX; x <= maxX; x++)
                    if (pixels[(origenY + y) * textura.width + origenX + x].a >= 100)
                    { izquierda = Mathf.Min(izquierda, x); derecha = Mathf.Max(derecha, x); }
                int ancho = derecha - izquierda + 1;
                if (ancho <= anchoBase) continue;
                anchoBase = ancho;
                centro = (izquierda + derecha + 1) * 0.5f - minX;
            }
            Sprite recortado = Sprite.Create(textura,
                new Rect(origenX + minX, origenY + minY, anchoVisible, altoVisible),
                new Vector2(0.5f, 0), original.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            recursos.Add(recortado);
            resultado.recortado = recortado;
            resultado.anchoMaceta = anchoBase;
            resultado.centroMaceta = centro;
        }
        catch (Exception error)
        {
            Debug.LogWarning("No se pudo medir el sprite " + original.name + ": " + error.Message, this);
        }
        finally
        {
            RenderTexture.active = anterior;
            if (temporal != null) RenderTexture.ReleaseTemporary(temporal);
            if (copia != null) Destroy(copia);
        }
        medidas[original] = resultado;
        return resultado;
    }

    private static TextMeshProUGUI Texto(string nombre, Transform padre, string contenido, float tamano)
    {
        TextMeshProUGUI t = Nuevo(nombre, padre).gameObject.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        t.fontSize = tamano;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(1, 0.96f, 0.86f);
        t.raycastTarget = false;
        return t;
    }
    private void Boton(string nombre, Transform padre, Sprite sprite, string etiqueta, Vector2 tamano, Vector2 posicion, Action accion)
    {
        if (sprite != null)
        {
            sprite = MedirSprite(sprite).recortado;
            float escala = Mathf.Min(tamano.x / sprite.rect.width, tamano.y / sprite.rect.height);
            tamano = sprite.rect.size * escala;
        }
        Image img = Imagen(nombre, padre, sprite, sprite != null ? Color.white : new Color(0.88f, 0.69f, 0.43f));
        Colocar(img.rectTransform, tamano, posicion);
        img.preserveAspect = sprite != null;
        img.raycastTarget = true;
        Button boton = img.gameObject.AddComponent<Button>();
        boton.targetGraphic = img;
        ColorBlock colores = boton.colors;
        colores.highlightedColor = new Color(1, 0.96f, 0.85f);
        colores.pressedColor = new Color(0.8f, 0.75f, 0.65f);
        boton.colors = colores;
        boton.onClick.AddListener(() => accion());
        if (string.IsNullOrEmpty(etiqueta)) return;
        TextMeshProUGUI texto = Texto("Texto", img.transform, etiqueta, 25);
        texto.color = new Color(0.22f, 0.12f, 0.06f);
        texto.enableAutoSizing = true;
        texto.fontSizeMin = 12;
        texto.fontSizeMax = 25;
        // Reserva los extremos decorados y evita que el texto sobresalga.
        texto.rectTransform.anchorMin = new Vector2(0.18f, 0.20f);
        texto.rectTransform.anchorMax = new Vector2(0.82f, 0.80f);
        texto.rectTransform.offsetMin = texto.rectTransform.offsetMax = Vector2.zero;
    }
    private static void Colocar(RectTransform r, Vector2 tamano, Vector2 posicion) { r.sizeDelta = tamano; r.anchoredPosition = posicion; }
    private static void Estirar(RectTransform r, float margen = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.one * margen; r.offsetMax = Vector2.one * -margen;
    }

    // Respaldo pixelado, solo si faltan los sprites definitivos.
    private Sprite Suplente(bool viva)
    {
        if (viva && suplenteViva != null) return suplenteViva;
        if (!viva && suplenteMarchita != null) return suplenteMarchita;
        Texture2D tex = new Texture2D(32, 48, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixeles = new Color[32 * 48];
        Color hojas = viva ? new Color(0.35f, 0.56f, 0.22f) : new Color(0.47f, 0.43f, 0.27f);
        for (int y = 0; y < 48; y++) for (int x = 0; x < 32; x++)
        {
            Color c = Color.clear;
            if (y >= 3 && y < 15 && x >= 8 && x < 24) c = new Color(0.65f, 0.32f, 0.19f);
            if (y >= 14 && y < 18 && x >= 6 && x < 26) c = new Color(0.84f, 0.52f, 0.31f);
            if (y >= 18 && y < 37 && x >= 15 && x <= 16) c = hojas;
            if ((Mathf.Abs(x - 10) + Mathf.Abs(y - 28) < 8) || (Mathf.Abs(x - 22) + Mathf.Abs(y - 34) < 8)) c = hojas;
            pixeles[y * 32 + x] = c;
        }
        tex.SetPixels(pixeles); tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 32, 48), new Vector2(0.5f, 0), 32);
        recursos.Add(sprite); recursos.Add(tex);
        if (viva) suplenteViva = sprite; else suplenteMarchita = sprite;
        return sprite;
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        if (canvasActual != null) Destroy(canvasActual);
        foreach (UnityEngine.Object recurso in recursos) if (recurso != null) Destroy(recurso);
    }
}
