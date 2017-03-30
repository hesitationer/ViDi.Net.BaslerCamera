using System;
using System.Collections.Generic;
using System.Linq;
using ViDi2.Camera;
using ViDi2.Training.UI;
using Basler.Pylon;

namespace ViDi2.Camera
{
    public class BaslerCameraPlugin : ICameraProvider , IPlugin
    {


        public string Name
        {
            get { return "Basler"; }
        }

        List<ICamera> cameras = new List<ICamera>();

        public System.Collections.ObjectModel.ReadOnlyCollection<ICamera> Discover()
        {
            cameras.Clear();

            List<ICameraInfo> allCameras = CameraFinder.Enumerate();

             foreach (ICameraInfo cameraInfo in allCameras)
             {
                 string t = cameraInfo[CameraInfoKey.FullName];
                 cameras.Add(new BaslerCamera(cameraInfo, this));
             }

            return cameras.AsReadOnly();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<ICamera> Cameras
        {
            get { return cameras.AsReadOnly();  }
        }

        public void DeInitialize()
        {
            foreach(var camera in Cameras)
            {
                camera.Close();
            }
        }

        public string Description
        {
            get { return "Provides Basler Camera Connectivity"; }
        }

        IPluginContext context;

        public void Initialize(IPluginContext context)
        {
            this.context = context;
        }

        public int Version
        {
            get { return 0; }
        }
    }
}
