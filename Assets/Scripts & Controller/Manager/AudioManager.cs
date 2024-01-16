using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    static AudioManager instance;

    // Array to store AudioSources
    AudioSource[] audioSources;

    // Array to store AudioFiles
    [SerializeField] AudioClip[] audioFiles;

    // Create a static reference to the instance
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                }
            }

            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Initialize the array of AudioSources
        audioSources = FindObjectsOfType<AudioSource>();
        Debug.Log("AudioManager: Found " + audioSources.Length + " AudioSources in the scene.");
    }

    // Play a sound by name
    public void PlaySound(string sourceName, float volume = 1f)
    {
        AudioSource audioSource = GetAudioSource(sourceName);

        if (audioSource != null)
        {
            audioSource.clip.LoadAudioData();
            audioSource.volume = volume;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound not found - " + sourceName);
        }
    }

    // Play a sound one shot by name
    public void PlaySoundOneShot(string sourceName, string clipName, float volume = 1f)
    {
        AudioSource audioSource = GetAudioSource(sourceName);
        AudioClip audioFile = GetAudioClip(clipName);

        if (audioSource != null && audioFile != null)
        {
            audioSource.volume = volume;
            audioSource.PlayOneShot(audioFile);
        }
        else
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioManager: AudioSource not found - " + sourceName);
            }
            else
            {
                Debug.LogWarning("AudioManager: AudioClip not found - " + clipName);
            }
        }
    }

    public void StopAudioSource(string sourceName)
    {
        AudioSource audioSource = GetAudioSource(sourceName);

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip.UnloadAudioData();
        }
        else
        {
            Debug.LogWarning("AudioManager: AudioSource not found - " + sourceName);
        }
    }

    // Stop all Audiosources
    public void StopAll()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
        }
    }

    // Get an AudioSource by name
    AudioSource GetAudioSource(string sourceName)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.gameObject.name == sourceName)
            {
                return audioSource;
            }
        }

        return null;
    }

    // Get an AudioClip by name
    AudioClip GetAudioClip(string clipName)
    {
        foreach (AudioClip audioClip in audioFiles)
        {
            if (audioClip.name == clipName)
            {
                audioClip.LoadAudioData();
                return audioClip;
            }
        }

        return null;
    }
}