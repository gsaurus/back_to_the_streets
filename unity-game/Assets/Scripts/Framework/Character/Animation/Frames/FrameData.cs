﻿using System;
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

	// Base class for a hitbox specific data
	public class HitData{
		// Nothing by default..
	}


	// Contains collision and hits information
	public class FrameData {
		public List<Box> collisions = new List<Box>();
		public List<HitBox> hits = new List<HitBox>();

		private Box collisionBoundingBox;
		private Box hitBoundingBox;


		public void ComputeBoundingBoxes(){
			if (collisions.Count > 0) { 
				FixedVector3 pointOne = new FixedVector3(float.MaxValue, float.MaxValue, float.MaxValue);
				FixedVector3 pointTwo = new FixedVector3(float.MinValue, float.MinValue, float.MinValue);
				foreach (Box box in collisions) {
					if (pointOne.X > box.pointOne.X) pointOne.X = box.pointOne.X;
					if (pointOne.Y > box.pointOne.Y) pointOne.Y = box.pointOne.Y;
					if (pointOne.Z > box.pointOne.Z) pointOne.Z = box.pointOne.Z;
					if (pointTwo.X < box.pointTwo.X) pointTwo.X = box.pointTwo.X;
					if (pointTwo.Y < box.pointTwo.Y) pointTwo.Y = box.pointTwo.Y;
					if (pointTwo.Z < box.pointTwo.Z) pointTwo.Z = box.pointTwo.Z;
				}
				collisionBoundingBox = new Box(pointOne, pointTwo);
			}else {
				collisionBoundingBox = null;
			}
			if (hits.Count > 0) { 
				FixedVector3 pointOne = new FixedVector3(float.MaxValue, float.MaxValue, float.MaxValue);
				FixedVector3 pointTwo = new FixedVector3(float.MinValue, float.MinValue, float.MinValue);
				Box box;
				foreach (HitBox hitBox in hits) {
					box = hitBox.box;
					if (pointOne.X > box.pointOne.X) pointOne.X = box.pointOne.X;
					if (pointOne.Y > box.pointOne.Y) pointOne.Y = box.pointOne.Y;
					if (pointOne.Z > box.pointOne.Z) pointOne.Z = box.pointOne.Z;
					if (pointTwo.X < box.pointTwo.X) pointTwo.X = box.pointTwo.X;
					if (pointTwo.Y < box.pointTwo.Y) pointTwo.Y = box.pointTwo.Y;
					if (pointTwo.Z < box.pointTwo.Z) pointTwo.Z = box.pointTwo.Z;
				}
				hitBoundingBox = new Box(pointOne, pointTwo);
			}else {
				hitBoundingBox = null;
			}
		}


		public bool CollisionCollisionCheck(FrameData other){
			if (collisions.Count == 0 || other.collisions.Count == 0) return false;
			if (collisionBoundingBox.Intersects(other.collisionBoundingBox)) {
				if (collisions.Count == 1 && other.collisions.Count == 1) return true;

				foreach (Box box in collisions) {
					foreach (Box otherBox in other.collisions) {
						if (box.Intersects(otherBox)){
							return true;
						}
					}
				}
			}
			return false;
		}


		public HitData HitCollisionCheck(FrameData other){
			if (hits.Count == 0 || other.collisions.Count == 0) return null;

			if (hitBoundingBox.Intersects(other.collisionBoundingBox)) {
				if (hits.Count == 1 && other.collisions.Count == 1) return hits[0].hitData;

				foreach (HitBox hitBox in hits) {
					foreach (Box otherBox in other.collisions) {
						if (hitBox.box.Intersects(otherBox)){
							return hitBox.hitData;
						}
					}
				}
			}
			return null;
		}


	}

}
