using UnityEngine;
using System.Collections;

public class CameraTracker : MonoBehaviour {

	private const float minCamX = -20;
	private const float maxCamX = 20;

	private const float lerpValue = 0.1f;

	void Start(){
		
	}

	private float GetHalfCamWidth(Camera camera){
		return camera.orthographicSize * camera.aspect;
	}

	// Update is called once per frame
	void Update() {
		Camera mainCamera = Camera.main;
	
		float halfCamWidth = GetHalfCamWidth(mainCamera);
		Vector3 target = new Vector3(
			Mathf.Clamp(transform.position.x, minCamX + halfCamWidth, maxCamX - halfCamWidth),
			mainCamera.transform.position.y,
			mainCamera.transform.position.z
		);

		mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, lerpValue);
	}

}
