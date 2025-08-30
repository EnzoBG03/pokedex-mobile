using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AppPokedex
{
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
            InitializeAnimations();
        }

        private async void InitializeAnimations()
        {
            // Animation d'entrée pour le logo
            PokedexLogo.Scale = 0;
            await PokedexLogo.ScaleTo(1, 1000, Easing.BounceOut);

            // Animation de pulsation continue pour le logo
            Device.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await PokedexLogo.ScaleTo(1.1, 500, Easing.CubicInOut);
                    await PokedexLogo.ScaleTo(1, 500, Easing.CubicInOut);
                });
                return true; // Continue le timer
            });
        }

        private async void start_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            // Animation du bouton
            await button.ScaleTo(0.95, 100);
            await button.ScaleTo(1, 100);

            // Effet de feedback visuel
            button.BackgroundColor = Color.FromHex("#28A428");
            await Task.Delay(100);
            button.BackgroundColor = Color.FromHex("#32CD32");

            // Navigation vers la page des choix
            await Navigation.PushAsync(new ChoicesPage());
        }

        private async void OnLogoTapped(object sender, EventArgs e)
        {
            var logo = sender as Grid;

            // Animation de rotation
            await logo.RotateTo(360, 1000, Easing.CubicInOut);
            logo.Rotation = 0;

            // Afficher un message Easter Egg
            await DisplayAlert("🎉 Easter Egg", "Vous avez découvert un secret ! Le Pokédex original a été créé par le Professeur Chen.", "Cool !");
        }

        private async void OnStatsClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.95, 100);
            await button.ScaleTo(1, 100);

            // Afficher les statistiques
            await DisplayAlert("📊 Statistiques",
                "🔹 Pokémon répertoriés : 1026\n" +
                "🔹 Régions explorées : 10\n" +
                "🔹 Types découverts : 18\n" +
                "🔹 Dernière mise à jour : Paldea",
                "Fermer");
        }

        private async void OnDecorativeTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;

            // Animation de couleur aléatoire
            var colors = new[] { "#FF6B35", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFD700", "#FF69B4" };
            var random = new Random();
            var randomColor = colors[random.Next(colors.Length)];

            await frame.ScaleTo(1.2, 100);
            frame.BackgroundColor = Color.FromHex(randomColor);
            await frame.ScaleTo(1, 100);

            // Retour à la couleur originale après 1 seconde
            await Task.Delay(1000);
            frame.BackgroundColor = Color.FromHex("#4169E1");
        }
    }
}
