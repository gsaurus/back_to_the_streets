using System;
using System.Collections.Generic;


namespace RetroBread{

	// Simple box information
	public class Box{
		
		public FixedVector3 pointOne;
		public FixedVector3 pointTwo;

		// Constructor given two points
		public Box(FixedVector3 pointOne, FixedVector3 pointTwo){
			this.pointOne = pointOne;
			this.pointTwo = pointTwo;
		}

		// Check if intersects with other box
		public bool Intersects(Box otherBox){
			return pointTwo.Z > otherBox.pointOne.Z
			    && pointOne.Z < otherBox.pointTwo.Z
				&& pointTwo.X > otherBox.pointOne.X
			   	&& pointOne.X < otherBox.pointTwo.X
			    && pointTwo.Y > otherBox.pointOne.Y
			    && pointOne.Y < otherBox.pointTwo.Y
			;
		}

		// Center, may be useful to find the center of an intersection
		public FixedVector3 Center(){
			return pointOne + (pointTwo - pointOne) * 0.5f;
		}
	}


	// Collision box, have the box and collision ID
	public class CollisionBox{
		public Box box { get; private set; }
		public int collisionId { get; private set; }

		public CollisionBox(Box box, int collisionId){
			this.box = box;
			this.collisionId = collisionId;
		}
	}


	// Base class for a hitbox
	public class HitBox{
		public Box box { get; private set; }
		public HitData hitData { get; private set; }

		public HitBox(Box box, HitData hitData){
			this.box = box;
			this.hitData = hitData;
		}
	}


	// Class for a hitbox specific data
	// TODO: if necessary break into inheritance and extract specific stuff
	public class HitData{

		public enum HitType{
			contact			= 0,
			KO				= 1,
			grab			= 2,
			electrocution	= 3,
			burn			= 4,
			freeze			= 5
		}

		public enum HitFacing{
			hitterLocation 				= 0,
			inverseHitterLocation 		= 1,
			hitterOrientation			= 2,
			inverseHitterOrientation	= 3,
			none						= 4
		}

		public int hitboxID;
		public HitType type;
		public int damage;
		public HitFacing facingOptions;

		public HitData(HitType type, int damage, HitFacing facingOptions){
			this.type = type;
			this.damage = damage;
			this.facingOptions = facingOptions;
		}
	}


	// Hit happening information: intersection area and hit data
	public class HitInformation{
		// Hit information
		public HitData hitData { get; private set; }
		// Intersection between hit and collision
		public Box intersection { get; private set; }
		// The collision box id it hit
		public int collisionId { get ; private set; }
		// Who we hit, or who was hit
		public ModelReference entityId { get; private set; }

		// Constructor based on hit-collision intersection
		public HitInformation(HitData data, Box hitBox, int collisionId, Box collisionBox){
			this.hitData = data;
			this.collisionId = collisionId;
			intersection = new Box(
				FixedVector3.Max (hitBox.pointOne, collisionBox.pointOne),
				FixedVector3.Min (hitBox.pointTwo, collisionBox.pointTwo)
			);
		}

		// Constructor based on hit-collision intersection
		private HitInformation(HitInformation info, ModelReference entityId){
			this.hitData = info.hitData;
			this.intersection = info.intersection;
			this.collisionId = info.collisionId;
			this.entityId = entityId;
		}

		// Create a copy adding the entity information (used for hitter and hitten entity) 
		public HitInformation HitWithEntity(ModelReference entityId){
			return new HitInformation(this, entityId);
		}

	}


	// Contains collision and hits information
	public class FrameData {
		public List<CollisionBox> collisions = new List<CollisionBox>();
		public List<HitBox> hits = new List<HitBox>();

		private Box collisionBoundingBox;
		private Box hitBoundingBox;


		public void ComputeBoundingBoxes(){
			// Collisions
			if (collisions.Count > 0) { 
				FixedVector3 pointOne = new FixedVector3(FixedFloat.MaxValue, FixedFloat.MaxValue, FixedFloat.MaxValue);
				FixedVector3 pointTwo = new FixedVector3(FixedFloat.MinValue, FixedFloat.MinValue, FixedFloat.MinValue);
				Box box;
				foreach (CollisionBox collisionBox in collisions) {
					box = collisionBox.box;
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
			// Hits
			if (hits.Count > 0) { 
				FixedVector3 pointOne = new FixedVector3(FixedFloat.MaxValue, FixedFloat.MaxValue, FixedFloat.MaxValue);
				FixedVector3 pointTwo = new FixedVector3(FixedFloat.MinValue, FixedFloat.MinValue, FixedFloat.MinValue);
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

		// Offsets a box, used to apply world coordinates of the entity, and it's orientation
		private Box OffsettedBox(Box box, FixedVector3 offset, bool facingRight){
			FixedVector3 newPos1 = box.pointOne;
			FixedVector3 newPos2 = box.pointTwo;
			if (!facingRight) {
				// swap and make symetric
				FixedFloat tmp = -newPos1.X;
				newPos1.X = -newPos2.X;
				newPos2.X = tmp;
			}
			return new Box(offset + newPos1, offset + newPos2);
		}


		public bool CollisionCollisionCheck(FixedVector3 offset, bool facingRight, FrameData other, FixedVector3 otherOffset, bool otherFacingRight){
			if (collisions.Count == 0 || other.collisions.Count == 0) return false;

			Box offsettedBox = OffsettedBox(collisionBoundingBox, offset, facingRight);
			Box otherOffsettedBox = OffsettedBox(other.collisionBoundingBox, otherOffset, otherFacingRight);

			if (offsettedBox.Intersects(otherOffsettedBox)) {
				if (collisions.Count == 1 && other.collisions.Count == 1) return true;

				foreach (CollisionBox collisionBox in collisions) {
					offsettedBox = OffsettedBox(collisionBox.box, offset, facingRight);
					foreach (CollisionBox otherCollisionBox in other.collisions) {
						otherOffsettedBox = OffsettedBox(otherCollisionBox.box, otherOffset, otherFacingRight);
						if (offsettedBox.Intersects(otherOffsettedBox)){
							return true;
						}
					}
				}
			}
			return false;
		}


		public HitInformation HitCollisionCheck(
			FixedVector3 offset, bool facingRight,
			FrameData other, FixedVector3 otherOffset, bool otherFacingRight,
			List<AnimationHittenEntities> hittenEntities,
			ModelReference targetEntityId
		){
			if (hits.Count == 0 || other.collisions.Count == 0) return null;
			if (hits.Count == 1
				&& hittenEntities != null
				&& hittenEntities.Count > hits[0].hitData.hitboxID
				&& hittenEntities[hits[0].hitData.hitboxID].entities.Find(x => x == targetEntityId) != null
			) {
				// already hit this entity
				return null;
			}

			Box offsettedBox = OffsettedBox(hitBoundingBox, offset, facingRight);
			Box otherOffsettedBox = OffsettedBox(other.collisionBoundingBox, otherOffset, otherFacingRight);

			if (offsettedBox.Intersects(otherOffsettedBox)) {
				if (hits.Count == 1 && other.collisions.Count == 1) {
					return new HitInformation(hits[0].hitData, offsettedBox, other.collisions[0].collisionId, otherOffsettedBox);
				}
				foreach (HitBox hitBox in hits) {

					// Check if this hit already hit this entity
					if (hittenEntities != null
						&& hittenEntities.Count > hitBox.hitData.hitboxID
						&& hittenEntities[hitBox.hitData.hitboxID].entities.Find(x => x == targetEntityId) != null
					) {
						// already hit this entity
						continue;
					}

					// Check collisions against each collision box of the entity
					offsettedBox = OffsettedBox(hitBox.box, offset, facingRight);
					foreach (CollisionBox collisionBox in other.collisions) {
						otherOffsettedBox = OffsettedBox(collisionBox.box, otherOffset, otherFacingRight);
						if (offsettedBox.Intersects(otherOffsettedBox)){
							return new HitInformation(hitBox.hitData, offsettedBox, collisionBox.collisionId, otherOffsettedBox);
						}
					}
				}
			}
			return null;
		}


	}

}
