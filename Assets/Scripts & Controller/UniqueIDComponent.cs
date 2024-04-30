using UnityEngine;

public class UniqueIDComponent : MonoBehaviour
{
    #region Fields

    [Tooltip("The unique ID of the object.")]
    [SerializeField] private string _uniqueID = "";

    #endregion




    #region Properties

    /// Gets or sets the unique ID of the object.
    public string UniqueID
    {
        get { return _uniqueID; }
        set
        {
            // Check if the new unique ID is already used
            if (UniqueIDManager.IsUniqueIDUsed(value))
            {
                Debug.LogWarning($"Object '{gameObject.name}' cannot set duplicate unique ID: {value}");
                return;
            }

            // Unregister the previous unique ID (if any)
            UniqueIDManager.UnregisterUniqueID(_uniqueID);

            // Update the unique ID and register the new one
            _uniqueID = value;
            UniqueIDManager.RegisterUniqueID(_uniqueID, gameObject);
        }
    }

    #endregion
}