using UnityEngine;
using System.Collections;

public class CameraTracker : MonoBehaviour {

	private float lerpValue = 1.0f; //0.02f;

	private Vector3 previousPos;
	private Quaternion previousRot;

	private Vector3 originalPosition;
	private Vector3 originalRotation;

	void Start(){
		Camera mainCamera = Camera.main;
		originalPosition = mainCamera.transform.position;
		originalRotation = mainCamera.transform.localEulerAngles;
//		mainCamera.transform.position = transform.position + new Vector3(4,3,0);
//		mainCamera.transform.LookAt(transform.position);
//		previousPos = mainCamera.transform.position;
//		previousRot = mainCamera.transform.rotation;
	}

	// Update is called once per frame
	void Update() {
		Camera mainCamera = Camera.main;
		mainCamera.transform.position = new Vector3(transform.position.x, 0, transform.position.z) + originalPosition;
		Vector3 originalSkierAngles = transform.localEulerAngles;
		mainCamera.transform.localEulerAngles = new Vector3(originalRotation.x, Mathf.Lerp(mainCamera.transform.localEulerAngles.y, Mathf.Lerp(originalRotation.y, originalSkierAngles.y, 0.2f), 0.1f));
//	originalSkierAngles
//		Vector3 target = new Vector3(transform.position.x, //Mathf.Lerp(0f, transform.position.x, 0.95f),
//		                             transform.position.y + 4,
//		                             -12f
//		                             );
//
//		mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, lerpValue);
//		mainCamera.transform.LookAt(transform.position);
//		if (lerpValue < 0.1f){
//			lerpValue = Mathf.Lerp(lerpValue, 0.1f, 0.01f);
//		}
////		mainCamera.transform.position = Vector3.Lerp(previousPos, mainCamera.transform.position, 0.2f);
////		mainCamera.transform.rotation = Quaternion.Lerp(previousRot, mainCamera.transform.rotation, 0.2f);
////		previousPos = mainCamera.transform.position;
////		previousRot = mainCamera.transform.rotation;
	}

}
