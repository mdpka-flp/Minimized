using UnityEngine;

public class Pause : MonoBehaviour
{
    bool paused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }

        Time.timeScale = paused ? 0f : 1f;
    }
}
