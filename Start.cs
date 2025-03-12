
using System.IO;
using System.Reflection;
using OpenTK.Mathematics;
using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

namespace opentk3
{
    internal class Start
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello, World!");
            foreach(string s in args)
            System.Console.WriteLine(s);
            
            
            using (Renderer game = new Renderer(800, 600, "LearnOpenTK"))
            {
                //game.Tables
                game.Run();
            }
        }
    }

    public static class ShaderBuilder
    {

        public static string full = @"#version 330 core
layout (location = 0) in vec3 Position;

uniform mat4 view;

void main()
{
   gl_Position = vec4(Position, 1.0) * view;
}";

        public static string full2 = @"#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}";

        public static Table FromMTL(Mtl mtl, Renderer r)
        {
            bool HasMap = false;
            bool diffuseMap = mtl.diffuseMap != null,
                specularMap = mtl.specularMap != null,
                ambientMap = mtl.ambientMap != null;

            if (diffuseMap || specularMap || ambientMap)
                HasMap = true;


            string VertShader = "";

            VertShader += @"#version 330 core \nlayout(location = 0) in vec3 Position;\n";

            if (HasMap)
            {
                VertShader += @"layout(location = 1) in vec2 TextureCoord;\n";
                VertShader += @"\n";
                VertShader += @"out vec2 texCoord;\n";
            }
            VertShader += @"\n";
            VertShader += @"uniform mat4 view;";

            VertShader += @"void main()\n{\n";
            if (HasMap)
            {
                VertShader += @"texCoord = TextureCoord;\n";
            }
            VertShader += @"gl_Position = vec4(Position, 1.0) * view;\n";
            VertShader += @"}";

            string FragShader = "";

            FragShader += @"#version 330 core \n\n";
            FragShader += @"out vec4 FragColor;\n\n";
            if (HasMap)
            {
                FragShader += @"in vec2 texCoord;\n\n";
                if (diffuseMap) FragShader += @"uniform sampler2D diffuseMap;\n";
                if (specularMap) FragShader += @"uniform sampler2D specularMap;\n";
                if (ambientMap) FragShader += @"uniform sampler2D ambientMap;\n";
            }
            FragShader += @"void main()\n{\n";
            FragShader += @"FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);\n";
            FragShader += @"}";

            Table table = new Table(r, VertShader, FragShader, true);

            StbImage.stbi_set_flip_vertically_on_load(1);

            if (diffuseMap)
            {
                var t = new Texture(mtl.diffuseMap);
                GL.Uniform1(table.shader.shader.GetUniformLocation("diffuseMap"), 0);
            }


            //if (diffuseMap) table.shader.shader.GetUniformLocation("diffuseMap");
            return table;

        }
    }
    public class Texture
    {
        int Handle;

        public Texture(string path)
        {
            Handle = GL.GenTexture();
            ImageResult image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
            Use();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        }
        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }

    public class Mtl
    {
        public string name;
        public Vector3 diffuse;
        public Vector3 ambient;
        public Vector3 specular;

        public string diffuseMap;
        public string ambientMap;
        public string specularMap;

        public Mtl(string Name)
        {
            name = Name;
        }
    }

    public class WavefrontFile
    {
        string Name;

        public List<Vector3> Verts = new List<Vector3>();

        public List<Vector2> TextureCoord = new List<Vector2>();

        public List<uint> Faces = new List<uint>();

        public List<uint> FaceMtl = new List<uint>();

        public List<int> FaceTextureCoordIndex = new List<int>();

        public List<Mtl> mtls = new List<Mtl>();

        public WavefrontFile(string name)
        {
            Name = name;

            var obj = File.ReadAllLines("Objects\\" + Name + @".obj");
            var mtl = File.ReadAllLines("Objects\\" + Name + @".mtl");

            IdentifyMtls(mtl);

            IdentifyVertexTextureCoords(obj);

            for (int lineIndex = 0; lineIndex < obj.Length; lineIndex++)
            {
                var s = obj[lineIndex];

                if (s.StartsWith("v "))
                {
                    var val = s.Substring(2);
                    var split = val.Split(' ');
                    Verts.Add(new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])));
                }
                if (s.StartsWith("usemtl"))
                {
                    var val = s.Substring(7);
                    var mtlIndex=-1;
                    for (int i = 0; i < mtls.Count; i++)
                    {
                        if (mtls[i].name == val)
                        {
                            mtlIndex = i;
                            break;
                        }
                    }
                    for (int i =lineIndex+1; i < obj.Length; i++)
                    {
                        var line = obj[i];
                        if (line.StartsWith("f"))
                        {
                            var val2 = line.Substring(2);
                            var split2 = val2.Split(" ");

                            if (val2.Contains('/'))
                            {
                                for (int TriPoint = 0; TriPoint < 3; TriPoint++)
                                {
                                    var split3 = split2[TriPoint].Split('/');
                                    Faces.Add(uint.Parse(split3[0])-1);
                                    FaceTextureCoordIndex.Add(int.Parse(split3[1]));
                                }
                                FaceMtl.Add((uint)mtlIndex);
                            }
                            else
                            {
                                for (int TriPoint = 0; TriPoint < 3; TriPoint++)
                                {
                                    var split3 = split2[TriPoint];
                                    Faces.Add(uint.Parse(split3)-1);
                                    FaceTextureCoordIndex.Add(0);
                                }
                                FaceMtl.Add((uint)mtlIndex);
                            }
                        } else
                        {
                            lineIndex = i;
                            break;
                        }
 
                    }
                }
            }

        }
        private void IdentifyVertexTextureCoords(string[] obj)
        {
            foreach (string s in obj)
            {
                if (s.StartsWith("vt "))
                {
                    var val = s.Substring(3);
                    var split = val.Split(' ');
                    TextureCoord.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
                }
            }
        }
        private void IdentifyMtls(string[] mtl)
        {
            for (int i = 0; i < mtl.Length; i++) //populate mtls
            {
                if (mtl[i].StartsWith("newmtl"))
                {
                    var newmtl = new Mtl(mtl[i].Substring(7));

                    for (int a = i; a < mtl.Length; a++)
                    {
                        var line = mtl[a];

                        if (line.StartsWith("Kd"))
                        {
                            var val = line.Substring(3);
                            var split = val.Split(' ');
                            newmtl.diffuse = (new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])));
                        }

                        if (line.StartsWith("Ka"))
                        {
                            var val = line.Substring(3);
                            var split = val.Split(' ');
                            newmtl.ambient = (new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])));
                        }

                        if (line.StartsWith("Ks"))
                        {
                            var val = line.Substring(3);
                            var split = val.Split(' ');
                            newmtl.specular = (new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])));
                        }

                        if (line.StartsWith("map_Kd"))
                        {
                            var val = line.Substring(7);
                            newmtl.diffuseMap = val;
                        }

                        if (line.StartsWith("map_Ka"))
                        {
                            var val = line.Substring(7);
                            newmtl.ambientMap = val;
                        }

                        if (line.StartsWith("map_Ks"))
                        {
                            var val = line.Substring(7);
                            newmtl.specularMap = val;
                        }

                        if (line.Length < 1 || a + 1 == mtl.Length)
                        {
                            mtls.Add(newmtl);
                            break;
                        }
                    }
                }
            }
        }
    }
}
