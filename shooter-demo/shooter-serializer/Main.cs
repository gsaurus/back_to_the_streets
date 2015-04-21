using System;
using RetroBread;
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

			model.Compile(SerializationConstants.SerializerName, SerializationConstants.SerializerDllName);
		}
	}

}
