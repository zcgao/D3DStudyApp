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
    public partial class mainForm : Form
    {
        private Device device; //
        private Mesh mesh;
       Material[]   meshMaterials;
       Texture[] meshTextures;
        int[,] texturenum = new int[100, 100];
        Texture[,] people = new Texture[4, 4];
        int x = 0;
        D3D.Sprite d3dsprite;
        Random rnd = new Random();
        public mainForm()
        {
            InitializeComponent();
            this.ClientSize = new Size(800, 600); //form size
            this.Text = "Direct3D ----Test Application";//form title
        }
        private void OnResetDevice(object sender, EventArgs e)
        {
            //
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
        /// <summary>render </summary>
        private void Render()
        {
            if (device == null)
            {
                return;
            }
            device.Clear(D3D.ClearFlags.Target, System.Drawing.Color.Blue, 1.0f, 0);
            device.BeginScene();
            //
            this.LoadMesh("test.X");
            device.EndScene();
            device.Present();
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

        // 
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
            using (mainForm form = new mainForm())
            {
                if (!form.InitializeGraphics())
                {
                    MessageBox.Show("sorry can not initial device!");
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
                meshMaterials = new Material[mtrl.Length]; //
                meshTextures = new Texture[mtrl.Length]; //

                for (int i = 0; i < mtrl.Length; i++)
                {
                    meshMaterials[i] = mtrl[i].Material3D;
                    if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename != string.Empty))
                    {
                        meshTextures[i] = TextureLoader.FromFile(device, mtrl[i].TextureFilename);//
                    }
                }

                for (int i = 0; i < meshMaterials.Length; i++) //
                {
                    device.Material = meshMaterials[i];
                    device.SetTexture(0, meshTextures[i]);
                    mesh.DrawSubset(i); 
                }

                device.Lights[0].Diffuse = System.Drawing.Color.White;
                device.Lights[0].Enabled = true;
                device.Lights[0].Direction = new Vector3(0.0f, 1.0f, 0.0f);//
                device.Lights[0].Type = LightType.Directional;

                device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, -1000.0f, 200.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
                device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, this.Width / this.Height, 1.0f, 10000.0f);
            }
        }

    }
}
