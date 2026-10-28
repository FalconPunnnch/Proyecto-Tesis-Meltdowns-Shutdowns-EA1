using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ShapeShrinker : MonoBehaviour
{
    private bool isReducing = false;
    private bool isDragging = false;

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // --- 1. CLIC IZQUIERDO: Hacer desaparecer ---
        if (Mouse.current.leftButton.wasPressedThisFrame && !isReducing && !isDragging)
        {
            if (IsTopmostObjectAtMouse(mousePos, out GameObject topmost))
            {
                if (topmost == gameObject)
                {
                    StartCoroutine(ReduceAndDisappear());
                }
            }
        }

        // --- 2. CLIC DERECHO: Iniciar arrastre ---
        if (Mouse.current.rightButton.wasPressedThisFrame && !isReducing && !isDragging)
        {
            if (IsTopmostObjectAtMouse(mousePos, out GameObject topmost))
            {
                if (topmost == gameObject)
                {
                    isDragging = true;
                }
            }
        }

        // --- 3. MOVIMIENTO Y SOLTADO CON CLIC DERECHO ---
        if (isDragging)
        {
            // Mientras se mantenga presionado el botón derecho, la figura sigue al mouse
            if (Mouse.current.rightButton.isPressed)
            {
                transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
            }
            else
            {
                // Al soltar el botón derecho, dejamos de arrastrar
                isDragging = false;
            }
        }
    }

    // Método auxiliar para verificar si este objeto es el que está más arriba visualmente
    private bool IsTopmostObjectAtMouse(Vector2 mousePos, out GameObject topmost)
    {
        topmost = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        if (hits.Length == 0) return false;

        int highestOrder = int.MinValue;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
                int currentOrder = sr != null ? sr.sortingOrder : 0;

                if (currentOrder > highestOrder)
                {
                    highestOrder = currentOrder;
                    topmost = hit.collider.gameObject;
                }
            }
        }
        return topmost != null;
    }

    public IEnumerator ReduceAndDisappear()
    {
        isReducing = true;
        Vector3 initialScale = transform.localScale;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color initialColor = sr != null ? sr.color : Color.white;

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            if (sr != null)
            {
                Color newColor = initialColor;
                newColor.a = Mathf.Lerp(initialColor.a, 0f, t);
                sr.color = newColor;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}