using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Feels
{
    public partial class Info : ContentPage
    {
        public Info()
        {
            InitializeComponent();
        }

        //go back
        async void HandleBackClicked(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
