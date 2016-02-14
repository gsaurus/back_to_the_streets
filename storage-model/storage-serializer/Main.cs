using System;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Storage;
using ProtoBuf.Meta;

namespace ShooterSerializer
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
