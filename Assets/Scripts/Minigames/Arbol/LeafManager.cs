using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeafManager : MonoBehaviour
{
    [Header("Prefab y Referencias")]
    public GameObject leafPrefab;

    [Header("Sliders de UI")]
    public Slider densitySlider; // Slider horizontal de densidad

    [Header("Control de Frecuencia de Hojas")]
    public float maxSpawnInterval = 3.0f;
    public float minSpawnInterval = 0.8f;

    [Header("Botón de Pausa")]
    public Button botonPararHojas; // Arrastra aquí tu botón de pausa

    private float spawnTimer = 0f;
    private bool estaPausado = false;

    void Start()
    {
        // Si el slider no está asignado, intenta buscarlo en la escena automáticamente
        if (densitySlider == null)
        {
            densitySlider = GameObject.Find("DensitySlider")?.GetComponent<Slider>();
        }

        // Configurar el evento del botón para pausar/reanudar
        if (botonPararHojas != null)
        {
            botonPararHojas.onClick.RemoveAllListeners();
            botonPararHojas.onClick.AddListener(AlternarCaidaHojas);
        }
    }

    void Update()
    {
        // Si no está pausado, genera hojas infinitamente
        if (!estaPausado)
        {
            HandleSpawning();
        }
    }

    void HandleSpawning()
    {
        float spawnRate = 1f; // Valor por defecto para asegurar que caigan si el slider está oculto o en 0

        if (densitySlider != null)
        {
            spawnRate = densitySlider.value;
            // Si el slider está completamente en cero, no genera hojas
            if (spawnRate <= 0.05f) return;
        }

        spawnTimer += Time.deltaTime;

        float maxVal = (densitySlider != null) ? densitySlider.maxValue : 1f;
        float currentInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, spawnRate / maxVal);

        if (spawnTimer >= currentInterval)
        {
            SpawnLeaf();
            spawnTimer = 0f;
        }
    }

    void SpawnLeaf()
    {
        if (leafPrefab == null) return;

        float leftBoundary = -8f;
        float rightBoundary = 8f;
        float spawnY = 6f;

        if (Camera.main != null)
        {
            leftBoundary = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).x + 1f;
            rightBoundary = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 0f)).x - 1f;
            spawnY = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y + 1f;
        }

        float randomX = Random.Range(leftBoundary, rightBoundary);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        Instantiate(leafPrefab, spawnPos, Quaternion.identity);
    }

    // Método que se ejecuta al presionar el botón
    public void AlternarCaidaHojas()
    {
        estaPausado = !estaPausado;

        // Cambiar el texto del botón dinámicamente (compatible con TextMeshPro o Text normal)
        TextMeshProUGUI tmpText = botonPararHojas.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = estaPausado ? "Reanudar hojas" : "Parar de ver caer las hojas";
        }
        else
        {
            Text normalText = botonPararHojas.GetComponentInChildren<Text>();
            if (normalText != null)
            {
                normalText.text = estaPausado ? "Reanudar hojas" : "Parar de ver caer las hojas";
            }
        }
    }
}