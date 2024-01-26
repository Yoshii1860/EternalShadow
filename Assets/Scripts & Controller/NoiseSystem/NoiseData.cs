using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Noise Enum

// Enum to represent different noise types
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

#endregion

#region Noise Data Class

// ScriptableObject to store noise data
[CreateAssetMenu(fileName = "NoiseData", menuName = "Noise/Noise Data", order = 1)]

#endregion

public class NoiseData : ScriptableObject
{
    #region PlayerController Modifiers

    public float walkModifier = 1f;
    public float runModifier = 1.5f;
    public float crouchModifier = 0.5f;

    #endregion

    #region Surface Noise Levels

    public float grassNoiseLevel = 5f;
    public float concreteNoiseLevel = 8f;
    public float woodenPlanksNoiseLevel = 7f;
    public float carpetNoiseLevel = 4f;

    #endregion

    #region Object Noise Levels

    public float trashNoiseLevel = 10f;
    public float glassNoiseLevel = 14f;
    public float plasticNoiseLevel = 12f;
    public float brokenWoodenPlankNoiseLevel = 15f;

    #endregion

    #region Shooting Noise Level

    public float shootNoiseLevel = 25f;

    #endregion

    #region Priority Levels

    public int lowPriority = 1;
    public int mediumPriority = 2;
    public int highPriority = 3;

    #endregion
}