using UnityEngine;

public class GamepadCursor : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float boostedSpeed = 25f;
    public LayerMask draggableMask;
    public float grabDistance = 0.5f;

    private Draggable currentTarget;
    private Draggable heldObject;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // Получаем ввод с правого стика
        float x = Input.GetAxis("Joystick Right Stick Horizontal");
        float y = -Input.GetAxis("Joystick Right Stick Vertical");

        // Проверка зажат ли LT (Trigger ось < 0 — это левый триггер, обычно на оси 3 или 9)
        float lt = Input.GetAxis("Triggers"); // если ты уже использовал её для спринта

        float currentSpeed = (lt < -0.1f) ? boostedSpeed : moveSpeed;

        Vector3 movement = new Vector3(x, y, 0f) * currentSpeed * Time.deltaTime;
        transform.position += movement;

        // Ограничим курсор рамками экрана камеры
        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);
        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);
        transform.position = mainCam.ViewportToWorldPoint(viewPos);

        // Наведение
        Collider2D hit = Physics2D.OverlapCircle(transform.position, grabDistance, draggableMask);

        currentTarget = hit ? hit.GetComponent<Draggable>() : null;

        // Взять / отпустить
        if (Input.GetKeyDown(KeyCode.Joystick1Button2)) // X кнопка
        {
            if (heldObject == null && currentTarget != null)
            {
                heldObject = currentTarget;
                heldObject?.Grab(transform);
            }
            else if (heldObject != null)
            {
                heldObject?.Release();
                heldObject = null;
            }
        }
    }
}
