
using System.Reflection;
using OpenTK.Mathematics;

namespace opentk3
{
    internal class Start
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello, World!");
            foreach(string s in args)
            System.Console.WriteLine(s);
            var a = new WavefrontFile("object4");

            using (Renderer game = new Renderer(800, 600, "LearnOpenTK"))
            {
                //game.Tables
                game.Run();
            }
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

        public List<uint> FaceTextureCoordIndex = new List<uint>();

        public List<Mtl> mtls = new List<Mtl>();

        public WavefrontFile(string name)
        {
            Name = name;

            var obj = File.ReadAllLines("Objects\\" + Name + @".obj");
            var mtl = File.ReadAllLines("Objects\\" + Name + @".mtl");

            for(int i = 0; i < mtl.Length; i++) //populate mtls
            {
                if (mtl[i].StartsWith("newmtl"))
                {
                    var newmtl = new Mtl(mtl[i].Substring(7));

                    for (int a = i; a < mtl.Length; a ++)
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

            foreach (string s in obj)
            {
                if (s.StartsWith("vt "))
                {
                    var val = s.Substring(3);
                    var split = val.Split(' ');
                    TextureCoord.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
                }
            }

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
                                    FaceTextureCoordIndex.Add(uint.Parse(split3[1]));
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
    }

    public class ObjFile
    {
        public List<Vector3> vertexes { get; set; }
        public List<int> faceIndices { get; set; }
        public List<int> textureIndexPerVertex { get; set; }
        public List<Vector2> textureCoordnatesPerVertex { get; set; }

        //fragment from a past project
        //public static VertexStore DefaultVertexStore { get; set; }

        string[] lines { get; set; }


        public ObjFile(string name)
        {
            vertexes = [];
            faceIndices = [];
            textureIndexPerVertex = [];
            textureCoordnatesPerVertex = [];


            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Objects/" + name + ".obj");

            lines = File.ReadAllLines(path);
            //var sections = new List<string[]>();
            string input = "";
            foreach (string s in File.ReadAllLines(path))
                input += s + "\n";

            for (int i = 0; i < lines.Length; i++)
            {
                var str = lines[i];

                if (str.StartsWith("vt "))
                {
                    var val = str.Substring(3);
                    var split = val.Split(' ');
                    textureCoordnatesPerVertex.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
                }
                if (str.StartsWith("v "))
                {
                    var val = str.Substring(2);
                    var split = val.Split(' ');
                    vertexes.Add(new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])));
                }
                if (str.StartsWith("f "))
                {

                    var val = str.Substring(2);
                    var split = val.Split(' ');

                    if (!str.Contains("/"))
                    {
                        var f = new int[] { int.Parse(split[0]) - 1, int.Parse(split[1]) - 1, int.Parse(split[2]) - 1 };
                        faceIndices.AddRange(f);
                        textureIndexPerVertex.Add(-1);
                        textureIndexPerVertex.Add(-1);
                        textureIndexPerVertex.Add(-1);
                    }
                    else
                    {
                        var f = new int[]
                        {
                               int.Parse(split[0].Split('/')[0])-1,
                               int.Parse(split[1].Split('/')[0])-1,
                               int.Parse(split[2].Split('/')[0])-1,
                        };
                        faceIndices.AddRange(f);
                        var b = split[0].Split('/');
                        textureIndexPerVertex[int.Parse(b[0]) - 1] = int.Parse(b[1]) - 1;
                        b = split[1].Split('/');
                        textureIndexPerVertex[int.Parse(b[0]) - 1] = int.Parse(b[1]) - 1;
                        b = split[2].Split('/');
                        textureIndexPerVertex[int.Parse(b[0]) - 1] = int.Parse(b[1]) - 1;

                    }
                }
            }
        }
    } //the inital for mesh data and a easy way to decompile obj files made by me :)
}
