using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using opentk3;

namespace opentk3
{
    public class MainLoop
    {
    }

    public class BasicObject
    {
        public string Name = "Unnamed Object";

        public Vector3
            Position = Vector3.Zero,
            Rotation = Vector3.Zero,
            Scale    = Vector3.Zero;

        public BasicObject()
        {

        }
    
    }

    public class MeshData
    {
        public static int count = 0;
        public int ID { get; set; } = 0;

        public BasicObject Parent { get; set; }

        public Vector3[] BasePoints { get; set; } = [];
        public Vector4[] Colors { get; set; } = [];
        public uint[] Faces { get; set; } = [];
        public uint[] FaceTexutre { get; set; } = [];
        public Vector2[] TexCoords { get; set; } = [];

        public Vector3[] WorldPoints { get; set; } = [];

        public float[] RenderData { get; set; } = [];
        public uint[] RenderData1 { get; set; } = [];


        public Vector3 LocalCenter { get; set; }
        public Vector3 WorldCenter { get; set; }




    }
}
