using UnityEngine;

public class Spike : MonoBehaviour
{
    public GameManager gameManager;

    private DeathSound deathSound;
    private Player player;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        deathSound = DeathSound.Instance;
        player = FindAnyObjectByType<Player>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && player.isDead == false)
        {
            player.isDead = true;
            gameManager.BreakPlayer(other.gameObject);
            deathSound.PlayDeathSound();
        }
        else if (other.CompareTag("Draggable"))
        {
            gameManager.BreakCube(other.gameObject); // куб ломается
            deathSound.PlayDeathSound();
        }
    }
}
