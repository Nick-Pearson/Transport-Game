using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour {
    //time in seconds
    public float time = 0;

    public float dayLength = 60;

    //light object
    public Light directionalLight;

    public float maxLightIntensity = 1;

	// Use this for initialization
	void Start () {
	    if(directionalLight == null)
        {
            directionalLight = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>();
        }
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;

        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((time / dayLength) * 180, 330, 0));

        if(time > dayLength)
        {
            float nightAmount = (time - dayLength) / dayLength;

            if(nightAmount > 0 && nightAmount < 0.1f)
            {
                directionalLight.intensity = (1 - (nightAmount * 10)) * maxLightIntensity;
            }
            else if(nightAmount > 0.9f && nightAmount < 1)
            {
                directionalLight.intensity = ((nightAmount - 0.9f) * 10) * maxLightIntensity;
            }
        }

        time = time % (dayLength * 2);
	}
}
