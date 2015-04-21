using System;
using System.Collections.Generic;
using ProtoBuf;


namespace RetroBread{


	[ProtoContract]
	public class PhysicWorldModel: Model<PhysicWorldModel> {

		// Gravity acceleration
		[ProtoMember(1)]
		public FixedVector3 gravity;

		// This is used to load the world static properties on the controller
		[ProtoMember(2)]
		public string worldName;

		// Id of the point models to wich we want to detect collisions against planes
		[ProtoMember(3)]
		public List<ModelReference> pointModels = new List<ModelReference>();

		// Id of the dynamic plane models to wich we want to detect collisions against points
		[ProtoMember(4)]
		public List<ModelReference> planeModels = new List<ModelReference>();


		#region Constructors

		// Default constructor
		public PhysicWorldModel():this(null, FixedVector3.Zero){}

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

