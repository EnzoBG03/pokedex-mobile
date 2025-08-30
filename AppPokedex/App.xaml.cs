using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPokedex
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new StartPage()) { BarBackgroundColor = Color.FromHex("#DC143C") };
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
