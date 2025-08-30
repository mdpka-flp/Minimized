using UnityEngine;
using System.Collections.Generic;

public class InputDeviceSwitcher : MonoBehaviour
{
    [Header("Настройки курсоров")]
    public GameObject gamepadCursor;
    public bool hideSystemCursorInGamepadMode = true;

    [Header("Настройки переключения")]
    public float switchDelay = 0.5f;
    public float gamepadDeadzone = 0.2f;

    public bool IsUsingGamepad { get; private set; }

    private float lastMouseActivityTime;
    private float lastKeyboardInputTime;
    private float lastGamepadInputTime;
    private bool initialSwitchDone = false;

    public event System.Action<bool> OnInputModeChanged;

    private List<KeyCode> monitoredKeys = new List<KeyCode>()
    {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
        KeyCode.Space, KeyCode.LeftShift, KeyCode.E, KeyCode.Q
    };

    void Start()
    {
        IsUsingGamepad = IsGamepadConnected();
        lastMouseActivityTime = Time.unscaledTime;
        lastKeyboardInputTime = Time.unscaledTime;
        lastGamepadInputTime = IsUsingGamepad ? Time.unscaledTime : -1000f;
        UpdateCursorState();
    }

    void Update()
    {
        UpdateInputTimes();
        CheckForSwitch();
    }

    void UpdateInputTimes()
    {
        if (IsMouseActive()) lastMouseActivityTime = Time.unscaledTime;
        if (IsKeyboardInput()) lastKeyboardInputTime = Time.unscaledTime;
        if (IsGamepadInput()) lastGamepadInputTime = Time.unscaledTime;
    }

    void CheckForSwitch()
    {
        bool mouseKeyboardActive = Time.unscaledTime - lastMouseActivityTime < switchDelay ||
                                 Time.unscaledTime - lastKeyboardInputTime < switchDelay;

        bool gamepadActive = Time.unscaledTime - lastGamepadInputTime < switchDelay;

        if (mouseKeyboardActive && !gamepadActive && IsUsingGamepad)
        {
            SetInputMode(false);
        }
        else if (gamepadActive && !mouseKeyboardActive && !IsUsingGamepad && IsGamepadConnected())
        {
            SetInputMode(true);
        }
    }

    void UpdateCursorState()
    {
        if (gamepadCursor != null)
        {
            gamepadCursor.SetActive(IsUsingGamepad);
        }

        Cursor.visible = !IsUsingGamepad || !hideSystemCursorInGamepadMode;
        Cursor.lockState = IsUsingGamepad ? CursorLockMode.Locked : CursorLockMode.None;
    }

    bool IsMouseActive()
    {
        // Проверяем движение мыши
        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
            Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
            return true;

        // Проверяем нажатие кнопок мыши (левая, правая, средняя)
        for (int i = 0; i < 3; i++)
        {
            if (Input.GetMouseButton(i))
                return true;
        }

        return false;
    }

    bool IsKeyboardInput()
    {
        foreach (KeyCode key in monitoredKeys)
        {
            if (Input.GetKey(key)) return true;
        }
        return false;
    }

    bool IsGamepadInput()
    {
        // Проверка кнопок
        for (int i = 0; i <= 19; i++)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + i)))
                return true;
        }

        // Проверка осей с deadzone
        Vector2 leftStick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 rightStick = new Vector2(Input.GetAxis("Joystick Right Stick Horizontal"),
                                        Input.GetAxis("Joystick Right Stick Vertical"));

        return leftStick.magnitude > gamepadDeadzone ||
               rightStick.magnitude > gamepadDeadzone ||
               Mathf.Abs(Input.GetAxis("Triggers")) > gamepadDeadzone;
    }

    bool IsGamepadConnected()
    {
        string[] names = Input.GetJoystickNames();
        foreach (var name in names)
        {
            if (!string.IsNullOrEmpty(name))
                return true;
        }
        return false;
    }

    void SetInputMode(bool useGamepad)
    {
        if (IsUsingGamepad != useGamepad)
        {
            IsUsingGamepad = useGamepad;
            UpdateCursorState();
            OnInputModeChanged?.Invoke(useGamepad);
        }
    }
}