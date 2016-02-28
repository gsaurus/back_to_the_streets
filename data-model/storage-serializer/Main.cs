using System;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Storage;
using ProtoBuf.Meta;

namespace StorageSerializer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			RuntimeTypeModel model = TypeModel.Create();

			// There seems to be a bug in protobuf-net,
			// if I don't do those deepclones they doesn't get registered...
			if (model.CanSerialize (typeof(Character))) {
				Character state = new Character ();
				model.DeepClone (state);
				Console.WriteLine ("deep cloned Character");
			}

			Console.WriteLine("compiling..");
			model.Compile(SerializationConstants.StorageSerializerName, SerializationConstants.StorageDllName);
		}
	}

}
