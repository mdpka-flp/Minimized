using UnityEngine;

public class FootParticleEmitter : MonoBehaviour
{
    [Header("References")]
    public Player player;                 // Игрок
    public ParticleSystem footParticles;  // Префаб партиклов
    public Transform footPoint;           // Одна точка ног
    public float sideOffset = 0.5f;       // Сдвиг по X

    [Header("Emission Settings")]
    public float particlesPerSecond = 20f;
    public float minSpeedToEmit = 0.05f;
    public Color fallbackColor = Color.white;

    private ParticleSystem.MainModule mainModule;
    private float accumulator;

    void Start()
    {
        if (footParticles != null)
        {
            mainModule = footParticles.main;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        }
    }

    void Update()
    {
        if (player == null || footParticles == null || footPoint == null) return;

        float speedX = player.rb.linearVelocity.x;

        // Проверяем, движется ли игрок и стоит ли на земле
        if (!player.isGrounded || Mathf.Abs(speedX) < minSpeedToEmit)
        {
            accumulator = 0f;
            return;
        }

        accumulator += particlesPerSecond * Time.deltaTime;
        while (accumulator >= 1f)
        {
            EmitParticle(speedX);
            accumulator -= 1f;
        }
    }

    void EmitParticle(float speedX)
    {
        Vector3 emitPos = footPoint.position;

        // Сдвиг точки в зависимости от направления
        float dir = Mathf.Sign(speedX);
        if (dir == 0) dir = 1;
        emitPos.x -= dir * sideOffset;

        // Берём цвет из SpriteRenderer на footPoint (или fallback)
        Color color = fallbackColor;
        SpriteRenderer sr = footPoint.GetComponent<SpriteRenderer>();
        if (sr != null)
            color = sr.color;

        // Настраиваем EmitParams
        ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams();
        emit.startColor = color;
        emit.position = emitPos;
        emit.velocity = new Vector3(-dir * Random.Range(0.5f, 1.5f), Random.Range(0.3f, 1f), 0);

        footParticles.Emit(emit, 1);
    }
}
