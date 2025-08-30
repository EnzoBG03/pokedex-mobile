using AppPokedex.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPokedex.RecherchePages.FormPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormPage1 : ContentPage
    {
        private bool isSystemActive = true;

        public FormPage1()
        {
            InitializeComponent();
            InitializeStatusUpdates();
            InitializeAnimations();
        }

        private void InitializeStatusUpdates()
        {
            // Mise à jour périodique du statut
            Device.StartTimer(TimeSpan.FromSeconds(7), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (isSystemActive)
                    {
                        var statuses = new[] { "ANALYSE DES RÉGIONS...", "SCAN DES FORMES...", "FORMES RÉGIONALES" };
                        var colors = new[] { "#FFD700", "#FF7F50", "#9370DB" };

                        var random = new Random();
                        int index = random.Next(statuses.Length);

                        StatusLabel.Text = statuses[index];
                        StatusLabel.TextColor = Color.FromHex(colors[index]);

                        Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                        {
                            StatusLabel.Text = "FORMES RÉGIONALES";
                            StatusLabel.TextColor = Color.FromHex("#9370DB");
                            return false;
                        });
                    }
                });
                return isSystemActive;
            });
        }

        private async void InitializeAnimations()
        {
            // Animation d'entrée pour les éléments
            await Task.Delay(100);

            var mainStack = ((ScrollView)Content).Content as Grid;
            if (mainStack?.Children[1] is StackLayout stackLayout)
            {
                foreach (var child in stackLayout.Children)
                {
                    if (child is Frame frame)
                    {
                        frame.Scale = 0.8;
                        frame.Opacity = 0;
                        await frame.ScaleTo(1, 300, Easing.CubicOut);
                        await frame.FadeTo(1, 200);
                        await Task.Delay(100);
                    }
                }
            }
        }

        private async void alola_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new FormPage2("Alola"));
        }

        private async void galar_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new FormPage2("Galar"));
        }

        private async void hisui_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new FormPage2("Hisui"));
        }

        private async void paldea_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new FormPage2("Paldea"));
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.9, 100);
            await button.ScaleTo(1, 100);

            isSystemActive = false;
            await Navigation.PopAsync();
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);

            await DisplayAlert("❓ Aide - Formes Régionales",
                "🔹 Formes d'Alola : Pokémon adaptés au climat tropical avec de nouveaux types\n\n" +
                "🔹 Formes de Galar : Pokémon influencés par l'industrialisation et la culture britannique\n\n" +
                "🔹 Formes d'Hisui : Pokémon anciens de l'époque féodale du Japon\n\n" +
                "🔹 Formes de Paldea : Pokémon de la région inspirée de la péninsule ibérique\n\n" +
                "💡 Chaque région offre des variations uniques de Pokémon connus !",
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

            // Easter egg régional
            var random = new Random();
            if (random.Next(1, 12) == 1) // ~8% de chance
            {
                var regionalEggs = new[]
                {
                    "Un Pokémon d'Alola vous salue ! 🏝️",
                    "Une forme de Galar apparaît ! 🏰",
                    "Un esprit d'Hisui vous observe ! ⛩️",
                    "Un Pokémon de Paldea vous sourit ! 🎓",
                    "Un Professeur régional vous félicite ! 👨‍🔬"
                };

                var randomEgg = regionalEggs[random.Next(regionalEggs.Length)];
                await DisplayAlert("✨ Découverte Régionale !", randomEgg, "Fantastique !");
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

            // Effet de pulsation
            await button.ScaleTo(1.05, 50);
            await button.ScaleTo(1, 50);
        }

        protected override void OnDisappearing()
        {
            isSystemActive = false;
            base.OnDisappearing();
        }

        // Méthodes d'animation supplémentaires
        private async Task AnimateElementEntry(View element, int delay = 0)
        {
            if (delay > 0)
                await Task.Delay(delay);

            element.Scale = 0.8;
            element.Opacity = 0;

            await Task.WhenAll(
                element.ScaleTo(1, 400, Easing.SpringOut),
                element.FadeTo(1, 300)
            );
        }

        private async Task PulseElement(View element)
        {
            await element.ScaleTo(1.05, 200, Easing.CubicInOut);
            await element.ScaleTo(1, 200, Easing.CubicInOut);
        }

        // Gestion des couleurs thématiques par région
        private Color GetRegionColor(string region)
        {
            switch (region)
            {
                case "Alola":
                    return Color.FromHex("#FF7F50");
                case "Galar":
                    return Color.FromHex("#4169E1");
                case "Hisui":
                    return Color.FromHex("#8FBC8F");
                case "Paldea":
                    return Color.FromHex("#DAA520");
                default:
                    return Color.FromHex("#9370DB");
            }
        }

        // Informations sur les régions
        private string GetRegionInfo(string region)
        {
            switch (region)
            {
                case "Alola":
                    return "Région tropicale avec des formes adaptées au climat chaud";
                case "Galar":
                    return "Région industrielle influencée par la culture britannique";
                case "Hisui":
                    return "Région ancienne avec des formes primitives de Pokémon";
                case "Paldea":
                    return "Région diversifiée inspirée de la péninsule ibérique";
                default:
                    return "Région avec des formes uniques";
            }
        }
    }
}