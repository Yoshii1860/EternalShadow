using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
	private float time = 0.05f;
	private float timer;
	
	void Start ()
	{
		timer = transform.parent.GetComponent<ParticleSystem>().main.duration;
	}

	public void StartFlicker()
	{
		StartCoroutine(Flicker());
	}
	
	IEnumerator Flicker()
	{
		float elapsedTime = 0f;
		while(timer > 0)
		{
			elapsedTime += Time.deltaTime;
			if (elapsedTime > time)
			{
				GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
				elapsedTime = 0;
			}	
			timer -= Time.deltaTime;
			yield return null;
		}
		GetComponent<Light>().enabled = false;
	}
}
