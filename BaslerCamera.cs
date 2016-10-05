using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basler.Pylon;
using System.ComponentModel;

namespace ViDi2.Camera
{

    class BaslerCameraCapabilities : ICameraCapabilities
    {

        public bool CanGrabSingle
        {
            get { return false; }
        }

        public bool CanGrabContinuous
        {
            get { return true; }
        }

        public bool CanSaveParametersToFile
        {
            get { return false; }
        }

        public bool CanSaveParametersToDevice
        {
            get { return false; }
        }
    }


    class BaslerCamera : ICamera
    {
        BaslerCameraPlugin provider;

        BaslerCameraCapabilities capabilities = new BaslerCameraCapabilities();

        Basler.Pylon.Camera camera = null;
        Basler.Pylon.ICameraInfo info;
        public event ImageGrabbedHandler ImageGrabbed;

        static Version Sfnc2_0_0 = new Version(2, 0, 0);

        public BaslerCamera(ICameraInfo info, BaslerCameraPlugin provider)
        {
            this.info = info;
            this.provider = provider;

            camera = new Basler.Pylon.Camera(info);

            camera.CameraOpened += Configuration.AcquireContinuous;

            camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;

            parameters = new List<ICameraParameter>
            {
                new CameraParameter("Exposure Time", () => ExposureTime, (value) => ExposureTime = (double)value),
                new CameraParameter("Frame Rate", () => FrameRate, (value) => FrameRate = (double)value)
              //  new CameraParameter("Binning", () => Binning, (value) => Binning = (Point)value),
              //   new CameraParameter("AOI Offset", () => AOIOffset, (value) => AOIOffset = (Point)value),
              //  new CameraParameter("AOI Size", () => AOISize, (value) => AOISize = (Point)value),
              //  new CameraParameter("Pixel Clock", () => PixelClock, (value) => PixelClock = (int)value),
              //  new CameraParameter("Managed Image", () => ManagedImages, (value) => ManagedImages = (bool)value),
            
            };
        }


        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
          try{
                // Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.
                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;
                if (!grabResult.GrabSucceeded)
                    throw new Exception("grab unsuccessful ????");

                int channels = 0;
                ViDi2.ImageChannelDepth depth = ImageChannelDepth.Depth8;

               switch(grabResult.PixelTypeValue)
               {
                   case PixelType.Mono8 :
                       channels = 1;
                       break;
                   case PixelType.RGB8planar:
                       channels = 3;
                       break;
                   case PixelType.BGR8packed:
                        channels = 3;
                        break;
                   default:
                       throw new Exception(string.Format("pixel type not supported.", grabResult.PixelTypeValue.ToString()));
               }

                byte[] data = grabResult.PixelData as byte[];

                ByteImage img = new ByteImage(grabResult.Width, grabResult.Height,channels,depth,data,grabResult.Width+grabResult.PaddingX);

                ImageGrabbed(this, img);
            }
            catch (Exception exception)
            {
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        

        }


        public double FrameRate
        {
            get
            {
                if (camera.GetSfncVersion() < Sfnc2_0_0)
                {
                    return camera.Parameters[PLCamera.AcquisitionFrameRateAbs].GetValue();
                }
                else
                {
                    return camera.Parameters[PLUsbCamera.AcquisitionFrameRate].GetValue();
                }
            }
            set
            {
                if (camera.GetSfncVersion() < Sfnc2_0_0)
                {
                    camera.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(value);
                }
                else
                {
                    camera.Parameters[PLUsbCamera.AcquisitionFrameRate].SetValue(value);
                }
            }
        }

        public double ExposureTime
        {
            get
            {
                if (camera.GetSfncVersion() < Sfnc2_0_0)
                {
                    return camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();
                }
                else
                {
                    return camera.Parameters[PLUsbCamera.ExposureTime].GetValue();
                }
            }
            set
            {
                if (camera.GetSfncVersion() < Sfnc2_0_0)
                {
                    camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(value);
                }
                else
                {
                    camera.Parameters[PLUsbCamera.ExposureTime].SetValue(value);
                }
            }
        }



        public string Name
        {
            get { return info[CameraInfoKey.FullName]; }
        }

        public void Open()
        {
            camera.Open();
            RaisePropertyChanged("IsOpen");
        }

        public bool IsOpen
        {
            get { return camera.IsOpen ; }
        }

        public void Close()
        {
            camera.Close();
            RaisePropertyChanged("IsOpen");

        }

        public bool IsGrabbingContinuous
        {
            get { return camera.StreamGrabber.IsGrabbing; }
        }

        public void StartGrabContinuous()
        {
            camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            RaisePropertyChanged("IsGrabbingContinuous");
        }

        public void StopGrabContinuous()
        {
            camera.StreamGrabber.Stop();
            RaisePropertyChanged("IsGrabbingContinuous");
        }

        public void LoadParameters(string parametersFile)
        {
            throw new NotImplementedException();
        }

        public void SaveParameters(string parametersFile)
        {
            throw new NotImplementedException();
        }

        public void SaveParametersToDevice()
        {
            throw new NotImplementedException();
        }

        public ViDi2.IImage GrabSingle()
        {
            throw new NotImplementedException();
        }

        public ICameraCapabilities Capabilities
        {
            get { return capabilities; }
        }

         List<ICameraParameter> parameters;

        public IEnumerable<ICameraParameter> Parameters
        {
            get { return parameters; }
        }

        public ICameraProvider Provider
        {
            get { return provider; }
        }


        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
