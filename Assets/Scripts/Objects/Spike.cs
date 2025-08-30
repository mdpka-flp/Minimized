using UnityEngine;

public class Spike : MonoBehaviour
{
    public GameManager gameManager;
    private AudioSource audioSource;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.BreakPlayer(other.gameObject);
            audioSource.Play();
        }
        else if (other.CompareTag("Draggable"))
        {
            gameManager.BreakCube(other.gameObject); // куб ломается
            audioSource.Play();
        }
    }
}
