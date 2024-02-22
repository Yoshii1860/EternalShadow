using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UniqueIDComponent))]
public class InteractableObject : MonoBehaviour
{
    #region Variables

    [Header("If not Pickup - no base code will be run")]
    [SerializeField] bool isPickup = true;
    public bool active = false;

    [Header("Object Properties")]
    [SerializeField] Transform objectPosition;
    [SerializeField] bool rotateX = false;
    [SerializeField] bool rotateY = false;
    [SerializeField] bool rotateZ = false;
    [SerializeField] TextMeshProUGUI objectName;
    [SerializeField] TextMeshProUGUI objectDescription;
    [SerializeField] string objectNameString;
    [SerializeField] string objectDescriptionString;

    Coroutine rotationCoroutine;

    #endregion

    #region Interaction

    public virtual void RendererToggle(GameObject go, bool active)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Renderer renderer = go.transform.GetChild(i).gameObject.GetComponent<Renderer>();
            if (renderer != null && renderer.gameObject.activeSelf) renderer.enabled = active;
            if (go.transform.GetChild(i).childCount > 0) RendererToggle(go.transform.GetChild(i).gameObject, active);
        }
    }

    public virtual void Interact()
    {
        Debug.Log("Interacting with the base interactable object.");

        if (!isPickup) 
        {
            RunItemCode();
            return;
        }

        // loop through all active children and grand children of fpsArms and disable mesh renderer
        RendererToggle(GameManager.Instance.fpsArms, false);

        // Open Canvas
        GameManager.Instance.pickupCanvas.SetActive(true);

        // Pause UpdateManager
        GameManager.Instance.PickUp();

        // Create Duplicate Object on Canvas
        GameObject newItem = Instantiate(gameObject, GameManager.Instance.pickupCanvas.transform);

        // Deactivate all lights from showcase object
        DeactivateLights(newItem);

        // Show Object Details on Screen
        objectName.text = objectNameString;
        objectDescription.text = objectDescriptionString;

        // Make in-scene object invisible
        DeactivateRendererAndLights(transform);

        // Run Item Code
        StartCoroutine(ItemCode(newItem));

        // Deactivate Collider
        Collider collider = newItem.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // Set world position and rotation
        newItem.transform.position = objectPosition.position;
        newItem.transform.rotation = objectPosition.rotation;
        newItem.transform.localScale = objectPosition.localScale;

        // Rotate the object
        rotationCoroutine = StartCoroutine(RotateObject(newItem));
    }

    void DeactivateRendererAndLights(Transform objTransform)
    {
        Renderer renderer = objTransform.GetComponent<Renderer>();
        Light[] lights = objTransform.GetComponentsInChildren<Light>();

        if (renderer != null)
        {
            renderer.enabled = false;
        }
        else
        {
            Renderer[] renderers = objTransform.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }

        foreach (Light l in lights)
        {
            l.enabled = false;
        }
    }

    void DeactivateLights(GameObject obj)
    {
        Light[] lights = obj.GetComponentsInChildren<Light>();
        foreach (Light l in lights)
        {
            l.enabled = false;
        }
    }

    #endregion

    #region Coroutines

    IEnumerator ItemCode(GameObject newItemToDestroy)
    {
        // Wait for the player to return to gameplay mode
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Default);

        RendererToggle(GameManager.Instance.fpsArms, true);

        // Stop coroutine to prevent errors before deleting
        StopCoroutine(rotationCoroutine);

        // Destroy duplicated object
        Destroy(newItemToDestroy);

        // Deactivate Canvas
        GameManager.Instance.pickupCanvas.SetActive(false);

        // Run object-specific code
        RunItemCode();

        // Destroy the interactable object
        Destroy(gameObject);
    }

    IEnumerator RotateObject(GameObject itemToRotate)
    {
        float elapsedTime = 0f;
        float rotateTime = 10.0f; // Adjust this value for the rotation speed

        while (elapsedTime < rotateTime)
        {
            Vector3 rotation = Vector3.zero;

            if (rotateX) rotation.x = (360 / rotateTime) * Time.deltaTime;
            if (rotateY) rotation.y = (360 / rotateTime) * Time.deltaTime;
            if (rotateZ) rotation.z = (360 / rotateTime) * Time.deltaTime;

            itemToRotate.transform.rotation *= Quaternion.Euler(rotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion

    #region Item-Specific Code

    protected virtual void RunItemCode()
    {
        Debug.Log("Running the base item code if available.");
    }

    #endregion
}