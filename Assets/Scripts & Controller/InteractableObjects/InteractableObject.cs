using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UniqueIDComponent))]
public class InteractableObject : MonoBehaviour
{
    #region Variables

    [Header("If not Pickup - no base code will be run")]
    public bool isPickup = true;
    public bool active = false;
    public bool inventoryItem = true;
    public bool useEmission = true;

    [Header("Object Properties")]
    [SerializeField] Transform objectPosition;
    [SerializeField] bool rotateX = false;
    [SerializeField] bool rotateY = false;
    [SerializeField] bool rotateZ = false;
    [SerializeField] TextMeshProUGUI objectName;
    [SerializeField] TextMeshProUGUI objectDescription;
    [SerializeField] string objectNameString;
    [SerializeField] string objectDescriptionString;
    public string clipName;

    Coroutine rotationCoroutine;

    #endregion

    #region Interaction

    public virtual void Interact()
    {
        Debug.Log("Interacting with the base interactable object.");

        if (active) active = false;

        if (!isPickup) 
        {
            RunItemCode();
            return;
        }

        // loop through all active children and grand children of fpsArms and disable mesh renderer
        GameManager.Instance.playerController.ToggleArms(false);

        // Open Canvas
        GameManager.Instance.pickupCanvas.SetActive(true);

        // Pause UpdateManager
        GameManager.Instance.PickUp();

        if (clipName == "") clipName = "pickup item";
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, clipName, 0.6f, 1f);

        DeactivateEmission();

        // Create Duplicate Object on Canvas
        GameObject newItem = Instantiate(gameObject, GameManager.Instance.pickupCanvas.transform);

        // Deactivate Collider
        Collider[] collider = newItem.GetComponents<Collider>();
        if (collider.Length > 0) 
        {
            foreach (Collider c in collider)
            {
                c.enabled = false;
            }
        }

        // Deactivate all lights from showcase object
        DeactivateLights(newItem);

        // Show Object Details on Screen
        objectName.text = objectNameString;
        objectDescription.text = objectDescriptionString;

        // Make in-scene object invisible
        DeactivateRendererAndLights(transform);

        // Run Item Code
        StartCoroutine(ItemCode(newItem));

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

    void DeactivateEmission()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            foreach(Material material in renderer.materials)
            {
                material.DisableKeyword("_EMISSION");
            }
        }
        else
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            if (renderers != null)
            {
                foreach (MeshRenderer rendererChild in renderers)
                {
                    foreach (Material material in rendererChild.materials)
                    {
                        material.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }

    #endregion

    #region Coroutines

    IEnumerator ItemCode(GameObject newItemToDestroy)
    {
        // Wait for the player to return to gameplay mode
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Default);

        // Stop coroutine to prevent errors before deleting
        StopCoroutine(rotationCoroutine);

        // Destroy duplicated object
        Destroy(newItemToDestroy);

        // Deactivate Canvas
        GameManager.Instance.playerController.ToggleArms(true);
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
        Debug.Log("Running base item code.");
    }

    #endregion
}