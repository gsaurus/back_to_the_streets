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
}


// Movement events
[Serializable]
public class AxisInputEvent: Event{
	public FixedVector3 axis;
}

