
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
            using (Renderer game = new Renderer(800, 600, "LearnOpenTK"))
            {
                //game.Tables
                game.Run();
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
