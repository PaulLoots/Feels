//using System;
//using System.Collections.Generic;
//using System.Timers;

using Xamarin.Forms;
//using Xamarin.Essentials;
//using AiForms.Effects;

//using Firebase.Database;
//using Firebase.Database.Query;

namespace Feels
{
    public partial class AddPerson : ContentPage
    {
        //    //firebase
        //    FirebaseClient firebase = new FirebaseClient("https://feels-d9ab9.firebaseio.com/");

        //    //addmoji varibles
        //    private View toneFace;

        //    //tone
        //    string passmojiTone = "none";

        //    //finger value
        //    double initialX;
        //    double initialY;
        //    double initialToneX;
        //    double initialToneY;

        //    //locationData
        //    Location currentLocation;
        //    Objects.Location locationObject;
        //    string locationObjectKey = "none";

        //    int nearbyCount = 0;

        //    //Observable vars
        //    private static System.Timers.Timer observableTimer;
        //    private bool observed = false;

        //    public AddPerson()
        //    {
        //        InitializeComponent();

        //        ActivateAddMoji();

        //        GetLocationAsync();
        //        SetObservable();

        //        //hide radarrings
        //        radarrings.Opacity = 0;
        //        radarrings.ScaleTo(0.1, 0);
        //    }


        //    //set Observable
        //    private void SetObservable()
        //    {
        //        SetTimer();

        //        var observable = firebase.Child("searching")
        //          .AsObservable<Objects.Location>()
        //          .Subscribe(d =>
        //          {
        //              observed = true;
        //          });
        //    }

        //    //store selected location in db
        //    private async void SetDBLocationAsync()
        //    {
        //        if (locationObjectKey == "none")
        //        {
        //            locationObjectKey = "notNone";

        //            var location = await firebase.Child("searching").PostAsync(new Objects.Location { UserID = Preferences.Get("userID", "none"), Tone = "none", Latitude = currentLocation.Latitude, Longitude = currentLocation.Longitude });

        //            locationObject = location.Object;
        //            locationObjectKey = location.Key;
        //        }
        //    }

        //    //store selected tone in db
        //    private async void UpdateDBToneAsync()
        //    {
        //        locationObject.Tone = passmojiTone;
        //        await firebase.Child("searching").Child(locationObjectKey).PutAsync(locationObject);
        //    }

        //    //store selected tone in db
        //    private async void DeleteDBSearchingAsync()
        //    {
        //        await firebase.Child("searching").Child(locationObjectKey).DeleteAsync();
        //    }

        //    private void GetSearchingPeople()
        //    {
        //        Device.BeginInvokeOnMainThread(async () =>
        //        {
        //            var searchings = await firebase.Child("searching").OnceAsync<Objects.Location>();
        //            var conversations = await firebase.Child("conversations").OnceAsync<Objects.Conversation>();

        //            nearbyCount = 0;
        //            nearbyTxt.FontAttributes = FontAttributes.None;
        //            nearbyTxt.Text = "Looking for neaby people...";
        //            nearbyTxt.TextColor = Color.FromHex("#828282");

        //            foreach (var searching in searchings)
        //            {
        //                bool isNew = true;
        //                foreach (var conversation in conversations)
        //                {
        //                    if (conversation.Object.Person1 == Preferences.Get("userID", "none") || conversation.Object.Person2 == Preferences.Get("userID", "none"))
        //                    {
        //                        if (conversation.Object.Person1 == searching.Object.UserID || conversation.Object.Person2 == searching.Object.UserID)
        //                        {
        //                            isNew = false;
        //                        }
        //                    }
        //                }
        //                if (isNew)
        //                {
        //                    Location searchingLocation = new Location(searching.Object.Latitude, searching.Object.Longitude);
        //                    double distanceKM = Location.CalculateDistance(searchingLocation, currentLocation, DistanceUnits.Kilometers);
        //                    Console.WriteLine(distanceKM);

        //                    if (distanceKM < 1 && searching.Object.UserID != Preferences.Get("userID", "none"))
        //                    {
        //                        PulseRadar();
        //                        nearbyCount++;
        //                        if (passmojiTone != "none")
        //                        {
        //                            if (searching.Object.Tone == passmojiTone)
        //                            {
        //                                Console.WriteLine("It's a match!" + passmojiTone);
        //                                var matchpage = new Match(searching.Object.UserID);
        //                                await Navigation.PushModalAsync(matchpage);
        //                                DeleteDBSearchingAsync();

        //                                matchpage.Disappearing += (sender2, e2) =>
        //                                {
        //                                    System.Diagnostics.Debug.WriteLine("The modal page is dismissed, do something now");
        //                                    if (Preferences.Get("newUser", false))
        //                                    {
        //                                        Navigation.PopModalAsync();
        //                                    }
        //                                };
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            if (nearbyCount > 1)
        //            {
        //                nearbyTxt.Text = nearbyCount.ToString() + " people detected";
        //                nearbyTxt.FontAttributes = FontAttributes.Bold;
        //                nearbyTxt.TextColor = Color.FromHex("#FFFFFF");
        //            }
        //            else if (nearbyCount > 0)
        //            {
        //                nearbyTxt.Text = nearbyCount.ToString() + " person detected";
        //                nearbyTxt.FontAttributes = FontAttributes.Bold;
        //                nearbyTxt.TextColor = Color.FromHex("#FFFFFF");
        //            }
        //        });
        //    }

        //    //make addmoji
        //    private void ActivateAddMoji()
        //    {
        //        //tone face
        //        var toneface = new Image { Source = "wavinghand" };
        //        toneFace = toneface;

        //        //touch recogniser
        //        var recognizer = AddTouch.GetRecognizer(addmojiArea);

        //        recognizer.TouchBegin += async (sender, e) => {

        //            //set initial touch position
        //            initialX = e.Location.X;
        //            initialY = e.Location.Y;

        //            initialToneX = addmojiIcon.X;
        //            initialToneY = addmojiIcon.Y;

        //            ////create emoji
        //            addmojiArea.Children.Add(toneFace);

        //            //scale radarrings
        //            HideRadar();
        //            toneFace.ScaleTo(0.6, 0).Forget();

        //            ////show picker grid and emoji
        //            addmojiIcon.Source = "addmojigrid";
        //            addmojiIcon.TranslateTo(initialToneX, initialToneY + 50, 0).Forget();
        //            addmojiIcon.ScaleTo(1.2, 400, Easing.SpringOut).Forget();

        //            toneFace.TranslateTo(initialX - 170, initialY - 250, 0).Forget();
        //            await toneFace.ScaleTo(1.2, 400, Easing.SpringOut);
        //        };

        //        recognizer.TouchMove += (sender, e) => {
        //            toneFace.TranslateTo(e.Location.X - 190, e.Location.Y - 350, 30);
        //            var currentTone = GetCurrentTone(e.Location.Y);
        //            toneface.Source = currentTone;
        //        };

        //        recognizer.TouchEnd += async (sender, e) => {

        //            ShowRadar();
        //            addmojiArea.Children.Remove(toneFace);

        //            if (passmojiTone != "none")
        //            {
        //                addmojiIcon.Source = passmojiTone;
        //                addmojiIcon.ScaleTo(0.7, 200, Easing.SpringOut).Forget();
        //                addmojiIcon.TranslateTo(initialToneX, initialToneY - 10, 0).Forget();
        //            }
        //            else
        //            {
        //                addmojiIcon.Source = "addmojipicker";
        //                addmojiIcon.ScaleTo(1, 200, Easing.SpringOut).Forget();
        //                await addmojiIcon.TranslateTo(initialToneX, initialToneY, 0);
        //            }

        //            UpdateDBToneAsync();
        //        };
        //    }

        //    //get selected tone
        //    private string GetCurrentTone(double y)
        //    {
        //        int ySpacer = 40;
        //        double yDifference = (initialY + 120) - y;
        //        string tone = "";

        //        switch (yDifference)
        //        {
        //            case double n when n <= (ySpacer * 3) && n > (ySpacer * 2):
        //                tone = "faceblowingakiss";
        //                passmojiTone = "faceblowingakiss";
        //                break;
        //            case double n when n <= (ySpacer * 2) && n > (ySpacer):
        //                tone = "partyingface";
        //                passmojiTone = "partyingface";
        //                break;
        //            case double n when (n <= ySpacer) && n > -(ySpacer):
        //                tone = "wavinghand";
        //                passmojiTone = "wavinghand";
        //                break;
        //            case double n when n <= -(ySpacer) && n > -(ySpacer * 2 + 10):
        //                tone = "zippermouthface";
        //                passmojiTone = "zippermouthface";
        //                break;
        //            case double n when n <= -(ySpacer * 2 + 10) && n > -(ySpacer * 3 + 20):
        //                tone = "pileofpoo";
        //                passmojiTone = "pileofpoo";
        //                break;
        //            default:
        //                tone = "cancel";
        //                passmojiTone = "none";
        //                break;
        //        }

        //        return tone;

        //    }

        //    //get location
        //    async void GetLocationAsync()
        //    {
        //        if (!Preferences.Get("allowLocation", true))
        //        {
        //            locationObjectKey = "some";
        //            Device.BeginInvokeOnMainThread(async () =>
        //            {
        //                bool answer = await DisplayAlert("You disbled location access", "Would you like to enable location to search for a person?", "Yes", "No");
        //                if (answer == true)
        //                {
        //                    Preferences.Set("allowLocation", true);
        //                    locationObjectKey = "none";
        //                }
        //                else
        //                {
        //                    await Navigation.PopModalAsync();
        //                }
        //            });
        //        } else 
        //        {
        //            try
        //            {
        //                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
        //                var location = await Geolocation.GetLocationAsync(request);

        //                if (location != null)
        //                {
        //                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

        //                    currentLocation = location;
        //                    SetDBLocationAsync();
        //                    ShowRadar();
        //                }
        //            }
        //            catch (FeatureNotSupportedException fnsEx)
        //            {
        //                // Handle not supported on device exception
        //                await DisplayAlert("Faild", fnsEx.Message, "OK");
        //            }
        //            catch (FeatureNotEnabledException fneEx)
        //            {
        //                // Handle not enabled on device excepti629on
        //                await DisplayAlert("Faild", fneEx.Message, "OK");
        //            }
        //            catch (PermissionException pEx)
        //            {
        //                // Handle permission exception
        //                await DisplayAlert("Faild", pEx.Message, "OK");
        //            }
        //            catch (Exception ex)
        //            {
        //                // Unable to get location
        //                await DisplayAlert("Faild", ex.Message, "OK");
        //            }
        //        }

        //    }

        //    //timer
        //    private void SetTimer()
        //    {
        //        // Create a timer with a two second interval.
        //        observableTimer = new System.Timers.Timer(1000);
        //        // Hook up the Elapsed event for the timer. 
        //        observableTimer.Elapsed += TimerDone;
        //        observableTimer.AutoReset = true;
        //        observableTimer.Enabled = true;
        //    }

        //    //stop timer
        //    private void StopTimer()
        //    {
        //        observableTimer.Enabled = false;
        //    }

        //    //timer done
        //    private void TimerDone(Object source, ElapsedEventArgs e)
        //    {
        //        if (locationObjectKey == "none")
        //        {
        //            GetLocationAsync();
        //            Console.WriteLine("relocate");
        //        }

        //        if (observed)
        //        {
        //            observed = false;
        //            Console.WriteLine("observed!");
        //            GetSearchingPeople();
        //        }

        //    }

        void BackClicked(object sender, System.EventArgs e)
        {
            //StopTimer();
            //if (locationObjectKey != null)
            //{
            //    DeleteDBSearchingAsync();
            //}
            Navigation.PopModalAsync();
        }

        //    async void ShowRadar()
        //    {
        //        radarrings.Opacity = 1;
        //        await radarrings.ScaleTo(1.6, 300, Easing.CubicInOut);
        //    }

        //    async void HideRadar()
        //    {
        //        await radarrings.ScaleTo(0.1, 300, Easing.CubicOut);
        //        radarrings.Opacity = 0;
        //    }

        //    async void PulseRadar()
        //    {
        //        await radarrings.ScaleTo(2, 200, Easing.CubicIn);
        //        await radarrings.ScaleTo(1.6, 200, Easing.BounceOut);
        //    }
    }
}
