using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

namespace studyDX
{
    public partial class Form1 : Form
    {
        private Device device; //定义设备
        private Mesh mesh;
       Material[]   meshMaterials;
       Texture[] meshTextures;
         int[,] texturenum = new int[100, 100];
        Texture texture1;
        Texture texture2;
        Texture texture3;
        Texture[,] people = new Texture[4, 4];
        int x = 0;
        int minx = 0;
        int miny = 0;
        D3D.Sprite d3dsprite;
        Random rnd = new Random();
        public Form1()
        {
            InitializeComponent();
            this.ClientSize = new Size(800, 600); //设置窗口大小
            this.Text = "Direct3D Tutorial----MyDirect3DApplication";//设置窗口的标题
        }
        private void OnResetDevice(object sender, EventArgs e)
        {
            //渲染对象初始化
        }
        /// <summary>Initializes DirectX graphics</summary>
        /// <returns>true on success, false on failure</returns>
        private bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
                presentParams.BackBufferFormat = Format.Unknown;
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                presentParams.EnableAutoDepthStencil = true;

                int adapterOrdinal = D3D.Manager.Adapters.Default.Adapter;
                CreateFlags flags = CreateFlags.SoftwareVertexProcessing;
                D3D.Caps caps = D3D.Manager.GetDeviceCaps(adapterOrdinal, D3D.DeviceType.Hardware);
                if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                { flags = CreateFlags.HardwareVertexProcessing; }

                device = new Device(0, DeviceType.Hardware, this, flags, presentParams);
                device.DeviceLost += new EventHandler(this.InvalidateDeviceObjects);
                device.DeviceReset += new EventHandler(this.RestoreDeviceObjects);
                device.Disposing += new EventHandler(this.DeleteDeviceObjects);
                device.DeviceResizing += new CancelEventHandler(this.EnvironmentResizeing);
                texture1 = TextureLoader.FromFile(device, Application.StartupPath + "\\Jellyfish.jpg", 50, 50, 1, 0, Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, (unchecked((int)0xff000000)));
                texture2 = TextureLoader.FromFile(device, Application.StartupPath + "\\Koala.jpg", 50, 50, 1, 0, Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, (unchecked((int)0xff000000)));
                texture3 = TextureLoader.FromFile(device, Application.StartupPath + "\\Lighthouse.jpg", 62, 82, 1, 0, Format.A8R8G8B8, Pool.Managed, Filter.Point, Filter.Point, (unchecked((int)0xff000000)));
         
                d3dsprite = new Sprite(device);
                return true;
            }
            catch (DirectXException)
            {
                return false;
            }
        }

        protected virtual void InvalidateDeviceObjects(object sender, EventArgs e)
        { }
        protected virtual void RestoreDeviceObjects(object sender, EventArgs e)
        { D3D.Device device = (D3D.Device)sender; }
        protected virtual void DeleteDeviceObjects(object sender, EventArgs e)
        { }
        protected virtual void EnvironmentResizeing(object sender, CancelEventArgs e)
        { e.Cancel = true; }
        /// <summary>程序的核心部分----渲染</summary>
        private void Render()
        {
            if (device == null)
            {
                return;
            }
            device.Clear(D3D.ClearFlags.Target, System.Drawing.Color.Snow, 1.0f, 0);
            device.BeginScene();
            //此处添加你要渲染的对象
          //  this.DrawMap();
            this.LoadMesh("test.X");
            device.EndScene();
            device.Present();
        }
        private void DrawMap()
        {
            d3dsprite.Begin(SpriteFlags.AlphaBlend);
            for (int i = (45 + minx); i < (55 + minx); i++)
            {
                for (int j = (45 + miny); j < (55 + miny); j++)
                {
                    if (texturenum[i, j] == 0)
                    {
                        d3dsprite.Draw(texture1, new Rectangle(0, 0, 100, 100), new Vector3(0f, 0f, 0f), new Vector3(((float)(i - 45 - minx) * 50), ((float)(j - (45 + miny)) * 50), 0f), Color.FromArgb(255, 255, 255, 255));
                    }
                    else if (texturenum[i, j] == 1)
                    {
                        d3dsprite.Draw(texture2, new Rectangle(0, 0, 50, 50), new Vector3(0f, 0f, 0f), new Vector3(((float)(i - 45 - minx) * 50), ((float)(j - (45 + miny)) * 50), 0f), Color.FromArgb(255, 255, 255, 255));
                    }
                    else
                    {
                        d3dsprite.Draw(texture3, new Rectangle(0, 0, 50, 50), new Vector3(0f, 0f, 0f), new Vector3(((float)(i - 45 - minx) * 50), ((float)(j - (45 + miny)) * 50), 0f), Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }        
            d3dsprite.End();
        }
        /// <summary>Application idle event. renders frames.</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        public void OnApplicationIdle(object sender, EventArgs args)
        {
            while (AppStillIdle)
            {
                Render();
            }
        }

        // 检测程序是否处于空闲
        private bool AppStillIdle
        {
            get
            {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }
        /// <summary>Native methods</summary>
        public class NativeMethods
        {
            /// <summary>Windows Message</summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Message
            {
                public IntPtr hWnd;
                public uint msg;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint time;
                public System.Drawing.Point p;
            }

            [System.Security.SuppressUnmanagedCodeSecurity]
            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
        }
        private void Initiallizetexturenum()
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    texturenum[i, j] = rnd.Next(0, 3);
                }
            }
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Form1 form = new Form1())
            {
                if (!form.InitializeGraphics())
                {
                    MessageBox.Show("很抱歉，不能初始化3D设备!");
                    form.Dispose();
                    return;
                }
                Application.Idle += new EventHandler(form.OnApplicationIdle);
                Application.Run(form);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Initiallizetexturenum();

            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            x = x + 1;
        }


        private void LoadMesh(string file)
        {
            ExtendedMaterial[] mtrl;
            mesh = Mesh.FromFile(file, MeshFlags.Managed, device, out mtrl);

            if ((mtrl != null) && (mtrl.Length > 0))
            {
                meshMaterials = new Material[mtrl.Length]; //事先建立的Material数组，用于存放从.x文件中读入的材质信息
                meshTextures = new Texture[mtrl.Length]; //事先建立的Texture数组，用于存放从.x文件中读入的纹理信息

                for (int i = 0; i < mtrl.Length; i++)
                {
                    meshMaterials[i] = mtrl[i].Material3D;
                    if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename != string.Empty))
                    {
                        meshTextures[i] = TextureLoader.FromFile(device, mtrl[i].TextureFilename);//纹理的文件名是存在.x中的
                    }
                }

                for (int i = 0; i < meshMaterials.Length; i++) //meshMaterials上面已经提到，是被附了值的Material数组
                {
                    device.Material = meshMaterials[i];
                    device.SetTexture(0, meshTextures[i]);
                    mesh.DrawSubset(i); //茶壶那个例子已经谈到，这是在画这个3d实体
                }

                device.Lights[0].Diffuse = System.Drawing.Color.White;
                device.Lights[0].Enabled = true;
                device.Lights[0].Direction = new Vector3(0.0f, 1.0f, 0.0f);//以原点为起点
                device.Lights[0].Type = LightType.Directional;

                device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, -1000.0f, 200.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
                device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, this.Width / this.Height, 1.0f, 10000.0f);
            }
        }

    }
}
