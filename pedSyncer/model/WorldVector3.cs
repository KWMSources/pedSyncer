using MessagePack;
using System;
using System.Numerics;

namespace NavMesh_Graph
{
    [MessagePackObject]
    public class WorldVector3
    {
        [Key(0)]
        public float X { get; set; }

        [Key(1)]
        public float Y { get; set; }

        [Key(2)]
        public float Z { get; set; }

        public WorldVector3(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        
        //Convert WorldVector3 to Vector2
        public Vector2 ToVector2()
        {
            return new Vector2(this.X, this.Y);
        }

        //Convert WorldVector3 to Vector3
        public Vector3 ToVector3()
        {
            return new Vector3(this.X, this.Y, this.Z);
        }

        //Convert Vector3 to WorldVector3
        public static WorldVector3 ToWorldVector3(Vector3 vector)
        {
            return new WorldVector3(vector.X, vector.Y, vector.Z);
        }

        //Convert WorldVector3 to string
        public string ToString()
        {
            return X.ToString() + "; " + Y.ToString() + "; " + Z.ToString();
        }

        //Method to check if two WorldVelctor3s are equal
        public static bool equals(WorldVector3 vec1, WorldVector3 vec2)
        {
            return (vec1.X == vec2.X && vec1.Y == vec2.Y && vec1.Z == vec2.Z);
        }

        //Method to get the directional angle of two WorldVector3, also know as "gon"
        public static double directionalAngle(WorldVector3 vec1, Vector3 vec2)
        {
            return directionalAngle(vec1, WorldVector3.ToWorldVector3(vec2));
        }

        //Get the directional angle (gon) from two vectors (overloaded)
        public static double directionalAngle(Vector3 vec1, WorldVector3 vec2)
        {
            return directionalAngle(WorldVector3.ToWorldVector3(vec1), vec2);
        }
        public static double directionalAngle(Vector3 vec1, Vector3 vec2)
        {
            return directionalAngle(WorldVector3.ToWorldVector3(vec1), WorldVector3.ToWorldVector3(vec2));
        }

        public static double directionalAngle(WorldVector3 vec1, WorldVector3 vec2)
        {
            if ((vec2.Y - vec1.Y) > 0 && (vec2.X - vec1.X) > 0) return radianToGon(Math.Atan((vec2.Y - vec1.Y) / (vec2.X - vec1.X)));
            if (
                (vec2.Y - vec1.Y) > 0 && (vec2.X - vec1.X) < 0 ||
                (vec2.Y - vec1.Y) < 0 && (vec2.X - vec1.X) < 0) return radianToGon(Math.Atan((vec2.Y - vec1.Y) / (vec2.X - vec1.X))) + 200;
            if ((vec2.Y - vec1.Y) < 0 && (vec2.X - vec1.X) > 0) return radianToGon(Math.Atan((vec2.Y - vec1.Y) / (vec2.X - vec1.X))) + 400;
            return radianToGon(Math.Atan((vec2.Y - vec1.Y) / (vec2.X - vec1.X)));
        }

        //Transform radian to gon
        public static float radianToGon(double radian)
        {
            return (float)radian * (200 / (float)Math.PI);
        }
    }
}