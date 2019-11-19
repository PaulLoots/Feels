using System;
using System.Collections.Generic;

using Xamarin.Forms;

using Xamarin.Essentials;
using AiForms.Effects;
using System.Diagnostics;

using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;

using Feels.Objects;
using System.Linq;

namespace Feels
{
    public partial class Login : ContentPage
    {
        //firebase
        FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAgFUuyXFGbhGi2Lq2GA0ldOM_Ocd_1Bxs"));
        FirebaseClient firebase = new FirebaseClient("https://feels-d9ab9.firebaseio.com/");

        //passmoji varibles
        private View toneFace;
        private Image selectedPassmoji;
        private String[] passwordItems = new String[4];
        private Int16 thisPasswordArea;

        //tone
        string passmojiTone = "none";

        //finger value
        double initialX;
        double initialY;
        double initialToneX;
        double initialToneY;

        public Login()
        {
            InitializeComponent();
            ActivatePassmojiBar();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        void SwitchToLogin(object sender, System.EventArgs e)
        {
            //hide any errors
            HideErrors();

            loginLabel.TextColor = Color.FromHex("1976D2");
            signupLabel.TextColor = Color.FromHex("4F4F4F");

            loginButton.IsVisible = true;
            signUpButton.IsVisible = false;

            nickNameSection.IsVisible = false;
        }

        void SwitchToSignup(object sender, System.EventArgs e)
        {                //hide any errors
            HideErrors();

            loginLabel.TextColor = Color.FromHex("4F4F4F");
            signupLabel.TextColor = Color.FromHex("7CB342");

            loginButton.IsVisible = false;
            signUpButton.IsVisible = true;

            nickNameSection.IsVisible = true;
        }

        //make input bar
        private void ActivatePassmojiBar()
        {
            Array.Clear(passwordItems, 0, 3);

            //tone face
            var toneface = new Image { Source = "seenoevilmonkey" };
            toneFace = toneface;

            //touch recogniser
            var recognizer = AddTouch.GetRecognizer(passmojiArea);

            recognizer.TouchBegin += (sender, e) => {

                //hide any errors
                HideErrors();

                //hide button
                signUpButton.Opacity = 0;
                loginButton.Opacity = 0;

                //set initial touch position
                initialX = e.Location.X;
                initialY = e.Location.Y;

                thisPasswordArea = 1;

                //lcheck which passmoji is being touched
                if (initialX < passmojiconatiner1.X + 60)
                {
                    thisPasswordArea = 0;
                    selectedPassmoji = passmojiicon1;
                }
                else if (initialX > passmojiconatiner2.X - 30 && initialX < passmojiconatiner2.X + 70)
                {
                    thisPasswordArea = 1;
                    selectedPassmoji = passmojiicon2;
                }
                else if (initialX > passmojiconatiner3.X - 30 && initialX < passmojiconatiner3.X + 70)
                {
                    thisPasswordArea = 2;
                    selectedPassmoji = passmojiicon3;
                }
                else if (initialX > passmojiconatiner4.X - 30 && initialX < passmojiconatiner4.X + 120)
                {
                    thisPasswordArea = 3;
                    selectedPassmoji = passmojiicon4;
                } else {
                    selectedPassmoji = passmojiicon1;
                }

                initialToneX = selectedPassmoji.X;
                initialToneY = selectedPassmoji.Y;

                ////create emoji
                passmojiArea.Children.Add(toneFace);

                //prep content area
                toneFace.ScaleTo(0.1, 0);
                DimContent();

                selectedPassmoji.Opacity = 1;

                ////show picker grid and emoji
                selectedPassmoji.Source = "passmojigrid";
                selectedPassmoji.TranslateTo(initialToneX - 5, initialToneY - 120, 300, Easing.SpringOut);
                selectedPassmoji.ScaleTo(1.2, 300, Easing.SpringOut);
                toneFace.TranslateTo(initialX - 300, initialY - 120, 0);
                toneFace.ScaleTo(2, 400, Easing.SpringOut);
            };

            recognizer.TouchMove += (sender, e) => {
                toneFace.TranslateTo(e.Location.X - 300, e.Location.Y - 120, 20, Easing.SpringOut);
                var currentTone = GetCurrentTone(e.Location.Y);
                toneface.Source = currentTone;
            };

            recognizer.TouchEnd += async (sender, e) => {
                await toneFace.ScaleTo(0.1, 0);
                await selectedPassmoji.TranslateTo(initialToneX - 7, initialToneY - 22, 20, Easing.SpringOut);
                ShowContent();
                passmojiArea.Children.Remove(toneFace);

                signUpButton.Opacity = 1;
                loginButton.Opacity = 1;

                if (passmojiTone != "cancel")
                {
                    passwordItems[thisPasswordArea] = passmojiTone.ToUpper().Substring(0, 2) + passmojiTone + thisPasswordArea.ToString() + passmojiTone.Length;
                    Console.WriteLine(passwordItems[thisPasswordArea]);

                    selectedPassmoji.Source = passmojiTone;
                    await selectedPassmoji.ScaleTo(1, 200, Easing.SpringOut);
                } else
                {
                    selectedPassmoji.Source = "seenoevilmonkey";
                    await selectedPassmoji.ScaleTo(1, 200, Easing.SpringOut);
                }


            };
        }

        //get selected tone
        private string GetCurrentTone(double Y)
        {
            int ySpacer = 40;
            double yDifference = initialY - Y;
            string tone = "";

            switch (yDifference)
            {
                case double n when n <= (ySpacer * 4) && n > (ySpacer * 3):
                    tone = "coldface";
                    passmojiTone = "coldface";
                    break;
                case double n when n <= (ySpacer * 3) && n > (ySpacer * 2):
                    tone = "nauseatedface";
                    passmojiTone = "nauseatedface";
                    break;
                case double n when n <= (ySpacer * 2) && n > (ySpacer):
                    tone = "zippermouthface";
                    passmojiTone = "zippermouthface";
                    break;
                case double n when (n <= ySpacer) && n > -(ySpacer):
                    tone = "seenoevilmonkey";
                    passmojiTone = "seenoevilmonkey";
                    break;
                case double n when n <= -(ySpacer) && n > -(ySpacer * 2 + 10):
                    tone = "smilingfacewithhorns";
                    passmojiTone = "smilingfacewithhorns";
                    break;
                case double n when n <= -(ySpacer * 2 + 10) && n > -(ySpacer * 3 + 20):
                    tone = "hotface";
                    passmojiTone = "hotface";
                    break;
                case double n when n <= -(ySpacer * 3 + 20) && n > -(ySpacer * 4 + 30):
                    tone = "pileofpoo";
                    passmojiTone = "pileofpoo";
                    break;
                default:
                    tone = "cancel";
                    passmojiTone = "cancel";
                    break;
            }

            return tone;

        }

        //hander for signup
        async void HandleSignUpAsync(object sender, System.EventArgs e)
        {
            //hide any errors
            HideErrors();

            //show loading indicator
            loading.IsRunning = true;
            loading.IsVisible = true;

            string passwordString = passwordItems[0] + "-" + passwordItems[1] + "-" + passwordItems[2] + "-" + passwordItems[3];
            string scrambledPassword = ScramblePassword(passwordString);

            await authProvider.CreateUserWithEmailAndPasswordAsync(emailField.Text, scrambledPassword, nickNameField.Text, false).ContinueWith( async auth => {
                if (auth.IsCanceled)
                {
                    loading.IsVisible = false;
                    errorIcon.IsVisible = true;
                    errorCanceledLabel.IsVisible = true; 
                    Debug.Write("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (auth.IsFaulted)
                {
                    loading.IsVisible = false;
                    errorIcon.IsVisible = true;
                    errorEmailLabel.IsVisible = true;
                    Debug.Write("CreateUserWithEmailAndPasswordAsync encountered an error: " + auth.Exception.GetBaseException());
                    return;
                }

                HideErrors();

                var newUser = await firebase.Child("users").PostAsync(new Objects.User { ID = auth.Result.User.LocalId, Name = auth.Result.User.DisplayName, Image = "profileimagelight", Tone = "neutral", ToneSrc = "slightlysmilingface" });

                Preferences.Set("userID", newUser.Key);
                Preferences.Set("loggedIn", true);
                Console.WriteLine(newUser.Object.Name);

                PopNavAsync();
            });
        }

        //handler for login
        async void HandleLoginAsync(object sender, System.EventArgs e)
        {
            //hide any errors
            HideErrors();

            //show loading indicator
            loading.IsRunning = true;
            loading.IsVisible = true;

            string passwordString = passwordItems[0] + "-" + passwordItems[1] + "-" + passwordItems[2] + "-" + passwordItems[3];
            string scrambledPassword = ScramblePassword(passwordString);

            Console.WriteLine(scrambledPassword);

            await authProvider.SignInWithEmailAndPasswordAsync(emailField.Text, scrambledPassword).ContinueWith(async auth => {
                if (auth.IsCanceled)
                {
                    loading.IsVisible = false;
                    errorIcon.IsVisible = true;
                    errorCanceledLabel.IsVisible = true;
                    Debug.Write("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (auth.IsFaulted)
                {
                    loading.IsVisible = false;
                    errorIcon.IsVisible = true;
                    errorLabel.IsVisible = true;
                    Debug.Write("CreateUserWithEmailAndPasswordAsync encountered an error: " + auth.Exception.GetBaseException());
                    return;
                }

                HideErrors();

                var users = await firebase.Child("users").OnceAsync<Objects.User>();

                foreach (var user in users)
                {
                    if (user.Object.ID == auth.Result.User.LocalId)
                    {
                        Preferences.Set("userID", user.Key);
                        Preferences.Set("loggedIn", true);
                        PopNavAsync();
                    }
                }
            });
        }

        void PopNavAsync()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                //hide loading indicator
                loading.IsRunning = false;
                loading.IsVisible = false;

                await Navigation.PopModalAsync();
            });
        }

        private string ScramblePassword(string input)
        {
            char[] chars = input.ToArray();
            for (int i = 0; i < chars.Length; i++)
            {
                switch (chars[i])
                {
                    case 'o':
                        Array.Resize(ref chars, chars.Length + 1);
                        chars[chars.GetUpperBound(0)] = '$';
                        break;
                    case 'p':
                        Array.Resize(ref chars, chars.Length + 1);
                        chars[chars.GetUpperBound(0)] = '*';
                        break;
                    case 'd':
                        Array.Resize(ref chars, chars.Length + 1);
                        chars[chars.GetUpperBound(0)] = '#';
                        break;
                    case 's':
                        Array.Resize(ref chars, chars.Length + 1);
                        chars[chars.GetUpperBound(0)] = '~';
                        break;
                    case 'u':
                        Array.Resize(ref chars, chars.Length + 1);
                        chars[chars.GetUpperBound(0)] = '%';
                        break;
                }
            }

            Random r = new Random(chars.Length);
            for (int j = 0; j < chars.Length; j++)
            {
                int randomIndex = r.Next(0, chars.Length);
                char temp = chars[randomIndex];
                chars[randomIndex] = chars[j];
                chars[j] = temp;
            }
            string scrambled = new string(chars);

            return scrambled;
        }

        void HideErrors()
        {
            errorLabel.IsVisible = false;
            errorCanceledLabel.IsVisible = false;
            errorEmailLabel.IsVisible = false;
            errorIcon.IsVisible = false;
        }

        private void DimContent()
        {
            logoImg.Opacity = 0.5;
            headingBtns.Opacity = 0.5;
            emailSection.Opacity = 0.5;
            passmojiLabels.Opacity = 0.5;
            passmojiicon1.Opacity = 0.5;
            passmojiicon2.Opacity = 0.5;
            passmojiicon3.Opacity = 0.5;
            passmojiicon4.Opacity = 0.5;
            passmojiFrame.BackgroundColor = Color.FromHex("33333333");
        }

        private void ShowContent()
        {
            logoImg.Opacity = 1;
            headingBtns.Opacity = 1;
            emailSection.Opacity = 1;
            passmojiLabels.Opacity = 1;
            passmojiicon1.Opacity = 1;
            passmojiicon2.Opacity = 1;
            passmojiicon3.Opacity = 1;
            passmojiicon4.Opacity = 1;
            passmojiFrame.BackgroundColor = Color.FromHex("333333");
        }
    }
}
