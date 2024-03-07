using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MirrorCode : MonoBehaviour, ICustomUpdatable
{
    LayerMask playerLayer; // Layer to identify the player object
    LayerMask mirrorLayer; // Layer to identify the mirror object
    Transform spotlightTransform; // Transform of the spotlight on the mirror
    [SerializeField] Transform camRootTransform; // Transform of the camRoot on the player
    [SerializeField] GameObject secondSpotlight; // Angle of the spotlight
    [SerializeField] CinemachineVirtualCamera vcam; // Reference to the virtual camera
    [SerializeField] Transform newFollowTarget; // Reference to the vcam follow object
    [SerializeField] GameObject particles; // Reference to the particles on the cross
    [SerializeField] Animator animator; // Reference to the animator on the cross

    void Start()
    {
        playerLayer = 1 << GameManager.Instance.player.gameObject.layer; // Set the player layer to the layer of the player object
        spotlightTransform = transform.GetChild(0); // Get the spotlight transform from the child object
        mirrorLayer = 1 << gameObject.layer; // Set the mirror layer to the layer of the mirror object
        spotlightTransform.gameObject.SetActive(false); // Disable the spotlight initially
        secondSpotlight.SetActive(false); // Disable the second spotlight initially

        ///////////////////////////////////////
        // SET THIS AT A PROPER TIME IN THE GAME
        ///////////////////////////////////////
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(this); // Add this script to the custom update manager
        ///////////////////////////////////////
    }
    
    public void CustomUpdate(float deltaTime)
    {
        RaycastOnMirror();
    }

    void RaycastOnMirror()
    {
        // Raycast from the flashlight towards the direction it's facing
        Ray ray = new Ray(camRootTransform.position, camRootTransform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red); // Draw the ray in the scene view

        // Check for collision with the mirror object
        if (Physics.Raycast(ray, out hit, 20f, mirrorLayer))
        {
            Debug.Log("Hit the mirror!");

            float dotProduct = Vector3.Dot(camRootTransform.forward.normalized, hit.normal.normalized);
            float angleInRadians = Mathf.Acos(dotProduct);
            float angleInDegrees = Mathf.Rad2Deg * angleInRadians;
            float angleDegreesNormal = angleInDegrees - 90f;

            spotlightTransform.position = hit.point; // Set the position of the spotlight to the hit point

            // Enable the spotlight on the mirror
            spotlightTransform.gameObject.SetActive(true);

            // Get the local Y-axis of the spotlight
            Vector3 angle = new Vector3(0, angleDegreesNormal, 0);

            // Smoothly rotate the spotlight towards the target rotation
            spotlightTransform.localEulerAngles = angle;

            CheckMirrorHit(angleDegreesNormal);
        }
        else
        {
            Debug.Log("No mirror hit!");
            
            // Disable the spotlight if not pointing at the mirror
            spotlightTransform.gameObject.SetActive(false);
        }
    }

    void CheckMirrorHit(float angle)
    {
        if (spotlightTransform.localPosition.y  > -0.06 
        && spotlightTransform.localPosition.y   < 0.06
        && angle                                > 14.2 
        && angle                                < 14.6)
        {
            Vector3 playerPosition = GameManager.Instance.player.transform.position;
            Vector3 playerEulerAngles = GameManager.Instance.player.transform.eulerAngles;
            StartCoroutine(EndEvent(playerPosition, playerEulerAngles));
        }
        else
        {
            secondSpotlight.SetActive(false);
        }
    }

    IEnumerator EndEvent(Vector3 position, Vector3 eulerAngles)
    {
        float time = 1.5f;
        while (time > 0)
        {
            GameManager.Instance.player.transform.eulerAngles = eulerAngles;
            GameManager.Instance.player.transform.position = position;
            time -= Time.deltaTime;
            yield return null;
        }
        GameManager.Instance.GameplayEvent();
        secondSpotlight.SetActive(true);
        vcam.Follow = newFollowTarget;
        yield return new WaitForSeconds(1.5f);
        particles.SetActive(true);
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("Fall");
    }
}
