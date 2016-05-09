using UnityEngine;
using System.Collections;

public class CameraTracker : MonoBehaviour {

	private Vector3 previousPos;
	private Quaternion previousRot;

	private static Vector3 originalPosition;
	private static Vector3 originalRotation;

	static CameraTracker(){
		Camera mainCamera = Camera.main;
		originalPosition = mainCamera.transform.position;
		originalRotation = mainCamera.transform.localEulerAngles;
	}

	// Update is called once per frame
	void Update() {
		Camera mainCamera = Camera.main;
		mainCamera.transform.position = new Vector3(transform.position.x, 0, transform.position.z) + originalPosition;
		Vector3 originalSkierAngles = transform.localEulerAngles;
		mainCamera.transform.localEulerAngles = new Vector3(originalRotation.x, Mathf.Lerp(mainCamera.transform.localEulerAngles.y, Mathf.Lerp(originalRotation.y, originalSkierAngles.y, 0.2f), 0.1f));
	}

}
