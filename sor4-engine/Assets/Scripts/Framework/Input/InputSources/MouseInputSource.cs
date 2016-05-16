
using System;
using UnityEngine;
using System.Collections;


namespace RetroBread{


	public class MouseInputSource: MonoBehaviour{

		const float minDelayBetweenEvents = 0.1f; // in seconds

		public static bool useMouseAngle = true; //false;

		public static float mouseSensivity = 1.8f;

	#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR

		// Control events sent
		private double lastEventTimeStamp = 0.0;
		private float latestAxis = 0.0f;
		private bool isCoroutineRunning = false;

		private float previousAxis;
		private float previousMousePosition;
		private bool angledMouseIsActive = false;


		public void Awake(){
			
		}

		void SendAxis(float axis){
			if (axis == latestAxis) return;
			latestAxis = axis;
			double newTimeStamp = DateTime.Now.TimeOfDay.TotalSeconds;
			if (!isCoroutineRunning) {
				double timeToWait = newTimeStamp - lastEventTimeStamp;
				if (timeToWait > minDelayBetweenEvents){
					AddAxisEventToStateManager(latestAxis);
				}else {
					StartCoroutine(WaitAndSendNextAxis((float)timeToWait));
					newTimeStamp += timeToWait;
				}
			}
			lastEventTimeStamp = newTimeStamp;
		}

		IEnumerator WaitAndSendNextAxis(float timeToWait){
			isCoroutineRunning = true;
			yield return new WaitForSeconds(timeToWait);
			AddAxisEventToStateManager(latestAxis);
			isCoroutineRunning = false;
		}

		void AddAxisEventToStateManager(float newAxis){
//			UnityEngine.Debug.Log("Axis sent: " + newAxis);
			StateManager.Instance.AddEvent(new AxisInputEvent(newAxis));
		}


		public void Update(){
			float axis;
			float newAxis = 0;
			axis = Input.GetAxis ("Horizontal Keyboard") * Screen.width;
			if (axis == 0) axis = Input.GetAxis ("Horizontal Joystick") * Screen.width;
			if (axis == 0) {
				if (useMouseAngle && (angledMouseIsActive || Input.mousePosition.x != previousMousePosition)) {
					previousMousePosition = Input.mousePosition.x;
					axis = 1 - (2 * Input.mousePosition.x / Screen.width);
					axis *= mouseSensivity;
					axis += 10005; // mega hammer to flag this kind of controls
					angledMouseIsActive = true;
				} else {
					axis = Input.GetAxis ("Horizontal");
					axis /= Screen.dpi == 0 ? 1 : Screen.dpi;
					axis *= mouseSensivity;
					if (angledMouseIsActive) previousAxis = 0;
					angledMouseIsActive = false;
				}
			} else {
				if (angledMouseIsActive) previousAxis = 0;
				angledMouseIsActive = false;
			}
			newAxis = Mathf.Lerp(previousAxis, axis, 0.5f);
			previousAxis = axis;
			SendAxis(newAxis);
		}

	#endif

	}


}

