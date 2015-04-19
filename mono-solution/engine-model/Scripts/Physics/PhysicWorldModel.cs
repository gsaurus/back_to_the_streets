using System;
using System.Collections.Generic;


namespace RetroBread{


	[Serializable]
	public class PhysicWorldModel: Model<PhysicWorldModel> {

		// Gravity acceleration
		public FixedVector3 gravity;

		// This is used to load the world static properties on the controller
		public string worldName;

		// Id of the point models to wich we want to detect collisions against planes
		public SerializableList<ModelReference> pointModels = new SerializableList<ModelReference>();

		// Id of the dynamic plane models to wich we want to detect collisions against points
		public SerializableList<ModelReference> planeModels = new SerializableList<ModelReference>();


		#region Constructors


		// Constructor	
		public PhysicWorldModel(string name, FixedVector3 gravity)
		:this(name, gravity, DefaultVCFactoryIds.PhysicWorldControllerFactoryId, null, DefaultUpdateOrder.WorldUpdateOrder){}

		// Constructor	
		public PhysicWorldModel(string name, FixedVector3 gravity, string controllerFactoryId)
		:this(name, gravity, controllerFactoryId, null, DefaultUpdateOrder.WorldUpdateOrder){}

		// Constructor with name 	
		public PhysicWorldModel(string name,
			                        FixedVector3 gravity,
			                        string controllerFactoryId,
			                        string viewFactoryId,
			                        int updateOrder
		):base(controllerFactoryId, viewFactoryId, updateOrder){
			worldName = name;
			this.gravity = gravity;
		}

		#endregion
		
	}



}

