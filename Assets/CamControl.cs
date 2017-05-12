using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour {

    Camera cam;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cam.transform.position += new Vector3(-0.1f, 0, 0.1f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            cam.transform.position += new Vector3(0.1f, 0, -0.1f);

        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            cam.transform.position += new Vector3(-0.1f, 0, -0.1f);

        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            cam.transform.position += new Vector3(0.1f, 0, 0.1f);

        }
    }
}
