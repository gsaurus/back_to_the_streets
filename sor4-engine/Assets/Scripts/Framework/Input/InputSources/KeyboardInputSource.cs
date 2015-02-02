
using System;
using UnityEngine;

enum InputSourceAxisDownDirection {Left, Right, Up, Down, None};

class DirectionDownControl{
	// Used to check when up/down/left/right
	// are down (pressed in a single frame)
	
	private bool isLeftDown, isRightDown, isUpDown,isDownDown;
	private bool isLeftHolded, isRightHolded, isUpHolded,isDownHolded;
	
	public void CheckAxisDown(){
		
		// reset down states
		isLeftDown = isRightDown = isUpDown = isDownDown = false;
		
		// Right
		if (Input.GetAxisRaw("Horizontal") > 0) {
			if (!isRightHolded) {
				isRightDown = isRightHolded = true;
			}
		}else{
			isRightHolded = false;
		}
		
		// Left
		if (Input.GetAxisRaw("Horizontal") < 0) {
			if (!isLeftHolded) {
				isLeftDown = isLeftHolded = true;
			}
		}else{
			isLeftHolded = false;
		}
		
		// Up
		if (Input.GetAxisRaw("Vertical") > 0) {
			if (!isUpHolded) {
				isUpDown = isUpHolded = true;
			}
		}else{
			isUpHolded = false;
		}
		
		// Down
		if (Input.GetAxisRaw("Vertical") < 0) {
			if (!isDownHolded) {
				isDownDown = isDownHolded = true;
			}
		}else{
			isDownHolded = false;
		}
		
	}
	
	public bool GetDirectionDown(InputSourceAxisDownDirection direction){
		
		switch (direction) {
			case InputSourceAxisDownDirection.Right: 	return isRightDown;
			case InputSourceAxisDownDirection.Left: 	return isLeftDown;
			case InputSourceAxisDownDirection.Up: 		return isUpDown;
			case InputSourceAxisDownDirection.Down: 	return isDownDown;
			default: return false;
		}
	}
	
} // DirectionDownControl




public class KeyboardInputSource: MonoBehaviour{

	public float minAxisDelta = 0.1f;
	public string[] buttonNames = {"A", "B", "C", "D", "E", "F"};
	private Vector3 lastAxis = Vector3.zero;

	// Variables for double tapping detection
	public bool supportDoubleTap = true;
	public float directionKeyCooler = 0.2f;
	public float walkHorizontalFactor = 0.5f;
	public float walkVerticalFactor = 0.6f;
	private float directionCooler = 0;
	private int tapCount = 0;
	private InputSourceAxisDownDirection tappedDirection = InputSourceAxisDownDirection.None;
	private DirectionDownControl directionControl = new DirectionDownControl();



	private bool CheckBothAxisHoldedAtOnce() {
		return Input.GetAxisRaw("Horizontal") != 0 && Input.GetAxisRaw("Vertical") != 0;
	}
	
	
	
	private bool CheckDoubleDirectionForDirection(InputSourceAxisDownDirection direction, bool hasPreviousTap) {
		if (directionControl.GetDirectionDown(direction)){
			if (hasPreviousTap && tappedDirection == direction) {
				++tapCount;
			}else{
				tappedDirection = direction;
				tapCount = 1;
			}
			directionCooler = directionKeyCooler;
			return true;
		}
		return false;
	}


	private void ResetDoubleDirection(){
		tapCount = 0;
		directionCooler = 0;
		tappedDirection = InputSourceAxisDownDirection.None;
	}
	
	private void CheckDoubleDirection() {

		// Check if it's running
		if (tapCount == 2){

			// if no longer holding any horizontal direction
			if (Input.GetAxisRaw("Horizontal") == 0){
				ResetDoubleDirection();
			}
		}else{
			if (CheckBothAxisHoldedAtOnce()) {
				// both axis hold means no run
				ResetDoubleDirection();
			} else {
				
				// Check current cooler
				if (directionCooler > 0) {
					directionCooler -= Time.deltaTime;
					if (directionCooler <= 0){
						// end of the cooler
						ResetDoubleDirection();
					}
				}
				
				bool hasPreviousTap = directionCooler > 0 && tapCount > 0;
				
				// This unused bool has the purpose of simulating a
				// fake if-else-if on the function calls
				#pragma warning disable 219
				bool fakeIfElseIf =
						   CheckDoubleDirectionForDirection(InputSourceAxisDownDirection.Right, hasPreviousTap)
						|| CheckDoubleDirectionForDirection(InputSourceAxisDownDirection.Left, hasPreviousTap)
						|| CheckDoubleDirectionForDirection(InputSourceAxisDownDirection.Up, hasPreviousTap)
						|| CheckDoubleDirectionForDirection(InputSourceAxisDownDirection.Down, hasPreviousTap)
						;
				#pragma warning restore 219
				
			}
		}
		
	}



	private void ProcessAxis(){

		float walkFactorX = 1;
		float walkFactorZ = 1;
		if (supportDoubleTap){
			CheckDoubleDirection();
			if (tapCount < 2){
				walkFactorX = walkHorizontalFactor;
				walkFactorZ = walkVerticalFactor;
			}
		}

		Vector3 newAxis = new Vector3(Input.GetAxis("Horizontal") * walkFactorX, 0, Input.GetAxis("Vertical")*walkFactorZ);
		if (Vector3.Distance(lastAxis,newAxis) >= minAxisDelta){
			StateManager.Instance.AddEvent(new AxisInputEvent((FixedVector3)newAxis));
			lastAxis = newAxis;
		}

	}


	private void ProcessButtons(){

		for (uint i = 0 ; i < buttonNames.Length ; ++i){
			if (Input.GetButtonDown(buttonNames[i])) {
				StateManager.Instance.AddEvent(new ButtonInputEvent(i,true));
			}else if (Input.GetButtonDown(buttonNames[i])) {
				StateManager.Instance.AddEvent(new ButtonInputEvent(i,false));
			}
		}
	
	}



	public void Update(){

		directionControl.CheckAxisDown();

		ProcessAxis();
		ProcessButtons();
	}

}

