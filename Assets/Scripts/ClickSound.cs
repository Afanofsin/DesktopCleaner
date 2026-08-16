using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalClickSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Start() => audioSource = GetComponent<AudioSource>();

    void Update()
    {
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
