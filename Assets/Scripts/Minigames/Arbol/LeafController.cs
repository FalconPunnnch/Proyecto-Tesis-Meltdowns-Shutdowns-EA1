using UnityEngine;

public class LeafController : MonoBehaviour
{
    private Rigidbody2D rb;
    private float randomSeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        randomSeed = Random.Range(0f, 100f);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        float currentWindForce = 0f;

        // Buscamos el WindManager y le pedimos el valor
        if (WindManager.Instance != null)
        {
            currentWindForce = WindManager.Instance.GetWindForce();

            // <-- ESTO IMPRIMIRÁ EN TU CONSOLA EL VALOR DEL VIENTO EN TIEMPO REAL -->
            Debug.Log("Fuerza de viento actual: " + currentWindForce);
        }
        else
        {
            Debug.LogWarning("¡Atención! No se encuentra el WindManager en la escena.");
        }

        float verticalVelocity = rb.linearVelocity.y;
        float sway = Mathf.Sin(Time.time * 2.5f + randomSeed) * 0.5f;
        float targetXVelocity = sway + currentWindForce;

        rb.linearVelocity = new Vector2(targetXVelocity, verticalVelocity);
    }
}