
using System;
using UnityEngine;


public class TouchScreenInputSource: MonoBehaviour{

	public Vector2[] buttonsLocation;
	public float buttonsMaxRadius = 100;
	public float axisMaxRadius = 50;

	public float minAxisDelta = 0.1f;
	private Vector2 lastAxis = Vector2.zero;

	private bool[] buttonsDown;


	public void Awake(){
		Input.simulateMouseWithTouches = false;
		buttonsDown = new bool[PlayerInputModel.NumButtonsSupported];
	}


	private int CheckButtonsRange(Vector2 pos){
		for(int i = 0 ; i < buttonsLocation.Length ; ++i){
			if (Vector2.Distance(pos, buttonsLocation[i]) <= buttonsMaxRadius){
				// in radial range, now, check frindges against other buttons
				// TODO: optimize this (store intermediate results)
				bool acceptPoint = true;
				for(uint j = 0 ; j < buttonsLocation.Length ; ++j){
					if (j == i) continue;
					Vector2 midlePoint = (buttonsLocation[j] + buttonsLocation[i]) * 0.5f;
					Vector2 frindgeVec = buttonsLocation[j] - buttonsLocation[i];
					frindgeVec = new Vector2(-frindgeVec.y, frindgeVec.x);
					if (Vector2.Dot(frindgeVec, pos-buttonsLocation[i]) > 0){
						// fail
						acceptPoint = false;
						break;
					}
				}
				if (acceptPoint){
					return i;
				}
			}
		}
		return -1;
	}

	public void Update(){

		// Check what buttons the touches are hitting
		bool[] newButtonsDown = new bool[PlayerInputModel.NumButtonsSupported];
		Vector2 axisPos = Vector2.zero;
		if (Input.touchCount > 0){
			int buttonId;
			foreach (Touch touch in Input.touches){
				buttonId = CheckButtonsRange(touch.position);
				if (buttonId >= 0){
					// got a button pressed
					newButtonsDown[buttonId] = true;
					if (buttonId == 0){
						axisPos = touch.position;
					}
				}
			}
		}

		Vector3 newAxis = Vector3.zero;
		if (newButtonsDown[0]){
			newAxis = axisPos - buttonsLocation[0];
			newAxis = newAxis / axisMaxRadius;
		}

		if (Vector2.Distance(lastAxis, newAxis) >= minAxisDelta){
			StateManager.Instance.AddEvent(new AxisInputEvent(new FixedVector3(newAxis.x, 0, newAxis.y)));
			lastAxis = newAxis;
		}

		// In the end check the differences between old and new touched buttons
		for (uint i = 1 ; i < buttonsDown.Length ; ++i){
			if (buttonsDown[i] != newButtonsDown[i]){
				// generate a button event
				StateManager.Instance.AddEvent(new ButtonInputEvent(i,newButtonsDown[i]));
			}
		}

	}

}

