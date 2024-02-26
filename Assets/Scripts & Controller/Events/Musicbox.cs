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

    [SerializeField] GameObject itemLight;

    [SerializeField] Transform[] waypoints;
    Transform currentWaypoint;
    Transform previousWaypoint;
    int waypointIndex = 0;
    Collider waypointTrigger;

    bool inside = false;
    bool started = false;

    int warningCounter = 0;

    bool exited = false;

    bool end = false;

    bool once = false;

    YardEvent yardEvent;

    [SerializeField] ItemController musicBoxObject;
    [SerializeField] Transform musicBoxLid;

    void Start()
    {
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(this);
        monsterAudioID = monsterAudio.GetInstanceID();
        yardEvent = FindObjectOfType<YardEvent>();
    }

    public void CustomUpdate(float deltaTime)
    {
        if (!started && yardEvent.musicBox) GetCloser();
        if (waypoints[waypointIndex].childCount > 0) exited = waypoints[waypointIndex].GetComponentInChildren<MusicboxWay>().exited;
        if (yardEvent.musicBox) MusicBoxVolume();
        if (!started && yardEvent.musicBox) AdjustEnvironmentVolume();
        else if (started && !once) 
        {
            AudioManager.Instance.PauseAudio(AudioManager.Instance.environment);
            once = true;
        }
    }

    void GetCloser()
    {
        if (!AudioManager.Instance.IsPlaying(gameObject.GetInstanceID())) AudioManager.Instance.PlayAudio(gameObject.GetInstanceID(), 1f, 1f, true);
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
            if (Vector3.Distance(transform.position, waypoints[waypoints.Length-1].position) < 0.1f)
            {
                if (end) return;
                StopAllCoroutines();
                StartCoroutine(FadeOut());
                itemLight.SetActive(true);
                GameManager.Instance.player.lightAvail = true;
                end = true;
                return;
            }
            if (inside) return;
            else inside = true;
            if (!started)
            {
                started = true;
                StartCoroutine(FadeToBlack());
                door.CloseDoor();
                door.locked = true;
                StartCoroutine(LeavingWay());
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
        GameManager.Instance.GameplayEvent();
        if (GameManager.Instance.player.flashlight.enabled) GameManager.Instance.player.LightSwitch();
        GameManager.Instance.player.lightAvail = false;
        AudioManager.Instance.PlaySoundOneShot(monsterAudioID, "MB grunt", 0.8f, 1f);
        yield return new WaitForSeconds(1f);
        StartCoroutine(TextFade());
        StartCoroutine(MoveMusicbox());
    }

    IEnumerator FadeOut()
    {
        foreach (Transform waypoint in waypoints)
        {
            waypoint.gameObject.SetActive(false);
        }
        musicBoxObject.enabled = false;
        text.gameObject.SetActive(false);
        for (float i = image.color.a; i > 0; i -= 0.01f)
        {
            image.color = new Color(0, 0, 0, i);
            yield return new WaitForSeconds(0.01f);
        }
        door.locked = false;
        door.OpenDoor();
        AudioManager.Instance.FadeOut(gameObject.GetInstanceID(), 5f);
        yield return new WaitUntil(() => !AudioManager.Instance.IsPlaying(gameObject.GetInstanceID()));
        for (float i = musicBoxLid.localEulerAngles.x; i > 0; i -= 0.5f)
        {
            musicBoxLid.localEulerAngles = new Vector3(i, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }
        musicBoxObject.enabled = true;
        AudioManager.Instance.UnpauseAudio(AudioManager.Instance.environment);
        AudioManager.Instance.SetAudioVolume(AudioManager.Instance.environment, 0.1f);
        this.enabled = false;
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
        Debug.Log("MoveMusicbox started");  
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        // when starting the event, previousWaypoint is null, so set it to the first waypoint
        if (previousWaypoint == null) previousWaypoint = waypoints[waypointIndex]; 
        else 
        {
            previousWaypoint.gameObject.SetActive(false);
            previousWaypoint = currentWaypoint;
        }

        // if waypointIndex+1 is out of bounds of waypoints length, set currentWaypoint to the last waypoint
        if (waypointIndex == waypoints.Length - 1) yield break;
        else currentWaypoint = waypoints[waypointIndex+1];
        currentWaypoint.gameObject.SetActive(true);

        yield return new WaitUntil(() => inside);

        GameManager.Instance.GameplayEvent();

        while (Vector3.Distance(transform.position, currentWaypoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, 0.1f);
            yield return wait;
        }

        GameManager.Instance.ResumeFromEventScene();

        inside = false;

        // preventing first waypoint to be deactivated prematurely
        if (!waypoints[waypointIndex+1].gameObject.activeSelf) previousWaypoint.gameObject.SetActive(false);
        // activates the players path of the next waypoint
        currentWaypoint.GetChild(0).gameObject.SetActive(true);
        waypointIndex++;
        StartCoroutine(MoveMusicbox());

        if (warningCounter == 3)
        {
            AudioManager.Instance.PlaySoundOneShot(monsterAudioID, "MB attack", 1f, 1f);
            Debug.Log("DEATH!!!!!!!!!!!!!");
        }
    }

    IEnumerator LeavingWay()
    {
        Debug.Log("\"LeavingWay\" started");
        yield return new WaitUntil(() => exited);
        Debug.Log("\"LeavingWay\" triggered");
        int random = Random.Range(1, 6);
        string sound = "MB alert " + random;
        AudioManager.Instance.PlaySoundOneShot(monsterAudioID, sound, 0.8f, 1f);
        warningCounter++;
        Debug.Log("WARNING " + warningCounter);
        StartCoroutine(TimeToLeave());
        yield return new WaitUntil(() => !exited);
        StartCoroutine(LeavingWay());
    }

    IEnumerator TimeToLeave()
    {
        int counter = warningCounter;
        yield return new WaitForSeconds(15f);
        if (exited && counter == warningCounter)
        {
            AudioManager.Instance.PlaySoundOneShot(monsterAudioID, "MB attack", 1f, 1f);
            Debug.Log("DEATH!!!!!!!!!!!!!");
        }
    }

    void MusicBoxVolume()
    {
        // Calculate the angle between player and music box
        Vector3 direction = transform.position - GameManager.Instance.player.transform.position;
        float angle = Vector3.Angle(GameManager.Instance.player.transform.forward, direction);

        // Inverse relationship: lower angle (facing) -> higher volume, higher angle (looking away) -> lower volume
        float volume = Mathf.SmoothStep(1f, 0.5f, angle / 180f); // Reverse order of arguments

        // Clamp volume to your desired range (0.5f minimum, 1f maximum)
        volume = Mathf.Clamp(volume, 0.5f, 1f);

        // Set music box volume
        AudioManager.Instance.SetAudioVolume(gameObject.GetInstanceID(), volume);
    }

    void AdjustEnvironmentVolume()
    {
        // Calculate distance between player and object
        float distance = Vector3.Distance(transform.position, GameManager.Instance.player.transform.position);

        // Customize distance range and volume curve to your preferences
        float targetVolume = Mathf.Lerp(0.02f, 0.1f, Mathf.Clamp01(distance / 5f)); // Adjust values as needed

        // Set environment music volume
        AudioManager.Instance.SetAudioVolume(AudioManager.Instance.environment, targetVolume);
    }
}
