
using System;
using UnityEngine;
using System.Collections;


namespace RetroBread{


	public class KeyboardInputSource: MonoBehaviour{

		public float minDelayBetweenEvents = 0.1f; // in seconds

	#if !UNITY_IPHONE && !UNITY_ANDROID

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
				float timeToWait = newTimeStamp - lastEventTimeStamp - minDelayBetweenEvents;
				if (timeToWait <= 0.0f){
					AddAxisEventToStateManager(latestAxis);
				}else {
					StartCoroutine(WaitAndSendNextAxis(timeToWait));
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
			StateManager.Instance.AddEvent(new AxisInputEvent(newAxis));
		}


		public void Update(){

			float axis = Input.GetAxis("Horizontal");
			SendAxis(axis);

		}

	#endif

	}


}

