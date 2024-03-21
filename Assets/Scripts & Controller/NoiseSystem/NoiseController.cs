using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseController : MonoBehaviour
{
    #region Variables

    [SerializeField] NoiseType noiseType;
    int priority = 0;
    float noiseLevel = 0f;

    #endregion

    #region Initialization

    // Start method to initialize noise type
    void Start()
    {
        ChangeNoiseType(noiseType);
        AudioManager.Instance.PlayAudio(gameObject.GetInstanceID());
    }

    #endregion

    #region Collision and Noise

    /*
    void OnCollisionStay(Collision other)
    {
        // Commented out collision logic, as it seems to be unused or incomplete
        // If intended to be used, complete the logic and uncomment as needed
    }
    */

    public NoiseType GetNoiseType()
    {
        return noiseType;
    }

    // Triggered when a shoot noise occurs
    public void ShootNoise()
    {
        // Change the noise type to shoot and update the noise data
        ChangeNoiseType(NoiseType.SHOOT);
        NoiseManager.Instance.UpdateCurrentNoiseData(noiseLevel, priority);
    }

    // Update the noise level without changing the noise type
    public void UpdateNoiseLevel()
    {
        NoiseManager.Instance.UpdateCurrentNoiseData(noiseLevel, priority);
    }

    #endregion

    #region Noise Type Management

    // Method to change the noise type and update noise data accordingly
    // This method will be automatically called whenever a serialized property is changed in the Inspector
    public void ChangeNoiseType(NoiseType newNoiseType)
    {
        // Update the noiseLevel and priority based on the selected noiseType

        if (newNoiseType == noiseType) return;
        AudioSource audioSource = null;

        switch (newNoiseType)
        {
            case NoiseType.GRASS:
                noiseLevel = NoiseManager.Instance.noiseData.grassNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "grass step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Grass step");
                break;
            case NoiseType.CONCRETE:
                noiseLevel = NoiseManager.Instance.noiseData.concreteNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "stone step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Stone step");
                break;
            case NoiseType.WOOD:
                noiseLevel = NoiseManager.Instance.noiseData.woodenPlanksNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "wood step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Wood step");
                break;
            case NoiseType.CARPET:
                noiseLevel = NoiseManager.Instance.noiseData.carpetNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "carpet step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Carpet step");
                break;
            case NoiseType.TRASH:
                noiseLevel = NoiseManager.Instance.noiseData.trashNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "trash step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Trash step");
                break;
            case NoiseType.GLASS:
                noiseLevel = NoiseManager.Instance.noiseData.glassNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "glass step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Glass step");
                break;
            case NoiseType.PLASTIC:
                noiseLevel = NoiseManager.Instance.noiseData.plasticNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "plastic step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Plastic step");
                break;
            case NoiseType.WOODEN_PLANK:
                noiseLevel = NoiseManager.Instance.noiseData.brokenWoodenPlankNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                audioSource = AudioManager.Instance.GetAudioSource(gameObject.GetInstanceID());
                AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "plank step", audioSource.volume, audioSource.pitch, true);
                noiseType = newNoiseType;
                Debug.Log("Plank step");
                break;
            case NoiseType.SHOOT:
                noiseLevel = NoiseManager.Instance.noiseData.shootNoiseLevel;
                priority = NoiseManager.Instance.noiseData.highPriority;
                break;
        }
    }

    #endregion
}