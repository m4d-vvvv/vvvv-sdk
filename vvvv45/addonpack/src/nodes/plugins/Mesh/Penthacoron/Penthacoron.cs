#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Shared.VSlimDX;

using SlimDX;
using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class Penthacoron: IPlugin, IDisposable, IPluginDXMesh
	{
		#region field declaration

        //input pin declaration
        private IValueIn Radius;
        private double r = 1.0;
        //private float r = 1.0f;

        private IValueIn Perspective;
        private double p = 0.5;

        //private IValueIn LeftQuaternion;
        //private Vector4D L = new Vector4D(1, 0, 0, 0);
        //private Quaternion left = new Quaternion();

        private IValueIn LeftX;
        private IValueIn LeftY; 
        private IValueIn LeftZ; 
        private IValueIn LeftW;

        private double leftX;
        private double leftY;
        private double leftZ;
        private double leftW;

        //private IValueIn RightQuaternion;
        //private Vector4D R = new Vector4D(1, 0, 0, 0);
        //private Quaternion right;

        private IValueIn RightX;
        private IValueIn RightY;
        private IValueIn RightZ;
        private IValueIn RightW;

        private double rightX;
        private double rightY;
        private double rightZ;
        private double rightW;

        //private Vector4D[] vertex = new Vector4D[5];
        private Vector4[] vertex = new Vector4[5];
        
		//the host (mandatory)
		private IPluginHost FHost;
		//Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//a mesh output pin
		private IDXMeshOut FMyMeshOutput;
		
		//a list that holds a mesh for every device
		private Dictionary<int, Mesh> FDeviceMeshes = new Dictionary<int, Mesh>();

        private SlimDX.DataStream sVx;
        private SlimDX.DataStream sIx;
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public Penthacoron()
		{
			//the nodes constructor
			//nothing to declare for this node
		}
		
		// Implementing IDisposable's Dispose method.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.

				//FHost.Log(TLogType.Debug, "PluginMeshTemplate is being deleted");
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~Penthacoron()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}
		#endregion constructor/destructor
		
		#region node name and info
		
		//provide node infos
		private static IPluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Penthacoron";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "EX9.Geometry";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "fibo";
					//describe the nodes function
                    FPluginInfo.Help = "The 5 vertices regular polytope in 4D";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "4D";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//leave below as is
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
					System.Diagnostics.StackFrame sf = st.GetFrame(0);
					System.Reflection.MethodBase method = sf.GetMethod();
					FPluginInfo.Namespace = method.DeclaringType.Namespace;
					FPluginInfo.Class = method.DeclaringType.Name;
					//leave above as is
				}
				return FPluginInfo;
			}
		}

		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		#endregion node name and info
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

            //create inputs
            FHost.CreateValueInput("Radius",1,null,TSliceMode.Single,TPinVisibility.True,out Radius);
            Radius.SetSubType(0.001,1000.0,0.001,1.0,false,false,false);

            FHost.CreateValueInput("Perspective", 1, null, TSliceMode.Single, TPinVisibility.True, out Perspective);
            Perspective.SetSubType(0.1, 1, 0.001, 0.5, false, false, false);
            /*
            FHost.CreateValueInput("Left Quaternion Input",4, null,TSliceMode.Single, TPinVisibility.True, out LeftQuaternion);

            FHost.CreateValueInput("Right Quaternion Input",4,null, TSliceMode.Single, TPinVisibility.True, out RightQuaternion);
	    	*/

            FHost.CreateValueInput("LeftX", 1, null, TSliceMode.Single, TPinVisibility.True, out LeftX);
            LeftX.SetSubType(-1, 1, 0.001, 1, false, false, false);
            FHost.CreateValueInput("LeftY", 1, null, TSliceMode.Single, TPinVisibility.True, out LeftY);
            LeftY.SetSubType(-1, 1, 0.001, 0, false, false, false);
            FHost.CreateValueInput("LeftZ", 1, null, TSliceMode.Single, TPinVisibility.True, out LeftZ);
            LeftZ.SetSubType(-1, 1, 0.001, 0, false, false, false);
            FHost.CreateValueInput("LeftW", 1, null, TSliceMode.Single, TPinVisibility.True, out LeftW);
            LeftW.SetSubType(-1, 1, 0.001, 0, false, false, false);

            FHost.CreateValueInput("RightX", 1, null, TSliceMode.Single, TPinVisibility.True, out RightX);
            RightX.SetSubType(-1, 1, 0.001, 1, false, false, false);
            FHost.CreateValueInput("RightY", 1, null, TSliceMode.Single, TPinVisibility.True, out RightY);
            RightY.SetSubType(-1, 1, 0.001, 0, false, false, false);
            FHost.CreateValueInput("RightZ", 1, null, TSliceMode.Single, TPinVisibility.True, out RightZ);
            RightZ.SetSubType(-1, 1, 0.001, 0, false, false, false);
            FHost.CreateValueInput("RightW", 1, null, TSliceMode.Single, TPinVisibility.True, out RightW);
            RightW.SetSubType(-1, 1, 0.001, 0, false, false, false);

			//create outputs
			FHost.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out FMyMeshOutput);
      
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			FMyMeshOutput.SliceCount = SpreadMax; // by now, this node is not spreadable so it shoul always be 1.

            bool clearMesh = false;

            if (Radius.PinIsChanged)
            {
                Radius.GetValue(0, out r);
                clearMesh = true;
            }

            if (Perspective.PinIsChanged) 
            {
                Perspective.GetValue(0, out p);
                clearMesh = true;
            }

            if (LeftX.PinIsChanged)
            {
                LeftX.GetValue(0, out leftX);
                clearMesh = true;
            }

            if (LeftY.PinIsChanged)
            {
                LeftY.GetValue(0, out leftY);
                clearMesh = true;
            }

            if (LeftZ.PinIsChanged)
            {
                LeftZ.GetValue(0, out leftZ);
                clearMesh = true;
            }

            if (LeftW.PinIsChanged)
            {
                LeftW.GetValue(0, out leftW);
                clearMesh = true;
            }

            if (RightX.PinIsChanged)
            {
                RightX.GetValue(0, out rightX);
                clearMesh = true;
            }

            if (RightY.PinIsChanged)
            {
                RightY.GetValue(0, out rightY);
                clearMesh = true;
            }

            if (RightZ.PinIsChanged)
            {
                RightZ.GetValue(0, out rightZ);
                clearMesh = true;
            }

            if (RightW.PinIsChanged)
            {
                RightW.GetValue(0, out rightW);
                clearMesh = true;
            }

            //finally
            if (clearMesh) 
            { 
                FDeviceMeshes.Clear(); 
            }
		}
		
		#endregion mainloop
		
		#region DXMesh
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			//Called by the PluginHost every frame for every device. Therefore a plugin should only do 
			//device specific operations here and still keep node specific calculations in the Evaluate call.

            try
            {
                Mesh m = FDeviceMeshes[OnDevice];  
            }
            catch
            {
                //if resource is not yet created on given Device, create it now
                //FHost.Log(TLogType.Debug, "Creating Resource...");
                Device dev = Device.FromPointer(new IntPtr(OnDevice));
                FDeviceMeshes.Add(OnDevice, createMesh(dev));

                //dispose device
                dev.Dispose();
            }

		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			//Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
			//This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()

			try
			{
				Mesh m = FDeviceMeshes[OnDevice];
				//FHost.Log(TLogType.Debug, "Destroying Resource...");
				FDeviceMeshes.Remove(OnDevice);
				
				//dispose mesh
				m.Dispose();
			}
			catch
			{
				//resource is not available for this device. good. nothing to do then.
			}
		}
		
		public void GetMesh(IDXMeshOut ForPin, int OnDevice, out int MeshPointer)
		{
			// Called by the PluginHost everytime a mesh is accessed via a pin on the plugin.
			// This is called from the PluginHost from within DirectX BeginScene/EndScene,
			// therefore the plugin shouldn't be doing much here other than handing back the right mesh
			
			MeshPointer = 0;
			//in case the plugin has several mesh outputpins a test for the pin can be made here to get the right mesh.
			if (ForPin == FMyMeshOutput)
			{
				Mesh m = FDeviceMeshes[OnDevice];
				if (m != null)
					MeshPointer = m.ComPointer.ToInt32();
			}
		}

        public Mesh createMesh(Device dev)
        {

            int NumIndices = 10;
            int NumVertices = 5;

            Matrix Projection = new Matrix();
            Projection.set_Rows(0,new Vector4(1.0f,0.0f,0.0f,0.0f));
            Projection.set_Rows(1,new Vector4(0.0f,1.0f,0.0f,0.0f));
            Projection.set_Rows(2,new Vector4(0.0f,0.0f,1.0f,0.0f));
            Projection.set_Rows(3, new Vector4((float)p, (float)p, (float)p, 0.0f));

            /*
            Matrix4x4 Projection = new Matrix4x4(
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                p,p,p,0
                );
         
             
            vertex[0] = new Vector4D(0, 0, 0, 1);
            vertex[1] = new Vector4D(-0.559, 0.559, 0.559, -0.25);
            vertex[2] = new Vector4D(0.559, -0.559, 0.559, -0.25);
            vertex[3] = new Vector4D(0.559, 0.559, -0.559, -0.25);
            vertex[4] = new Vector4D(-0.559, -0.559, -0.559, -0.25);
            */

            vertex[0] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            vertex[1] = new Vector4(-0.559f, 0.559f, 0.559f, -0.25f);
            vertex[2] = new Vector4(0.559f, -0.559f, 0.559f, -0.25f);
            vertex[3] = new Vector4(0.559f, 0.559f, -0.559f, -0.25f);
            vertex[4] = new Vector4(-0.559f, -0.559f, -0.559f, -0.25f);

            Quaternion q;
            Quaternion right; // left e right si possono togliere??
            Quaternion left;

            for (int i = 0; i < NumVertices; i++) 
            {
  
                // scale it
                //vertex[i] *= r;

               q = new Quaternion(vertex[i].X,vertex[i].Y,vertex[i].Z,vertex[i].W);

               left = new Quaternion((float)leftX, (float)leftY, (float)leftZ, (float)leftW);
               right = new Quaternion((float)rightX, (float)rightY, (float)rightZ, (float)rightW);

               
               q = Quaternion.Multiply(left,q);
               q = Quaternion.Multiply(q, right);
               // scale it
               q = Quaternion.Multiply(q, (float)r);

               vertex[i].X = q.X;
               vertex[i].Y = q.Y;
               vertex[i].Z = q.Z;
               vertex[i].W = q.W;

               vertex[i] = Vector4.Transform(vertex[i], Projection);
                FHost.Log(TLogType.Debug, vertex[i].ToString());

                /* 
                 * let transform the vertices in 4D space before to project them in 3D space:
                 * 
                 * vertex |--> L * vertex * R , 
                 *
                 * left and right action,
                 * based on quaternion multiplication law
                 * 
                 * (a1,b1,c1,d1)(a2,b2,c2,d2) =(a1a2 − b1b2 − c1c2 − d1d2,a1b2 + b1a2 + c1d2 − d1c2,a1c2 − b1d2 + c1a2 + d1b2,a1d2 + b1c2 − c1b2 + d1a2).
                 */

                // x->w , y->x , z->y, w->z
                // apply left tranform.
/*                vertex[i].x = L.x * vertex[i].x - L.y * vertex[i].y - L.z * vertex[i].z - L.w * vertex[i].w;
                vertex[i].y = L.x * vertex[i].y + L.y * vertex[i].x + L.z * vertex[i].w - L.w * vertex[i].z;
                vertex[i].z = L.x * vertex[i].z - L.y * vertex[i].w + L.z * vertex[i].x + L.w * vertex[i].y;
                vertex[i].w = L.x * vertex[i].w + L.y * vertex[i].z - L.z * vertex[i].y + L.w * vertex[i].x;
                */
                //vertex[i].w = L.w * vertex[i].w - L.x * vertex[i].x - L.y * vertex[i].y - L.z * vertex[i].z;
                //vertex[i].x = L.w * vertex[i].x + L.x * vertex[i].w + L.y * vertex[i].z - L.z * vertex[i].y;
                //vertex[i].y = L.w * vertex[i].y - L.x * vertex[i].z + L.y * vertex[i].w + L.z * vertex[i].x;
                //vertex[i].z = L.w * vertex[i].z + L.x * vertex[i].y - L.y * vertex[i].x + L.z * vertex[i].w;

                //SlimDX.Quaternion.Multiply(left, right);
                /*
                vertex[i] = new Vector4D(
                    L.w * vertex[i].x + L.x * vertex[i].w + L.y * vertex[i].z - L.z * vertex[i].y,
                    L.w * vertex[i].y - L.x * vertex[i].z + L.y * vertex[i].w + L.z * vertex[i].x,
                    L.w * vertex[i].z + L.x * vertex[i].y - L.y * vertex[i].x + L.z * vertex[i].w,
                    L.w * vertex[i].w - L.x * vertex[i].x - L.y * vertex[i].y - L.z * vertex[i].z
                    );
                */
                // apply right tranform.
                /*vertex[i].x = vertex[i].x * R.x - vertex[i].y * R.y - vertex[i].z * R.z - vertex[i].w * R.w;
                vertex[i].y = vertex[i].x * R.y + vertex[i].y * R.x + vertex[i].z * R.w - vertex[i].w * R.z;
                vertex[i].z = vertex[i].x * R.z - vertex[i].y * R.w + vertex[i].z * R.x + vertex[i].w * R.y;
                vertex[i].w = vertex[i].x * R.w + vertex[i].y * R.z - vertex[i].z * R.y + vertex[i].w * R.x;
               */
                //vertex[i].w = vertex[i].x * R.w - vertex[i].x * R.x - vertex[i].y * R.y - vertex[i].z * R.z;
                //vertex[i].x = vertex[i].x * R.x + vertex[i].x * R.w + vertex[i].y * R.z - vertex[i].z * R.y;
                //vertex[i].y = vertex[i].x * R.y - vertex[i].x * R.z + vertex[i].y * R.w + vertex[i].z * R.x;
                //vertex[i].z = vertex[i].x * R.z + vertex[i].x * R.y - vertex[i].y * R.x + vertex[i].z * R.w;
                /*
                vertex[i] = new Vector4D(
                    vertex[i].x * R.x + vertex[i].x * R.w + vertex[i].y * R.z - vertex[i].z * R.y,
                    vertex[i].x * R.y - vertex[i].x * R.z + vertex[i].y * R.w + vertex[i].z * R.x,
                    vertex[i].x * R.z + vertex[i].x * R.y - vertex[i].y * R.x + vertex[i].z * R.w,
                    vertex[i].x * R.w - vertex[i].x * R.x - vertex[i].y * R.y - vertex[i].z * R.z
                    );
                */
                // projection in 3D euclidean space.
                //vertex[i] = Projection * vertex[i]; 
            }

            // create new Mesh
            Mesh NewMesh = new Mesh(dev, NumIndices, NumVertices,
                                    MeshFlags.Dynamic | MeshFlags.WriteOnly,
                                    VertexFormat.Position);
            
            // lock buffers
            sVx = NewMesh.LockVertexBuffer(LockFlags.Discard);
            sIx = NewMesh.LockIndexBuffer(LockFlags.Discard);

            // write buffers
            for (int i = 0; i < NumVertices; i++)
            {
                //Vector3 v = VVVV.Shared.VSlimDX.VSlimDXUtils.Vector3DToSlimDXVector3(vertex[i].xyz);
                //sVx.Write(v);
                
                sVx.Write(vertex[i].X);
                sVx.Write(vertex[i].Y);
                sVx.Write(vertex[i].Z);
            }
            
            sIx.Write<short>(0); sIx.Write<short>(1); sIx.Write<short>(2);
            sIx.Write<short>(0); sIx.Write<short>(1); sIx.Write<short>(3);
            sIx.Write<short>(0); sIx.Write<short>(1); sIx.Write<short>(4);
            sIx.Write<short>(0); sIx.Write<short>(2); sIx.Write<short>(3);
            sIx.Write<short>(0); sIx.Write<short>(2); sIx.Write<short>(4);
            sIx.Write<short>(0); sIx.Write<short>(3); sIx.Write<short>(4);
            sIx.Write<short>(1); sIx.Write<short>(2); sIx.Write<short>(3);
            sIx.Write<short>(1); sIx.Write<short>(2); sIx.Write<short>(4);
            sIx.Write<short>(1); sIx.Write<short>(3); sIx.Write<short>(4);
            sIx.Write<short>(2); sIx.Write<short>(3); sIx.Write<short>(4);

            // unlock buffers
            NewMesh.UnlockIndexBuffer();
            NewMesh.UnlockVertexBuffer();

            return NewMesh; 
        }

		#endregion
	}
}
