using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SpaceInputHandler : MonoBehaviour
{
	private uint latestSavedState = 0;

	void Update(){


		// Note: ofcourse this should use GetAxis, GetButtonDown and GetButtonUp
		// Movement
		if (Input.GetKeyDown("left")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Left;
			currentEvent.state = ShipInputState.Pressed;
			StateManager.Instance.AddEvent(currentEvent);
		}else if (Input.GetKeyUp("left")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Left;
			currentEvent.state = ShipInputState.Released;
			StateManager.Instance.AddEvent(currentEvent);
		}
		if (Input.GetKeyDown("right")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Right;
			currentEvent.state = ShipInputState.Pressed;
			StateManager.Instance.AddEvent(currentEvent);
		}else if (Input.GetKeyUp("right")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Right;
			currentEvent.state = ShipInputState.Released;
			StateManager.Instance.AddEvent(currentEvent);
		}
		if (Input.GetKeyDown("up")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Up;
			currentEvent.state = ShipInputState.Pressed;
			StateManager.Instance.AddEvent(currentEvent);
		}else if (Input.GetKeyUp("up")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Up;
			currentEvent.state = ShipInputState.Released;
			StateManager.Instance.AddEvent(currentEvent);
		}
		if (Input.GetKeyDown("down")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Down;
			currentEvent.state = ShipInputState.Pressed;
			StateManager.Instance.AddEvent(currentEvent);
		}else if (Input.GetKeyUp("down")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Down;
			currentEvent.state = ShipInputState.Released;
			StateManager.Instance.AddEvent(currentEvent);
		}

		// Jump
		if (Input.GetKeyDown("c")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Jump;
			currentEvent.state = ShipInputState.Pressed;
			StateManager.Instance.AddEvent(currentEvent);
		}else if (Input.GetKeyUp("c")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Jump;
			currentEvent.state = ShipInputState.Released;
			StateManager.Instance.AddEvent(currentEvent);
		}

		// Fire
		if (Input.GetKeyDown("x")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Fire;
			currentEvent.state = ShipInputState.Pressed;
			StateManager.Instance.AddEvent(currentEvent);
		}else if (Input.GetKeyUp("x")) {
			ShipInputEvent currentEvent = new ShipInputEvent();
			currentEvent.type = ShipInputType.Fire;
			currentEvent.state = ShipInputState.Released;
			StateManager.Instance.AddEvent(currentEvent);
		}
	

		// handle testing save/load states
		//HandleSaveLoadStateKeys();
	}



	void HandleSaveLoadStateKeys(){

		if (Input.GetKeyDown("1")) {
			latestSavedState = StateManager.Instance.SaveBufferedState();
		}else if (Input.GetKeyDown("2")){
			StateManager.Instance.LoadBufferedState(latestSavedState, true);
		}else if (Input.GetKeyDown("3")){
			StateManager.Instance.LoadBufferedState(latestSavedState + 50, false);
		}



	}


}

