using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;



namespace RetroBread{



	// Button events
	[ProtoContract]
	public class ButtonInputEvent: Event{
		// id of the button
		[ProtoMember(1)]
		public uint button; 
		// pressed state
		[ProtoMember(2)]
		public bool isPressed;


		// Default Constructor
		public ButtonInputEvent(){
			// Nothing to do
		}

		// Constructor
		public ButtonInputEvent(uint button, bool isPressed){
			this.button = button;
			this.isPressed = isPressed;
		}

	}


	// Movement events
	[ProtoContract]
	public class AxisInputEvent: Event{

		[ProtoMember(1)]
		public FixedVector3 axis;

		// Default Constructor
		public AxisInputEvent(){
			// Nothing to do
		}

		// Constructor
		public AxisInputEvent(FixedVector3 axis){
			this.axis = axis;
		}

	}



}
