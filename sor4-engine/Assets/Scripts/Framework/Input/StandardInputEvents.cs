using System;
using System.Collections;
using System.Collections.Generic;




// Button events
[Serializable]
public class ButtonInputEvent: Event{
	// id of the button
	public uint button; 
	// pressed state
	public bool isPressed;


	public ButtonInputEvent(uint button, bool isPressed){
		this.button = button;
		this.isPressed = isPressed;
	}

}


// Movement events
[Serializable]
public class AxisInputEvent: Event{
	public FixedVector3 axis;

	public AxisInputEvent(FixedVector3 axis){
		this.axis = axis;
	}

}

