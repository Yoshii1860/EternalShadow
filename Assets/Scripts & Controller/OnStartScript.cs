using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartScript : MonoBehaviour
{
    [SerializeField] GameObject girl;
    
    // Start is called before the first frame update
    void Start()
    {
        girl.SetActive(false);
        GameManager.Instance.StartGame();
        Invoke("SetAndPlay", 2f);
    }

    void SetAndPlay()
    {
        int id = AudioManager.Instance.playerSpeaker.GetInstanceID();
        AudioManager.Instance.PlaySoundOneShot(id, "player1", 0.8f, 1f, false);
    }
}
