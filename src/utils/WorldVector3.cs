using MessagePack;
using System;
using System.Numerics;

namespace PedSyncer.Model
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

        //Convert WorldVector3 to Vector3
        public Vector3 ToVector3()
        {
            return new Vector3(this.X, this.Y, this.Z);
        }
    }
}