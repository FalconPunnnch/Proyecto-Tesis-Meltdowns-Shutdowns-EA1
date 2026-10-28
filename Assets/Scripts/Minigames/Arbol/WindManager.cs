using UnityEngine;
using UnityEngine.UI;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance;

    [Header("Slider de Viento")]
    public Slider windSlider; // Arrastra aquí tu WindSlider

    [Header("Configuración de Fuerza")]
    [Tooltip("Fuerza máxima del viento cuando el slider está al tope (bájalo si sopla muy fuerte)")]
    public float maxWindForce = 3.5f; // Lo bajamos a 3.5 para que sea una brisa suave

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para calcular la fuerza actual del viento de forma suave
    public float GetWindForce()
    {
        if (windSlider == null) return 0f;
        return Mathf.Lerp(0f, maxWindForce, windSlider.value / windSlider.maxValue);
    }
}