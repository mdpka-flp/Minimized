using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ShatterManager shatterManager;

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BreakCube(GameObject cube)
    {
        Color cubeColor = cube.GetComponent<SpriteRenderer>().color;
        shatterManager.BreakCube(cube.transform.position, cubeColor);
        Destroy(cube);
    }

    public void BreakPlayer(GameObject player)
    {
        shatterManager.BreakPlayer(player.transform.position);
        Destroy(player);
    }
}
