using UnityEngine;

public class UniqueIDComponent : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private string uniqueID = "";

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the unique ID of the object.
    /// </summary>
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

    #endregion
}