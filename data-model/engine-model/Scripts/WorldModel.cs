using System;
using ProtoBuf;
using System.Collections.Generic;

namespace RetroBread{

[ProtoContract]
public class WorldModel:Model<WorldModel>{
	
	// Reference to player models
	// Key: player number; value: player model
	[ProtoMember(1, OverwriteList=true)]
	public Dictionary<uint, ModelReference> players;

	// Reference to physics model
	[ProtoMember(2)]
	public ModelReference physicsModelId = new ModelReference();

	// TeamsManagerModel: Information about teams
	[ProtoMember(3)]
	public ModelReference teamsModelId = new ModelReference();

	// Global game variables
	[ProtoMember(4, OverwriteList=true)]
	public Dictionary<string, int> globalVariables;



	// Constructor
	public WorldModel()
	:this(DefaultVCFactoryIds.WorldControllerFactoryId,
			DefaultVCFactoryIds.WorldViewFactoryId,
			DefaultUpdateOrder.WorldUpdateOrder
	){}

	public WorldModel(string controllerFactoryId, string viewFactoryId, int updatingOrder)
	:base(controllerFactoryId, viewFactoryId, updatingOrder){
		players = new Dictionary<uint, ModelReference>();
		globalVariables = new Dictionary<string, int>();
	}
		

}


}

