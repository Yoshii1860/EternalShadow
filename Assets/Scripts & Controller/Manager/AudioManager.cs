using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    #region Singleton

    // Singleton instance
    private static AudioManager _instance;

    // Create a static reference to the _instance
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    _instance = obj.AddComponent<AudioManager>();
                }
            }

            return _instance;
        }
    }

    #endregion




    #region Fields

    private List<AudioSource> _audioSources;
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    // Debug settings
    [Header("Debug Settings")]
    [Tooltip("Enable debug messages and warnings")]
    [SerializeField] private bool _debugSettings = true;
    [Tooltip("Print the list of AudioSources")]
    [SerializeField] private bool _debugPrintSources = false;
    [Space(10)]

    // GameObject references for environment and player speakers
    [Header("Audio Source Objects")]
    [SerializeField] private GameObject _environmentObject;
    [SerializeField] private GameObject _playerSpeakerObject;
    [SerializeField] private GameObject _playerSpeaker2Object;
    [Space(10)]

    // Instance IDs for environment and player speakers
    [Header("Audio Source IDs - Do Not Modify")]
    public int Environment;
    public int PlayerSpeaker;
    public int PlayerSpeaker2;

    #endregion




    #region Unity Callbacks

    // Awake is called before Start, used for initialization
    private void Awake()
    {
        // Load audio sources and audio files
        LoadAudioSources();

        // Load audio files dynamically from Resources folder
        LoadAudioClips();

        // Set default _environmentObject if not assigned
        if (_environmentObject == null) _environmentObject = transform.GetChild(0).GetComponent<AudioSource>().gameObject;
        if (_playerSpeakerObject == null) _playerSpeakerObject = transform.GetChild(1).GetComponent<AudioSource>().gameObject;
        if (_playerSpeaker2Object == null) _playerSpeaker2Object = transform.GetChild(2).GetComponent<AudioSource>().gameObject;

        // Set instance IDs for environment and player speakers
        Environment = _environmentObject.GetInstanceID();
        PlayerSpeaker = _playerSpeakerObject.GetInstanceID();
        PlayerSpeaker2 = _playerSpeaker2Object.GetInstanceID();
    }

    private void Update()
    {
        if (_debugPrintSources)
        {
            string[] audioSourceNames = new string[_audioSources.Count];
            foreach (AudioSource audioSource in _audioSources)
            {
                if (audioSource.gameObject.name == "MenuManager") continue;
                audioSourceNames[_audioSources.IndexOf(audioSource)] = audioSource.gameObject.name;
            }
            Debug.Log("AudioManager: AudioSources in the list - " + string.Join(", ", audioSourceNames));
            _debugPrintSources = false;
        } 
    }

    #endregion




    #region Audio Loading

    // Load audio files from Resources folder
    private void LoadAudioClips()
    {
        // Load all audio clips from the Resources folder
        foreach (AudioClip clip in Resources.LoadAll<AudioClip>("SFX"))
        {
            _audioClips.Add(clip.name, clip);
        }
    }

    // Load AudioSources in the scene
    public void LoadAudioSources()
    {
        // Find all AudioSources in the scene
        _audioSources = new List<AudioSource>(FindObjectsOfType<AudioSource>(true).Where(x => x.gameObject.name != "MenuManager"));

        if (_debugSettings) Debug.Log($"AudioManager: Found {_audioSources.Count} AudioSources in the scene.");
    }

    // Remove an AudioSource from the list
    public void RemoveAudioSource(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        _audioSources.Remove(audioSource);
        if (_debugSettings) Debug.Log($"AudioManager: Removed {audioSource.gameObject.name} from the list.");
    }

    // Add an AudioSource to the list
    public void AddAudioSource(AudioSource audioSource)
    {
        if (!_audioSources.Contains(audioSource))
        {
            _audioSources.Add(audioSource);
            if (_debugSettings) Debug.Log($"AudioManager: Added {audioSource.gameObject.name} to the list.");
        }
    }

    #endregion




    #region Audio Set/Get

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
            if (_debugSettings) Debug.Log($"AudioManager: Set {audioFile.name} on {audioSource.gameObject.name}");
        }
        else
        {
            if (audioSource == null && _debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
            else if (_debugSettings) Debug.LogWarning($"AudioManager: AudioClip not found - {clipName}");
        }
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
        AudioSource audioSource = GetAudioSource(Environment);
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

        SetAudioClip(Environment, clipName);
        PlayAudio(Environment, volume, pitch, loop);
    }

    // Get an AudioSource by instance ID
    public AudioSource GetAudioSource(int gameObjectID)
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            if (audioSource.gameObject.GetInstanceID() == gameObjectID)
            {
                return audioSource;
            }
        }

        if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
        return null;
    }

    // Get an AudioClip by name
    private AudioClip GetAudioClip(string clipName)
    {
        if (_audioClips.ContainsKey(clipName))
        {
            return _audioClips[clipName];
        }
        else
        {
            if (_debugSettings) Debug.LogWarning($"AudioManager: AudioClip not found - {clipName}");
            return null;
        }
    }

    #endregion




    #region Audio Play

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
            if (_debugSettings) Debug.Log($"AudioManager: Playing {audioSource.clip.name} on {audioSource.gameObject.name}");
            return true;
        }
        else
        {
            if (_debugSettings) Debug.LogWarning($"AudioManager: Sound not found - {gameObjectID}");
            return false;
        }
    }

    // Play a sound one-shot by name
    public void PlayClipOneShot(int gameObjectID, string clipName, float volume = 1f, float pitch = 1f)
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
                if (_debugSettings) Debug.LogWarning($"AudioManager: AudioClip not ready to play - {clipName}");
                return;
            }
            if (_debugSettings) Debug.Log($"AudioManager: Playing {audioFile.name} on {audioSource.gameObject.name}");
        }
        else
        {
            if (audioSource == null)
            {
                if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
            }
            else
            {
                if (_debugSettings) Debug.LogWarning($"AudioManager: AudioClip not found - {clipName}");
            }
        }
    }

    // Play audio with a delay
    public void PlayAudioWithDelay(int gameObjectID, float delay)
    {
        StartCoroutine(PlayAudioDelayed(gameObjectID, delay));
    }

    private IEnumerator PlayAudioDelayed(int gameObjectID, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioSource audioSource = GetAudioSource(gameObjectID);
        audioSource.Play();
    }

    public void PlayClipOneShotWithDelay(int gameObjectID, string clipName, float delay, float volume = 1f, float pitch = 1f)
    {
        StartCoroutine(PlayClipOneShotDelayed(gameObjectID, clipName, delay, volume, pitch));
    }

    private IEnumerator PlayClipOneShotDelayed(int gameObjectID, string clipName, float delay, float volume = 1f, float pitch = 1f)
    {
        yield return new WaitForSeconds(delay);
        PlayClipOneShot(gameObjectID, clipName, volume, pitch);
    }

    // Fade in audio for an AudioSource
    public void FadeInAudio(int gameObjectID, float fadeInDuration, float maxVolume = 1f, float startVolume = 0f)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        StartCoroutine(FadeInAudioRoutine(audioSource, fadeInDuration, maxVolume, startVolume));
    }

    IEnumerator FadeInAudioRoutine(AudioSource audioSource, float fadeInDuration, float maxVolume = 1f, float startVolume = 0f)
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

    // Unpause audio for an AudioSource
    public void UnpauseAudio(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.UnPause();
            if (_debugSettings) Debug.Log($"AudioManager: Unpausing {audioSource.clip.name} on {audioSource.gameObject.name}");
        }
        else if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    #endregion




    #region Audio Stop

    // Stop playing audio for an AudioSource
    public void StopAudio(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource == null)
        {
            if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
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
            if (_debugSettings) Debug.Log($"AudioManager: Stopping {audioSource.gameObject.name}");
        }
    }

    public void FadeOutAll(float fadeOutDuration)
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            if (audioSource.isPlaying) StartCoroutine(FadeOutRoutine(audioSource, fadeOutDuration));
        }
    }


    // Fade out audio for an AudioSource
    public void FadeOutAudio(int gameObjectID, float fadeOutDuration)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);
        StartCoroutine(FadeOutRoutine(audioSource, fadeOutDuration));
    }

    private IEnumerator FadeOutRoutine(AudioSource audioSource, float fadeOutDuration)
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

    // Stop audio with a delay
    public void StopAudioWithDelay(int gameObjectID, float delay = 0f)
    {
        StartCoroutine(StopAudioDelayed(gameObjectID, delay));
    }

    private IEnumerator StopAudioDelayed(int gameObjectID, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
            StopAudio(gameObjectID);
        }
        else
        {
            AudioSource audioSource = GetAudioSource(gameObjectID);
            audioSource.loop = false;

            yield return new WaitUntil(() => !IsPlaying(gameObjectID));
            StopAudio(gameObjectID);
        }
    }

    // Stop all AudioSources
    public void StopAll()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.Stop();
            if (_debugSettings) Debug.Log("AudioManager: Stopping all AudioSources.");
        }
        if (_debugSettings) Debug.Log("AudioManager: Stopping all AudioSources.");
    }

    // Stop all AudioSources except the specified one
    public void StopAllExcept(int gameObjectID)
    {
        string audioSourceName = "";
        foreach (AudioSource audioSource in _audioSources)
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
        if (_debugSettings) Debug.Log("AudioManager: Stopping all AudioSources except " + audioSourceName);
    }

    // Pause audio for an AudioSource
    public void PauseAudio(int gameObjectID)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            audioSource.Pause();
            if (_debugSettings) Debug.Log($"AudioManager: Pausing {audioSource.clip.name} on {audioSource.gameObject.name}");
        }
        else if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    #endregion




    #region Audio Settings

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
            if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
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
        else if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    // Set the audio volume for an AudioSource gradually
    public void SetVolumeOverTime(int gameObjectID, float volume, float duration = 0.5f)
    {
        AudioSource audioSource = GetAudioSource(gameObjectID);

        if (audioSource != null)
        {
            StartCoroutine(SetVolumeOverTimeRoutine(audioSource, duration, volume));
        }
        else if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    private IEnumerator SetVolumeOverTimeRoutine(AudioSource audioSource, float duration, float endVolume)
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
        else if (_debugSettings) Debug.LogWarning($"AudioManager: AudioSource not found - {gameObjectID}");
    }

    #endregion
}