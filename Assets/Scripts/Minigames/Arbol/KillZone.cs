using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el objeto que toca la zona es una hoja, se destruye al instante
        if (collision.gameObject.name.Contains("Hojita") || collision.gameObject.CompareTag("Hoja"))
        {
            Destroy(collision.gameObject);
        }
    }
}