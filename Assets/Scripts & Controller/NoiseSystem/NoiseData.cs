using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoiseType
{
    GRASS,
    CONCRETE,
    WOOD,
    CARPET,
    TRASH,
    GLASS,
    PLASTIC,
    WOODEN_PLANK,
    SHOOT
}

[CreateAssetMenu(fileName = "NoiseData", menuName = "Noise/Noise Data", order = 1)]
public class NoiseData : ScriptableObject
{
    // Modifier for PlayerController
    public float walkModifier = 1f;
    public float runModifier = 1.5f;
    public float crouchModifier = 0.5f;

    // Noise levels for surfaces
    public float grassNoiseLevel = 5f;
    public float concreteNoiseLevel = 8f;
    public float woodenPlanksNoiseLevel = 7f;
    public float carpetNoiseLevel = 4f;

    // Noise levels for objects
    public float trashNoiseLevel = 10f;
    public float glassNoiseLevel = 14f;
    public float plasticNoiseLevel = 12f;
    public float brokenWoodenPlankNoiseLevel = 15f;

    // Noise level for shooting
    public float shootNoiseLevel = 25f;

    // Priority for noise
    public int lowPriority = 1;
    public int mediumPriority = 2;
    public int highPriority = 3;
}