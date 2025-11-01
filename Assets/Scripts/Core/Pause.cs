using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    bool paused = false;

    public GameObject pausePanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }

        //Time.timeScale = paused ? 0f : 1f;

        if (paused)
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
        }
    }

    public void Continue()
    {
        paused = false;
    }

    public void Menu()
    {
        paused = false;
        SceneManager.LoadScene("Menu");
    }
}
