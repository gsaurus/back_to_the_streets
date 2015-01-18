using System;
using System.Collections.Generic;

public class PhysicPointController: Controller<PhysicPointModel>{

	// Key of collision reaction velocity affector
	public static readonly string collisionVelocityAffectorName = "collisionVelocityAffectorName";

	// Temporary changes to take place on post-update
	private Dictionary<string,FixedVector3> affectorsChanged = new Dictionary<string,FixedVector3>();
	private Dictionary<string,FixedVector3> affectorsIncremented = new Dictionary<string,FixedVector3>();
	private List<string> affectorsRemoved = new List<string>();

	private FixedVector3 newPosition;
	private bool hasNewPosition;

	// Planes against what the point collided with during this frame
	private List<PhysicPlaneModel> collisionPlanes;


	// Update natural physics 
	public override void Update(PhysicPointModel model){

		model.lastPosition = model.position;

		// Apply all velocity factors to the model's position
		foreach (FixedVector3 velocity in model.velocityAffectors.Values) {
			model.position += velocity;
		}

		// Reset collision velocity affector, directly
		model.velocityAffectors[collisionVelocityAffectorName] = FixedVector3.Zero;
	}
	
	// Post update used to consolidate direct modifications
	public override void PostUpdate(PhysicPointModel model){

		// Modified affectors (first increments, then sets, sets have priority over increments)
		foreach (KeyValuePair<string, FixedVector3> pair in affectorsIncremented){
			if (model.velocityAffectors.ContainsKey(pair.Key)){
				model.velocityAffectors[pair.Key] += pair.Value;
			}else{
				model.velocityAffectors[pair.Key] = pair.Value;
			}
		}
		affectorsIncremented.Clear();
		foreach (KeyValuePair<string, FixedVector3> pair in affectorsChanged){
			model.velocityAffectors[pair.Key] = pair.Value;
		}
		affectorsChanged.Clear();


		// Removed affectors
		foreach (string key in affectorsRemoved){
			model.velocityAffectors.Remove(key);
		}
		affectorsRemoved.Clear();

		// New position (teleport)
		if (hasNewPosition){
			model.position = newPosition;
			model.lastPosition = newPosition;
			hasNewPosition = false;
		}

	}

	// Teleport to a new position
	public void SetPosition(FixedVector3 newPos){
		newPosition = newPos;
		hasNewPosition = true;
	}

	// Set a velocity affector (force)
	public void SetVelocityAffector(string key, FixedVector3 affectorValue){
		affectorsChanged[key] = affectorValue;
	}

	// Add force to an existing velocity affector
	public void AddVelocityAffector(string key, FixedVector3 affectorValue){
		if (affectorsIncremented.ContainsKey(key)){
			affectorsIncremented[key] += affectorValue;
		}else {
			affectorsIncremented[key] = affectorValue;
		}
	}


	// Add or set a velocity affector (force)
	public void SetDefaultVelocityAffector(FixedVector3 affectorValue){
		SetVelocityAffector(PhysicPointModel.defaultVelocityAffectorName, affectorValue);
	}

	// Increment a velocity affector (force)
	public void AddDefaultVelocityAffector(FixedVector3 affectorValue){
		AddVelocityAffector(PhysicPointModel.defaultVelocityAffectorName, affectorValue);
	}


	// Remove a velocity affector
	public void RemoveVelocityAffector(string key){
		affectorsRemoved.Add(key);
	}


	protected void GainPlaneVelocity(PhysicWorldModel world, PhysicPlaneModel planeModel){
		FixedVector3 velocityToAdd = planeModel.GetVelocity();
		// Only add velocity that doesn't conflict with gravity, otherwise it starts flickering
		if (velocityToAdd.X > 0 && world.gravity.X < 0 || velocityToAdd.X < 0 && world.gravity.X > 0){
			velocityToAdd.X = 0;
		}
		if (velocityToAdd.Y > 0 && world.gravity.Y < 0 || velocityToAdd.Y < 0 && world.gravity.Y > 0){
			velocityToAdd.Y = 0;
		}
		if (velocityToAdd.Z > 0 && world.gravity.Z < 0 || velocityToAdd.Z < 0 && world.gravity.Z > 0){
			velocityToAdd.Z = 0;
		}

		AddVelocityAffector(collisionVelocityAffectorName, velocityToAdd);
	}


	protected void KillGravityEffectAgainstPlane(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel){
		FixedVector3 normal = planeModel.Normal;
		FixedVector3 defaultVel = pointModel.GetDefaultVelocityAffector();
		//FixedVector3 velToAdd = FixedVector3.Zero;
		if ((world.gravity.X < 0 && defaultVel.X < world.gravity.X && normal.X > 0) || (world.gravity.X > 0 && defaultVel.X > world.gravity.X && normal.X < 0)){
			defaultVel.X = world.gravity.X;
			//velToAdd.X = world.gravity.X - defaultVel.X;
		}

		if ((world.gravity.Y < 0 && defaultVel.Y < world.gravity.X && normal.Y > 0) || (world.gravity.Y > 0 && defaultVel.Y > world.gravity.Y && normal.Y < 0)){
			defaultVel.Y = world.gravity.Y;
			//velToAdd.Y = world.gravity.Y - defaultVel.Y;
		}

		if ((world.gravity.Z < 0 && defaultVel.Z < world.gravity.Z && normal.Z > 0) || (world.gravity.Z > 0 && defaultVel.Z > world.gravity.Z && normal.Z < 0)){
			defaultVel.Z = world.gravity.Z;
			//velToAdd.Z = world.gravity.Z - defaultVel.Z;
		}

		//UnityEngine.Debug.Log("Setting default velocity affector: " + defaultVel);
		pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] = defaultVel;
		//SetDefaultVelocityAffector(defaultVel);
		//AddDefaultVelocityAffector(velToAdd);
	}



	// Follow the plane except on Y axis, so it won't slide with gravity
	protected bool CollisionGroundReaction(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel, FixedVector3 intersection){
		
		bool intersectionChanged = false;

		if (pointModel.position != intersection){

			// Project remaining velocity against the plane
			FixedVector3 normal = planeModel.Normal;
			FixedVector3 pos1 = pointModel.position;
			FixedVector3 pos2 = pointModel.position + FixedVector3.Up;
			FixedVector3 pointDeltaPos = pos2 - pos1;
			FixedVector3 pos1ToOrigin = planeModel.origin - pos1;
			FixedFloat dotDeltaPosNormal = FixedVector3.Dot(pointDeltaPos,normal);
			FixedFloat t = FixedVector3.Dot(pos1ToOrigin, normal) / dotDeltaPosNormal;
			FixedVector3 newIntersection = pos1 + t * pointDeltaPos;
			if (newIntersection != intersection) {
				intersection = newIntersection;
				intersectionChanged = true;
			}
			// finally, our new position is the intersection point
			pointModel.position = intersection;
		}
		
		// in the end, also sum the plane velocity, essential for platforms
		GainPlaneVelocity(world, planeModel);
		
		// Also kill gravity effect
		KillGravityEffectAgainstPlane(world, pointModel, planeModel);
		
		return !intersectionChanged; // if intersection didn't change, it's considered stable
	}


	// Natural reaction: slide along the plane
	protected bool CollisionNaturalReaction(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel, FixedVector3 intersection){

		bool intersectionChanged = false;

		if (pointModel.position != intersection){

			// Project remaining velocity against the plane
			FixedVector3 normal = planeModel.Normal;
			FixedVector3 pos1 = pointModel.position;
			FixedVector3 pos2 = pointModel.position + normal;
			FixedVector3 pointDeltaPos = pos2 - pos1;
			FixedVector3 pos1ToOrigin = planeModel.origin - pos1;
			FixedFloat dotDeltaPosNormal = FixedVector3.Dot(pointDeltaPos,normal);
			FixedFloat t = FixedVector3.Dot(pos1ToOrigin, normal) / dotDeltaPosNormal;
			FixedVector3 newIntersection = pos1 + t * pointDeltaPos;
			if (newIntersection != intersection) {
				intersection = newIntersection;
				intersectionChanged = true;
			}

			// finally, our new position is the intersection point
			pointModel.position = intersection;

		}

		// in the end, also sum the plane velocity, essential for platforms
		GainPlaneVelocity(world, planeModel);
		
		return !intersectionChanged; // if intersection didn't change, it's considered stable
	}

	// Collision reaction.
	// Return true if collision is considered stable (no position modifications occured)
	public virtual bool OnCollision(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel, FixedVector3 intersection){

//		if (pointModel.position == intersection){
//			// Nothing to change
//			return true;
//		}

		// Pick one of two methods depending on the plane's normal angle against up vector
		FixedFloat planeAngle = FixedVector3.Angle(planeModel.Normal, FixedVector3.Up);

		// If it's too much inclined, use natural reaction
		if (planeAngle > FixedFloat.PI * 0.4) {
			return CollisionNaturalReaction(world, pointModel, planeModel, intersection);
		}else {
			// Otherwise we're hitting the ground, do not slide
			// We use a lot of arguments here just to avoid recalculating them
			return CollisionGroundReaction(world, pointModel, planeModel, intersection);
		}
	}


}

