using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;

using Plugin.Media;
using Plugin.Media.Abstractions;

using Xamarin.Essentials;
using AiForms.Effects;
using System.Diagnostics;

using Xamarin.Forms;

using Firebase.Storage;
using Firebase.Database;
using Firebase.Database.Query;

using Feels.Objects;
using System.Text;

namespace Feels
{
    public partial class Chat : ContentPage
    {
        //swipe objects
        View swipeIcon;
        View swipeText;
        IGestureRecognizer swipeGesture;
        //IGestureRecognizer tapGesture;

        //input bar
        //Entry chatEntry;
        //View tonePicker;
        View toneFace;

        //finger value
        double initialX;
        double initialY;
        double initialToneX;
        double initialToneY;

        //firebase
        FirebaseClient firebase;
        ChildQuery conversationChild;
        ChildQuery conversationChildShort;

        //message vars
        private static System.Timers.Timer observableTimer;
        private bool observableMessage = false;

        //status vars
        string userStatusKey;
        ConversationStatus userStatus = new ConversationStatus();
        private bool observableStatus = false;

        //user data
        string userID;

        //person data
        string personID;

        //tone
        string messageTone = "none";
        string currentTone = "slightlysmilingface";
        string backgroundImageColour = "neutralbg";

        //initial
        bool firstActivate = true;
        string chatID = "none";
        string conversationID = "none";

        //stash vars
        Message currentMessage = new Message();
        Message fromMessage = new Message();
        Message toMessage = new Message();
        Message fromImage = new Message();
        Message toImage = new Message();

        public Chat(Person person)
        {
            InitializeComponent();

            //get and add data for person
            AddPersonData(person);

            //initialize firebase 
            InitializeFirebase(person);

            //set background colour
            SetBackground(person.Tone);

            //add dynamic data to chat
            CreateButtons(person);
        }

        //initialize firebase 
        private void InitializeFirebase(Person person)
        {
            SetTimer();

            firebase = new FirebaseClient("https://feels-d9ab9.firebaseio.com/");

            conversationChild = firebase.Child("conversations").Child(person.Conversation).Child("messages");
            conversationChildShort = firebase.Child("conversations").Child(person.Conversation);

            conversationID = person.Conversation;

            SetConversationStatus();

            AddChats();

            //GetMessagesAsync();

            var observable = conversationChild
              .AsObservable<Message>()
              .Subscribe(d =>
              {
                  observableMessage = true;

              });

            var observeStatus = conversationChildShort.Child("status")
              .AsObservable<ConversationStatus>()
              .Subscribe(d =>
              {
                  observableStatus = true;
              });
        }

        //add person data
        private void AddPersonData(Person person)
        {

            personName.Text = person.Name;
            personImage.Source = person.Image;

            personID = person.ID;
            userID = Preferences.Get("userID", "null");
        }

        //timer
        private void SetTimer()
        {
            // Create a timer with a two second interval.
            observableTimer = new System.Timers.Timer(100);
            // Hook up the Elapsed event for the timer. 
            observableTimer.Elapsed += TimerDone;
            observableTimer.AutoReset = true;
            observableTimer.Enabled = true;
        }

        //timer
        private void StopTimer()
        {
            // Hook up the Elapsed event for the timer. 
            observableTimer.Enabled = false;
        }

        //timer done
        private void TimerDone(Object source, ElapsedEventArgs e)
        {
            if (observableMessage)
            {
                Console.WriteLine("ho");
                observableMessage = false;
                GetMessagesAsync();
            }

            if (observableStatus)
            {
                observableStatus = false;
                ShowStatusAsync();
            }

        }

        //show messages
        private void GetMessagesAsync()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var messages = await conversationChild.OrderByKey().OnceAsync<Message>();
                Console.WriteLine("Message call made!");
                View mostRecentMessagView = toFrame;
                Message mostRecentMessage;
                string mostRecentTone = "neutral";
                string mostRecentImage = "slightlysmilingface";
                bool isFrom = false;
                bool isImage = false;
                int cycleCount = 0;
                string fromLabel = "";
                string toLabel = "";

                foreach (var message in messages)
                {
                    cycleCount++;
                    mostRecentMessage = message.Object;
                    mostRecentTone = mostRecentMessage.Tone;
                    mostRecentImage = mostRecentMessage.ToneSrc;
                    currentMessage = message.Object;


                    if (message.Object.Sender == userID)
                    {
                        isFrom = false;

                        toLabel = mostRecentMessage.Text;

                        mostRecentMessagView = toFrame;

                        isImage = mostRecentMessage.IsImage;
                    }
                    else
                    {
                        isFrom = true;

                        fromLabel = mostRecentMessage.Text;

                        mostRecentMessagView = fromFrame;

                        isImage = mostRecentMessage.IsImage;
                    }
                }

                Color toneColour = Color.FromHex(GetMessageColour(mostRecentTone));

                if (cycleCount > 0)
                {
                    if (isFrom)
                    {
                        //animation
                        mostRecentMessagView.ScaleTo(0, 200, Easing.SpringOut).Forget();
                        pictureFromFrame.ScaleTo(0, 200, Easing.SpringOut).Forget();

                        await mostRecentMessagView.FadeTo(0, 200, Easing.CubicOut);
                        pictureFromFrame.FadeTo(0, 200, Easing.CubicOut).Forget();

                        mostRecentMessagView.TranslationY = -200;
                        pictureFromFrame.TranslationY = -200;

                        mostRecentMessagView.Scale = 0.5;
                        pictureFromFrame.Scale = 0.5;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (!isImage)
                            {
                                fromMessage = currentMessage;

                                pictureFromFrame.IsVisible = false;

                                chatFromLabel.Text = fromLabel;

                                chatFromImage.Source = mostRecentImage;

                                if (mostRecentTone != "neutral")
                                {
                                    chatFromLabel.TextColor = Color.FromHex("#FFFFFF");
                                }
                                else
                                {
                                    chatFromLabel.TextColor = Color.FromHex("#000000");
                                }

                                chatFromImage.Source = mostRecentImage;

                                mostRecentMessagView.BackgroundColor = toneColour;
                                chatFromThing.Color = toneColour;

                                mostRecentMessagView.IsVisible = true;
                            } else
                            {
                                fromImage = currentMessage;

                                fromFrame.IsVisible = false;

                                pictureFrom.Source = fromLabel;
                                pictureFromFrame.BackgroundColor = toneColour;

                                pictureFromFrame.IsVisible = true;
                            }

                            SetBackground(mostRecentTone);

                            //chat from attributes
                            chatToImage.Source = "";
                            toFrame.BackgroundColor = Color.FromHex(GetMessageColour("neutral"));
                            chatToLabel.TextColor = Color.FromHex("#000000");
                            chatToThing.Color = Color.FromHex("#FFFFF");
                        });

                        toFrame.ScaleTo(0.6, 200, Easing.CubicInOut).Forget();
                        pictureToFrame.ScaleTo(0.6, 200, Easing.CubicInOut).Forget();

                        //animation
                        mostRecentMessagView.FadeTo(1, 200, Easing.CubicOut).Forget();
                        pictureFromFrame.FadeTo(1, 200, Easing.CubicOut).Forget();

                        mostRecentMessagView.TranslateTo(0, 0, 400, Easing.SpringOut).Forget();
                        pictureFromFrame.TranslateTo(0, 0, 400, Easing.SpringOut).Forget();

                        mostRecentMessagView.ScaleTo(1, 200, Easing.CubicOut).Forget();
                        pictureFromFrame.ScaleTo(1, 200, Easing.CubicOut).Forget();
                    }
                    else
                    {
                        //animation
                        mostRecentMessagView.ScaleTo(0, 200, Easing.SpringOut).Forget();
                        pictureToFrame.ScaleTo(0, 200, Easing.SpringOut).Forget();

                        await mostRecentMessagView.FadeTo(0, 200, Easing.CubicOut);
                        pictureToFrame.FadeTo(0, 200, Easing.CubicOut).Forget();

                        mostRecentMessagView.TranslationY = 200;
                        pictureToFrame.TranslationY = 200;

                        mostRecentMessagView.Scale = 0.5;
                        pictureToFrame.Scale = 0.5;

                        Device.BeginInvokeOnMainThread(() =>
                       {
                           if (!isImage)
                           {
                               toMessage = currentMessage;

                               pictureToFrame.IsVisible = false;

                               chatToLabel.Text = toLabel;

                               if (mostRecentTone != "neutral")
                               {
                                   chatToLabel.TextColor = Color.FromHex("#FFFFFF");
                               }
                               else
                               {
                                   chatToLabel.TextColor = Color.FromHex("#000000");
                               }

                               chatToImage.Source = mostRecentImage;

                               mostRecentMessagView.IsVisible = true;

                               mostRecentMessagView.BackgroundColor = toneColour;
                               chatToThing.Color = toneColour;
                               SetBackground(mostRecentTone);

                                //chat from attributes
                                chatFromImage.Source = "";
                                fromFrame.BackgroundColor = Color.FromHex(GetMessageColour("neutral"));
                                chatFromLabel.TextColor = Color.FromHex("#000000");
                                chatFromThing.Color = Color.FromHex("#FFFFF");

                                mostRecentMessagView.IsVisible = true;
                           }
                           else
                           {
                               toImage = currentMessage;

                               toFrame.IsVisible = false;

                               pictureTo.Source = toLabel;

                               pictureToFrame.IsVisible = true;
                           }

                           SetBackground(mostRecentTone);
                       });
                        //animation
                        fromFrame.ScaleTo(0.6, 200, Easing.CubicInOut).Forget();
                        pictureFromFrame.ScaleTo(0.6, 200, Easing.CubicInOut).Forget();

                        //animation
                        mostRecentMessagView.FadeTo(1, 200, Easing.CubicOut).Forget();
                        pictureToFrame.FadeTo(1, 200, Easing.CubicOut).Forget();

                        mostRecentMessagView.TranslateTo(0, 0, 400, Easing.SpringOut).Forget();
                        pictureToFrame.TranslateTo(0, 0, 400, Easing.SpringOut).Forget();

                        mostRecentMessagView.ScaleTo(1, 200, Easing.CubicOut).Forget();
                        pictureToFrame.ScaleTo(1, 200, Easing.CubicOut).Forget();
                    }

                    Console.WriteLine("done");
                    messageTone = mostRecentTone;
                    SetConverstaionToneAsync();
                }
            });
        }

        //create buttons
        private void CreateButtons(Person person)
        {
            var BackTapGestureRecognizer = new TapGestureRecognizer();
            BackTapGestureRecognizer.Tapped += async (s, e) => {
                // handle the tap
                CheckIfAvtiveAsync();

                StopTimer();

                await Navigation.PopAsync();
            };
            backImage.GestureRecognizers.Add(BackTapGestureRecognizer);

            var StashTapGestureRecognizer = new TapGestureRecognizer();
            StashTapGestureRecognizer.Tapped += async (s, e) => {
                // handle the tap
                var savedPage = new SavedMessages(person, backgroundImageColour);
                await Navigation.PushAsync(savedPage);
            };
            stashButton.GestureRecognizers.Add(StashTapGestureRecognizer);

            var ToMessageTapGestureRecognizer = new TapGestureRecognizer();
            ToMessageTapGestureRecognizer.NumberOfTapsRequired = 2;
            ToMessageTapGestureRecognizer.Tapped += async (s, e) => {
                // handle the tap
                AddToStash(toMessage);
                await toFrame.ScaleTo(1.2, 200, Easing.SpringIn);
                toFrame.ScaleTo(1, 300, Easing.SpringIn).Forget();
            };
            toFrame.GestureRecognizers.Add(ToMessageTapGestureRecognizer);

            var FromMessageTapGestureRecognizer = new TapGestureRecognizer();
            FromMessageTapGestureRecognizer.NumberOfTapsRequired = 2;
            FromMessageTapGestureRecognizer.Tapped += async (s, e) => {
                // handle the tap
                AddToStash(fromMessage);
                await fromFrame.ScaleTo(1.2, 200, Easing.SpringIn);
                fromFrame.ScaleTo(1, 300, Easing.SpringIn).Forget();
            };
            fromFrame.GestureRecognizers.Add(FromMessageTapGestureRecognizer);

            var ToImageTapGestureRecognizer = new TapGestureRecognizer();
            ToImageTapGestureRecognizer.NumberOfTapsRequired = 2;
            ToImageTapGestureRecognizer.Tapped += async (s, e) => {
                // handle the tap
                AddToStash(toImage);
                await chatToImage.ScaleTo(1.2, 200, Easing.SpringIn);
                chatToImage.ScaleTo(1, 300, Easing.SpringIn).Forget();
            };
            pictureToFrame.GestureRecognizers.Add(ToImageTapGestureRecognizer);

            var FromImageGestureRecognizer = new TapGestureRecognizer();
            FromImageGestureRecognizer.NumberOfTapsRequired = 2;
            FromImageGestureRecognizer.Tapped += async (s, e) => {
                // handle the tap
                AddToStash(fromImage);
                await chatFromImage.ScaleTo(1.2, 200, Easing.SpringIn);
                chatFromImage.ScaleTo(1, 300, Easing.SpringIn).Forget();
            };
            pictureFromFrame.GestureRecognizers.Add(FromImageGestureRecognizer);


            ActivateInputBar();
        }

        //make swipe recogniser and indicator
        private void CreateSwipeArea()
        {
            //swipe gesture
            var swipeUpGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
            swipeUpGesture.Swiped += OnSwipeUp;
            swipeGesture = swipeUpGesture;

            ////tap also
            //var tapType = new TapGestureRecognizer();
            //tapType.Tapped += (s, e) => {
            //    ShowInputBar();
            //};
            //tapGesture = tapType;

            //swipe icon
            var swipeicon = new Image { Source = "swipeup", Margin = new Thickness(0, 40, 0, 0) };
            swipeIcon = swipeicon;

            //swipe text
            var swipetext = new Label { Text = "Swipe Up To Chat", HorizontalOptions = LayoutOptions.CenterAndExpand, Margin = new Thickness(0, 0, 0, 100) };
            swipeText = swipetext;

            AddSwipeArea();
        }

        //make swipe recogniser and indicator
        private void AddSwipeArea()
        {
            swipeIndicator.TranslationY = 200;
            swipeIndicator.Opacity = 0;

            swipeIndicator.FadeTo(1, 300, Easing.CubicOut);
            swipeIndicator.TranslateTo(0,0, 300, Easing.CubicOut);
            //swipe gesture
            swipeIndicator.GestureRecognizers.Add(swipeGesture);

            //tap gesture
            //mainContainer.GestureRecognizers.Add(tapGesture);

            //swipe icon
            swipeIndicator.Children.Add(swipeIcon);
 
            //swipe text
            swipeIndicator.Children.Add(swipeText);

            //camera icon
            takePhotoBtn.IsVisible = true;
        }

        //remove swipe area
        private void RemoveSwipeArea()
        {
            swipeIndicator.Children.Remove(swipeIcon);
            swipeIndicator.Children.Remove(swipeText);
            mainContainer.GestureRecognizers.Remove(swipeGesture);
            //mainContainer.GestureRecognizers.Remove(tapGesture);

        }

        //chat entry handle focused
        private void Showentry()
        {
            //camera icon
            takePhotoBtn.IsVisible = false;

            RemoveSwipeArea();
            typeBar.IsVisible = true;
            chatEntry.Focus();
        }

        //chat entry handle focused
        private void Hideentry()
        {
            chatEntry.Unfocus();
            typeBar.IsVisible = false;
        }

        //chat entry handle focused
        void HandleEntryFocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            chatBubblesContainer.VerticalOptions = LayoutOptions.EndAndExpand;
            SetTypingTrue();
        }

        //chat entry handle unfocused
        void HandleEntryUnfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            chatBubblesContainer.VerticalOptions = LayoutOptions.CenterAndExpand;
            SetTypingFalse();
        }


        //make input bar
        //private void CreateInputBar()
        //{

        //    //chat entry
        //    var chatentry = new Entry { Placeholder = "Type Message Here", Keyboard = Keyboard.Chat, HeightRequest = 40, HorizontalOptions = LayoutOptions.FillAndExpand };
        //    chatentry.Unfocused += (sender, e) =>
        //    {
        //        chatBubblesContainer.VerticalOptions = LayoutOptions.CenterAndExpand;
        //        SetTypingFalse();
        //    };
        //    chatentry.Focused += (sender, e) =>
        //    {
        //        chatBubblesContainer.VerticalOptions = LayoutOptions.EndAndExpand;
        //        SetTypingTrue();
        //    };
        //    chatEntry = chatentry;

        //    ActivateInputBar();
        //}

        //swipe gesture event
        void OnSwipeUp(object sender, SwipedEventArgs e)
        {
            Showentry();
        }

        //display input bar
        //void ShowInputBar()
        //{
        //    RemoveSwipeArea();

        //    //if (messageTone == "none")
        //    //{
        //    //    Showentry();
        //    //}
        //    //else
        //    //{
        //        Showentry();
        //        chatEntry.Focus();
        //   // }
        //}

        //make input bar
        private void  ActivateInputBar()
        {
            //tone picker
            //toneArea.Children.Add(tonepicker);

            //tone face
            var toneface = new Image { Source = "slightlysmilingface" };
            toneFace = toneface;
            //set background image
            //BackgroundImage = "happybg";

            //touch recogniser
            var recognizer = AddTouch.GetRecognizer(toneArea);

            recognizer.TouchBegin += (sender, e) => {
                //set initial touch position
                initialX = e.Location.X;
                initialY = e.Location.Y;

                initialToneX = tonePicker.X;
                initialToneY = tonePicker.Y;

                //create emoji
                toneArea.Children.Add(toneFace);

                toneFace.ScaleTo(1, 0);
                DimContent();

                //show picker grid and emoji
                tonePicker.Source = "chatemojigrid";
                tonePicker.TranslateTo(e.Location.X - 130, e.Location.Y - 135, 300, Easing.SpringOut);
                tonePicker.ScaleTo(6, 300, Easing.SpringOut);
                toneFace.TranslateTo(e.Location.X - 60, e.Location.Y - 90, 0);
                toneFace.ScaleTo(3, 400, Easing.SpringOut);
            };

            recognizer.TouchMove += async (sender, e) => {
                await toneFace.TranslateTo(e.Location.X - 60, e.Location.Y - 80, 20, Easing.SpringOut);
                currentTone = GetCurrentTone(e.Location.X, e.Location.Y);
                toneface.Source = currentTone;
            };

            recognizer.TouchEnd += async (sender, e) => {
                await tonePicker.TranslateTo(initialToneX,initialToneY,0);
                ShowContent();
                tonePicker.Source = "tonepicker";
                toneArea.Children.Remove(toneFace);

                if (messageTone == "cancel")
                {
                   chatEntry.Focus();
                    await tonePicker.TranslateTo(initialToneX, initialToneY, 20);
                    await tonePicker.ScaleTo(1, 200, Easing.SpringOut);
                } 
                else
                {
                    await tonePicker.ScaleTo(1, 30, Easing.SpringOut);
                    Hideentry();
                    AddSwipeArea();
                    await SendMessageAsync(messageTone);
                }

            };

            recognizer.TouchCancel += (sender, e) => {
                Debug.WriteLine("TouchCancel");
            };
        }


        //get selected tone
        private string GetCurrentTone(double x, double y)
        {
            int xSpacer = 30;
            int ySpacer = 40;
            double yDifference = initialY - y;
            double xDifference = (initialX - 50) - x;
            string tone = "";

            switch (yDifference)
            {
                case double n when n <= (ySpacer * 6) && n > (ySpacer * 5):
                    switch (xDifference)
                    {
                        case double m when (m <= -30):
                            tone = "smilingfacewithhearteyes";
                            messageTone = "love";
                            break;
                        default:
                            tone = "cancel";
                            messageTone = "cancel";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 5) && n > (ySpacer * 4):
                    switch (xDifference)
                    {
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            tone = "zanyface";
                            messageTone = "crazyness";
                            break;
                        case double m when (m <= -30):
                            tone = "faceblowingakiss";
                            messageTone = "love";
                            break;
                        default:
                            tone = "cancel";
                            messageTone = "cancel";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 4) && n > (ySpacer * 3):
                    switch (xDifference)
                    {
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            tone = "explodinghead";
                            messageTone = "surprise";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            tone = "smilingfacewithhorns";
                            messageTone = "confident";
                            break;
                        case double m when (m <= -30):
                            tone = "partyingface";
                            messageTone = "celebration";
                            break;
                        default:
                            tone = "cancel";
                            messageTone = "cancel";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 3) && n > (ySpacer * 2):
                    switch (xDifference)
                    {
                        case double m when m <= (xSpacer * 4) && m > (xSpacer * 2):
                            tone = "sleepingface";
                            messageTone = "tired";
                            break;
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            tone = "facescreaminginfear";
                            messageTone = "surprise";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            tone = "smilingfacewithsunglasses";
                            messageTone = "confident";
                            break;
                        case double m when (m <= -30):
                            tone = "facewithtearsofjoy";
                            messageTone = "joy";
                            break;
                        default:
                            tone = "cancel";
                            messageTone = "cancel";
                            break;
                    }
                    break;
                case double n when n <= (ySpacer * 2) && n > (ySpacer): 
                    switch (xDifference)
                    {
                        case double m when m <= (xSpacer * 5) && m > (xSpacer * 4):
                            tone = "nauseatedface";
                            messageTone = "disgust";
                            break;
                        case double m when m <= (xSpacer * 4) && m > (xSpacer * 2):
                            tone = "facewithmonocle";
                            messageTone = "critical";
                            break;
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            tone = "flushedface";
                            messageTone = "fear";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            tone = "smilingfacewithhalo";
                            messageTone = "confident";
                            break;
                        case double m when (m <= -30):
                            tone = "beamingfacewithsmilingeyes";
                            messageTone = "joy";
                            break;
                        default:
                            tone = "cancel";
                            messageTone = "cancel";
                            break;
                    }
                    break;
                case double n when (n <= ySpacer):
                    switch (xDifference)
                    {
                        case double m when m <= (40 * 6) && m > (xSpacer * 5):
                            tone = "facewithsymbolsonmouth";
                            messageTone = "anger";
                            break;
                        case double m when m <= (xSpacer * 5) && m > (xSpacer * 4):
                            tone = "facewithsteamfromnose";
                            messageTone = "anger";
                            break;
                        case double m when m <= (xSpacer * 4) && m > (xSpacer * 2):
                            tone = "loudlycryingface";
                            messageTone = "sadness";
                            break;
                        case double m when m <= (xSpacer * 2) && m > ((20 * 2) - 15):
                            tone = "cryingface";
                            messageTone = "sadness";
                            break;
                        case double m when m <= ((20 * 2) - 15) && (m > -30):
                            tone = "facewithrollingeyes";
                            messageTone = "disagreement";
                            break;
                        case double m when (m <= -30):
                            tone = "slightlysmilingface";
                            messageTone = "neutral";
                            break;
                        default:
                            tone = "cancel";
                            messageTone = "cancel";
                            break;
                    }
                    break;
                default:
                    tone = "cancel";
                    messageTone = "cancel";
                    break;
            }

            return tone;

        }

        //message send with tone
        private async System.Threading.Tasks.Task SendMessageAsync(string tone)
        {
            await conversationChild.PostAsync(new Message { Text = chatEntry.Text, Tone = tone, Sender = userID, ToneSrc = currentTone, IsImage = false });

            chatEntry.Text = "";
        }

        //get selected tone
        private string GetMessageColour(string tone)
        {
            string colour = "";

            switch (tone)
            {
                case "neutral":
                    colour = "#FFFFFF";
                    break;
                case "critical":
                    colour = "#1976D2";
                    break;
                case "tired":
                    colour = "#0288D1";
                    break;
                case "joy":
                    colour = "#FBC02D";
                    break;
                case "love":
                    colour = "#C2185B";
                    break;
                case "confident":
                    colour = "#7B1FA2";
                    break;
                case "surprise":
                    colour = "#E64A19";
                    break;
                case "crazyness":
                    colour = "#F57C00";
                    break;
                case "celebration":
                    colour = "#FFA000";
                    break;
                case "anger":
                    colour = "#D32F2F";
                    break;
                case "fear":
                    colour = "#455A64";
                    break;
                case "sadness":
                    colour = "#0D47A1";
                    break;
                case "disagreement":
                    colour = "#5D4037";
                    break;
                case "disgust":
                    colour = "#7CB342";
                    break;
                default:
                    colour = "#FFFFFF";
                    break;
            }

            return colour;

        }

        //get selected tone
        private void SetBackground(string tone)
        {
            BackgroundImage = tone + "bg";
            backgroundImageColour = tone + "bg";
        }


        //set status of conversation
        private async void SetConversationStatus()
        {
            var status = await conversationChildShort.Child("status").PostAsync(new ConversationStatus { PersonID = userID, Live = true });
            userStatusKey = status.Key;
            userStatus = status.Object;
            ShowStatusAsync();
            if (firstActivate)
            {
                CreateSwipeArea();
                firstActivate = false;
            }
        }

        //set status of conversation
        private async void DeleteConversationStatus()
        {
            await conversationChildShort.Child("status").Child(userStatusKey).DeleteAsync();
        }

        //update typing true
        private async void SetTypingTrue()
        {
            userStatus.Typing = true;
            await conversationChildShort.Child("status").Child(userStatusKey).PutAsync(userStatus);
        }

        //update typing false
        private async void SetTypingFalse()
        {
            userStatus.Typing = false;
            await conversationChildShort.Child("status").Child(userStatusKey).PutAsync(userStatus);
        }

        //show status
        private void ShowStatusAsync()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var statusses = await conversationChildShort.Child("status").OrderByKey().OnceAsync<ConversationStatus>();

                personIndicator.Padding = new Thickness(0);
                personIndicator.Margin = new Thickness(0, 10, 0, 0);
                foreach (var status in statusses)
                {
                    if (status.Object.PersonID != userID)
                    {
                        if (status.Object.Live)
                        {
                            personIndicator.Padding = new Thickness(10);
                            personIndicator.Margin = new Thickness(0);
                        } else
                        {
                            personIndicator.Padding = new Thickness(0);
                            personIndicator.Margin = new Thickness(0, 10, 0, 0);
                        }

                        if (status.Object.Typing)
                        {
                            typingBubble.TranslationY = -30;
                            typingBubble.Scale = 0.5;
                            typingBubble.Opacity = 0;

                            typingBubble.IsVisible = true;

                            typingBubble.FadeTo(1, 100).Forget();
                            typingBubble.TranslateTo(0, 0, 200, Easing.SpringOut).Forget();
                            await typingBubble.ScaleTo(1, 200, Easing.CubicOut);
                        }
                        else
                        {
                            typingBubble.ScaleTo(0, 200, Easing.SpringIn).Forget();
                            await typingBubble.TranslateTo(0,-30, 200, Easing.CubicIn);

                            typingBubble.IsVisible = false;
                        }
                    }
                }
            });
        }

        private async void CheckIfAvtiveAsync()
        {
            var statusses = await conversationChildShort.Child("status").OrderByKey().OnceAsync<ConversationStatus>();

            int counter = 0;
            foreach (var status in statusses)
            {
                counter++;
            }

            if(counter < 2)
            {
                await conversationChild.DeleteAsync();
                DeleteChats();
            }

            DeleteConversationStatus();
        }

        //add chat to chats
        private async void AddChats()
        {
            var chats = await firebase.Child("chatting").OrderByKey().OnceAsync<Objects.Chat>();

            foreach (var chat in chats)
            {
                if (chat.Object.Person1 == userID || chat.Object.Person2 == userID)
                {
                    if (chat.Object.Person1 == personID || chat.Object.Person2 == personID)
                    {
                        chatID = chat.Key;
                    }
                }
            }

            Debug.WriteLine(chatID);
            if (chatID == "none")
            {
                var newChat = await firebase.Child("chatting").PostAsync(new Objects.Chat { Person1 = userID, Person2 = personID, ConversationID = conversationID });
                chatID = newChat.Key;
            }
        }

        //delete chat from chats
        private async void DeleteChats()
        {
            Console.WriteLine(chatID);
            await firebase.Child("chatting").Child(chatID).DeleteAsync();
        }

        public async void SetConverstaionToneAsync()
        {
            if (chatID != "none")
            {
                await firebase.Child("chatting").Child(chatID).PutAsync(new Objects.Chat { Person1 = userID, Person2 = personID, ConversationID = conversationID, Tone = messageTone });
            }
        }

        private void DimContent()
        {
            chatFrame.Opacity = 0.5;
            chatBubblesContainer.Opacity = 0.5;
            navBar.Opacity = 0.5;
        }

        private void ShowContent()
        {
            chatFrame.Opacity = 1;
            chatBubblesContainer.Opacity = 1;
            navBar.Opacity = 1;
        }

        //take photo button tapped
        async void TakePhotoClicked(object sender, System.EventArgs e)
        {
            Message photoMessage = new Message {  Tone = messageTone, Sender = userID, IsImage = true };

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Feels",
                SaveToAlbum = true,
                CompressionQuality = 75,
                CustomPhotoSize = 50,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 800,
                DefaultCamera = CameraDevice.Rear
            });

            if (file == null)
                return;

            var stream = File.Open(file.Path, FileMode.Open);

            var task = new FirebaseStorage("feels-d9ab9.appspot.com")
            .Child("chatImages")
            .Child(RandomString(6))
            .PutAsync(stream);

            // Track progress of the upload
            task.Progress.ProgressChanged += (s, a) =>
            {
                uploading.IsRunning = true;
                cameraIcon.IsVisible = false;
            };

            // await the task to wait until upload completes and get the download url
            var downloadUrl = await task;

            photoMessage.Text = downloadUrl;

            pictureFrom.Source = photoMessage.Text;

            await conversationChild.PostAsync(photoMessage);

            uploading.IsRunning = false;
            cameraIcon.IsVisible = true;
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 1; i < size + 1; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
                return builder.ToString();
        }

        //post message to stash
        async void AddToStash(Message message)
        {
            if (message.Sender != "none")
            {
                Console.WriteLine("stash");
                stashButton.Source = "stashopen";
                await stashButton.ScaleTo(1.5, 300, Easing.SpringOut);
                await firebase.Child("conversations").Child(conversationID).Child("stashed").PostAsync(message);
                message.Sender = "none";
                await stashButton.ScaleTo(1, 200, Easing.CubicOut);
                stashButton.Source = "stashbtn";
            } else
            {
                await stashButton.TranslateTo(-15, 0, 100, Easing.BounceIn);
                await stashButton.TranslateTo(0, 0, 200, Easing.BounceOut);
                Console.WriteLine("already stashed");
            }
        }
    }
}
