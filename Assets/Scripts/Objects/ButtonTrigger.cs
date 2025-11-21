using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class ButtonTrigger : MonoBehaviour
{
    [Header("Plunger Movement")]
    public Transform plunger;
    public float pressDepth = 0.1f;
    public float pressSpeed = 5f;

    [Header("Activation Settings")]
    public LayerMask activatorLayers;
    [Tooltip("����������� � ������������ ����� ������� ��� ��������� (0 = ����� �����)")]
    public float minMass = 0f;
    public float maxMass = 10;
    [Tooltip("���� �������� ��� ��������� (����� = ���)")]
    public List<string> activatorTags = new List<string>();

    [Header("Color Requirements")]
    public List<Draggable.ObjColor> allowedColors = new List<Draggable.ObjColor>();

    [Header("Is button broken?")]
    public bool Broken = false;
    [Header("Visual Effects")]
    public ParticleSystem sparks; // drag your ParticleSystem prefab here

    [Header("Events")]
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    [Header("Text Label")]
    public TextMeshProUGUI Label;

    private HashSet<Collider2D> currentColliders = new HashSet<Collider2D>();
    private Vector3 initialPlungerPos;
    private bool isPressed = false;
    private float lastMassCheckTime;
    private const float massCheckInterval = 0.1f;

    private SpriteRenderer plungerRenderer;
    private Color originalPlungerColor;

    private void Start()
    {
        if (plunger != null)
        {
            initialPlungerPos = plunger.localPosition;
            plungerRenderer = plunger.GetComponent<SpriteRenderer>();
            if (plungerRenderer != null)
                originalPlungerColor = plungerRenderer.color;
        }

        UpdateMassLabel();
    }

    private void Update()
    {
        if (plunger != null)
        {
            Vector3 targetPos = initialPlungerPos + (isPressed ? Vector3.down * pressDepth : Vector3.zero);
            plunger.localPosition = Vector3.Lerp(plunger.localPosition, targetPos, Time.deltaTime * pressSpeed);
        }

        if (Time.time - lastMassCheckTime > massCheckInterval)
        {
            CheckMassRequirements();
            lastMassCheckTime = Time.time;
        }

        ApplyColorToLabel();
        UpdateSparks();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsActivator(other) && Broken == false)
        {
            currentColliders.Add(other);
            CheckActivationState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentColliders.Contains(other))
        {
            currentColliders.Remove(other);
            CheckActivationState();
        }
    }

    private bool IsActivator(Collider2D col)
    {
        if (activatorLayers != 0 && ((1 << col.gameObject.layer) & activatorLayers) == 0)
            return false;

        if (activatorTags.Count > 0 && !activatorTags.Contains(col.tag))
            return false;

        // ����� �������� �� �����
        Draggable draggable = col.GetComponent<Draggable>();
        if (draggable != null && allowedColors.Count > 0 && !allowedColors.Contains(draggable.itsColor))
            return false;

        return true;
    }

    private bool MeetsMassRequirements(Collider2D col)
    {
        Rigidbody2D rb = col.attachedRigidbody;
        if (rb == null)
            return false;

        bool meetsMin = (minMass <= 0) || (rb.mass >= minMass);
        bool meetsMax = (maxMass <= 0) || (rb.mass <= maxMass);

        return meetsMin && meetsMax;
    }

    private void CheckMassRequirements()
    {
        currentColliders.RemoveWhere(col => col == null || !col.gameObject.activeInHierarchy);
        CheckActivationState();
    }

    private void CheckActivationState()
    {
        bool shouldBePressed = false;

        foreach (var col in currentColliders)
        {
            if (col == null) continue;
            if (!IsActivator(col)) continue;

            Rigidbody2D rb = col.attachedRigidbody;
            if (rb == null) continue;

            if (rb.mass > maxMass && maxMass > 0)
            {
                if (!Broken)
                {
                    Broken = true;
                    Release();
                    SetPlungerColor(new Color(0.5f, 0.5f, 0.5f));
                }
                return;
            }

            if (MeetsMassRequirements(col))
            {
                shouldBePressed = true;
            }
        }

        if (!Broken)
        {
            SetPlungerColor(originalPlungerColor);

            if (shouldBePressed && !isPressed)
            {
                Press();
            }
            else if (!shouldBePressed && isPressed)
            {
                Release();
            }
        }
    }

    private void Press()
    {
        isPressed = true;
        onPressed.Invoke();
    }

    private void Release()
    {
        isPressed = false;
        onReleased.Invoke();
    }

    private void SetPlungerColor(Color color)
    {
        if (plungerRenderer != null)
            plungerRenderer.color = color;
    }

    private void UpdateMassLabel()
    {
        if (Label == null)
            return;

        string minText = (minMass <= 0) ? "any" : minMass.ToString("0.##");
        string maxText = (maxMass <= 0) ? "any" : maxMass.ToString("0.##");

        Label.text = $"min: {minText}, max: {maxText}";
    }

    private void ApplyColorToLabel()
    {
        if (Label == null)
            return;

        Label.enableVertexGradient = false;
        Label.color = Color.white;

        if (allowedColors.Count == 1)
        {
            Label.color = GetColorFromObjColor(allowedColors[0]);
        }
        else if (allowedColors.Count == 2)
        {
            VertexGradient gradient = new VertexGradient(
                GetColorFromObjColor(allowedColors[0]),
                GetColorFromObjColor(allowedColors[0]),
                GetColorFromObjColor(allowedColors[1]),
                GetColorFromObjColor(allowedColors[1])
            );

            Label.enableVertexGradient = true;
            Label.colorGradient = gradient;
        }
        else
        {
            Label.color = Color.white;
        }
    }

    private Color GetColorFromObjColor(Draggable.ObjColor objColor)
    {
        switch (objColor)
        {
            case Draggable.ObjColor.Red:
                return new Color(1f, 0.53f, 0.53f);
            case Draggable.ObjColor.Green:
                return new Color(0.53f, 1f, 0.53f);
            case Draggable.ObjColor.Blue:
                return new Color(0.53f, 0.53f, 1f);
            default:
                return Color.white;
        }
    }

    private void UpdateSparks()
    {
        if (sparks == null) return;

        if (Broken)
        {
            if (!sparks.isPlaying)
                sparks.Play();
        }
        else
        {
            if (sparks.isPlaying)
                sparks.Stop();
        }
    }

    private void OnValidate()
    {
        UpdateMassLabel();
        ApplyColorToLabel();
    }
}
