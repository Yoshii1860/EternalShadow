using UnityEngine;

public class UniqueIDComponent : MonoBehaviour
{
    [SerializeField]
    private string uniqueID = "";

    public string UniqueID
    {
        get { return uniqueID; }
        set
        {
            // Check if the new unique ID is already used
            if (UniqueIDManager.IsUniqueIDUsed(value))
            {
                Debug.LogWarning($"Object '{gameObject.name}' cannot set duplicate unique ID: {value}");
                return;
            }

            // Unregister the previous unique ID (if any)
            UniqueIDManager.UnregisterUniqueID(uniqueID);

            // Update the unique ID and register the new one
            uniqueID = value;
            UniqueIDManager.RegisterUniqueID(uniqueID, gameObject);
        }
    }
}