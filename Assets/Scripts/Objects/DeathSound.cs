using UnityEngine;

public class DeathSound : MonoBehaviour
{
    public static DeathSound Instance; // доступ из других скриптов

    public AudioSource deathSource;
    public AudioClip deathClip;

    [Range(0.5f, 2f)] public float minPitch = 0.9f;
    [Range(0.5f, 2f)] public float maxPitch = 1.1f;

    private void Awake()
    {
        // Убедимся, что есть только один
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayDeathSound()
    {
        if (deathSource == null || deathClip == null)
            return;

        deathSource.pitch = Random.Range(minPitch, maxPitch);
        deathSource.PlayOneShot(deathClip);
    }
}
