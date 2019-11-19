using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

using Xamarin.Forms;
using Xamarin.Essentials;

using ImageCircle.Forms.Plugin.Abstractions;
using AiForms.Effects;

using Firebase.Database;
using Firebase.Database.Query;

using Feels.Objects;

namespace Feels
{
    public partial class People : ContentPage
    {
        //firebase
        FirebaseClient firebase = new FirebaseClient("https://feels-d9ab9.firebaseio.com/");

        //data on the current user
        private string userKey;
        private User userData = new User();
        Color myMoodFrameColour;
        Color myMoodColour;

        //container varibles
        StackLayout personIconContainer;
        Frame containFrame;
        bool conversationsVisible = false;
        bool neutralVisible = false;
        bool positiveVisible = false;
        bool negativeVisible = false;

        //finger value
        double initialX;
        double initialY;
        double initialToneX;
        double initialToneY;

        //emoji varibles
        private View toneFace;
        string myTone = "neutral";
        string thisTone = "neutral";

        //Observable vars
        private static System.Timers.Timer observableTimer;
        private bool observed = false;

        public People()
        {
            BackgroundImage = "peopleloading";

            InitializeComponent();

            SetObservable();

            CheckLogin();

            userKey = Preferences.Get("userID", "none");

            LoadUserData();
            LoadData();

            //add Dynamic Content
            CreateButtons();
            ActivateMoodSelect();

        }

        private void CheckLogin()
        {
            if (!Preferences.Get("loggedIn", false))
            {
                var loginPage = new Login();
                Navigation.PushModalAsync(loginPage, false);

                loginPage.Disappearing += (sender2, e2) =>
                {
                    System.Diagnostics.Debug.WriteLine("The modal page is dismissed, do something now");
                    userKey = Preferences.Get("userID", "none");
                    LoadUserData();
                    LoadData();
                };
            }
        }

        //load user data from the database
        async void LoadUserData()
        {
            var users = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in users)
            {
                if (user.Key == Preferences.Get("userID", userKey))
                {
                    userData = user.Object;

                    if (userData.Image == "profileimage")
                    {
                        profileFrame.Padding = new Thickness(-1);
                        profileFrame.CornerRadius = 21;
                        profileImage.Source = "profileicon";
                    } else
                    {
                        profileFrame.Padding = new Thickness(2);
                        profileFrame.CornerRadius = 24;
                        profileImage.Source = userData.Image;
                    }
                    toneIcon.Source = userData.ToneSrc;
                    myMoodFrameColour = Color.FromHex("33" + GetMessageColour(userData.Tone));
                    myMoodFrame.BackgroundColor = myMoodFrameColour;
                    myMoodColour = Color.FromHex(GetMessageColour(userData.Tone));
                    myMoodLabel.TextColor = myMoodColour;
                    profileFrame.BackgroundColor = myMoodColour;
                    addPersonFrame.BackgroundColor = myMoodColour;
                }
            }
        }

        //load page data from the database
        async void LoadData()
        {

            BackgroundImage = "peopleloading";
            loading.IsRunning = true;

            var conversations = await firebase.Child("conversations").OnceAsync<Conversation>();
            var people = await firebase.Child("users").OnceAsync<User>();

            foreach (var conversation in conversations)
            {
                if (conversation.Object.Person1 == Preferences.Get("userID", userKey) || conversation.Object.Person2 == Preferences.Get("userID", userKey))
                {
                    foreach (var person in people)
                    {
                        if (conversation.Object.Person1 == person.Key || conversation.Object.Person2 == person.Key)
                        {
                            if (person.Key != Preferences.Get("userID", userKey))
                            {
                                Person currentPerson = new Person { ID = person.Key, Tone = person.Object.Tone, Name = person.Object.Name, Image = person.Object.Image, Conversation = conversation.Key };
                                AddPersonIcon(currentPerson);
                            }
                        }
                    }
                }
            }

            BackgroundImage = "";
            loading.IsRunning = false;

            GetChattingPeople();

            SetSectionVisibility();
        }

        //add the icon for person inthe correct section
        private void AddPersonIcon(Person personData)
        {
            
            personIconContainer = new StackLayout { Margin = new Thickness(0, 0, 25, 0) };
            var personFrame = new Frame { Padding = new Thickness(3), HasShadow = false, CornerRadius = 42, BackgroundColor = Color.FromHex(GetMessageColour(personData.Tone)) };
            var personImage = new CircleImage { Aspect = Aspect.AspectFill, HeightRequest = 80, WidthRequest = 80, Source = "profileimage", HorizontalOptions = LayoutOptions.Start };
            var personNameContainer = new StackLayout { HorizontalOptions = LayoutOptions.Center };
            var personName = new Label { Text = personData.Name, FontSize = 12, TextColor = Color.FromHex("F2F2F2"), HorizontalOptions = LayoutOptions.StartAndExpand };

            string containerName;

            switch (personData.Tone)
            {
                case "neutral":
                    containerName = "Neutral";
                    break;
                case "critical":
                    containerName = "Neutral";
                    break;
                case "tired":
                    containerName = "Neutral";
                    break;
                case "joy":
                    containerName = "Positive";
                    break;
                case "love":
                    containerName = "Positive";
                    break;
                case "confident":
                    containerName = "Positive";
                    break;
                case "surprise":
                    containerName = "Positive";
                    break;
                case "crazyness":
                    containerName = "Positive";
                    break;
                case "celebration":
                    containerName = "Positive";
                    break;
                case "anger":
                    containerName = "Negative";
                    break;
                case "fear":
                    containerName = "Negative";
                    break;
                case "sadness":
                    containerName = "Negative";
                    break;
                case "disagreement":
                    containerName = "Negative";
                    break;
                case "disgust":
                    containerName = "Negative";
                    break;
                default:
                    containerName = "Neutral";
                    break;
            }

            switch (containerName)
            {
                case "Neutral":
                    neutralContainer.Children.Add(personIconContainer);
                    neutralVisible = true;
                    break;
                case "Positive":
                    positiveContainer.Children.Add(personIconContainer);
                    positiveVisible = true;
                    break;
                case "Negative":
                    negativeContainer.Children.Add(personIconContainer);
                    negativeVisible = true;
                    break;
            }

            //person tap
            var personTapGestureRecognizer = new TapGestureRecognizer();
            personTapGestureRecognizer.Tapped += (s, e) => {
                // handle the tap
                StopTimer();
                var chatPage = new Chat(personData);
                Navigation.PushAsync(chatPage);

                chatPage.Disappearing += (sender2, e2) =>
                {
                    Console.WriteLine("timer set");
                    SetTimer();
                };
            };

            personIconContainer.Scale = 0;

            personIconContainer.GestureRecognizers.Add(personTapGestureRecognizer);
            personIconContainer.Children.Add(personFrame);
            personFrame.Content = personImage;
            personIconContainer.Children.Add(personNameContainer);
            personNameContainer.Children.Add(personName);

            personImage.Source = personData.Image;

            personIconContainer.ScaleTo(1, 400, Easing.SpringOut);
        }

        private void CreateButtons()
        {
            //profile icon tap
            var ProfileTapGestureRecognizer = new TapGestureRecognizer();
            ProfileTapGestureRecognizer.Tapped += (s, e) => {
                // handle the tap
                var profilePage = new Profile(userData);
                Navigation.PushModalAsync(profilePage);

                profilePage.Disappearing += (sender2, e2) =>
                {
                    if (!Preferences.Get("loggedIn", false))
                    {
                        RemoveChildren();
                        CheckLogin();
                    } else
                    {
                        LoadUserData();
                    }
                };
            };
            profileImage.GestureRecognizers.Add(ProfileTapGestureRecognizer);

            //add person icon tap
            var AddPersonTapGestureRecognizer = new TapGestureRecognizer();
            AddPersonTapGestureRecognizer.Tapped += (s, e) => {
                // handle the tap
                var addPersonPage = new AddPerson();
                Navigation.PushModalAsync(addPersonPage);

                addPersonPage.Disappearing += (sender2, e2) =>
                {
                    RemoveChildren();
                    LoadData();
                };
            };
            addPersonImage.GestureRecognizers.Add(AddPersonTapGestureRecognizer);
        }

        void RemoveChildren()
        {
            if (conversationsVisible)
            {
                conversationContainer.Children.Clear();
                conversationsVisible = false;
            }
            if (neutralVisible)
            {
                neutralContainer.Children.Clear();
                neutralVisible = false;
            }
            if (positiveVisible)
            {
                positiveContainer.Children.Clear();
                positiveVisible = false;
            }
            if (negativeVisible)
            {
                negativeContainer.Children.Clear();
                negativeVisible = false;
            }

            SetSectionVisibility();
        }

        void SetSectionVisibility()
        {
            Console.WriteLine(neutralVisible);

            conversationWrapper.IsVisible = conversationsVisible;

            neutralWrapper.IsVisible = neutralVisible;

            positiveWrapper.IsVisible = positiveVisible;

            negativeWrapper.IsVisible = negativeVisible;
        }

        //observable
        private void SetObservable()
        {
            SetTimer();

            var observable = firebase.Child("chatting")
              .AsObservable<Conversation>()
              .Subscribe(d =>
              {
                  observed = true;
              });
        }

        //timer
        private void SetTimer()
        {
            Console.WriteLine("timer set");
            // Create a timer with a two second interval.
            observableTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            observableTimer.Elapsed += TimerDone;
            observableTimer.AutoReset = true;
            observableTimer.Enabled = true;
        }

        //stop timer
        private void StopTimer()
        {
            Console.WriteLine("timer Stopped");
            observableTimer.Enabled = false;
        }

        //timer done
        private void TimerDone(Object source, ElapsedEventArgs e)
        {
            if (observed)
            {
                observed = false;
                Console.WriteLine("observed!");
                GetChattingPeople();
            }

        }

        void GetChattingPeople()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {

                var chats = await firebase.Child("chatting").OnceAsync<Objects.Chat>();
                var people = await firebase.Child("users").OnceAsync<User>();

                Person fillerPerson = new Person { ID = "1", Tone = "neitral", Name = "default", Image = "profileimage", Conversation = "hi" };
                AddChatIcon(fillerPerson);

                conversationContainer.Children.Clear();
                conversationsVisible = false;

                foreach (var chat in chats)
                {
                    if (chat.Object.Person1 == Preferences.Get("userID", userKey) || chat.Object.Person2 == Preferences.Get("userID", userKey))
                    {
                        foreach (var person in people)
                        {
                            if (chat.Object.Person1 == person.Key || chat.Object.Person2 == person.Key)
                            {
                                if (person.Key != Preferences.Get("userID", userKey))
                                {
                                    Person currentPerson = new Person { ID = person.Key, Tone = chat.Object.Tone, Name = person.Object.Name, Image = person.Object.Image, Conversation = chat.Object.ConversationID };
                                    AddChatIcon(currentPerson);
                                    conversationsVisible = true;
                                    Console.WriteLine("chatAdd");
                                }
                            }
                        }
                    }
                }
                SetSectionVisibility();
            });
        }

        //add the icon for person inthe correct section

        public void AddChatIcon(Person personData)
        {
            Console.WriteLine($"Addperson {personData.Name}");
            containFrame = new Frame();
            containFrame.CornerRadius = 30;
            containFrame.HeightRequest = 44;
            containFrame.Padding = new Thickness(8);
            containFrame.Margin = new Thickness(0, 0, 10, 0);
            containFrame.BackgroundColor = Color.FromHex(GetMessageColour(personData.Tone));
            containFrame.HasShadow = false;
            personIconContainer = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = new Thickness(0) };
            var personImage = new CircleImage { Aspect = Aspect.AspectFill, HeightRequest = 44, WidthRequest = 44, Source = "profileimage", HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0) };
            var personName = new Label { Text = personData.Name, FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#FFFFFF"), VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.CenterAndExpand, Margin = new Thickness(5, 0, 10, 0) };

            //person tap
            var personTapGestureRecognizer = new TapGestureRecognizer();
            personTapGestureRecognizer.Tapped += (s, e) => {
                // handle the tap
                StopTimer();
                var chatPage = new Chat(personData);
                Navigation.PushAsync(chatPage);

                chatPage.Disappearing += (sender2, e2) =>
                {
                    Console.WriteLine("timer set");
                    SetTimer();
                };
            };

            conversationContainer.Scale = 0;

            conversationContainer.Children.Add(containFrame);
            containFrame.Content = personIconContainer;
            personIconContainer.GestureRecognizers.Add(personTapGestureRecognizer);
            personIconContainer.Children.Add(personImage);
            personIconContainer.Children.Add(personName);

            personImage.Source = personData.Image;

            conversationContainer.ScaleTo(1, 400, Easing.SpringOut);
        }

        //get selected tone
        private string GetMessageColour(string tone)
        {
            string colour = "";

            switch (tone)
            {
                case "neutral":
                    colour = "888888";
                    break;
                case "critical":
                    colour = "1976D2";
                    break;
                case "tired":
                    colour = "0288D1";
                    break;
                case "joy":
                    colour = "FBC02D";
                    break;
                case "love":
                    colour = "C2185B";
                    break;
                case "confident":
                    colour = "7B1FA2";
                    break;
                case "surprise":
                    colour = "E64A19";
                    break;
                case "crazyness":
                    colour = "F57C00";
                    break;
                case "celebration":
                    colour = "FFA000";
                    break;
                case "anger":
                    colour = "D32F2F";
                    break;
                case "fear":
                    colour = "455A64";
                    break;
                case "sadness":
                    colour = "0D47A1";
                    break;
                case "disagreement":
                    colour = "5D4037";
                    break;
                case "disgust":
                    colour = "7CB342";
                    break;
                default:
                    colour = "888888";
                    break;
            }

            return colour;

        }

        //Current mood picker
        private void ActivateMoodSelect()
        {
            //tone face
            var toneface = new Image { Source = "slightlysmilingface" };
            toneFace = toneface;

            //touch recogniser
            var recognizer = AddTouch.GetRecognizer(toneArea);

            recognizer.TouchBegin += (sender, e) => {
                //set initial touch position
                initialX = e.Location.X;
                initialY = e.Location.Y;

                initialToneX = toneIcon.X;
                initialToneY = toneIcon.Y;

                //create emoji
                toneArea.Children.Add(toneFace);

                toneFace.ScaleTo(1, 0);
                DimContent();

                //show picker grid and emoji
                toneIcon.Source = "chatemojigrid";
                toneIcon.TranslateTo(e.Location.X - 130, e.Location.Y - 135, 300, Easing.SpringOut);
                toneIcon.ScaleTo(6, 300, Easing.SpringOut);
                toneFace.TranslateTo(e.Location.X - 20, e.Location.Y - 140, 0);
                toneFace.ScaleTo(2.2, 400, Easing.SpringOut);
            };

            recognizer.TouchMove += (sender, e) => {
                toneFace.TranslateTo(e.Location.X - 20, e.Location.Y - 140, 30, Easing.SpringOut);
                GetCurrentTone(e.Location.X, e.Location.Y);
                toneface.Source = thisTone;
            };

            recognizer.TouchEnd += async (sender, e) => {
                ShowContent();
                userData.Tone = myTone;
                userData.ToneSrc = thisTone;
                UpdateMyMood();

                if (thisTone!= "cancel")
                {
                    toneIcon.Source = thisTone;
                } else
                {
                    toneIcon.Source = "slightlysmilingface";
                }

                toneArea.Children.Remove(toneFace);
                await toneIcon.TranslateTo(initialToneX, initialToneY, 20);
                await toneIcon.ScaleTo(1, 200, Easing.SpringOut);
            };

            recognizer.TouchCancel += (sender, e) => {
                Debug.WriteLine("TouchCancel");
            };
        }

        private void DimContent()
        {
            navLayout.Opacity = 0.5;
            conversationWrapper.Opacity = 0.5;
            conversationContainer.Opacity = 0.5;
            neutralContainer.Opacity = 0.5;
            positiveContainer.Opacity = 0.5;
            negativeContainer.Opacity = 0.5;
            myMoodFrame.BackgroundColor = Color.FromHex("#000000");
        }

        private void ShowContent()
        {
            navLayout.Opacity = 1;
            conversationWrapper.Opacity = 1;
            conversationContainer.Opacity = 1;
            neutralContainer.Opacity = 1;
            positiveContainer.Opacity = 1;
            negativeContainer.Opacity = 1;
            myMoodFrame.BackgroundColor = myMoodFrameColour;
        }

        //get selected tone
        private void GetCurrentTone(double x, double y)
        {
            int xSpacer = 30;
            int ySpacer = 40;
            double yDifference = (initialY) - y;
            double xDifference = (initialX - 50) - x;

            switch (yDifference)
            {
                case double n when n <= (ySpacer * 6) && n > (ySpacer * 5):
                    switch (xDifference)
                    {
                        case double m when (m <= -30):
                            thisTone = "smilingfacewithhearteyes";
                            myTone = "love";
                            break;
                        default:
                            thisTone = "cancel";
                            myTone = "neutral";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 5) && n > (ySpacer * 4):
                    switch (xDifference)
                    {
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            thisTone = "zanyface";
                            myTone = "crazyness";
                            break;
                        case double m when (m <= -30):
                            thisTone = "faceblowingakiss";
                            myTone = "love";
                            break;
                        default:
                            thisTone = "cancel";
                            myTone = "neutral";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 4) && n > (ySpacer * 3):
                    switch (xDifference)
                    {
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            thisTone = "explodinghead";
                            myTone = "surprise";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            thisTone = "smilingfacewithhorns";
                            myTone = "confident";
                            break;
                        case double m when (m <= -30):
                            thisTone = "partyingface";
                            myTone = "celebration";
                            break;
                        default:
                            thisTone = "cancel";
                            myTone = "neutral";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 3) && n > (ySpacer * 2):
                    switch (xDifference)
                    {
                        case double m when m <= (xSpacer * 4) && m > (xSpacer * 2):
                            thisTone = "sleepingface";
                            myTone = "tired";
                            break;
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            thisTone = "facescreaminginfear";
                            myTone = "surprise";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            thisTone = "smilingfacewithsunglasses";
                            myTone = "confident";
                            break;
                        case double m when (m <= -30):
                            thisTone = "facewithtearsofjoy";
                            myTone = "joy";
                            break;
                        default:
                            thisTone = "cancel";
                            myTone = "neutral";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 2) && n > (ySpacer):
                    switch (xDifference)
                    {
                        case double m when m <= (xSpacer * 5) && m > (xSpacer * 4):
                            thisTone = "nauseatedface";
                            myTone = "disgust";
                            break;
                        case double m when m <= (xSpacer * 4) && m > (xSpacer * 2):
                            thisTone = "facewithmonocle";
                            myTone = "critical";
                            break;
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            thisTone = "flushedface";
                            myTone = "fear";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            thisTone = "smilingfacewithhalo";
                            myTone = "confident";
                            break;
                        case double m when (m <= -30):
                            thisTone = "beamingfacewithsmilingeyes";
                            myTone = "joy";
                            break;
                        default:
                            thisTone = "cancel";
                            myTone = "neutral";
                            break;
                    }
                    break;
                case double n when (n <= ySpacer):
                    switch (xDifference)
                    {
                        case double m when m <= (40 * 6) && m > (xSpacer * 5):
                            thisTone = "facewithsymbolsonmouth";
                            myTone = "anger";
                            break;
                        case double m when m <= (xSpacer * 5) && m > (xSpacer * 4):
                            thisTone = "facewithsteamfromnose";
                            myTone = "anger";
                            break;
                        case double m when m <= (xSpacer * 4) && m > (xSpacer * 2):
                            thisTone = "loudlycryingface";
                            myTone = "sadness";
                            break;
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            thisTone = "cryingface";
                            myTone = "sadness";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            thisTone = "facewithrollingeyes";
                            myTone = "disagreement";
                            break;
                        case double m when (m <= -30):
                            thisTone = "slightlysmilingface";
                            myTone = "neutral";
                            break;
                        default:
                            thisTone = "cancel";
                            myTone = "neutral";
                            break;
                    }
                    break;
                default:
                    thisTone = "cancel";
                    myTone = "cancel";
                    break;
            }

        }

        private async void UpdateMyMood()
        {
            PopButtons();
            myMoodFrameColour = Color.FromHex("44" + GetMessageColour(userData.Tone));
            myMoodFrame.BackgroundColor = myMoodFrameColour;
            myMoodColour = Color.FromHex(GetMessageColour(userData.Tone));
            myMoodLabel.TextColor = myMoodColour;
            profileFrame.BackgroundColor = myMoodColour;
            addPersonFrame.BackgroundColor = myMoodColour;
            await firebase.Child("users").Child(userKey).PutAsync(userData);
        }

        private async void PopButtons()
        {
            profileFrame.ScaleTo(0.5, 100, Easing.CubicOut).Forget();
            await addPersonFrame.ScaleTo(0.5, 100, Easing.CubicOut);

            profileFrame.ScaleTo(1, 300, Easing.BounceOut).Forget();
            addPersonFrame.ScaleTo(1, 300, Easing.BounceOut).Forget();
        }

        void InfoClicked(object sender, System.EventArgs e)
        {
            Navigation.PushModalAsync(new Info());
        }
    }
}
