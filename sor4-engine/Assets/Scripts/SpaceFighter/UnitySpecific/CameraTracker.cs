using UnityEngine;
using System.Collections;

public class CameraTracker : MonoBehaviour {

	private float lerpValue = 0.01f;

	void Start(){
		Camera mainCamera = Camera.main;
		mainCamera.transform.position = transform.position + new Vector3(4,3,0);
		mainCamera.transform.LookAt(transform.position);
	}

	// Update is called once per frame
	void Update () {
		Camera mainCamera = Camera.main;

		mainCamera.transform.LookAt(transform.position);
		Vector3 target = new Vector3(Mathf.Lerp(0f, transform.position.x, 0.95f),
		                             Mathf.Lerp(12f, transform.position.y + 3, 0.4f),
		                             -12f
		                             );

		mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, lerpValue);
		if (lerpValue < 0.1f){
			lerpValue = Mathf.Lerp(lerpValue, 0.1f, 0.01f);
		}
	}

}
