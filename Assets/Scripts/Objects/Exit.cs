using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Draggable;

public class Exit : MonoBehaviour
{
    public enum TeleportTypes { ToScene, ToPoint }

    [Header("Exit Colors")]
    public Color openedColor = new Color(0.7012957f, 1f, 0.6556604f);
    public Color closedColor = new Color(0.7012957f, 1f, 0.6556604f);

    [Header("Exit Settings")]
    public TeleportTypes TeleportType = TeleportTypes.ToPoint;
    public Transform teleportTarget;
    public string SceneName = "Lvl2";
    public bool Active = false;

    void Update()
    {
        if (Active)
        {
            gameObject.GetComponent<Renderer>().material.color = openedColor;
        }
        else if (!Active)
        {
            gameObject.GetComponent<Renderer>().material.color = closedColor;
        }
    }

    public void OpenPortal()
    {
        Active = true;
    }

    public void ClosePortal()
    {
        Active = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Active && other.CompareTag("Player"))
        {
            if (TeleportType == TeleportTypes.ToPoint)
            {
                other.transform.position = teleportTarget.position;
            }
            else if (TeleportType == TeleportTypes.ToScene)
            {
                SceneManager.LoadScene(SceneName);
            }
        }
    }
}
