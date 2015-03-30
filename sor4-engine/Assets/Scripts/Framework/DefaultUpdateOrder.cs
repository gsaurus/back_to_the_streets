using UnityEngine;
using System.Collections;

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

