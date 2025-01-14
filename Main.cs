﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using opentk3;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector2 = OpenTK.Mathematics.Vector2;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public uint stride = 0;

        public Action[] RenderActions;

        private Table shader;
        public Table Shader
        {
            get { return shader; }
            set { ChangeShader(value); }
        }

        public void ChangeShader(Table table)
        {
            var shader = table.shader;
            var attribs = shader.Attributes;

            RenderActions = new Action[shader.AttributeCount];

            stride = 0;

            for (int i = 0; i < shader.AttributeCount; i ++)
            {
                uint prevStride = stride; //dont really know if i need this var but its would take a while to debug and isnt a speed problem

                switch (attribs[i].name)
                {
                    case "Position":
                        RenderActions[i] = () => RenderPos(prevStride);
                        break;
                    case "Color":
                        RenderActions[i] = () => RenderCol(prevStride);
                        break;
                    case "TextureIndex":
                        RenderActions[i] = () => RenderTextureIndex(prevStride);
                        break;
                    case "TextureCoord":
                        RenderActions[i] = () => RenderTextureCoord(prevStride);
                        break;
                }

                stride += (uint)attribs[i].size;
            }
        }

        public MeshData(BasicObject parent)
        {
            ID = count++;
            Parent = parent;
        }

        public MeshData(BasicObject parent, ObjFile data)
        {
            ID = count++;
            Parent = parent;

            BasePoints = data.vertexes.ToArray();

            Faces = IntArrayToUintArray(data.faceIndices.ToArray());

            FaceTexutre = IntArrayToUintArray(data.textureIndexPerVertex.ToArray());

            TexCoords = data.textureCoordnatesPerVertex.ToArray();

            Colors = new Vector4[data.vertexes.Count];
        }
        private float positionHash = 0;
        private void RenderPos(uint PreviousStride)
        {
            float hash = WorldPoints[0].LengthFast;
            if (hash == positionHash)
            {
                return;
            }
            positionHash = hash;
            Vector3 p;
            uint CurrentStride;
            for (uint i = 0; i < WorldPoints.Length; i++)
            {
                p = WorldPoints[i];
                CurrentStride = (i * stride) + PreviousStride;
                RenderData[CurrentStride] = p.X;
                RenderData[CurrentStride + 1] = p.Y;
                RenderData[CurrentStride + 2] = p.Z;
            }
        }
        private void RenderCol(uint PreviousStride1)
        {
            Vector4 p;
            uint CurrentStride;
            for (uint i = 0; i < Colors.Length; i++)
            {
                p = Colors[i];
                CurrentStride = (i * stride) + PreviousStride1;
                RenderData[CurrentStride] = p.X;
                RenderData[CurrentStride + 1] = p.Y;
                RenderData[CurrentStride + 2] = p.Z;
            }
        }
        private void RenderTextureCoord(uint PreviousStride1)
        {
            Vector2 p;
            uint CurrentStride;
            for (uint i = 0; i < TexCoords.Length; i++)
            {
                p = TexCoords[i];
                CurrentStride = (i * stride) + PreviousStride1;
                RenderData[CurrentStride] = p.X;
                RenderData[CurrentStride + 1] = p.Y;
            }
        }
        private void RenderTextureIndex(uint PreviousStride1)
        {
            uint CurrentStride;
            for (uint i = 0; i < FaceTexutre.Length; i++)
            {
                CurrentStride = (i * stride) + PreviousStride1;
                RenderData[CurrentStride] = (float)FaceTexutre[i];
            }
        }

        private static uint[] IntArrayToUintArray(int[] intArray)
        {
            var a = new uint[intArray.Length];
            for (int i = 0; i < intArray.Length; i++)
                a[i] = (uint)intArray[i];
            return a;
        }

    }
}
