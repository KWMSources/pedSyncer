using System;
using System.Numerics;

namespace PedSyncer
{
	public class Utils
	{
		/**
		 * Function to get the distance of two given positions
		 */
		public static double GetDistanceBetweenPos(Vector3 Position1, Vector3 Position2) 
		{
			return Math.Sqrt(Math.Pow(Position1.X - Position2.X, 2) + Math.Pow(Position1.Y - Position2.Y, 2) + Math.Pow(Position1.Z - Position2.Z, 2));
		}
	
		/**
		 * Boolean-Function which gives true if the distance of two positions is smaller than an given distance, else false
		 */
		public static bool InDistanceBetweenPos(Vector3 Position1, Vector3 Position2, float Distance) 
		{
			if (
				Position1.X-Position2.X > Distance ||
				Position1.Y-Position2.Y > Distance ||
				Position1.Z-Position2.Z > Distance ||
				Position1.X-Position2.X < (-1)*Distance ||
				Position1.Y-Position2.Y < (-1)*Distance ||
				Position1.Z-Position2.Z < (-1)*Distance 
			) return false;
			return GetDistanceBetweenPos(Position1, Position2) < Distance;
		}
	}
}
