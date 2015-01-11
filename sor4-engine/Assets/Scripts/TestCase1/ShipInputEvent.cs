using System;
using System.Collections;
using System.Collections.Generic;




public enum ShipInputType{
	Left,
	Right,
	Up,
	Down,
	Jump,
	Fire
}

public enum ShipInputState{
	Pressed,
	Released
}

[Serializable]
public class ShipInputEvent: Event{
	public ShipInputType type;
	public ShipInputState state;
}
