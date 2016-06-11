using UnityEngine;
using System.Collections;

public class CameraTracker : MonoBehaviour {

	private Vector3 previousPos;
	private Quaternion previousRot;

	private static Vector3 originalPosition;
	private static Vector3 originalRotation;

	private Vector3 demoLookAtPosition;


	static CameraTracker(){
		Camera mainCamera = Camera.main;
		originalPosition = new Vector3 (0, 4, 3);//mainCamera.transform.position;
		originalRotation = new Vector3(28, 180, 0); //mainCamera.transform.localEulerAngles;
	}

	// Update is called once per frame
	void Update() {
		Camera mainCamera = Camera.main;
		if (GuiMenus.Instance.IsDemoPlaying()) {
			Vector3 targetCenter;
			WorldModel world = GuiMenus.Instance.demoStateManager.state.MainModel as WorldModel;
			Vector3 avgPos = new Vector3();
			int activeSkiers = 0;
			foreach (SkierModel skier in world.skiers) {
				if (skier != null) {
					avgPos += new Vector3 ((float)skier.x, 0, (float)skier.y);
					++activeSkiers;
				}
			}
			avgPos /= activeSkiers;
			avgPos.y = 16;
//			if (Vector3.Distance(mainCamera.transform.position, avgPos) > 90) {
//				mainCamera.transform.position = avgPos;
//				mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z + 12);
//			}

			targetCenter = new Vector3(
				Mathf.Lerp(mainCamera.transform.position.x, avgPos.x + 15, 0.9f *Time.deltaTime),
				Mathf.Lerp(mainCamera.transform.position.y, avgPos.y, 0.15f *Time.deltaTime),
				Mathf.Lerp(mainCamera.transform.position.z, avgPos.z - 20, 0.7f *Time.deltaTime)
			);
			mainCamera.transform.position = targetCenter;

			avgPos.z -= 7;
			demoLookAtPosition = new Vector3(
				Mathf.Lerp(demoLookAtPosition.x, avgPos.x, 2.5f * Time.deltaTime),
				0,
				Mathf.Lerp(demoLookAtPosition.z, avgPos.z, 3.0f * Time.deltaTime)
			);
			Vector3 angles = mainCamera.transform.localEulerAngles;
			float previousAngleZ = angles.z;
			angles.z = 0;
			mainCamera.transform.LookAt(demoLookAtPosition);
			if (Vector3.Distance(angles, mainCamera.transform.localEulerAngles) > 0.2f) {
				mainCamera.transform.localEulerAngles = Vector3.Lerp(angles, mainCamera.transform.localEulerAngles, 0.2f * Time.deltaTime);
			}
			angles = mainCamera.transform.localEulerAngles;
			angles.z = Mathf.Lerp(previousAngleZ, 330.0f, 0.2f * Time.deltaTime);
			mainCamera.transform.localEulerAngles = angles;
		} else {
			mainCamera.transform.position = new Vector3(transform.position.x, 0, transform.position.z) + originalPosition;
			Vector3 originalSkierAngles = transform.localEulerAngles;
			mainCamera.transform.localEulerAngles = new Vector3 (originalRotation.x, Mathf.Lerp (mainCamera.transform.localEulerAngles.y, Mathf.Lerp (originalRotation.y, originalSkierAngles.y, 0.2f), 0.1f));
		}
	}

}
