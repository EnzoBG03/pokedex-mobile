using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AppPokedex.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: ExportRenderer(typeof(ContentPage), typeof(CustomPageRenderer))]
namespace AppPokedex.Droid
{
    public class CustomPageRenderer : PageRenderer
    {
        public CustomPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                // Personnaliser le thème des AlertDialog
                SetCustomAlertTheme();
            }
        }

        private void SetCustomAlertTheme()
        {
            var activity = Context as AppCompatActivity;
            if (activity != null)
            {
                // Appliquer le thème personnalisé
                activity.SetTheme(Resource.Style.CustomAlertTheme);
            }
        }
    }
}