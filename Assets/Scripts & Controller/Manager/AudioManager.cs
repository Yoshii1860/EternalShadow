using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    #region Singleton Instance

    // Singleton instance
    static AudioManager instance;

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

    #endregion

    #region Audio Data Lists

    // List to store AudioSources
    List<AudioSource> audioSourcesList = new List<AudioSource>();

    // List to store AudioFiles
    List<AudioClip> audioFilesList = new List<AudioClip>();

    #endregion

    #region GameObject References and IDs

    // Debug settings
    [Header("Debug Settings")]
    [Tooltip("Enable debug messages and warnings")]
    public bool debugSettings = true;
    [Space(10)]

    // GameObject references for environment and player speakers
    [Header("Audio Source Objects")]
    public GameObject environmentObject;
    public GameObject playerSpeakerObject;
    public GameObject playerSpeaker2Object;
    [Space(10)]

    // Instance IDs for environment and player speakers
    [Header("Audio Source IDs - Do Not Modify")]
    public int environment;
    public int playerSpeaker;
    public int playerSpeaker2;

    public bool girlToggle = false;
    public bool slenderToggle = false;

    #endregion

    #region Unity Callbacks

    // Awake is called before Start, used for initialization
    void Awake()
    {
        // Ensure only one instance of AudioManager exists
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
            audioSourcesList.Add(audioSource);
        }
        if (debugSettings)
        {
            Debug.Log($"AUDIO: Found {audioSources.Length} AudioSources in the scene.");
            for (int i = 0; i < audioSources.Length; i++)
            {
                Debug.Log($"AUDIO: AudioSource {i + 1} - {audioSources[i].gameObject.name}");
            }
        }

        // Load audio files dynamically from Resources folder
        LoadAudioFiles();

        // Set default environmentObject if not assigned
        if (environmentObject == null)
        {
            environmentObject = transform.gameObject.GetComponentInChildren<AudioSource>().gameObject;
        }

        // Set instance IDs for environment and player speakers
        environment = environmentObject.GetInstanceID();
        playerSpeaker = playerSpeakerObject.GetInstanceID();
        playerSpeaker2 = playerSpeaker2Object.GetInstanceID();
    }

    #endregion

    #region Audio Management

    // Load audio files from Resources folder
    void LoadAudioFiles()
    {
        // Load all audio clips from the Resources folder
        AudioClip[] loadedClips = Resources.LoadAll<AudioClip>("SFX");

        // Add loaded clips to the list
        audioFilesList.AddRange(loadedClips);
    }

    // Load AudioSources in the scene
    public void LoadAudioSources()
    {
        // Initialize the array of AudioSources
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            if (!audioSourcesList.Contains(audioSource))
            {
                audioSourcesList.Add(audioSource);
            }
        }
        if (debugSettings) Debug.Log($"AudioManager: Found {audioSources.Length} AudioSources in the scene.");
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
            if (debugSettings) Debug.Log($"AudioManager: Playing {audioSource.clip.name} on {audioSource.gameObject.name}");
            return true;
        }
        else
        {
            if (debugSettings) Debug.LogWarning($"AudioManager: Sound not found - {gameObjectID}");
            return false;
        }
    }

    // Pause audio for an AudioSource
    public void PauseAudio(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.Pause();
            if (debugSettings) Debug.Log($"AudioManager: Pausing {audioSource.clip.name} on {audioSource.gameObject.name}");
        }
        else if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    // Unpause audio for an AudioSource
    public void UnpauseAudio(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.UnPause();
            if (debugSettings) Debug.Log($"AudioManager: Unpausing {audioSource.clip.name} on {audioSource.gameObject.name}");
        }
        else if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    // Play a sound one-shot by name
    public void PlaySoundOneShot(int gameObjectID, string clipName, float volume = 1f, float pitch = 1f)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        AudioClip audioFile = GetAudioClip(clipName);
        audioFile.LoadAudioData();

        if (audioSource != null && audioFile != null)
        {
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            AudioDataLoadState loadState = audioFile.loadState;
            if (loadState == AudioDataLoadState.Loaded) audioSource.PlayOneShot(audioFile);
            else
            {
                if (debugSettings) Debug.LogWarning($"AudioManager: AudioClip not ready to play - {clipName}");
                return;
            }
            if (debugSettings) Debug.Log($"AudioManager: Playing {audioFile.name} on {audioSource.gameObject.name}");
        }
        else
        {
            if (audioSource == null)
            {
                if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
            }
            else
            {
                if (debugSettings) Debug.LogWarning($"AudioManager: AudioClip not found - {clipName}");
            }
        }
    }

    // Set the audio clip for an AudioSource
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
            if (debugSettings) Debug.Log($"AudioManager: Set {audioFile.name} on {audioSource.gameObject.name}");
        }
        else
        {
            if (audioSource == null && debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
            else if (debugSettings) Debug.LogWarning($"AudioManager: AudioClip not found - {clipName}");
        }
    }

    // Stop playing audio for an AudioSource
    public void StopAudio(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource == null)
        {
            if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
            return;
        }
        else if (!audioSource.isPlaying)
        {
            return;
        }
        else
        {
            audioSource.Stop();
            audioSource.clip.UnloadAudioData();
            if (debugSettings) Debug.Log($"AudioManager: Stopping {audioSource.gameObject.name}");
        }
    }

    public void FadeOutAll(float fadeOutDuration)
    {
        foreach (AudioSource audioSource in audioSourcesList)
        {
            if (audioSource.isPlaying) StartCoroutine(FadeOutCo(audioSource, fadeOutDuration));
        }
    }


    // Fade out audio for an AudioSource
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

    // Fade in audio for an AudioSource
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

    // Play audio with a delay
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

    // Stop audio with a delay
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

    // Stop all AudioSources
    public void StopAll()
    {
        foreach (AudioSource audioSource in audioSourcesList)
        {
            audioSource.Stop();
            if (debugSettings) Debug.Log("AudioManager: Stopping all AudioSources.");
        }
        if (debugSettings) Debug.Log("AudioManager: Stopping all AudioSources.");
    }

    // Stop all AudioSources except the specified one
    public void StopAllExcept(int gameObjectID)
    {
        string audioSourceName = "";
        foreach (AudioSource audioSource in audioSourcesList)
        {
            if (audioSource.gameObject.GetInstanceID() != gameObjectID)
            {
                audioSource.Stop();
            }
            else
            {
                audioSourceName = audioSource.gameObject.name;
            }
        }
        if (debugSettings) Debug.Log("AudioManager: Stopping all AudioSources except " + audioSourceName);
    }

    // Check if audio is playing for an AudioSource
    public bool IsPlaying(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            return audioSource.isPlaying;
        }
        else
        {
            if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
            return false;
        }
    }

    public void SetVolume(int gameObjectID, float volume)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        else if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    // Set the audio volume for an AudioSource gradually
    public void SetVolumeOverTime(int gameObjectID, float volume, float duration = 0.5f)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            StartCoroutine(SetAudioVolumeCo(audioSource, duration, volume));
        }
        else if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
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

        // Ensure the volume is set to the target value
        audioSource.volume = endVolume;
    }

    public void SetAudioPitch(int gameObjectID, float pitch)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.pitch = pitch;
        }
        else if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }


    // Get the name of the audio clip for an AudioSource
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

    // Get the current environment settings
    public string GetEnvironment()
    {
        AudioSource audioSource = GetAudioSource(environment);
        string currentEnvironment = $"{audioSource.clip.name} _ {audioSource.volume:F2} _ {audioSource.pitch:F2} _ {audioSource.loop}";
        return currentEnvironment;
    }

    // Set the environment settings
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

    // Remove an AudioSource from the list
    public void RemoveAudioSource(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        audioSourcesList.Remove(audioSource);
        if (debugSettings) Debug.Log($"AudioManager: Removed {audioSource.gameObject.name} from the list.");
    }

    // Add an AudioSource to the list
    public void AddAudioSource(AudioSource audioSource)
    {
        if (!audioSourcesList.Contains(audioSource))
        {
            audioSourcesList.Add(audioSource);
            if (debugSettings) Debug.Log($"AudioManager: Added {audioSource.gameObject.name} to the list.");
        }
    }

    // Get an AudioSource by instance ID
    public AudioSource GetAudioSource(int gameObjectID)
    {
        foreach (AudioSource audioSource in audioSourcesList)
        {
            if (audioSource.gameObject.GetInstanceID() == gameObjectID)
            {
                return audioSource;
            }
        }

        if (debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
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

    #endregion

    #region Character Specific Audio

    // Changing Volume based on the floor level
    public IEnumerator VolumeFloorChanger(int enemyID, int stepID, Transform enemy, int enemyType)
    {
        if (debugSettings) Debug.Log(enemy.gameObject.name + ": Starting volume floor changer!");
        Transform player = GameManager.Instance.player.transform;
        switch (enemyType)
        {
            case 0:
                while (!slenderToggle)
                {   
                    float volume = 1.0f - Mathf.Abs(player.position.y - enemy.position.y) / 4.0f;
                    volume = Mathf.Clamp(volume, 0.25f, 1.0f);
                    SetVolume(enemyID, volume);
                    SetVolume(stepID, volume);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
            case 1:
                while (!girlToggle)
                {   
                    float volume = 1.0f - Mathf.Abs(player.position.y - enemy.position.y) / 4.0f;
                    volume = Mathf.Clamp(volume, 0.25f, 1.0f);
                    SetVolume(enemyID, volume);
                    SetVolume(stepID, volume);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
        }
        if (debugSettings) Debug.Log(enemy.gameObject.name + ": Volume floor changer stopped!");
    }

    public void ToggleEnemyAudio(GameObject enemy, bool isChasing, int enemyType)
    {
        int enemyID = enemy.GetInstanceID();
        int stepID = enemy.transform.GetChild(0).gameObject.GetInstanceID();
        string clip = GetAudioClip(enemyID);

        switch (enemyType)
        {
            case 0:
                
                if (slenderToggle) return;
                else if (!IsPlaying(enemyID))
                {
                    if (debugSettings) Debug.Log("Enemy not playing - starting audio!");
                    StartCoroutine(VolumeFloorChanger(enemyID, stepID, enemy.transform, enemyType));
                    PlayAudio(enemyID);
                    PlayAudio(stepID);
                    return;
                }
                else if (isChasing && string.Compare(clip, "slender scream") == 0)
                {
                    return;
                }
                else if (!isChasing && string.Compare(clip, "slender hello") == 0)
                {
                    return;
                }

                if (!slenderToggle) 
                {
                    StartCoroutine(EnemyToggle(enemyID, stepID, isChasing, enemyType));
                    if (debugSettings) Debug.Log("Slender audio toggle started!");
                }
                return;

            case 1:

                if (girlToggle) return;
                else if (!IsPlaying(enemyID))
                {
                    if (debugSettings) Debug.Log("Enemy not playing - starting audio!");
                    StartCoroutine(VolumeFloorChanger(enemyID, stepID, enemy.transform, enemyType));
                    PlayAudio(enemyID);
                    PlayAudio(stepID);
                    return;
                }
                else if (isChasing && string.Compare(clip, "woman scream") == 0)
                {
                    return;
                }
                else if (!isChasing && string.Compare(clip, "weeping ghost woman") == 0)
                {
                    return;
                }

                if (!girlToggle) 
                {
                    StartCoroutine(EnemyToggle(enemyID, stepID, isChasing, enemyType));
                    if (debugSettings) Debug.Log("Girl audio toggle started!");
                }
                return;
        }
    }

    IEnumerator EnemyToggle(int enemyID, int stepID, bool isChasing, int enemyType)
    {
        string talkClip = "";
        string walkClip = "";
        float fadeTime = 1f;
        float volume = 1f;
        float pitch = 1f;

        switch (enemyType)
        {
            case 0:

                slenderToggle = true;

                if (isChasing)
                {
                    if (debugSettings) Debug.Log("Slender is chasing - changing audio!");
                    talkClip = "slender scream";
                    walkClip = "slender run";
                    fadeTime = 1f;
                }
                else
                {
                    if (debugSettings) Debug.Log("Slender is not chasing - changing audio!");
                    talkClip = "slender hello";
                    walkClip = "slender walk";
                    fadeTime = 5f;
                    pitch = 0.8f;
                }
                break;

            case 1:

                girlToggle = true;

                if (isChasing)
                {
                    if (debugSettings) Debug.Log("Girl is chasing - changing audio!");
                    talkClip = "woman scream";
                    walkClip = "woman step";
                    fadeTime = 1f;
                    pitch = 2f;
                }
                else
                {
                    if (debugSettings) Debug.Log("Girl is not chasing - changing audio!");
                    talkClip = "weeping ghost woman";
                    walkClip = "woman step";
                    fadeTime = 5f;
                    pitch = 1f;
                }
                break;
        }

        FadeOut(enemyID, fadeTime);
        FadeOut(stepID, fadeTime);
        AudioSource audioSource = GetAudioSource(enemyID);
        yield return new WaitUntil(() => !audioSource.isPlaying);
        SetAudioClip(enemyID, talkClip, volume, 1f, true);
        SetAudioClip(stepID, walkClip, volume, pitch, true);
        yield return new WaitForSeconds(0.25f);
        PlayAudio(enemyID, volume, 1f, true);
        PlayAudio(stepID, volume, pitch, true);
        switch (enemyType)
        {
            case 0:
                slenderToggle = false;
                break;
            case 1:
                girlToggle = false;
                break;
        }
        StartCoroutine(VolumeFloorChanger(enemyID, stepID, GetAudioSource(enemyID).transform, enemyType));
        if (debugSettings) Debug.Log("Enemy audio change DONE!");
    }

    #endregion
}