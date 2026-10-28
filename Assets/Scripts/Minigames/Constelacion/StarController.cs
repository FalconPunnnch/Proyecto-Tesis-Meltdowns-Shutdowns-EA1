using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class StarController : MonoBehaviour
{
    [Header("Movimiento")]
    [Min(0.1f)]
    public float moveSpeed = 6f;

    public bool permitirMouse = true;

    [Header("Referencias")]
    public Camera camaraJuego;
    public ConstellationManager constellationManager;

    private Rigidbody2D rb;
    private Vector2 direccion;
    private Vector2 destinoMouse;
    private bool moverConMouse;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        if (camaraJuego == null)
            camaraJuego = Camera.main;

        if (constellationManager == null)
        {
            constellationManager =
                FindAnyObjectByType<ConstellationManager>();
        }
    }

    private void Update()
    {
        direccion = Vector2.zero;
        moverConMouse = false;

        if (constellationManager != null &&
            constellationManager.Completada)
            return;

        Keyboard teclado = Keyboard.current;

        if (teclado != null)
        {
            if (teclado.dKey.isPressed ||
                teclado.rightArrowKey.isPressed)
                direccion.x++;

            if (teclado.aKey.isPressed ||
                teclado.leftArrowKey.isPressed)
                direccion.x--;

            if (teclado.wKey.isPressed ||
                teclado.upArrowKey.isPressed)
                direccion.y++;

            if (teclado.sKey.isPressed ||
                teclado.downArrowKey.isPressed)
                direccion.y--;
        }

        direccion = direccion.normalized;

        // El teclado tiene prioridad.
        if (direccion.sqrMagnitude > 0f)
            return;

        if (!permitirMouse ||
            Mouse.current == null ||
            camaraJuego == null ||
            !Mouse.current.leftButton.isPressed)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 pantalla = Mouse.current.position.ReadValue();

        // Proyecta el mouse sobre el plano donde está la estrella.
        Ray rayo = camaraJuego.ScreenPointToRay(
            new Vector3(pantalla.x, pantalla.y, 0f)
        );

        Plane plano = new Plane(
            Vector3.forward,
            new Vector3(0f, 0f, transform.position.z)
        );

        if (plano.Raycast(rayo, out float distancia))
        {
            Vector3 punto = rayo.GetPoint(distancia);
            destinoMouse = new Vector2(punto.x, punto.y);
            moverConMouse = true;
        }
    }

    private void FixedUpdate()
    {
        if (constellationManager != null &&
            constellationManager.Completada)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (direccion.sqrMagnitude > 0f)
        {
            rb.linearVelocity = direccion * moveSpeed;
        }
        else if (moverConMouse)
        {
            Vector2 diferencia = destinoMouse - rb.position;

            // Reduce la velocidad cerca del destino para no pasarse.
            rb.linearVelocity = Vector2.ClampMagnitude(
                diferencia / Time.fixedDeltaTime,
                moveSpeed
            );
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}