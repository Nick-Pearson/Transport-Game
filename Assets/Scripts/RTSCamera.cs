using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RTSCamera : MonoBehaviour {
    public Vector2 rotationSenitivity = new Vector2(0.5f, -0.5f);
    public Vector2 translationSenitivity = new Vector2(1,1);
    public float zoomSensitivity = 1;

    public float maxHeight = 100;

    bool isRotating = false;
    Vector3 mouseStartPositon;
    Vector3 startRotation;

    List<ICameraObserver> observers =  new List<ICameraObserver>();

	// Update is called once per frame
	void Update () {
        bool hasChanged = false;

        //Listen for RMB
        if(Input.GetKey(KeyCode.Mouse1))
        {
            if (!isRotating)
            {
                isRotating = true;
                mouseStartPositon = Input.mousePosition;
                startRotation = transform.rotation.eulerAngles;
            }
            else
            {
                Vector3 curMousePosition = Input.mousePosition;
                transform.rotation = Quaternion.Euler(new Vector3(startRotation.x + ((curMousePosition.y - mouseStartPositon.y) * rotationSenitivity.y), startRotation.y + ((curMousePosition.x - mouseStartPositon.x) * rotationSenitivity.x), 0));
            }

            hasChanged = true;
        }
        else
        {
            isRotating = false;
        }

        transform.Translate(Input.GetAxis("Horizontal"), translationSenitivity.x * Input.GetAxis("Vertical") * Mathf.Sin(transform.rotation.eulerAngles.x * (3.14f / 180)), translationSenitivity.y * Input.GetAxis("Vertical") * Mathf.Cos(transform.rotation.eulerAngles.x * (3.14f / 180)) + (Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity), Space.Self);

        if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            hasChanged = true;

        if (hasChanged)
            UpdateObservers();
	}

    public void Subscribe(ICameraObserver obs)
    {
        observers.Add(obs);
    }

    void UpdateObservers()
    {
        foreach (ICameraObserver i in observers)
            i.OnCameraMove(transform.position);
    }
}
