using System;
using System.Collections.Generic;
using System.IO;

using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

using Firebase.Storage;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;

using Feels.Objects;
using Xamarin.Essentials;

namespace Feels
{
    public partial class Profile : ContentPage
    {
        //firebase
        FirebaseClient firebase = new FirebaseClient("https://feels-d9ab9.firebaseio.com/");
        FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAgFUuyXFGbhGi2Lq2GA0ldOM_Ocd_1Bxs"));
        private Objects.User updatedUser = new Objects.User();

        public Profile(Objects.User user)
        {
            InitializeComponent();

            UpdateUserInfo(user);
        }

        //add user info to page
        void UpdateUserInfo(Objects.User user)
        {
            profileImage.Source = user.Image;

            nickNameField.Text = user.Name;

            updatedUser = user;

            locationSwitch.IsToggled = Preferences.Get("allowLocation", true);
        }

        //edit image clicked
        async void OnEditPictureClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(null, "Cancel", "Delete Image", "Take Photo", "Camera Roll");
            switch (action)
            {
                case "Take Photo":
                    TakePhoto();
                    break;
                case "Camera Roll":
                    SelectPhoto();
                    break;
                case "Delete Image":
                    SelectPhoto();
                    break;
            }
        }

        //nickname cahnged
        void Handle_Unfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            updatedUser.Name = nickNameField.Text;
            UpdateFirebase();
        }

        void Handle_Toggled(object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            Preferences.Set("allowLocation", e.Value);
        }

        //firebase update
        async void UpdateFirebase()
        {
            await firebase
              .Child("users")
              .Child(Preferences.Get("userID", "none"))
              .PutAsync(updatedUser);

            editBtn.Text = "Edit";
        }

        //go back
        async void HandleBackClicked(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }


        //log user out
        async void HandleLogoutClicked(object sender, System.EventArgs e)
        {
            Preferences.Clear();
            Preferences.Set("loggedIn", false);
            await Navigation.PopModalAsync();
        }

        //take photo with camera
        async void TakePhoto()
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Feels",
                SaveToAlbum = true,
                CompressionQuality = 100,
                MaxWidthHeight = 100,
                PhotoSize = PhotoSize.MaxWidthHeight,
                DefaultCamera = CameraDevice.Front
            });

            if (file == null)
                return;

            var stream = File.Open(file.Path, FileMode.Open);

            var task = new FirebaseStorage("feels-d9ab9.appspot.com")
            .Child("profileImages")
            .Child(Preferences.Get("userID", "none"))
            .PutAsync(stream);

            // Track progress of the upload
            task.Progress.ProgressChanged += (s, e) => editBtn.Text = "Uploading...";

            // await the task to wait until upload completes and get the download url
            var downloadUrl = await task;

            updatedUser.Image = downloadUrl;

            profileImage.Source = updatedUser.Image;

            UpdateFirebase();
        }

        //select photo from camera roll
        async void SelectPhoto()
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Photos Not Supported", "Permission not granted to photos.", "OK");
                return;
            }
            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                CompressionQuality = 100,
                MaxWidthHeight = 500,
                PhotoSize = PhotoSize.MaxWidthHeight,

            });

            if (file == null)
                return;

            var stream = File.Open(file.Path, FileMode.Open);

            var task = new FirebaseStorage("feels-d9ab9.appspot.com")
            .Child("profileImages")
            .Child(Preferences.Get("userID", "none"))
            .PutAsync(stream);

            // Track progress of the upload
            task.Progress.ProgressChanged += (s, e) => editBtn.Text = "Uploading...";

            // await the task to wait until upload completes and get the download url
            var downloadUrl = await task;

            updatedUser.Image = downloadUrl;

            profileImage.Source = updatedUser.Image;

            UpdateFirebase();
        }

        //remove image
        async void DeleteImage()
        {
            bool answer = await DisplayAlert("Are you sure?", "Are you sure you want to remove this image permanently?", "Yes", "No");
            if (answer)
            {
                profileImage.Source = "profileimagelight";
                UpdateFirebase();
            }
        }

        //async void takePhoto(object sender, System.EventArgs e)
        //{
        //        await CrossMedia.Current.Initialize();

        //        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
        //        {
        //            DisplayAlert("No Camera", ":( No camera available.", "OK");
        //            return;
        //        }

        //        var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
        //        {
        //            Directory = "Sample",
        //            Name = "test.jpg"
        //        });

        //        if (file == null)
        //            return;

        //        await DisplayAlert("File Location", file.Path, "OK");

        //        //image.Source = ImageSource.FromStream(() =>
        //        //{
        //        //    var stream = file.GetStream();
        //        //    return stream;
        //        //});
        //}
    }
}
