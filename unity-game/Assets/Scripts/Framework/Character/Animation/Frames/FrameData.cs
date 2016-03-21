using System;
using System.Collections.Generic;


namespace RetroBread{

	// Simple box information
	public class Box{
		public FixedVector3 pointOne;
		public FixedVector3 pointTwo;

		public Box(FixedVector3 pointOne, FixedVector3 pointTwo){
			this.pointOne = pointOne;
			this.pointTwo = pointTwo;
		}

		public bool Intersects(Box otherBox){
			return pointTwo.Z > otherBox.pointOne.Z
			    && pointOne.Z < otherBox.pointTwo.Z
				&& pointTwo.X > otherBox.pointOne.X
			   	&& pointOne.X < otherBox.pointTwo.X
			    && pointTwo.Y > otherBox.pointOne.Y
			    && pointOne.Y < otherBox.pointTwo.Y
			;
		}
	}

	// Base class for a hitbox
	public class HitBox{
		public Box box;
		public HitData hitData;

		public HitBox(Box box, HitData hitData){
			this.box = box;
			this.hitData = hitData;
		}
	}

	// Base class for a hitbox
	public class HitData{
		// Nothing by default..
	}

	// Contains collision and hits information
	public class FrameData {
		public List<Box> colisions = new List<Box>();
		public List<HitBox> hits = new List<HitBox>();

	}

}
