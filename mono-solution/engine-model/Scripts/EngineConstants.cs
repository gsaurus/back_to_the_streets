using System;

namespace RetroBread{

	public class DefaultUpdateOrder{

		// 1st: input
		// 2nd: animations
		// 3rd: physic components (points, planes)
		// 4th: collision detection (world update)
		// 5th: game entities


		// Input
		public const int InputUpdateOrder		= -875;
		// Animations
		public const int AnimationsUpdateOrder 	= -874;
		// Point & planes updates (apply forces over points and planes)
		public const int PhysicsUpdateOrder 	= -873;
		// World updates (collisions, consolidations, etc)
		public const int WorldUpdateOrder 		= -872;
		// Entities
		public const int EntitiesUpdateOrder	= -871;
	}


	public class DefaultVCFactoryIds{
		
		public const string PhysicPointControllerFactoryId		= "_rb_ppc";
		public const string PhysicPointViewFactoryId			= "_rb_ppv";
		public const string PhysicWorldControllerFactoryId		= "_rb_pwc";
		public const string PhysicPlaneControllerFactoryId		= "_rb_pplc";
		public const string PlayerInputControllerFactoryId		= "_rb_pic";
		public const string AnimationControllerFactoryId		= "_rb_ac";
		public const string AnimationViewFactoryId				= "_rb_av";
		public const string GameEntityControllerFactoryId		= "_rb_gec";
		public const string GameEntityViewFactoryId				= "_rb_gev";
		
	}

	public class SerializationConstants{
		public const string SerializerName = "RbSerializer";
		public const string SerializerDllName = "rb-serializer.dll";
	}

}

