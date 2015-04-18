using System;
using UnityEngine;


namespace RetroBread{


	public class TouchScreenInputSource: MonoBehaviour{

		public Vector2[] buttonsLocation;
		public float buttonsMaxRadius = 1f;
		public float axisMaxRadius = 0.2f;

		public float minAxisDelta = 0.1f;

	#if UNITY_IPHONE || UNITY_ANDROID
		private Vector2 lastAxis = Vector2.zero;
		private bool[] buttonsDown;


		public void Awake(){
			Input.simulateMouseWithTouches = true;
			buttonsDown = new bool[PlayerInputModel.NumButtonsSupported];
		}


		private int CheckButtonsRange(Vector2 pos){
			float minDist = buttonsMaxRadius;
			float distance;
			int buttonId = -1;
			for(int i = 0 ; i < buttonsLocation.Length ; ++i){
				distance = Vector2.Distance(pos, buttonsLocation[i]);
				if (distance < minDist){
					buttonId = i;
					minDist = distance;
				}
			}
			return buttonId;
		}


		public void Update(){

			// Check what buttons the touches are hitting
			bool[] newButtonsDown = new bool[PlayerInputModel.NumButtonsSupported];
			Vector2 axisPos = Vector2.zero;
			Vector2 relativePosTouch;
			if (Input.touchCount > 0){
				int buttonId;
				foreach (Touch touch in Input.touches){
					relativePosTouch = new Vector2(touch.position.x / Screen.width, touch.position.y / Screen.height);
					buttonId = CheckButtonsRange(relativePosTouch);
					if (buttonId >= 0){
						// got a button pressed
						newButtonsDown[buttonId] = true;
						if (buttonId == 0){
							axisPos = relativePosTouch;
						}
					}
				}
			}

			Vector2 newAxis = Vector2.zero;
			if (newButtonsDown[0]){
				newAxis = axisPos - buttonsLocation[0];
				float axisStrength = newAxis.magnitude / axisMaxRadius;
				if (axisStrength > 1) axisStrength = 1;
				newAxis = newAxis.normalized * axisStrength;
			}

			if (
				(Vector2.Distance(lastAxis,newAxis) >= minAxisDelta)
				|| (newAxis == Vector2.zero && lastAxis != Vector2.zero)
				|| (newAxis != Vector2.zero && lastAxis == Vector2.zero)
				|| (newAxis != Vector2.zero && lastAxis == Vector2.zero && newAxis.x > 0 != lastAxis.x > 0)
			){
				StateManager.Instance.AddEvent(new AxisInputEvent(new FixedVector3(newAxis.x, 0, newAxis.y)));
				lastAxis = newAxis;
			}

			// In the end check the differences between old and new touched buttons
			for (uint i = 1 ; i < buttonsDown.Length ; ++i){
				if (buttonsDown[i] != newButtonsDown[i]){
					// generate a button event
					StateManager.Instance.AddEvent(new ButtonInputEvent(i-1,newButtonsDown[i]));
				}
			}
			buttonsDown = newButtonsDown;

		}
	#endif

	}



}
