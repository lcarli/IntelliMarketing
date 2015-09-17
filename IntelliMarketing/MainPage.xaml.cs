using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Display;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.Media.FaceAnalysis;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.System.Display;
using Windows.Media;
using Windows.Graphics.Imaging;
using Windows.Media.SpeechSynthesis;
using Windows.Media.SpeechRecognition;
using Windows.Networking.PushNotifications;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Graphics;
using Windows.Storage.FileProperties;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IntelliMarketing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Inicializate faceServiceCliente. Input faceAPI Key!!!
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("cefde33b85354ecd9167d261c186dc19");

        //Inicializate Camera
        MediaCapture mc;
        private bool _isInitialized;
        private bool _isPreviewing;
        private bool _externalCamera;
        private bool _mirroringPreview;
        private IMediaEncodingProperties _previewProperties;

        // Receive notifications about rotation of the device and UI and apply any necessary rotation to the preview stream and UI controls
        private readonly DisplayInformation _displayInformation = DisplayInformation.GetForCurrentView();
        private readonly SimpleOrientationSensor _orientationSensor = SimpleOrientationSensor.GetDefault();
        //private SimpleOrientation _deviceOrientation = SimpleOrientation.NotRotated;
        private DisplayOrientations _displayOrientation = DisplayOrientations.Portrait;
        private SimpleOrientation _deviceOrientation = SimpleOrientation.NotRotated;
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        // Prevent the screen from sleeping while the camera is running
        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        // For listening to media property changes
        private readonly SystemMediaTransportControls _systemMediaControls = SystemMediaTransportControls.GetForCurrentView();

        //Directory
        const string faceImageDir = @"C:\Users\Lucas\Pictures\Faces";

        //GroupID
        string personGroupId = "mscm";

        //Timer
        DispatcherTimer timer;

        //Contador
        int contagem;

        //DeviceFamily
        string deviceFamily;

        //Person name
        string pname;
        Face[] faces;

        //Age and Gender variable
        string age;
        string gender;

        //SpeechRecognizer Object
        SpeechRecognizer voiceRecognizer;

        //List faces (It's necessary 'cause directory doesn't works.
        private List<Face> listFaceID;


        //Create Enum of Errors
        enum Error
        {
            Not_Found,
            Not_Recognized,
            No_Face,
            Expensive
        };




        public MainPage()
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            this.InitializeComponent();
            listFaceID = new List<Face>();
            //createListFaceID();
            inicializar();
            ajustes();
        }

        public MainPage(string command)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            this.InitializeComponent();
            listFaceID = new List<Face>();
            //createListFaceID();
            inicializar();
            ajustes();
            if (command != null)
            {
                cortanaAction(command);
            }
        }


        #region Cortana Method
        private void cortanaAction(string command)
        {
            if (command == "takePhoto")
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 3, 0);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            captureElement();
        }

        #endregion

        #region Init Methods

        private async void ajustes()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }

            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
            deviceFamily = qualifiers["DeviceFamily"];

            double Width = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds.Width;
            double Height = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds.Height;

            _displayOrientation = _displayInformation.CurrentOrientation;
            if (_orientationSensor != null)
            {
                _deviceOrientation = _orientationSensor.GetCurrentOrientation();
            }

            if (deviceFamily == "Mobile")
            {
                //Content.Visibility = Visibility.Collapsed;

                MainGrid.ColumnDefinitions[0].Width = new GridLength(1577 + 723);
                MainGrid.ColumnDefinitions[1].Width = new GridLength(0);
                MainGrid.RowDefinitions[0].Height = new GridLength(1106);
                MainGrid.RowDefinitions[1].Height = new GridLength(2990);

                MainGrid.ColumnDefinitions[1].Width = new GridLength(0);

                Page.SetValue(Grid.ColumnProperty, 0);
                Page.SetValue(Grid.RowProperty, 1);
                Page.Margin = new Thickness(Width * 0.026);

                myImage.Width = 2100;
                myImage.Height = 1400;

                //Rotate();

                //HoldCamera.Margin = new Thickness(Width * 0.06);
                //LeftPanel.Margin = new Thickness(Width * 0.06);
            }
            else
            {
                //MainGrid.ColumnDefinitions[0].Width = new GridLength(Width * 0.27);
                //MainGrid.ColumnDefinitions[1].Width = new GridLength(Width * 0.72);
                //MainGrid.RowDefinitions[0].Height = new GridLength(Height * 0.27);
                //MainGrid.RowDefinitions[1].Height = new GridLength(Height * 0.72);

                //GridLength minWidth = new GridLength(300);
                //GridLength mg = MainGrid.ColumnDefinitions[0].Width;

                //if (mg.Value < minWidth.Value)
                //{
                //    MainGrid.ColumnDefinitions[0].Width = minWidth;
                //    HoldCamera.Margin = new Thickness(300 * 0.026);
                //    LeftPanel.Margin = new Thickness(300 * 0.026);
                //}

                //HoldCamera.Margin = new Thickness(Width * 0.026);
                //LeftPanel.Margin = new Thickness(Width * 0.026);
                //ProductImage.Margin = new Thickness(Width * 0.026);
                //Content.Margin = new Thickness(Width * 0.026);
                //ProductName.Margin = new Thickness(Width * 0.015625, Height * 0.027777, Width * 0.15625, Height * 0.925925);
                //Price.Margin = new Thickness(Width * 0.3125, Height * 0.027777, Width * 0.015625, Height * 0.185185);
                //logoStore.Margin = new Thickness(Width * 0.3125, Height * 0.185185, Width * 0.015625, Height * 0.027777);
            }

            RegisterEventHandlers();
        }

        private async void inicializar()
        {
            await InitializeCameraAsync();
        }

        #endregion

        #region Orientation Methods


        private void OrientationSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            if (args.Orientation != SimpleOrientation.Faceup && args.Orientation != SimpleOrientation.Facedown)
            {
                // Only update the current orientation if the device is not parallel to the ground. This allows users to take pictures of documents (FaceUp)
                // or the ceiling (FaceDown) in portrait or landscape, by first holding the device in the desired orientation, and then pointing the camera
                // either up or down, at the desired subject.
                //Note: This assumes that the camera is either facing the same way as the screen, or the opposite way. For devices with cameras mounted
                //      on other panels, this logic should be adjusted.
                _deviceOrientation = args.Orientation;
            }
        }

        private SimpleOrientation GetCameraOrientation()
        {
            if (_externalCamera)
            {
                // Cameras that are not attached to the device do not rotate along with it, so apply no rotation
                return SimpleOrientation.NotRotated;
            }

            var result = _deviceOrientation;

            // Account for the fact that, on portrait-first devices, the camera sensor is mounted at a 90 degree offset to the native orientation
            if (_displayInformation.NativeOrientation == DisplayOrientations.Portrait)
            {
                switch (result)
                {
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        result = SimpleOrientation.NotRotated;
                        break;
                    case SimpleOrientation.Rotated180DegreesCounterclockwise:
                        result = SimpleOrientation.Rotated90DegreesCounterclockwise;
                        break;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        result = SimpleOrientation.Rotated180DegreesCounterclockwise;
                        break;
                    case SimpleOrientation.NotRotated:
                        result = SimpleOrientation.Rotated270DegreesCounterclockwise;
                        break;
                }
            }

            // If the preview is being mirrored for a front-facing camera, then the rotation should be inverted
            if (_mirroringPreview)
            {
                // This only affects the 90 and 270 degree cases, because rotating 0 and 180 degrees is the same clockwise and counter-clockwise
                switch (result)
                {
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        return SimpleOrientation.Rotated270DegreesCounterclockwise;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        return SimpleOrientation.Rotated90DegreesCounterclockwise;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the given orientation of the device in space to the corresponding rotation in degrees
        /// </summary>
        /// <param name="orientation">The orientation of the device in space</param>
        /// <returns>An orientation in degrees</returns>
        private static int ConvertDeviceOrientationToDegrees(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return 90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return 180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return 270;
                case SimpleOrientation.NotRotated:
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts the given orientation of the app on the screen to the corresponding rotation in degrees
        /// </summary>
        /// <param name="orientation">The orientation of the app on the screen</param>
        /// <returns>An orientation in degrees</returns>
        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts the given orientation of the device in space to the metadata that can be added to captured photos
        /// </summary>
        /// <param name="orientation">The orientation of the device in space</param>
        /// <returns></returns>
        private static PhotoOrientation ConvertOrientationToPhotoOrientation(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return PhotoOrientation.Rotate90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return PhotoOrientation.Rotate180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return PhotoOrientation.Rotate270;
                case SimpleOrientation.NotRotated:
                default:
                    return PhotoOrientation.Normal;
            }
        }

        #endregion

        #region Trainning Faces
        private async void createListFaceID()
        {
            //Chama listFaces passando uma lista de paths de fotos e o nome da pessoa. Pode ser substituido por uma URL na web ou um local no banco de dados

            //Delete Group
            await faceServiceClient.DeletePersonGroupAsync(personGroupId);
            Debug.WriteLine("DELETADO O GRUPO");
            //Create Group
            await faceServiceClient.CreatePersonGroupAsync(personGroupId, "employees");
            Debug.WriteLine("CRIADO O GRUPO");

            //Daibert
            var daibertv = await KnownFolders.PicturesLibrary.GetFileAsync("od1.jpg");
            var daibert = daibertv.Path;
            var daibertv1 = await KnownFolders.PicturesLibrary.GetFileAsync("od2.jpg");
            var daibert1 = daibertv1.Path;
            var daibertv2 = await KnownFolders.PicturesLibrary.GetFileAsync("od3.jpg");
            var daibert2 = daibertv2.Path;
            //Hara
            var harav = await KnownFolders.PicturesLibrary.GetFileAsync("f1.jpg");
            var hara = harav.Path;
            var harav1 = await KnownFolders.PicturesLibrary.GetFileAsync("f2.jpg");
            var hara1 = harav1.Path;

            List<string> pathsDaibert = new List<string>() { daibert, daibert1, daibert2 };
            List<string> pathsHara = new List<string>() { hara, hara1 };

            contagem = 0;
            await Task.Run(async () => { await listFaces(pathsDaibert, "Daibert"); });
            listFaceID.Clear();
            await Task.Run(async () => { await listFaces(pathsHara, "Hara"); });
            listFaceID.Clear();
        }

        private async Task listFaces(List<string> paths, string name)
        {
            //foreach (string imagePath in Directory.GetFiles(faceImageDir, "*.*"))
            foreach (string imagePath in paths)
            {
                Debug.WriteLine(">>>>>PATH>>>>> " + name);
                using (Stream s = File.OpenRead(imagePath))
                {
                    var faces = await faceServiceClient.DetectAsync(s);
                    if (faces.Length == 0)
                    {
                        Debug.WriteLine("No face detected in {0}.", imagePath);
                        continue;
                    }
                    listFaceID.Add(faces[0]);
                }
            }

            try
            {
                var friend1FaceIds = listFaceID.Select(face => face.FaceId).ToArray();
                CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(personGroupId, friend1FaceIds, name);

                //Trainning Group. Como melhorar isso?!?!??!
                contagem += 1;
                if (contagem == 2)
                {
                    await faceServiceClient.TrainPersonGroupAsync(personGroupId);
                    TrainingStatus trainingStatus = null;
                    while (true)
                    {
                        trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);
                        if (trainingStatus.Status != "running")
                        {
                            Debug.WriteLine("Trainned");
                            break;
                        }
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Extra Methods
        private async void AnalyzeReaction()
        {
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("ReactionPhoto.png", CreationCollisionOption.GenerateUniqueName);
            // take photo
            await mc.CapturePhotoToStorageFileAsync(imgFormat, file);
            //Analyse person
            try
            {
                Person p = await identifyFace(file.Path);
                if (p.Name == pname)
                {

                }
            }
            catch (Exception)
            {
                //msgBox.Text = "Face not detected";
                ReadVoice(Error.No_Face);
            }
        }
        #endregion

        #region PageNavigation Methods
        private void Page_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Page.Visibility = Visibility.Visible;
        }
        #endregion

        #region Camera Methods

        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (mc == null)
            {
                // Attempt to get the front camera if one is available, but use any camera device if not
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Front);

                if (cameraDevice == null)
                {
                    Debug.WriteLine("No camera device found!");
                    return;
                }

                // Create MediaCapture and its settings
                mc = new MediaCapture();

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                // Initialize MediaCapture
                try
                {
                    await mc.InitializeAsync(settings);
                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception when initializing MediaCapture with {0}: {1}", cameraDevice.Id, ex.ToString());
                }

                // If initialization succeeded, start the preview
                if (_isInitialized)
                {
                    // Figure out where the camera is located
                    if (cameraDevice.EnclosureLocation == null || cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
                    {
                        // No information on the location of the camera, assume it's an external camera, not integrated on the device
                        _externalCamera = true;
                    }
                    else
                    {
                        // Camera is fixed on the device
                        _externalCamera = false;

                        // Only mirror the preview if the camera is on the front panel
                        _mirroringPreview = (cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
                    }

                    await StartPreviewAsync();

                }
            }
        }

        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);
            //DeviceInformation desiredDevice = allVideoDevices[0];

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
            //return desiredDevice;
        }

        private async Task StartPreviewAsync()
        {
            // Prevent the device from sleeping while the preview is running
            _displayRequest.RequestActive();

            // Set the preview source in the UI and mirror it if necessary
            HoldCamera.Source = mc;
            HoldCamera.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Start the preview
            try
            {
                await mc.StartPreviewAsync();
                _isPreviewing = true;
                _previewProperties = mc.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when starting the preview: {0}", ex.ToString());
            }

            // Initialize the preview to the current orientation
            if (_previewProperties != null)
            {
                _displayOrientation = _displayInformation.CurrentOrientation;

                await SetPreviewRotationAsync();
            }
        }

        private async void previsualizar()
        {
            if (mc != null)
            {
                HoldCamera.Source = mc;
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                await mc.StartPreviewAsync();
            }
        }

        private async Task SetPreviewRotationAsync()
        {
            // Only need to update the orientation if the camera is mounted on the device
            if (_externalCamera) return;

            // Calculate which way and how far to rotate the preview
            int rotationDegrees = ConvertDisplayOrientationToDegrees(_displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored
            if (_mirroringPreview)
            {
                rotationDegrees = (360 - rotationDegrees) % 360;
            }

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = mc.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotationDegrees);
            await mc.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }

        private void RegisterEventHandlers()
        {
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            }

            // If there is an orientation sensor present on the device, register for notifications
            if (_orientationSensor != null)
            {
                _orientationSensor.OrientationChanged += OrientationSensor_OrientationChanged;

            }

            _displayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            _systemMediaControls.PropertyChanged += SystemMediaControls_PropertyChanged;
        }

        private async void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            _displayOrientation = sender.CurrentOrientation;

            if (_isPreviewing)
            {
                await SetPreviewRotationAsync();
            }
        }

        private async void SystemMediaControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Only handle this event if this page is currently being displayed
                if (args.Property == SystemMediaTransportControlsProperty.SoundLevel && Frame.CurrentSourcePageType == typeof(MainPage))
                {
                    // Check to see if the app is being muted. If so, it is being minimized.
                    // Otherwise if it is not initialized, it is being brought into focus.
                    if (sender.SoundLevel == SoundLevel.Muted)
                    {
                        //await CleanupCameraAsync();
                    }
                    else if (!_isInitialized)
                    {
                        await InitializeCameraAsync();
                    }
                }
            });
        }

        #endregion

        #region FaceAPI - Methods
        private async Task<Person> identifyFace(string path)
        {
            using (Stream s = File.OpenRead(path))
            {
                faces = null;
                Person pessoa = null;
                faces = await faceServiceClient.DetectAsync(s, true, true, true, true);
                age = faces[0].Attributes.Age.ToString();
                gender = faces[0].Attributes.Gender.ToString();
                age_genre.Text = gender + " - " + age + " years";
                var faceIds = faces.Select(face => face.FaceId).ToArray();
                var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);

                foreach (var identifyResult in results)
                {
                    Debug.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Length == 0)
                    {
                        Debug.WriteLine("No one identified");
                        pessoa = null;
                    }
                    else
                    {
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                        pessoa = person;
                    }
                }
                return pessoa;
            }
        }

        private async void saveIDlocal(Dictionary<string, string> dic)
        {
            foreach (var item in dic)
            {
                using (Stream imageFileStream = File.OpenRead(item.Value))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    foreach (var face in faces)
                    {

                    }
                }
            }
        }

        private async Task<FaceRectangle[]> UploadAndDetectFaces(string imageFilePath)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, false, true, true, true);
                    foreach (var face in faces)
                    {
                        var rect = face.FaceRectangle;
                        var landmarks = face.FaceLandmarks;
                        var attributes = face.Attributes;
                    }
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    return faceRects.ToArray();
                }
            }
            catch (Exception e)
            {
                return new FaceRectangle[0];
            }
        }

        private async void Recognize(string path)
        {
            FaceRectangle[] faceRects = await UploadAndDetectFaces(path);
            if (faceRects != null && faceRects.Count() > 0)
            {
                rectFace.Stroke = new SolidColorBrush(Colors.AliceBlue);
                rectFace.Width = faceRects[0].Width / 2;
                rectFace.Height = faceRects[0].Height / 2;
                Canvas.SetLeft(rectFace, faceRects[0].Left / 2.2);
                Canvas.SetTop(rectFace, faceRects[0].Top / 2.5);

                //Drawing polygon
                polAge.Points.Add(new Point(faceRects[0].Left / 2.2 + (faceRects[0].Width / 2) / 5, (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 30));
                polAge.Points.Add(new Point(faceRects[0].Left / 2.2 + (((faceRects[0].Width / 2) / 5) * 4), (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 30));
                polAge.Points.Add(new Point(faceRects[0].Left / 2.2 + (((faceRects[0].Width / 2) / 5) * 4), (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 0));
                polAge.Points.Add(new Point(faceRects[0].Left / 2.2 + (faceRects[0].Width / 2) / 2 + 5, (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 0));
                polAge.Points.Add(new Point(faceRects[0].Left / 2.2 + (faceRects[0].Width / 2) / 2, (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) + 7));
                polAge.Points.Add(new Point(faceRects[0].Left / 2.2 + (faceRects[0].Width / 2) / 2 - 5, (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 0));
                polAge.Points.Add(new Point(faceRects[0].Left / 2.2 + (faceRects[0].Width / 2) / 5, (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 0));


                textAge.Margin = new Thickness(faceRects[0].Left / 2.2 + ((faceRects[0].Width / 2) / 5 * 2), (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 25, 0, 0);
                textAge.Text = age;

                try
                {
                    Person p = await identifyFace(path);
                    textAge.Margin = new Thickness(faceRects[0].Left / 2.2 + ((faceRects[0].Width / 2) / 5 * 2), (faceRects[0].Top / 2.5 - (faceRects[0].Width / 2) / 5) - 25, 0, 0);
                    textAge.Text = age;
                    age_genre.Visibility = Visibility.Visible;
                    polAge.Stroke = new SolidColorBrush(Colors.Black);
                    if (gender == "male")
                    {
                        rectFace.Stroke = new SolidColorBrush(Colors.LightBlue);
                        polAge.Fill = new SolidColorBrush(Colors.LightBlue);
                    }
                    else if (gender == "female")
                    {
                        rectFace.Stroke = new SolidColorBrush(Colors.LightPink);
                        polAge.Fill = new SolidColorBrush(Colors.LightPink);
                    }
                    if (p != null)
                    {
                        pname = p.Name;
                        setProduct(p.Name);
                        ReadVoiceName(p.Name);
                    }
                    else
                    {
                        //msgBox.Text = "Face not recognize.";
                        ReadVoice(Error.Not_Recognized);
                    }
                }
                catch (Exception e)
                {
                    //msgBox.Text = "Face not detected";
                    ReadVoice(Error.No_Face);
                }
            }
            else
            {
                //msgBox.Text = "Face not detected. Please take another photo.";
                //myImage.Source = null;
                ReadVoice(Error.No_Face);
            }
        }

        #endregion

        #region Voice Synth
        private async void ReadVoice(Error name)
        {
            // The media object for controlling and playing audio.
            MediaElement mediaElement = new MediaElement();

            // The object for controlling the speech synthesis engine (voice).
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();

            // Generate the audio stream from plain text.
            SpeechSynthesisStream stream;
            switch (name)
            {
                case Error.Not_Recognized:
                    stream = await synth.SynthesizeTextToStreamAsync("Oops! Someone was do not recognized. Please, show me someone that I met before!");
                    break;
                case Error.No_Face:
                    stream = await synth.SynthesizeTextToStreamAsync("I can't find a face. Do you really show me someone? Please, try again.");
                    break;
                case Error.Not_Found:
                    stream = await synth.SynthesizeTextToStreamAsync("I can't find another product for you.");
                    break;
                case Error.Expensive:
                    stream = await synth.SynthesizeTextToStreamAsync("You need to order Danilo to raise your paycheck. Let me check another product for you, for now.");
                    break;
                default:
                    stream = await synth.SynthesizeTextToStreamAsync("Hello " + name + "! Let me check some products for you.");
                    break;
            }
            // Send the stream to the media object.
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        private async void ReadVoiceName(string name)
        {
            // The media object for controlling and playing audio.
            MediaElement mediaElement = new MediaElement();

            // The object for controlling the speech synthesis engine (voice).
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();

            // Generate the audio stream from plain text.
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync("Hello " + name + "! Let me check some products for you.");

            // Send the stream to the media object.
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        private async void VoiceRecognizer()
        {
            voiceRecognizer = new SpeechRecognizer();
            SpeechRecognitionTopicConstraint topicContraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "development");
            voiceRecognizer.Constraints.Add(topicContraint);
            SpeechRecognitionCompilationResult result = await voiceRecognizer.CompileConstraintsAsync();
            SpeechRecognitionResult speechRecognitionResult = await voiceRecognizer.RecognizeAsync();
            //voiceRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            //voiceRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            //await voiceRecognizer.ContinuousRecognitionSession.StartAsync();
            if (pname == "Lorenzo")
            {
                if (speechRecognitionResult.Text.Contains("expensive") || speechRecognitionResult.Text.Contains("expense"))
                {
                    //speechText.Text = "So much expensive";
                    ReadVoice(Error.Not_Found);
                    //pageView.Navigate(new Uri("http://www.americanas.com.br/produto/113151382/carro-eletrico-sport-car-vermelho-6v"));
                }
                else
                {
                    ReadVoice(Error.Not_Found);
                }
            }
            else
            {
                ReadVoice(Error.Not_Found);
            }
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            //InputText(args.Result.Text);
        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            //var recognition = voiceRecognizer.RecognizeAsync();
            //recognition.Completed += this.OnRecoginitionCompletedHandler();
        }

        private void Voice_Click(object sender, RoutedEventArgs e)
        {
            VoiceRecognizer();
        }

        private void InputText(string texto)
        {
            //speechText.Text = texto;
        }

        #endregion

        #region Verify Connection
        private bool ConnectedToInternet()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null)
            {
                return false;
            }

            var level = InternetConnectionProfile.GetNetworkConnectivityLevel();
            return level == NetworkConnectivityLevel.InternetAccess;
        }
        #endregion

        #region General Methods
        public async void captureElement()
        {
            if (ConnectedToInternet())
            {
                var stream = new InMemoryRandomAccessStream();
                await mc.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                var photoOrientation = ConvertOrientationToPhotoOrientation(GetCameraOrientation());
                string uriPhoto = await ReencodeAndSavePhotoAsync(stream, photoOrientation);

                BitmapImage bmpImage = new BitmapImage(new Uri(uriPhoto));

                // imagePreivew is a <Image> object defined in XAML
                myImage.Source = bmpImage;

                //Recognize Someone
                Recognize(uriPhoto);
            }
            else
            {
                MessageDialog msg = new MessageDialog("No internet is avaiable. Please, check your connection.");
                await msg.ShowAsync();
            }
        }

        private void setProduct(string nome)
        {
            //BitmapImage product = new BitmapImage();
            //BitmapImage logo = new BitmapImage();
            Page.NavigationCompleted += Page_NavigationCompleted;
            switch (nome)
            {
                case "Daibert":
                    Page.Navigate(new Uri("http://www.amazon.com/Sphero-R001USA-BB-8-App-Enabled-Droid/dp/B0107H5FJ6/ref=sr_1_1?ie=UTF8&qid=1442444059&sr=8-1&keywords=sphero+bb-8"));
                    //ProductImage.Source = "Images/bb8.jpg"
                    //product.UriSource = new Uri("bb8.jpg", UriKind.Relative);
                    //ProductImage.Source = product;
                    //ProductImage.Source = new BitmapImage(new Uri("/IntelliMarketing;component/Images/bb8.png", UriKind.Relative));
                    //ProductName.Text = "Sphero BB-8 App-Enabled Droid";
                    //Price.Text = "US$ 149.99";
                    //logo.UriSource = new Uri("amazon-prime.jpg", UriKind.Relative);
                    //logoStore.Source = logo;
                    break;
                case "Hara":
                    Page.Navigate(new Uri("http://www.amazon.com/Nikon-Digital-1080p-Video-MODEL/dp/B006U49XM6/ref=sr_1_2?ie=UTF8&qid=1442487669&sr=8-2&keywords=nikon+d4"));
                    //product.UriSource = new Uri("Halo5.jpg", UriKind.Relative);
                    //ProductImage.Source = product;
                    //ProductName.Text = "Halo 5 - Xbox One";
                    //Price.Text = "US$ 49.99";
                    //logo.UriSource = new Uri("amazon-prime.jpg", UriKind.Relative);
                    //logoStore.Source = logo;
                    break;
                default:
                    break;
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Verify if product was showed and reset setup.
            if (Page.Visibility == Visibility.Visible)
            {
                age_genre.Text = "";
                age_genre.Visibility = Visibility.Collapsed;
                pname = "";
                age = "";
                gender = "";
                polAge.Points.Clear();
                myImage.Source = null;
                Page.Visibility = Visibility.Collapsed;
            }
            else
            {
                myImage.Source = null;
                age_genre.Text = "";
                age_genre.Visibility = Visibility.Collapsed;
                pname = "";
                age = "";
                gender = "";
                polAge.Points.Clear();
                captureElement();
            }
        }

        private void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
           captureElement();
        }

        #endregion

        #region Encode Image
        private static async Task<string> ReencodeAndSavePhotoAsync(IRandomAccessStream stream, PhotoOrientation photoOrientation)
        {
            //var file = await KnownFolders.PicturesLibrary.CreateFileAsync("FacePhoto.jpeg", CreationCollisionOption.ReplaceExisting);
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Face.jpeg", CreationCollisionOption.GenerateUniqueName);
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
            }

            return file.Path;
        }
        #endregion
    }
}
