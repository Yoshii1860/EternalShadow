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
    WOODEN_PLANK
}

#endregion

#region Noise Data Class

// ScriptableObject to store noise data
[CreateAssetMenu(fileName = "NoiseData", menuName = "Noise/Noise Data", order = 1)]

#endregion

public class NoiseData : ScriptableObject
{
    #region PlayerController Modifiers

    public float WalkModifier = 1f;
    public float RunModifier = 1.5f;
    public float CrouchModifier = 0.5f;

    #endregion

    #region Surface Noise Levels

    public float StopNoiseLevel = 1f;
    public float GrassNoiseLevel = 5f;
    public float ConcreteNoiseLevel = 8f;
    public float WoodenPlanksNoiseLevel = 7f;
    public float CarpetNoiseLevel = 4f;

    #endregion

    #region Object Noise Levels

    public float TrashNoiseLevel = 10f;
    public float GlassNoiseLevel = 14f;
    public float PlasticNoiseLevel = 12f;
    public float BrokenWoodenPlankNoiseLevel = 15f;

    #endregion

    #region Shooting Noise Level

    public float ShootNoiseLevel = 20f;

    #endregion

    #region Priority Levels

    public int LowPriority = 1;
    public int MediumPriority = 2;
    public int HighPriority = 3;

    #endregion
}