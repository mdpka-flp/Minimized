using UnityEngine;

public class SmoothTracking : MonoBehaviour
{
    public Transform Player;
    Vector3 Target;

    public float TrackingSpeed = 1.5f;

    private void Update()
    {
        if (Player)
        {
            Vector3 currentPosition = Vector3.Lerp(transform.position, Target, TrackingSpeed * Time.deltaTime);
            transform.position = currentPosition;

            Target = new Vector3(Player.transform.position.x, Player.transform.position.y, Player.transform.position.z  -3);
        }
    }
}
