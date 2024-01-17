using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    static AudioManager instance;

    // Array to store AudioSources
    AudioSource[] audioSources;

    // Array to store AudioFiles
    List<AudioClip> audioFilesList = new List<AudioClip>();

    public GameObject environment;

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

        // Load audio files dynamically from Resources folder
        LoadAudioFiles();
    }

    void LoadAudioFiles()
    {
        // Load all audio clips from the Resources folder
        AudioClip[] loadedClips = Resources.LoadAll<AudioClip>("SFX");

        // Add loaded clips to the list
        audioFilesList.AddRange(loadedClips);
    }

    // Play a sound by name
    public bool PlayAudio(int gameObjectID, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.clip.LoadAudioData();
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            audioSource.Play();
            return true;
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound not found - " + gameObjectID);
            return false;
        }
    }

    // Play a sound one shot by name
    public void PlaySoundOneShot(int gameObjectID, string clipName, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        AudioClip audioFile = GetAudioClip(clipName);

        if (audioSource != null && audioFile != null)
        {
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            audioSource.PlayOneShot(audioFile);
            Debug.Log("AudioManager: Playing " + clipName + " on " + transform.gameObject.name);
        }
        else
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioManager: AudioSource not found - " + gameObjectID);
            }
            else
            {
                Debug.LogWarning("AudioManager: AudioClip not found - " + clipName);
            }
        }
    }

    public void SetAudioClip(int gameObjectID, string clipName, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        AudioClip audioFile = GetAudioClip(clipName);

        if (audioSource != null && audioFile != null)
        {
            if (audioSource.clip != audioFile)
            {
                audioSource.clip = audioFile;
                audioFile.LoadAudioData();
            }
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
        }
        else
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioManager: AudioSource not found - " + gameObjectID);
            }
            else
            {
                Debug.LogWarning("AudioManager: AudioClip not found - " + clipName);
            }
        }
    }

    public void StopAudio(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip.UnloadAudioData();
        }
        else
        {
            Debug.LogWarning("AudioManager: AudioSource not found - " + gameObjectID);
        }
    }

    public void FadeOut(int gameObjectID, float fadeOutDuration)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        StartCoroutine(FadeOutCo(audioSource, fadeOutDuration));
    }

    IEnumerator FadeOutCo(AudioSource audioSource, float fadeOutDuration)
    {
        float elapsedTime = 0f;
        float startVolume = audioSource.volume;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        // Ensure the volume is set to 0 at the end of the fade-out
        audioSource.volume = 0f;

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public void FadeIn(int gameObjectID, float fadeInDuration, float maxVolume = 1f)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        StartCoroutine(FadeInCo(audioSource, fadeInDuration, maxVolume));
    }

    IEnumerator FadeInCo(AudioSource audioSource, float fadeInDuration, float maxVolume = 1f)
    {
        float elapsedTime = 0f;

        audioSource.volume = 0f;
        audioSource.Play();

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, maxVolume, elapsedTime / fadeInDuration);
            yield return null;
        }

        // Ensure the volume is set to 1 at the end of the fade-in
        audioSource.volume = maxVolume;
    }

    public void StopAudioWithDelay(int gameObjectID)
    {
        StartCoroutine(StopAudioDelayed(gameObjectID));
    }

    IEnumerator StopAudioDelayed(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        audioSource.loop = false;

        yield return new WaitUntil(() => !IsPlaying(gameObjectID));
        StopAudio(gameObjectID);
    }

    // Stop all Audiosources
    public void StopAll()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
        }
    }

    public void StopAllExcept(int gameObjectID)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.gameObject.GetInstanceID() != gameObjectID)
            {
                audioSource.Stop();
            }
        }
    }

    public bool IsPlaying(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            if (audioSource.isPlaying)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            Debug.LogWarning("AudioManager: AudioSource not found - " + gameObjectID);
            return false;
        }
    }

    // Get an AudioSource by name
    AudioSource GetAudioSource(int gameObjectID)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.gameObject.GetInstanceID() == gameObjectID)
            {
                return audioSource;
            }
        }

        Debug.LogWarning("AudioManager: AudioSource not found - " + gameObjectID);
        return null;
    }

    // Get an AudioClip by name
    AudioClip GetAudioClip(string clipName)
    {
        foreach (AudioClip audioClip in audioFilesList)
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