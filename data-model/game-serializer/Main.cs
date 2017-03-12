using System;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;
using ProtoBuf.Meta;

namespace GameSerializer{

class MainClass{
	
	public static void Main (string[] args){
		
		RuntimeTypeModel model = TypeModel.Create(); //RuntimeTypeModel.Default;

		// Add new derived models
//			Console.WriteLine("1");
//			model[typeof(Model)].AddSubType(100, typeof(WorldModel));
		Console.WriteLine("1");
		model[typeof(NetworkPlayerData)].AddSubType(100, typeof(NetworkSorPlayerData));

		// There seems to be a bug in protobuf-net,
		// if I don't do those deepclones they doesn't get registered...
		if (model.CanSerialize(typeof(InternalState))){
			InternalState state = new InternalState();
			model.DeepClone(state);
			Console.WriteLine("deep cloned InternalState");
		}

		Console.WriteLine("compiling..");
		model.Compile(SerializationConstants.StateSerializerName, SerializationConstants.StateDllName);
	}
}

}
