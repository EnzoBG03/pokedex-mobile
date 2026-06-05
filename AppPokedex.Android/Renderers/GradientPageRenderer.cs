using Android.Content;
using Android.Graphics.Drawables;
using AppPokedex;
using AppPokedex.Android.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(PokemonDetailPage), typeof(GradientPageRenderer))]

namespace AppPokedex.Android.Renderers
{
    public class GradientPageRenderer : PageRenderer
    {
        public GradientPageRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement is PokemonDetailPage page)
            {
                ApplyGradient(page);

                // Ré-applique le dégradé si les couleurs changent dynamiquement
                page.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == PokemonDetailPage.GradientStartColorProperty.PropertyName ||
                        args.PropertyName == PokemonDetailPage.GradientEndColorProperty.PropertyName)
                    {
                        ApplyGradient(page);
                    }
                };
            }
        }

        private void ApplyGradient(PokemonDetailPage page)
        {
            var startColor = page.GradientStartColor.ToAndroid();
            var endColor   = page.GradientEndColor.ToAndroid();

            var gradient = new GradientDrawable(
                GradientDrawable.Orientation.TopBottom,
                new int[] { startColor, endColor }
            );

            Background = gradient;
        }
    }
}
