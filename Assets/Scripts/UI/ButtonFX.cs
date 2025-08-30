using UnityEngine;

public class ButtonFX : MonoBehaviour
{
    public AudioSource Fx;
    public AudioClip Hovered;
    public AudioClip Clicked;

    public void HoverSound()
    {
        Fx.PlayOneShot(Hovered);
    }

    public void ClickSound()
    {
        Fx.PlayOneShot(Clicked);
    }
}
