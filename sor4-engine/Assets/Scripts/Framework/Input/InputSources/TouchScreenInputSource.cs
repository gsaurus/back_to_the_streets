using System;
using UnityEngine;
using System.Collections;


namespace RetroBread{


	public class TouchScreenInputSource: MonoBehaviour{

		public float axisMaxRadius = 0.2f;
		public float minDelayBetweenEvents = 0.1f; // in seconds

	#if UNITY_IPHONE || UNITY_ANDROID
	
		private bool isTouchingDown = false;
		private float axisCenter = 0.0f;

		private double lastEventTimeStamp = 0.0;
		private float latestAxis = 0.0f;
		private bool isCoroutineRunning = false;


		public void Awake(){
			// Nothing atm
		}



		void SendAxis(float axis){
			if (axis == latestAxis) return;
			latestAxis = axis;
			double newTimeStamp = DateTime.Now.TimeOfDay.TotalSeconds;
			if (!isCoroutineRunning) {
				float timeToWait = (float)(newTimeStamp - lastEventTimeStamp) - minDelayBetweenEvents;
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

			if (Input.touchCount > 0){
				Touch touch = Input.touches[0];

				if (isTouchingDown){
					float deltaX = touch.position.x - axisCenter;
					// Clamp to limits
					if (deltaX > axisMaxRadius){
						axisCenter = touch.position.x - axisMaxRadius;
						deltaX = axisMaxRadius;
					}else if (deltaX < axisMaxRadius) {
						axisCenter = touch.position.x + axisMaxRadius;
						deltaX = axisMaxRadius;
					}
					SendAxis(deltaX);
				}else {
					// first touch down, selects the axis center (no event sent)
					// TODO: well we can send a sync start here :D
					axisCenter = touch.position.x;
					isTouchingDown = true;
				}
			}

		}
	#endif

	}



}
