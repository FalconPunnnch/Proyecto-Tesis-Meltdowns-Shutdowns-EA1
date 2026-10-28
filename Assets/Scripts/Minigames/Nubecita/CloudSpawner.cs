using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("Configuración del Prefab")]
    public GameObject cloudPrefab; 

    [Header("Tiempos de Aparición")]
    public float spawnIntervalMin = 2f; 
    public float spawnIntervalMax = 4f; 
    private float timer;
    private float nextSpawnTime;

    [Header("Posición de Aparición")]
    public float spawnXPosition = -12f; 
    public float minYPosition = -3f;    
    public float maxYPosition = 4f;    

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnCloud();
            timer = 0f;
            SetNextSpawnTime();
        }
    }

    void SetNextSpawnTime()
    {

        nextSpawnTime = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void SpawnCloud()
    {
        if (cloudPrefab == null)
        {
            Debug.LogWarning("El CloudPrefab no está asignado en el Spawner.");
            return;
        }


        float randomY = Random.Range(minYPosition, maxYPosition);
        Vector3 spawnPosition = new Vector3(spawnXPosition, randomY, 0f);


        Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);
    }
}