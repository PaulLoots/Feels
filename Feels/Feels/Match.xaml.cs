using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;

using Firebase.Database;
using Firebase.Database.Query;

using Feels.Objects;

namespace Feels
{
    public partial class Match : ContentPage
    {
        //firebase
        FirebaseClient firebase = new FirebaseClient("https://feels-d9ab9.firebaseio.com/");
        User person;

        string bigKey;
        string lastPerson;

        public Match(String personkey)
        {
            InitializeComponent();

            GetUser(personkey);
        }

        async void GetUser(String key)
        {
            var users = await firebase.Child("users").OnceAsync<User>();

            foreach (var user in users)
            {
                if (user.Key == key)
                {
                    person = user.Object;
                    lastPerson = key;
                }  else
                {
                    lastPerson = Preferences.Get("userID", key);
                }
            }

            bigKey = key;

            personImage.Source = person.Image;
            nameLabel.Text = "Do you want to chat to " + person.Name + "?";
        }

        async void YesClickedAsync(object sender, System.EventArgs e)
        {
            if (lastPerson == Preferences.Get("userID", "none"))
            {
                await firebase.Child("conversations").PostAsync(new Conversation { Person1 = Preferences.Get("userID", "none"), Person2 = bigKey, Tone = "Neutral" });
            }

            Preferences.Set("newUser", true);

            await Navigation.PopModalAsync();
        }

        void NoClicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}
