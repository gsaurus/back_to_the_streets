using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;



namespace RetroBread{

	// Movement events
	[ProtoContract]
	public class AxisInputEvent: Event{

		[ProtoMember(1)]
		public FixedFloat axis;

		// Default Constructor
		public AxisInputEvent(){
			// Nothing to do
		}

		// Constructor
		public AxisInputEvent(FixedFloat value){
			this.axis = value;
		}

	}



}
