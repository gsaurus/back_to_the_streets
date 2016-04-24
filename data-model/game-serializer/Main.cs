using System;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;
using ProtoBuf.Meta;

namespace GameSerializer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			RuntimeTypeModel model = TypeModel.Create(); //RuntimeTypeModel.Default;

			// Add new derived models
			Console.WriteLine("1");
			model[typeof(Model)].AddSubType(100, typeof(WorldModel));

			// There seems to be a bug in protobuf-net,
			// if I don't do those deepclones they doesn't get registered...
			if (model.CanSerialize(typeof(InternalState))){
				InternalState state = new InternalState();
				model.DeepClone(state);
				Console.WriteLine("deep cloned InternalState");
			}

			if (model.CanSerialize(typeof(NetworkPlayerData))){
				NetworkPlayerData playerData = new NetworkPlayerData();
				model.DeepClone(playerData);
				Console.WriteLine("deep cloned NetworkPlayerData");
			}

			if (model.CanSerialize(typeof(PhysicPlaneModel))){
				PhysicPlaneModel data = new PhysicPlaneModel();
				model.DeepClone(data);
				Console.WriteLine("deep cloned PhysicPlaneModel");
			}

			if (model.CanSerialize(typeof(Dictionary<string, NetworkPlayerData>))){
				Dictionary<string, NetworkPlayerData> data = new Dictionary<string, NetworkPlayerData>();
				data.Add("stuff", new NetworkPlayerData());
				model.DeepClone(data);
				Console.WriteLine("deep cloned Dictionary<string, NetworkPlayerData>");
			}
			if (model.CanSerialize(typeof(Dictionary<string, uint>))){
				Dictionary<string, uint> data = new Dictionary<string, uint>();
				data.Add("stuff", 5);
				model.DeepClone(data);
				Console.WriteLine("deep cloned Dictionary<string, uint>");
			}

			if (model.CanSerialize(typeof(TeamsManagerModel))) {
				TeamsManagerModel data = new TeamsManagerModel(2);
				data.teams[0].entities.Add(new ModelReference());
				model.DeepClone(data);
				Console.WriteLine ("deep cloned TeamsManagerModel");
			}

			if (model.CanSerialize(typeof(AxisInputEvent))){
				Event e = new AxisInputEvent();
				model.DeepClone(e);
				Console.WriteLine("deep cloned AxisInputEvent as Event");
			}

			Console.WriteLine("compiling..");
			model.Compile(SerializationConstants.StateSerializerName, SerializationConstants.StateDllName);
		}
	}

}
