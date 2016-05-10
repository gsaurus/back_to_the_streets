
using System;
using UnityEngine;
using System.Collections;


namespace RetroBread{


	public class MouseInputSource: MonoBehaviour{

		const float minDelayBetweenEvents = 0.1f; // in seconds

	#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR

		// Control events sent
		private double lastEventTimeStamp = 0.0;
		private float latestAxis = 0.0f;
		private bool isCoroutineRunning = false;

		private float previousAxis;


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
			float axis = Input.GetAxis("Horizontal");
			axis /= Screen.dpi == 0 ? 1 : Screen.dpi;
			float axisKeyboard = Input.GetAxis("Horizontal Keyboard") * Screen.width;
			float axisJoystick = Input.GetAxis("Horizontal Joystick") * Screen.width;
			if (axis == 0) axis = axisKeyboard;
			if (axis == 0) axis = axisJoystick;
			float newAxis = Mathf.Lerp(previousAxis, axis, 0.5f);
			previousAxis = axis;
			SendAxis(newAxis);
		}

	#endif

	}


}

