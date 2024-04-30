using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseController : MonoBehaviour
{
    #region Variables

    [SerializeField] private NoiseType _noiseType;
    private int _priority = 0;
    private float _noiseLevel = 0f;
    private bool _shootTimer = false;

    [SerializeField] private bool _debugMode = false;

    #endregion




    #region Unity Methods

    // Start method to initialize noise type
    void Start()
    {
        ChangeNoiseType(_noiseType);
        AudioManager.Instance.PlayAudio(gameObject.GetInstanceID());
    }

    #endregion




    #region Public Methods

    public NoiseType GetNoiseType()
    {
        return _noiseType;
    }

    // Triggered when a shoot noise occurs
    public void TriggerShootNoise()
    {
        _shootTimer = true;
        StartCoroutine(ShootNoiseCoroutine());
        UpdateNoiseLevel();
    }

    // Update the noise level without changing the noise type
    public void UpdateNoiseLevel(float magnitude = 1f)
    {
        if (!_shootTimer)
        {
            if (magnitude < 0.1f)
            {
                NoiseManager.Instance.UpdateCurrentNoiseData(NoiseManager.Instance.NoiseData.StopNoiseLevel, NoiseManager.Instance.NoiseData.LowPriority);
            }
            else
            {
                NoiseManager.Instance.UpdateCurrentNoiseData(_noiseLevel, _priority);
            }
        }
        else
        {
            NoiseManager.Instance.UpdateCurrentNoiseData(NoiseManager.Instance.NoiseData.ShootNoiseLevel, NoiseManager.Instance.NoiseData.HighPriority);
        }
    }

    // Method to change the noise type and update noise data accordingly
    // This method will be automatically called whenever a serialized property is changed in the Inspector
    public void ChangeNoiseType(NoiseType newNoiseType)
    {
        // Update the _noiseLevel and _priority based on the selected _noiseType

        if (newNoiseType == _noiseType || _shootTimer) return;

        AudioSource audioSource = null;

        switch (newNoiseType)
        {
            case NoiseType.GRASS:
                _noiseLevel = NoiseManager.Instance.NoiseData.GrassNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.LowPriority;
                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "grass step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Grass step");
                break;

            case NoiseType.CONCRETE:
                _noiseLevel = NoiseManager.Instance.NoiseData.ConcreteNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.LowPriority;
                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "stone step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Stone step");
                break;

            case NoiseType.WOOD:
                _noiseLevel = NoiseManager.Instance.NoiseData.WoodenPlanksNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.LowPriority;

                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "wood step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Wood step");
                break;

            case NoiseType.CARPET:
                _noiseLevel = NoiseManager.Instance.NoiseData.CarpetNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.LowPriority;
                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "carpet step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Carpet step");
                break;

            case NoiseType.TRASH:
                _noiseLevel = NoiseManager.Instance.NoiseData.TrashNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.MediumPriority;
                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "trash step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Trash step");
                break;

            case NoiseType.GLASS:
                _noiseLevel = NoiseManager.Instance.NoiseData.GlassNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.MediumPriority;
                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "glass step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Glass step");
                break;

            case NoiseType.PLASTIC:
                _noiseLevel = NoiseManager.Instance.NoiseData.PlasticNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.MediumPriority;
                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "plastic step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Plastic step");
                break;

            case NoiseType.WOODEN_PLANK:
                _noiseLevel = NoiseManager.Instance.NoiseData.BrokenWoodenPlankNoiseLevel;
                _priority = NoiseManager.Instance.NoiseData.MediumPriority;
                _noiseType = newNoiseType;

                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "plank step", audioSource.volume, audioSource.pitch, true);
                
                if (_debugMode) Debug.Log("Plank step");
                break;
        }
    }

    #endregion




    #region Coroutines

    IEnumerator ShootNoiseCoroutine()
    {
        if (_debugMode) Debug.Log("Shoot Noise");
        yield return new WaitForSeconds(3f);
        _shootTimer = false;
        if (_debugMode) Debug.Log("Shoot Noise End");
    }

    #endregion
}