using System;

namespace RetroBread{

	public class DefaultUpdateOrder{

		// 1st: input
		// 2nd: teams (hit detection)
		// 3rd: animations
		// 4th: physic components (points, planes)
		// 5th: collision detection (world update)
		// 6th: game entities


		// Input
		public const int InputUpdateOrder			= -876;
		// Teams Manager (hits detection)
		public const int TeamsManagerUpdateOrder	= -875;
		// Animations
		public const int AnimationsUpdateOrder 		= -874;
		// Point & planes updates (apply forces over points and planes)
		public const int PhysicsUpdateOrder 		= -873;
		// World updates (collisions, consolidations, etc)
		public const int WorldUpdateOrder 			= -872;
		// Entities
		public const int EntitiesUpdateOrder		= -871;
	}


	public class DefaultVCFactoryIds{

		public const string TeamsManagerControllerFactoryId		= "_rb_tmc";
		public const string PhysicPointControllerFactoryId		= "_rb_ppc";
		public const string PhysicPointViewFactoryId			= "_rb_ppv";
		public const string PhysicWorldControllerFactoryId		= "_rb_pwc";
		public const string PlayerInputControllerFactoryId		= "_rb_pic";
		public const string AnimationControllerFactoryId		= "_rb_ac";
		public const string AnimationViewFactoryId				= "_rb_av";
		public const string GameEntityControllerFactoryId		= "_rb_gec";
		public const string GameEntityViewFactoryId				= "_rb_gev";
		
	}

	public class SerializationConstants{
		// Serializer used for game state (for network and save / load state)
		public const string StateSerializerName 	= "RbStateSerializer";
		public const string StateDllName	   		= "game-serializer.dll";
		
		// Serializer used for content storage (characters, levels...)
		public const string StorageSerializerName	= "RbStorageSerializer";
		public const string StorageDllName 			= "storage-serializer.dll";
	}

}

