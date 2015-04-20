using System;
using System.Collections.Generic;


namespace RetroBread{


	[Serializable]
	public class PhysicPointModel : Model<PhysicPointModel>{

		// Key of default world velocity affector
		[NonSerialized]
		public static readonly string defaultVelocityAffectorName = "_rb_default_velocity";

		// Id of the model that owns this point, used to get the respective gameobject from the owner
		public ModelReference ownerId;

		// Linear Position
		public FixedVector3 position;

		// Position on the previous frame
		public FixedVector3 lastPosition;

		// Step tolerance, used to try to climb small steps
		public FixedVector3 stepTolerance;

		// Collision impact is the maximum difference of point VS plane velocities
		public FixedVector3 collisionInpact;

		// how many frames passed since it was grounded last time.
		// Usefull to know if it was grounded just a few momments ago,
		// providing a safer ground interval
		public uint framesSinceLastTimeGrounded;

		// Velocity = difference between current and previous positions
		public FixedVector3 GetVelocity(){
			return position - lastPosition;
		}

		// Forces applied to this point, identified by a string (force source)
		public Dictionary<string,FixedVector3> velocityAffectors;


		#region Constructors


		// Constructor
		public PhysicPointModel(ModelReference ownerId, FixedVector3 position, FixedVector3 stepTolerance)
		:this(ownerId,
			  position,
			  stepTolerance,
			  DefaultVCFactoryIds.PhysicPointControllerFactoryId,
			  DefaultVCFactoryIds.PhysicPointViewFactoryId,
			  DefaultUpdateOrder.PhysicsUpdateOrder
		){}


		// Constructor
		public PhysicPointModel(ModelReference ownerId,
			                        FixedVector3 position,
			                        FixedVector3 stepTolerance,
			                        string controllerFactoryId,
			                        string viewFactoryId,
			                        int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.ownerId = ownerId;
			velocityAffectors = new Dictionary<string,FixedVector3>();
			this.position = this.lastPosition = position;
			this.stepTolerance = stepTolerance;
		}


		// Constructor
		public PhysicPointModel(ModelReference ownerId)
		:this(ownerId,
			  DefaultVCFactoryIds.PhysicPointControllerFactoryId,
			  DefaultVCFactoryIds.PhysicPointViewFactoryId,
			  DefaultUpdateOrder.PhysicsUpdateOrder
		 ){}

		// Constructor
		public PhysicPointModel(ModelReference ownerId,
			                        string controllerFactoryId,
			                        string viewFactoryId,
			                        int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.ownerId = ownerId;
			velocityAffectors = new Dictionary<string,FixedVector3>();
		}

		#endregion


		// Default world velocity affector
		public FixedVector3 GetDefaultVelocityAffector(){
			FixedVector3 vec;
			if (velocityAffectors.TryGetValue(defaultVelocityAffectorName, out vec))
				return vec;
			return FixedVector3.Zero;
		}

	}



}

