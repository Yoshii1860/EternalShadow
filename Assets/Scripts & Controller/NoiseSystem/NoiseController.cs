using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseController : MonoBehaviour
{
    [SerializeField] NoiseType noiseType;
    int priority = 0;
    float noiseLevel = 0f;

    /*
    void OnCollisionStay(Collision other)
    {
        if (transform.gameObject.tag == "Player") return;
        else if (noiseType == NoiseType.SHOOT) return;
        if (other.gameObject.CompareTag("Player"))
        {
            NoiseManager.Instance.UpdateCurrentNoiseData(noiseLevel, priority);
        }
    }
    */

    public void ShootNoise()
    {
        ChangeNoiseType(NoiseType.SHOOT);
        NoiseManager.Instance.UpdateCurrentNoiseData(noiseLevel, priority);
    }

    void Start()
    {
        ChangeNoiseType(noiseType);
    }

    public void UpdateNoiseLevel()
    {
        NoiseManager.Instance.UpdateCurrentNoiseData(noiseLevel, priority);
    }

    // This method will be automatically called whenever a serialized property is changed in the Inspector
    public void ChangeNoiseType(NoiseType noiseType)
    {
        // Update the noiseLevel and priority based on the selected noiseType
        switch (noiseType)
        {
            case NoiseType.GRASS:
                noiseLevel = NoiseManager.Instance.noiseData.grassNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                break;
            case NoiseType.CONCRETE:
                noiseLevel = NoiseManager.Instance.noiseData.concreteNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                break;
            case NoiseType.WOOD:
                noiseLevel = NoiseManager.Instance.noiseData.woodenPlanksNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                break;
            case NoiseType.CARPET:
                noiseLevel = NoiseManager.Instance.noiseData.carpetNoiseLevel;
                priority = NoiseManager.Instance.noiseData.lowPriority;
                break;
            case NoiseType.TRASH:
                noiseLevel = NoiseManager.Instance.noiseData.trashNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                break;
            case NoiseType.GLASS:
                noiseLevel = NoiseManager.Instance.noiseData.glassNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                break;
            case NoiseType.PLASTIC:
                noiseLevel = NoiseManager.Instance.noiseData.plasticNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                break;
            case NoiseType.WOODEN_PLANK:
                noiseLevel = NoiseManager.Instance.noiseData.brokenWoodenPlankNoiseLevel;
                priority = NoiseManager.Instance.noiseData.mediumPriority;
                break;
            case NoiseType.SHOOT:
                noiseLevel = NoiseManager.Instance.noiseData.shootNoiseLevel;
                priority = NoiseManager.Instance.noiseData.highPriority;
                break;
        }
    }
}