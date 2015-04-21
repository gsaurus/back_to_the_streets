using System;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;
using ProtoBuf.Meta;

namespace ShooterSerializer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			RuntimeTypeModel model = TypeModel.Create(); //RuntimeTypeModel.Default;

			// Add new derived models
			model[typeof(Model)].AddSubType(100, typeof(WorldModel));
			model[typeof(GameEntityModel)].AddSubType(100, typeof(ShooterEntityModel));
			model[typeof(PhysicPlaneModel)].AddSubType(100, typeof(MovingPlaneModel));
			model[typeof(PhysicPointModel)].AddSubType(100, typeof(BulletPointModel));

			// There seems to be a bug in protobuf-net,
			// if I don't do those deepclones they doesn't get registered...
			if (model.CanSerialize(typeof(InternalState))){
				Console.WriteLine("Can serialize InternalState");
				InternalState state = new InternalState();
				model.DeepClone(state);
				Console.WriteLine("deep cloned InternalState");
			}

			if (model.CanSerialize(typeof(NetworkPlayerData))){
				Console.WriteLine("Can serialize NetworkPlayerData");
				NetworkPlayerData playerData = new NetworkPlayerData();
				model.DeepClone(playerData);
				Console.WriteLine("deep cloned NetworkPlayerData");
			}

			if (model.CanSerialize(typeof(Dictionary<string, NetworkPlayerData>))){
				Console.WriteLine("Can serialize Dictionary<string, NetworkPlayerData>");
				Dictionary<string, NetworkPlayerData> data = new Dictionary<string, NetworkPlayerData>();
				data.Add("stuff", new NetworkPlayerData());
				model.DeepClone(data);
				Console.WriteLine("deep cloned Dictionary<string, NetworkPlayerData>");
			}
			if (model.CanSerialize(typeof(Dictionary<string, uint>))){
				Console.WriteLine("Can serialize Dictionary<string, uint>");
				Dictionary<string, uint> data = new Dictionary<string, uint>();
				data.Add("stuff", 5);
				model.DeepClone(data);
				Console.WriteLine("deep cloned Dictionary<string, uint>");
			}

			if (model.CanSerialize(typeof(AxisInputEvent))){
				Console.WriteLine("Can serialize AxisInputEvent");
				Event e = new AxisInputEvent();
				model.DeepClone(e);
				Console.WriteLine("deep cloned AxisInputEvent as Event");
			}

			model.Compile(SerializationConstants.SerializerName, SerializationConstants.SerializerDllName);
		}
	}

}
