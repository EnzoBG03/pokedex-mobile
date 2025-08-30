using AppPokedex.RecherchePages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPokedex
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChoicesPage : ContentPage
    {
        private bool isSystemActive = true;

        public ChoicesPage()
        {
            InitializeComponent();
            InitializeStatusUpdates();
        }

        private void InitializeStatusUpdates()
        {
            // Mise à jour périodique du statut
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (isSystemActive)
                    {
                        StatusLabel.Text = "ANALYSE EN COURS...";
                        StatusLabel.TextColor = Color.FromHex("#FFD700");

                        Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                        {
                            StatusLabel.Text = "SYSTÈME OPÉRATIONNEL";
                            StatusLabel.TextColor = Color.FromHex("#32CD32");
                            return false; // Arrêter ce timer
                        });
                    }
                });
                return isSystemActive; // Continue tant que le système est actif
            });
        }

        private async void all_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new AllPage());
        }

        private async void idname_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new NamePage());
        }

        private async void form_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new RecherchePages.FormPages.FormPage1());
        }

        private async void gen_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new RecherchePages.GenPages.GenPage1());
        }

        private async void return_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.9, 100);
            await button.ScaleTo(1, 100);

            isSystemActive = false; // Arrêter les mises à jour de statut
            await Navigation.PopAsync();
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);

            await DisplayAlert("❓ Aide",
                "🔹 Tous les Pokémon : Parcourir tous les Pokémon par ordre du Pokédex National\n\n" +
                "🔹 ID/Nom : Recherche directe par numéro (#001-#1025) ou nom\n\n" +
                "🔹 Formes Régionales : Pokémon avec des variantes spécifiques aux régions\n\n" +
                "🔹 Générations : Pokémon groupés par jeux/régions d'origine\n\n" +
                "💡 Astuce : Touchez les éléments colorés pour des interactions surprises !",
                "Compris");
        }

        private async void OnGridItemTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;

            // Animation avec couleur temporaire
            var originalColor = frame.BackgroundColor;
            await frame.ScaleTo(1.3, 150, Easing.CubicOut);
            frame.BackgroundColor = Color.White;
            await frame.ScaleTo(1, 150, Easing.CubicIn);

            // Retour à la couleur originale
            await Task.Delay(200);
            frame.BackgroundColor = originalColor;

            // Easter egg aléatoire
            var random = new Random();
            if (random.Next(1, 10) == 1) // 10% de chance
            {
                await DisplayAlert("✨ Bonus !", "Vous avez trouvé un objet rare : Potion Max !", "Génial !");
            }
        }

        private async Task AnimateButtonClick(Button button)
        {
            if (button == null) return;

            // Animation de clic avec changement de couleur
            var originalColor = button.BackgroundColor;
            await button.ScaleTo(0.95, 100);
            button.BackgroundColor = Color.FromRgba(originalColor.R, originalColor.G, originalColor.B, 0.8);
            await button.ScaleTo(1, 100);
            button.BackgroundColor = originalColor;
        }

        protected override void OnDisappearing()
        {
            isSystemActive = false;
            base.OnDisappearing();
        }
    }
}