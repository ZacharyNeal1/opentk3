using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using static System.Net.Mime.MediaTypeNames;

namespace opentk3
{
    public class Renderer : GameWindow
    {


        public Renderer(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { }

        public List<Table> Tables = [];

        Table t;
        Table tb;
        Table tbb;

        public MainLoop Game;

        protected override void OnLoad()
        {
            base.OnLoad();


            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            //t = new Table(this, ShaderBuilder.full, ShaderBuilder.full2, true);
            //t = new Table(this, "Shaders\\shader.vert", "Shaders\\shader.frag");
            tb = new Table(this, "Shaders\\shader2.vert", "Shaders\\shader2.frag");
            tbb = new Table(this, "Shaders\\shader3.vert", "Shaders\\shader3.frag");

            //tbb.vertices =

            // new float[] {
            //    -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, //Bottom-left vertex
            //    0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, //Bottom-right vertex
            //    0.0f,  0.5f, 0.0f, 0.0f, 0.0f, 1.0f  //Top vertex
            //};

            Game = new MainLoop(this);
            //Code goes here
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            this.Title = (e.Time).ToString("#0000.00") + " SEC | " + (1.0/e.Time).ToString("#0000.00") + " FPS";

            GL.Clear(ClearBufferMask.ColorBufferBit);

            var mat = Game.Camera.WorldToScreenMatrix(Size);

            foreach (Table t in Tables)
                t.FullDraw(mat);

            //foreach (KeyValuePair<string, Table> tableKey )

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }
        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            base.OnUnload();
        }
    }
    public static class Tables
    { 
    
    }

    /// <summary>
    /// Contains a shader, drawing buffers, meshes and really everything needed to draw meshes
    /// </summary>
    public class Table
    {
//        public float[] vertices = {
//    -0.5f, -0.5f, 0.0f, //Bottom-left vertex
//     0.5f, -0.5f, 0.0f, //Bottom-right vertex
//     0.0f,  0.5f, 0.0f  //Top vertex
//};
//        uint[] indices =
//{
//            0,1,2
//        };

        public static int count = 0;
        public int ID;

        public string Name = "Unnamed Shader";

        public Renderer renderer;

        public FullShader shader;

        int VertexBufferObject; //all of the points
        int VertexArrayObject; //how the points are stored (attributes)
        int ElementBufferObject; // indexes of the points (sets of triangles)

        public List<MeshData> Meshes = new List<MeshData>();

        public float[] Vertices = new float[0];
        public uint[] Indices = new uint[0];

        public bool UseMatrix = false;

        public Table(Renderer r, string vertexPath, string fragmentPath, bool rawCode = false)
        {
            
            
            ID = count++;

            renderer = r;
            renderer.Unload += Unload;

            shader = new FullShader(vertexPath, fragmentPath, rawCode);

            GenerateBuffers();

            BindBuffers();

            LoadVAO();
        }

        /// <summary>
        /// this expects that the vao is already binded. sets up the attribute pointers for the vao
        /// </summary>
        public void LoadVAO()
        {
            //if (GL.GetInteger(GetPName.VertexArray) != VertexArrayObject) GL.BindVertexArray(VertexArrayObject);

            int stride = 0;
            foreach (AttributeInfo a in shader.Attributes)
                stride += a.realSize * sizeof(float);
            int offset = 0;

            for (int i = 0; i < shader.Attributes.Length; i++)
            {
                var attributeInfo = shader.Attributes[i];
                GL.VertexAttribPointer(i, attributeInfo.realSize, VertexAttribPointerType.Float, false, stride, offset);
                GL.EnableVertexAttribArray(i);

                offset += attributeInfo.realSize * sizeof(float);
            }
        }

        public void GenerateBuffers()
        {
            VertexBufferObject = GL.GenBuffer();
            VertexArrayObject = GL.GenVertexArray();
            ElementBufferObject = GL.GenBuffer();
        }

        public void UpdateBuffer()
        {
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StreamDraw);

            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StreamDraw);
        }

        public void BindBuffers() //should come before updating the buffers
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        }

        public void FullDraw()
        {
            shader.shader.Use();
            Draw();
        }

        public void FullDraw(Matrix4 ViewMatrix)
        {
            shader.shader.Use();
            int loc = GL.GetUniformLocation(shader.shader.Handle, "view");
            if (loc != -1)
            {
                GL.UniformMatrix4(loc, true, ref ViewMatrix);
            }
                Draw();
        }

        void UpdateRenderData()
        {
            //makes all the meshes generate thier render data and makes the arrays long enough for the data
            //{
            int length1 = 0;
            int length2 = 0;

            for (int i = 0; i < Meshes.Count; i++)
            {
                var mesh = Meshes[i];
                mesh.Transform();
                mesh.GenerateRenderData(length1);
                length1 += mesh.RenderData.Length;
                length2 += mesh.RenderData1.Length;
            }

            if (Vertices.Length != length1)
                Array.Resize(ref Vertices, length1);

            if (Indices.Length != length2)
                Array.Resize(ref Indices, length2);
            //}

            int index1 = 0; //the indexes of where in the array the data will be copied to
            int index2 = 0;

            foreach (MeshData m in Meshes)
            {
                //copy over vertex data
                Array.Copy(m.RenderData, 0, Vertices, index1, m.RenderData.Length);
                index1 += m.RenderData.Length;

                //copy over indcies
                Array.Copy(m.RenderData1, 0, Indices, index2, m.RenderData1.Length);
                index2 += m.RenderData1.Length;
            } //completed all vertex data
        }

        void Draw()
        {
            UpdateRenderData();
            BindBuffers();
            UpdateBuffer();
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void Unload()
        {
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteBuffer(ElementBufferObject);
        }

    }
    //HEAVY thanks to https://neokabuto.blogspot.com/2014/03/opentk-tutorial-6-part-1-loading.html
    //article written by "Kabuto"?
    /// <summary>
    /// A class that holds a shader along with more infromation about it such as attributes and uniforms
    /// </summary>
    public class FullShader
    {
        public Shader shader;

        public static int count = 0;
        public int ID;

        public int ProgramID = -1;
        public int VertexShader = -1;
        public int FragmentShader = -1;
        public int AttributeCount = 0;
        public int UniformCount = 0;

        public AttributeInfo[] Attributes = [];
        public UniformInfo[] Uniforms = [];


        public FullShader(string vertexPath, string fragmentPath, bool rawCode)
        {
            ID = count++;

            shader = new Shader(vertexPath, fragmentPath, rawCode);

            ProgramID = shader.Handle;

            FindAttributesAndUniforms();

            shader.DeleteDetatch();
        }
        private void FindAttributesAndUniforms()
        {
            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveAttributes, out AttributeCount);

            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniforms, out UniformCount);

            Attributes = new AttributeInfo[AttributeCount];
            Uniforms = new UniformInfo[UniformCount]; 

            for (int i = 0; i < AttributeCount; i++)
            {
                AttributeInfo info = new AttributeInfo();
                int length = 0;
                GL.GetActiveAttrib(ProgramID, i, 256, out length, out info.size, out info.type, out info.name);

                switch (info.type) //real size is used in stride caculations
                {
                    case ActiveAttribType.FloatVec3:
                        info.realSize = 3;
                        break;
                    case ActiveAttribType.FloatVec4:
                        info.realSize = 4;
                        break;
                    case ActiveAttribType.FloatVec2:
                        info.realSize = 2;
                        break;
                    case ActiveAttribType.Float:
                        info.realSize = 1;
                        break;
                    default:
                        info.realSize = info.size;
                        break;
                }
                info.address = GL.GetAttribLocation(ProgramID, info.name);

                Attributes[i] = (info);
            }
            //attributes was orginally a list(hence why .Sort is there) but switched to arrays for faster handling
            //too lazy to care about the opitmation of code that runs only a few times then never again
            // hell, this was orginally orginally a dictonary
            var attribList = Attributes.ToList();
            attribList.Sort(delegate (AttributeInfo x, AttributeInfo y)
            {
                if (x.address < y.address) return -1;
                if (x.address > y.address) return 1;
                return 0;
            });
            Attributes = attribList.ToArray();

            for (int i = 0; i < UniformCount; i++)
            {
                UniformInfo info = new UniformInfo();
                int length = 0;
                GL.GetActiveUniform(ProgramID, i, 256, out length, out info.size, out info.type, out info.name);
                info.address = GL.GetUniformLocation(ProgramID, info.name);

                Uniforms[i] = (info);
            }
        }
    }
    public class Shader
    {
        public int Handle;

        int VertexShader;
        int FragmentShader;

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public Shader(string vertexPath, string fragmentPath, bool rawCode = false)
        {
            string VertexShaderSource;

            if (rawCode)
                VertexShaderSource = vertexPath;
            else 
                VertexShaderSource = File.ReadAllText(vertexPath);

            string FragmentShaderSource;

            if (rawCode)
                FragmentShaderSource = fragmentPath; 
            else
                FragmentShaderSource = File.ReadAllText(fragmentPath);

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int success1);
            if (success1 == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(infoLog);
            }

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success2);
            if (success2 == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

        }

        public void DeleteDetatch()
        {
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }
        public int GetUniformLocation(string uniformName)
        {
            return GL.GetUniformLocation(Handle, uniformName);
        }
        
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            if (disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    public class AttributeInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public ActiveAttribType type;
        public int realSize = 0;
    }

    public class UniformInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public ActiveUniformType type;
    }
}
