using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Musicbox : MonoBehaviour, ICustomUpdatable
{
    public float maxDistance = 10f; // Distance at which the canvas becomes fully black

    [SerializeField] Image image;

    [SerializeField] Door door;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject monsterAudio;
    int monsterAudioID;

    [SerializeField] Transform[] waypoints;
    Transform currentWaypoint;
    int waypointIndex = 0;

    bool inside = false;
    bool started = false;

    float warningDistance = 5f;
    int warningCounter = 0;

    bool waypointMoving = true;

    void Start()
    {
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(this);
        monsterAudioID = monsterAudio.GetInstanceID();
    }

    public void CustomUpdate(float deltaTime)
    {
        if (!started) GetCloser();
        else if (!waypointMoving && started) MoveAway();
    }

    void GetCloser()
    {
        // Calculate direction to the player
        float distance = Vector3.Distance(transform.position, GameManager.Instance.player.transform.position);
        // If the player gets closer, fade to black
        float alpha = Mathf.SmoothStep(0.99f, 0f, distance / maxDistance);
        // Clamp the alpha value to the range [0, 1]
        alpha = Mathf.Clamp(alpha, 0f, 1f);
        // Set the alpha value of the image color
        image.color = new Color(0, 0, 0, alpha);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            if (inside) return;
            else inside = true;
            if (!started)
            {
                started = true;
                StartCoroutine(FadeToBlack());
                door.CloseDoor();
                door.locked = true;
            }
        }
    }

    IEnumerator FadeToBlack()
    {
        for (float i = image.color.a; i < 1; i += 0.01f)
        {
            image.color = new Color(0, 0, 0, i);
            if (i > 0.99f)
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlaySoundOneShot(monsterAudioID, "MB grunt", 0.8f, 1f);
        yield return new WaitForSeconds(1f);
        StartCoroutine(TextFade());
        StartCoroutine(MoveMusicbox());
    }

    IEnumerator TextFade(bool alpha = true)
    {
        float scale = 0.002f;
        WaitForSeconds wait = new WaitForSeconds(0.01f);

        // slowly fade alpha to 1 and scale to 1.2
        if (alpha)
        {
            for (float i = text.color.a; i < 1; i += 0.01f)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, i);
                text.transform.localScale = new Vector3(text.transform.localScale.x + scale, text.transform.localScale.y + scale, text.transform.localScale.z + scale);
                yield return wait;
            }
            StartCoroutine(TextFade(false));
        }
        else
        {
            for (float i = text.color.r; i >= 0; i -= 0.01f)
            {
                text.color = new Color(i, text.color.g, text.color.b, text.color.a);
                text.transform.localScale = new Vector3(text.transform.localScale.x + scale, text.transform.localScale.y + scale, text.transform.localScale.z + scale);
                yield return wait;
            }
        }
    }

    IEnumerator MoveMusicbox()
    {
        if (waypointIndex <= waypoints.Length) currentWaypoint = waypoints[waypointIndex];
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        // if player is inside transform.position, move transform to currentWaypoint
        yield return new WaitUntil(() => inside);
        waypointMoving = true;
        while (Vector3.Distance(transform.position, currentWaypoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, 0.1f);
            yield return wait;
        }
        waypointMoving = false;
        inside = false;
        if (waypoints.Length > 1)
        {
            Destroy(waypoints[waypointIndex].gameObject);
            waypointIndex++;
            StartCoroutine(MoveMusicbox());
        }
        else
        {
            Debug.Log("Second last waypoint, turn item light on");
        }
    }

    void MoveAway()
    {
        float distance = Vector3.Distance(transform.position, GameManager.Instance.player.transform.position);
        while (!waypointMoving)
        {
            if (distance < warningDistance)
            {
                int random = Random.Range(1, 6);
                string sound = "MB alert " + random;
                AudioManager.Instance.PlaySoundOneShot(monsterAudioID, sound, 0.8f, 1f);
                distance = Vector3.Distance(transform.position, GameManager.Instance.player.transform.position);
                warningCounter++;
                Debug.Log("WARNING " + warningCounter);
            }
            if (warningCounter > 3)
            {
                AudioManager.Instance.PlaySoundOneShot(monsterAudioID, "MB attack", 1f, 1f);
                Debug.Log("YOU LOST");
            }
        }
    }
}
