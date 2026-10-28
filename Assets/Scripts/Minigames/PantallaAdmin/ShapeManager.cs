using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    [Header("Asigna tus Prefabs aquí")]
    public GameObject prefabCirculo;
    public GameObject prefabCuadrado;
    public GameObject prefabTriangulo;

    // Contador global para que cada figura nueva tenga una capa superior a la anterior
    private int currentSortingOrder = 0;

    public void AddCirculo()
    {
        SpawnShape(prefabCirculo);
    }

    public void AddCuadrado()
    {
        SpawnShape(prefabCuadrado);
    }

    public void AddTriangulo()
    {
        SpawnShape(prefabTriangulo);
    }

    public void AddAleatorio()
    {
        GameObject[] prefabsDisponibles = { prefabCirculo, prefabCuadrado, prefabTriangulo };
        int randomIndex = Random.Range(0, prefabsDisponibles.Length);
        SpawnShape(prefabsDisponibles[randomIndex]);
    }

    private void SpawnShape(GameObject shapePrefab)
    {
        if (shapePrefab == null) return;

        Vector3 randomPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0f);
        GameObject nuevaFigura = Instantiate(shapePrefab, randomPosition, Quaternion.identity);

        // Incrementamos y asignamos el sortingOrder para que la nueva esté siempre arriba
        SpriteRenderer sr = nuevaFigura.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            currentSortingOrder++;
            sr.sortingOrder = currentSortingOrder;
        }
    }
}