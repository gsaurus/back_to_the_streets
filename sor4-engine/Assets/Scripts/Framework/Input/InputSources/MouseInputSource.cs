
using System;
using UnityEngine;
using System.Collections;


namespace RetroBread{


	public class MouseInputSource: MonoBehaviour{

		public float minDelayBetweenEvents = 0.1f; // in seconds

	#if !UNITY_IPHONE && !UNITY_ANDROID

		// Control events sent
		private double lastEventTimeStamp = 0.0;
		private float latestAxis = 0.0f;
		private bool isCoroutineRunning = false;


		public void Awake(){
			// nothing to do
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
			if (Network.NetworkCenter.Instance.IsConnected()) {
				float axis = Input.GetAxis("Horizontal");
				SendAxis(axis);
			}
		}

	#endif

	}


}

