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
    private Vector2 localOffset;

    void Start()
    {
        mainCam = Camera.main;
        localOffset = Vector2.zero;
    }

    void Update()
    {
        // Ввод с правого стика
        float x = Input.GetAxis("Joystick Right Stick Horizontal");
        float y = -Input.GetAxis("Joystick Right Stick Vertical");

        // Проверка зажат ли LT для ускорения
        float lt = Input.GetAxis("Triggers");
        float currentSpeed = (lt < -0.1f) ? boostedSpeed : moveSpeed;

        // Обновляем локальный оффсет
        localOffset += new Vector2(x, y) * currentSpeed * Time.deltaTime;

        // Ограничим курсор в пределах камеры
        Vector2 worldPos = (Vector2)mainCam.transform.position + localOffset;
        Vector2 viewportPos = mainCam.WorldToViewportPoint(worldPos);
        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);
        worldPos = mainCam.ViewportToWorldPoint(viewportPos);

        // Пересчитаем локальный оффсет
        localOffset = worldPos - (Vector2)mainCam.transform.position;

        // Наведение на объекты
        Collider2D hit = Physics2D.OverlapCircle(worldPos, grabDistance, draggableMask);
        currentTarget = hit ? hit.GetComponent<Draggable>() : null;

        // Взятие/отпуск объектов
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

    void LateUpdate()
    {
        // Обновляем позицию курсора
        transform.position = (Vector2)mainCam.transform.position + localOffset;
    }
}
