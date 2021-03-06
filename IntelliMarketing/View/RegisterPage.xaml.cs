﻿using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IntelliMarketing.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        private List<Face> listFaceID;
        string uriPhoto;

        public RegisterPage()
        {
            this.InitializeComponent();

            if (MainPage.deviceFamily == "Mobile")
            {
                tip.Visibility = Visibility.Collapsed;
                UserPic.SetValue(Grid.RowProperty, 1);
                Rpainel.SetValue(Grid.ColumnProperty, 0);
                Rpainel.SetValue(Grid.RowProperty, 1);
                MainGrid.ColumnDefinitions[0].Width = new GridLength(1577 + 723);
                MainGrid.ColumnDefinitions[1].Width = new GridLength(0);
                MainGrid.RowDefinitions[0].Height = new GridLength(1106);
                MainGrid.RowDefinitions[1].Height = new GridLength(2990);
                username.Width = 1400;
            }

            listFaceID = new List<Face>();

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }

            };

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                //Also use hardware back button
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (r, e) =>
                {
                    if (Frame.CanGoBack)
                    {
                        Registering.IsActive = false;
                        e.Handled = true;
                        Frame.GoBack();
                    }
                };
            }
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                e.Handled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.Parameter is string && !string.IsNullOrWhiteSpace(e.Parameter as string))
            {
                BitmapImage bmpImage = new BitmapImage(new Uri(e.Parameter.ToString()));
                UserPic.Source = bmpImage;
                uriPhoto = e.Parameter.ToString();
            }
        }

        #region FaceAPI trainning
        private async Task listFaces(string path, string name)
        {
            //Caso queira adicionar mais fotos...
            //foreach (string imagePath in paths)
            //{
            using (Stream s = File.OpenRead(path))
            {
                var faces = await MainPage.faceServiceClient.DetectAsync(s);
                if (faces.Length == 0)
                {
                    Debug.WriteLine("No face detected in {0}.", path);
                    //continue;
                }
                listFaceID.Add(faces[0]);
            }
            //}

            try
            {
                var friend1FaceIds = listFaceID.Select(face => face.FaceId).ToArray();
                CreatePersonResult friend1 = await MainPage.faceServiceClient.CreatePersonAsync(MainPage.personGroupId, friend1FaceIds, name);
                await MainPage.faceServiceClient.TrainPersonGroupAsync(MainPage.personGroupId);
                TrainingStatus trainingStatus = null;
                while (true)
                {
                    trainingStatus = await MainPage.faceServiceClient.GetPersonGroupTrainingStatusAsync(MainPage.personGroupId);
                    if (trainingStatus.Status != "running")
                    {
                        Registering.IsActive = false;
                        username.IsEnabled = true;
                        saveButton.IsEnabled = true;
                        Debug.WriteLine("Trainned");
                        MessageDialog msg = new MessageDialog("Trainned");
                        await msg.ShowAsync();
                        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                        listFaceID.Clear();
                        break;
                    }
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion

        private async void send_Click(object sender, RoutedEventArgs e)
        {
            if (username.Text.Length > 0)
            {
                if (App.ConnectedToInternet())
                {
                    Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    Registering.IsActive = true;
                    UserPic.Visibility = Visibility.Collapsed;
                    saveButton.IsEnabled = false;
                    username.IsEnabled = false;
                    await listFaces(uriPhoto, username.Text);
                }
                else
                {
                    MessageDialog msg = new MessageDialog("Sem conexão com a internet. Por favor, verifique sua conexão.");
                    await msg.ShowAsync();
                }
            }
            else
            {
                MessageDialog msg = new MessageDialog("Por favor digite um nome para o usuário.");
                await msg.ShowAsync();
            }
        }
    }
}
