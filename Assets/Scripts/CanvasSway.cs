using UnityEngine;

public class CanvasSway : MonoBehaviour
{
    public RectTransform canvasTransform; // The RectTransform of the Canvas
    public float swayAmount = 10f; // Maximum sway amount
    public float swaySpeed = 5f; // Speed of sway
    private Vector2 initialPosition;
    private Vector2 lastMousePosition;

    void Start()
    {
        if (canvasTransform == null)
        {
            canvasTransform = GetComponent<RectTransform>();
        }
        initialPosition = canvasTransform.anchoredPosition;
        lastMousePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the center of the screen
        Cursor.visible = false; // Hides the cursor
    }

    void Update()
    {
        // Get the mouse movement delta
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Calculate sway offset based on mouse movement
        Vector2 swayOffset = mouseDelta * swayAmount;

        // Apply sway to the canvas
        canvasTransform.anchoredPosition = initialPosition + swayOffset;

        // Smoothly interpolate the position for a smoother sway effect
        canvasTransform.anchoredPosition = Vector2.Lerp(canvasTransform.anchoredPosition, initialPosition + swayOffset, Time.deltaTime * swaySpeed);
    }
}
