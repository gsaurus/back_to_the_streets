using UnityEngine;
using System.Collections;

public class CameraTracker : MonoBehaviour {

	private float lerpValue = 0.01f;

	private Vector3 previousPos;
	private Quaternion previousRot;

	void Start(){
		Camera mainCamera = Camera.main;
		mainCamera.transform.position = transform.position + new Vector3(4,3,0);
		mainCamera.transform.LookAt(transform.position);
		previousPos = mainCamera.transform.position;
		previousRot = mainCamera.transform.rotation;
	}

	// Update is called once per frame
	void Update() {
		Camera mainCamera = Camera.main;
	
		Vector3 target = new Vector3(Mathf.Lerp(0f, transform.position.x, 0.95f),
		                             Mathf.Lerp(12f, transform.position.y + 2, 0.6f),
		                             -12f
		                             );

		mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, lerpValue);
		mainCamera.transform.LookAt(transform.position);
		if (lerpValue < 0.1f){
			lerpValue = Mathf.Lerp(lerpValue, 0.1f, 0.01f);
		}
		mainCamera.transform.position = Vector3.Lerp(previousPos, mainCamera.transform.position, 0.2f);
		mainCamera.transform.rotation = Quaternion.Lerp(previousRot, mainCamera.transform.rotation, 0.2f);
		previousPos = mainCamera.transform.position;
		previousRot = mainCamera.transform.rotation;
	}

}
