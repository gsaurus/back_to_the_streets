using System;
using UnityEngine;
using System.Collections;


namespace RetroBread{


	public class TouchScreenInputSource: MonoBehaviour{

		public float minDelayBetweenEvents = 0.1f; // in seconds

		public float touchesMultFactor = 0.002f;

	#if UNITY_IPHONE || UNITY_ANDROID

		// Control timing of events sent
		private double lastEventTimeStamp = 0.0;
		private float latestAxis = 0.0f;
		private bool isCoroutineRunning = false;

		private float lastTouchPos = 0;


		public void Awake(){
			// Nothing atm
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
			StateManager.Instance.AddEvent(new AxisInputEvent(newAxis));
		}




		public void Update(){
			if (Network.NetworkCenter.Instance.IsConnected() && Input.touchCount > 0){
				Touch touch = Input.touches[0];
				switch (touch.phase) {
					case TouchPhase.Began: {
						lastTouchPos = touch.position.x;
					}break;
					case TouchPhase.Moved: {
						SendAxis((touch.position.x - lastTouchPos) * touchesMultFactor);
						lastTouchPos = touch.position.x;
					}break;
					case TouchPhase.Ended:
					case TouchPhase.Canceled:{
						SendAxis(0);
					}break;
				}

//				if (touch.phase == TouchPhase.Moved){
//					SendAxis((touch.deltaPosition.x) * touchesMultFactor);
//				}else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
//					SendAxis(0);
//				}

			}

		}
	#endif

	}



}
