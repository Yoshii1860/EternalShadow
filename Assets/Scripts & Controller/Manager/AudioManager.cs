using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    static AudioManager instance;

    // Array to store AudioSources
    List<AudioSource> audioSourcesList = new List<AudioSource>();

    // Array to store AudioFiles
    List<AudioClip> audioFilesList = new List<AudioClip>();

    public GameObject environmentObject;
    public GameObject playerSpeakerObject;
    public int environment;
    public int playerSpeaker;
    public string currentEnvironment;

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

        // Initialize the list of AudioSources
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            this.audioSourcesList.Add(audioSource);
        }
        Debug.Log("AudioManager: Found " + audioSources.Length + " AudioSources in the scene.");
        // Load audio files dynamically from Resources folder
        LoadAudioFiles();

        if (environmentObject == null) environmentObject = transform.gameObject.GetComponentInChildren<AudioSource>().gameObject;
        if (playerSpeakerObject == null) playerSpeakerObject = GameManager.Instance.player.GetComponentInChildren<AudioSource>().gameObject;
        environment = environmentObject.GetInstanceID();
        playerSpeaker = playerSpeakerObject.GetInstanceID();
    }

    void Start()
    {
        // Set the environment audio volume
    }

    void LoadAudioFiles()
    {
        // Load all audio clips from the Resources folder
        AudioClip[] loadedClips = Resources.LoadAll<AudioClip>("SFX");

        // Add loaded clips to the list
        audioFilesList.AddRange(loadedClips);
    }

    public void LoadAudioSources()
    {
        // Initialize the array of AudioSources
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            if (!this.audioSourcesList.Contains(audioSource))
            {
                this.audioSourcesList.Add(audioSource);
            }
        }
        Debug.Log("AudioManager: Found " + audioSources.Length + " AudioSources in the scene.");
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
            Debug.Log("AudioManager: Playing " + audioSource.clip.name + " on " + audioSource.gameObject.name);
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
        audioFile.LoadAudioData();

        if (audioSource != null && audioFile != null)
        {
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            if (audioFile.isReadyToPlay) audioSource.PlayOneShot(audioFile);
            else 
            {
                Debug.LogWarning("AudioManager: AudioClip not ready to play - " + clipName);
                return;
            }
            Debug.Log("AudioManager: Playing " + audioFile.name + " on " + audioSource.gameObject.name);
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

    public void FadeIn(int gameObjectID, float fadeInDuration, float maxVolume = 1f, float startVolume = 0f)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        StartCoroutine(FadeInCo(audioSource, fadeInDuration, maxVolume, startVolume));
    }

    IEnumerator FadeInCo(AudioSource audioSource, float fadeInDuration, float maxVolume = 1f, float startVolume = 0f)
    {
        float elapsedTime = 0f;

        audioSource.volume = startVolume;
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

    public void PlayAudioWithDelay(int gameObjectID, float delay)
    {
        StartCoroutine(PlayAudioDelayed(gameObjectID, delay));
    }

    IEnumerator PlayAudioDelayed(int gameObjectID, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioSource audioSource = GetAudioSource(gameObjectID);
        audioSource.Play();
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
        foreach (AudioSource audioSource in audioSourcesList)
        {
            audioSource.Stop();
        }
        Debug.Log("AudioManager: Stopping all AudioSources.");
    }

    public void StopAllExcept(int gameObjectID)
    {
        foreach (AudioSource audioSource in audioSourcesList)
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

    public void SetAudioVolume(int gameObjectID, float volume)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            // gradually set audio volume
            StartCoroutine(SetAudioVolumeCo(audioSource, 0.5f, volume));
        }
        else
        {
            Debug.LogWarning("AudioManager: AudioSource not found - " + gameObjectID);
        }
    }

    IEnumerator SetAudioVolumeCo(AudioSource audioSource, float duration, float endVolume)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, elapsedTime / duration);
            yield return null;
        }

        // Ensure the volume is set to 1 at the end of the fade-in
        audioSource.volume = endVolume;
    }

    public string GetAudioClip(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null && audioSource.clip != null)
        {
            return audioSource.clip.name;
        }
        else
        {
            return null;
        }
    }

    public string GetEnvironment()
    {
        AudioSource audioSource = GetAudioSource(environment);
        currentEnvironment = audioSource.clip.name + " _ " + audioSource.volume.ToString() + " _ " + audioSource.pitch.ToString() + " _ " + audioSource.loop.ToString();
        return currentEnvironment;
    }

    public void SetEnvironment(string environmentSettings)
    {
        string[] settings = environmentSettings.Split('_');
        string clipName = settings[0].Trim();
        float volume = float.Parse(settings[1].Trim());
        float pitch = float.Parse(settings[2].Trim());
        bool loop = bool.Parse(settings[3].Trim());

        SetAudioClip(environment, clipName);
        PlayAudio(environment, volume, pitch, loop);
    }

    public void AddAudioSource(AudioSource audioSource)
    {
        if (!audioSourcesList.Contains(audioSource))
        {
            audioSourcesList.Add(audioSource);
        }
    }

    // Get an AudioSource by name
    AudioSource GetAudioSource(int gameObjectID)
    {
        foreach (AudioSource audioSource in audioSourcesList)
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