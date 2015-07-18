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
		public uint buttonId; 
		// pressed state
		[ProtoMember(2)]
		public bool isPressed;


		// Default Constructor
		public ButtonInputEvent(){
			// Nothing to do
		}

		// Constructor
		public ButtonInputEvent(uint button, bool isPressed){
			this.buttonId = button;
			this.isPressed = isPressed;
		}

	}


	// Movement events
	[ProtoContract]
	public class AxisInputEvent: Event{

		// id of the axis
		[ProtoMember(1)]
		public uint axisId;

		[ProtoMember(2)]
		public FixedVector3 value;

		// Default Constructor
		public AxisInputEvent(){
			// Nothing to do
		}

		// Constructor
		public AxisInputEvent(uint axisId, FixedVector3 value){
			this.axisId = axisId;
			this.value = value;
		}

	}



}
