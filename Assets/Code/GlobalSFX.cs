using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlobalSFX : MonoBehaviour
{
    private AudioSource audioSource;
    public static GlobalSFX Instance;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }


    public void Play(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
