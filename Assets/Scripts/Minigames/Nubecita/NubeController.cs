using UnityEngine;

public class NubeController : MonoBehaviour
{
    [Header("Movimiento")]
    public float baseSpeed = 2f; 
    private float currentSpeed;
    private float boostMultiplier = 2.5f;
    private bool isBoosted = false;
    private float boostTimer = 0f;

    private float screenRightLimit = 20f; 

    void Start()
    {
        currentSpeed = baseSpeed;

        if (Camera.main != null)
        {
            screenRightLimit = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x + 4f;
        }
    }

    void Update()
    {
        transform.position += new Vector3(currentSpeed * Time.deltaTime, 0f, 0f);
        if (isBoosted)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0)
            {
                currentSpeed = baseSpeed;
                isBoosted = false;
            }
        }

        if (transform.position.x > screenRightLimit)
        {
            Destroy(gameObject);
        }
    }

    void OnMouseDown()
    {
        AccelerateCloud();
    }

    public void AccelerateCloud()
    {
        currentSpeed = baseSpeed * boostMultiplier;
        isBoosted = true;
        boostTimer = 1.5f; 
    }
}