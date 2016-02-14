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
			Console.WriteLine("compiling..");
			model.Compile(SerializationConstants.StorageSerializerName, SerializationConstants.StorageDllName);
		}
	}

}
