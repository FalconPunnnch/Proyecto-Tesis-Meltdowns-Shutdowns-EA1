using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GloboController : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public GameObject globoVisual;
    public TextMeshProUGUI textoContador;
    public TextMeshProUGUI textoInstruccion;
    public Button botonRespiracion;

    [Header("Configuración Técnica 4-7-8")]
    public int respiracionesTotales = 5;
    public float tiempoInhalacion = 4f;   // 4 segundos
    public float tiempoRetencion = 7f;    // 7 segundos
    public float tiempoExhalacion = 8f;   // 8 segundos
    
    [Tooltip("Cuánto crecerá el globo al final. 2f significa el doble de su tamaño original.")]
    public float multiplicadorCrecimiento = 2f; 

    private int respiracionesActuales = 0;
    private Vector3 escalaInicial;
    private Vector3 escalaFinal;

    void Awake()
    {
        // Forzamos los valores exactos para evitar que Unity mantenga caché previa en el Inspector
        tiempoInhalacion = 4f;
        tiempoRetencion = 7f;
        tiempoExhalacion = 8f;
    }

    void Start()
    {
        if (globoVisual == null)
        {
            Debug.LogError("¡Falta asignar el objeto Globo Visual en el Inspector de Unity!");
            return;
        }

        if (botonRespiracion != null)
        {
            botonRespiracion.interactable = true;
            Image imgBoton = botonRespiracion.GetComponent<Image>();
            if (imgBoton != null) imgBoton.raycastTarget = true;
        }

        escalaInicial = globoVisual.transform.localScale;
        escalaFinal = escalaInicial * multiplicadorCrecimiento; 

        respiracionesActuales = 0;
        ActualizarUI();

        textoInstruccion.text = "Simulemos que inflas un globo.\nPuedes empezar cuando desees.";

        botonRespiracion.onClick.RemoveAllListeners();
        botonRespiracion.onClick.AddListener(() => StartCoroutine(CicloRespiracion478()));
    }

    IEnumerator CicloRespiracion478()
    {
        botonRespiracion.interactable = false;

        // 1. INHALA (4 segundos exactos)
        for (int i = (int)tiempoInhalacion; i > 0; i--)
        {
            textoInstruccion.text = $"Inhala... {i}s";
            yield return new WaitForSeconds(1f);
        }

        // 2. MANTÉN / RETENCIÓN (7 segundos)
        for (int i = (int)tiempoRetencion; i > 0; i--)
        {
            textoInstruccion.text = $"Mantén el aire... {i}s";
            yield return new WaitForSeconds(1f);
        }

        // 3. EXHALA (8 segundos)
        for (int i = (int)tiempoExhalacion; i > 0; i--)
        {
            textoInstruccion.text = $"Exhala lentamente... {i}s";
            yield return new WaitForSeconds(1f);
        }

        respiracionesActuales++;
        float progreso = (float)respiracionesActuales / respiracionesTotales;
        
        // El globo crece de forma progresiva con cada ciclo completado
        globoVisual.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progreso);

        ActualizarUI();

        if (respiracionesActuales >= respiracionesTotales)
        {
            CompletarMinijuego();
        }
        else
        {
            botonRespiracion.interactable = true;
            textoInstruccion.text = "Haz click para el siguiente ciclo.";
        }
    }

    void ActualizarUI()
    {
        if (textoContador != null)
        {
            textoContador.text = respiracionesActuales + " / " + respiracionesTotales;
        }
    }

    void CompletarMinijuego()
    {
        textoInstruccion.text = "¡Excelente! Has completado el ejercicio 4-7-8.";
        if (botonRespiracion != null)
        {
            botonRespiracion.gameObject.SetActive(false); 
        }
    }
}