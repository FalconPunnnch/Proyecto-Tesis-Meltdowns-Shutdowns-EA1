using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StarCollector : MonoBehaviour
{
    [SerializeField]
    private ConstellationManager constellationManager;

    private void Start()
    {
        if (constellationManager == null)
        {
            constellationManager =
                FindAnyObjectByType<ConstellationManager>();
        }

        if (constellationManager == null)
        {
            Debug.LogError(
                "No se encontró ConstellationManager en la escena.",
                this
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IntentarRecoger(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // También detecta un destello si la estrella empieza encima.
        IntentarRecoger(collision);
    }

    private void IntentarRecoger(Collider2D collision)
    {
        if (constellationManager == null)
            return;

        constellationManager.RegistrarEstrellaEncendida(collision);
    }
}