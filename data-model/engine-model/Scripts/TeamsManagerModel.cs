using System;
using ProtoBuf;
using System.Collections.Generic;


namespace RetroBread{


	// Team data structure (avoid nested lists as they are not supported in protobuf.net
	[ProtoContract]
	public class TeamData{

		// Team members
		[ProtoMember(1, OverwriteList=true)]
		public List<ModelReference> entities;

		// Default constructor
		public TeamData(){
			// Nothing to do
		}

	}


	// Game entities are divided into teams (E.g. players, enemies, world objects...)
	// Teams manager keep track of what entities are in which team, and how they interact with each other
	[ProtoContract]
	public class TeamsManagerModel: Model<TeamsManagerModel> {

		// Every team's members
		[ProtoMember(1, OverwriteList=true)]
		public TeamData[] teams;




		#region Constructors


		// Default Constructor
		public TeamsManagerModel(){
			// Nothing to do
		}


		// Constructor with number of teams
		public TeamsManagerModel(int numTeams):base(
			DefaultVCFactoryIds.TeamsManagerControllerFactoryId,
			null, // No view
			DefaultUpdateOrder.TeamsManagerUpdateOrder
		){
			// Initialize teams structure
			teams = new TeamData[numTeams];
			for (int i = 0; i < numTeams; ++i) {
				teams[i] = new TeamData();
				teams[i].entities = new List<ModelReference>();
			}
		}


		#endregion

			

	}


}
