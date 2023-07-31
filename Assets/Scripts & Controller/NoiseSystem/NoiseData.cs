using UnityEngine;

[CreateAssetMenu(fileName = "NoiseData", menuName = "Noise/Noise Data", order = 1)]
public class NoiseData : ScriptableObject
{
    public float walkModifier = 1f;
    public float runModifier = 1.5f;
    public float crouchModifier = 0.5f;

    public float grassNoiseLevel = 5f;
    public float concreteNoiseLevel = 8f;
    public float woodenPlanksNoiseLevel = 7f;
    public float carpetNoiseLevel = 4f;

    public float trashNoiseLevel = 10f;
    public float glassNoiseLevel = 14f;
    public float plasticNoiseLevel = 12f;
    public float brokenWoodenPlankNoiseLevel = 15f;

    public float shootNoiseLevel = 25f;
}