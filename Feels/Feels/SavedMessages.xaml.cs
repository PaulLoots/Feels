using System;
using System.Collections.Generic;

using Xamarin.Forms;

using Firebase.Database;
using Firebase.Database.Query;

using Feels.Objects;
using Xamarin.Essentials;

namespace Feels
{
    public partial class SavedMessages : ContentPage
    {
        //firebase
        FirebaseClient firebase = new FirebaseClient("https://feels-d9ab9.firebaseio.com/");
        ChildQuery conversationChild;

        //person
        string personName = "";

        public SavedMessages(Person person, string background)
        {
            InitializeComponent();

            personName = person.Name;

            GetMessagesAsync(person);

            Console.WriteLine(background);

            BackgroundImage = background;
        }

        private async void GetMessagesAsync(Person person)
        {
            conversationChild = firebase.Child("conversations").Child(person.Conversation).Child("stashed");

            var messages = await conversationChild.OnceAsync<Objects.Message>();

            if (messages.Count > 0)
            {
                emptyMessage.IsVisible = false;
                emptyMessage.Text = "the stash is empty";
            } else
            {
                emptyMessage.Text = "The stash is empty. Double tap a message in the chat to stash it.";
            }

            foreach (var message in messages)
            {
                AddMessage(message);
            }
        }

        private void AddMessage(FirebaseObject<Message> messageObject)
        {
            Message message = messageObject.Object;

            var messageContainer = new StackLayout();
            var nameLabel = new Label { Text = personName, FontAttributes = FontAttributes.Bold, TextColor = new Color(0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            var bubbleContainer = new StackLayout { Orientation = StackOrientation.Horizontal };

            messageContainer.Children.Add(nameLabel);
            messageContainer.Children.Add(bubbleContainer);

            if (message.IsImage)
            {
                var bubbleFrame = new Frame { HasShadow = false, CornerRadius = 30, BackgroundColor = Color.Transparent, IsClippedToBounds = true, Padding = new Thickness(0), Margin = new Thickness(45, 5, 45, 15), HorizontalOptions = LayoutOptions.CenterAndExpand };
                var messageImage = new Image { Aspect = Aspect.AspectFill, HorizontalOptions = LayoutOptions.Center, HeightRequest = 250, Source = message.Text};

                bubbleContainer.Children.Add(bubbleFrame);
                bubbleFrame.Content = messageImage;
            }
            else
            {
                var bubbleFrame = new Frame { Padding = new Thickness(10, 10, 30, 10), HasShadow = false, CornerRadius = 30, BackgroundColor = Color.FromHex(GetMessageColour(message.Tone)), Margin = new Thickness(45, 5, 45, 15), HorizontalOptions = LayoutOptions.CenterAndExpand };
                var messagContents = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
                var messageImageContainer = new StackLayout();
                var messageImage = new Image { HeightRequest = 40, WidthRequest = 40, Source = message.ToneSrc };
                var messageTextContainer = new StackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.Center };
                var messageText = new Label { FontSize = 22, TextColor = new Color(0, 0, 0), Text = message.Text };

                bubbleContainer.Children.Add(bubbleFrame);
                bubbleFrame.Content = messagContents;
                messagContents.Children.Add(messageImageContainer);
                messageImageContainer.Children.Add(messageImage);
                messagContents.Children.Add(messageTextContainer);
                messageTextContainer.Children.Add(messageText);
            }

            var trashImage = new Image { Source = "trashclosed", HorizontalOptions = LayoutOptions.End, Margin = new Thickness(0, 0, -29, 5), IsVisible = false };

            //gestures
            var fingerDownGesture = new PanGestureRecognizer();

            bool deleteThreashold = false;

            fingerDownGesture.PanUpdated += async (s, e) => {
                // Handle the pan

                if (e.StatusType == GestureStatus.Started)
                {
                    trashImage.IsVisible = true;
                }
                else if (e.StatusType == GestureStatus.Running)
                {
                    if (e.TotalX < 0)
                    {
                        bubbleContainer.TranslateTo(e.TotalX, 0, 10).Forget();
                    }

                    if (e.TotalX < -120)
                    {
                        trashImage.Source = "trashopened";
                        deleteThreashold = true;
                    } else
                    {
                        trashImage.Source = "trashclosed";
                        deleteThreashold = false;
                    }
                } else
                {
                    if (deleteThreashold)
                    {
                        trashImage.FadeTo(0, 50, Easing.CubicIn).Forget();
                        await bubbleContainer.TranslateTo(-Application.Current.MainPage.Width,0, 300, Easing.CubicIn);
                        Console.WriteLine("message deleted");
                        mainContainer.Children.Remove(messageContainer);
                        await conversationChild.Child(messageObject.Key).DeleteAsync();

                        if (mainContainer.Children.Count < 2)
                        {
                            emptyMessage.IsVisible = true;
                            await Navigation.PopAsync();
                        }
                    }
                    else
                    {
                        bubbleContainer.TranslateTo(0, 0, 200, Easing.SpringOut).Forget();
                        trashImage.Source = "trashclosed";
                        trashImage.IsVisible = false;
                    }
                }
            };
            bubbleContainer.GestureRecognizers.Add(fingerDownGesture);


            bubbleContainer.Scale = 0;
            nameLabel.Opacity = 0;

            Console.WriteLine(message.Sender + "  " + Preferences.Get("userID", "none"));
            //Set sender label
            if (message.Sender == Preferences.Get("userID", "none"))
            {
                nameLabel.Text = "Me";
                Console.WriteLine(nameLabel.Text);
            }

            //add last children
            bubbleContainer.Children.Add(trashImage);
            mainContainer.Children.Add(messageContainer);

            bubbleContainer.ScaleTo(1, 400, Easing.SpringOut);
            nameLabel.FadeTo(1, 400);
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

        void BackTapped(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
