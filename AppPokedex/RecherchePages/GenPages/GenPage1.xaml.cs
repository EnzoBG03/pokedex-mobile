using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPokedex.RecherchePages.GenPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GenPage1 : ContentPage
    {
        private bool isSystemActive = true;

        public GenPage1()
        {
            InitializeComponent();
            InitializeStatusUpdates();
            InitializeAnimations();
        }

        private void InitializeStatusUpdates()
        {
            // Mise à jour périodique du statut
            Device.StartTimer(TimeSpan.FromSeconds(8), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (isSystemActive)
                    {
                        var statuses = new[] { "SCAN DES GÉNÉRATIONS...", "ANALYSE DES RÉGIONS...", "GÉNÉRATIONS" };
                        var colors = new[] { "#FFD700", "#FF7F50", "#4169E1" };

                        var random = new Random();
                        int index = random.Next(statuses.Length);

                        StatusLabel.Text = statuses[index];
                        StatusLabel.TextColor = Color.FromHex(colors[index]);

                        Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                        {
                            StatusLabel.Text = "GÉNÉRATIONS";
                            StatusLabel.TextColor = Color.FromHex("#4169E1");
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
                        await Task.Delay(80);
                    }
                }
            }
        }

        private async void gen1_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(1, "Kanto", "#FF4500", "#001-#151"));
        }

        private async void gen2_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(2, "Johto", "#DAA520", "#152-#251"));
        }

        private async void gen3_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(3, "Hoenn", "#1E90FF", "#252-#386"));
        }

        private async void gen4_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(4, "Sinnoh", "#4B0082", "#387-#493"));
        }

        private async void gen5_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(5, "Unys", "#2F4F4F", "#494-#649"));
        }

        private async void gen6_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(6, "Kalos", "#FF69B4", "#650-#721"));
        }

        private async void gen7_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(7, "Alola", "#FF7F50", "#722-#809"));
        }

        private async void gen8_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(8, "Galar/Hisui", "#8B4513", "#810-#905"));
        }

        private async void gen9_Clicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PushAsync(new GenPage2(9, "Paldea", "#32CD32", "#906-#1025"));
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

            await DisplayAlert("❓ Aide - Générations Pokémon",
                "🔹 Génération I : Les 150 Pokémon originaux de Kanto + Mew\n\n" +
                "🔹 Génération II : Introduction des types Acier et Ténèbres\n\n" +
                "🔹 Génération III : Ajout des capacités spéciales\n\n" +
                "🔹 Génération IV : Division Attaque/Défense Spéciale\n\n" +
                "🔹 Génération V : 156 nouveaux Pokémon (record)\n\n" +
                "🔹 Génération VI : Introduction du type Fée\n\n" +
                "🔹 Génération VII : Formes d'Alola et Capacités Z\n\n" +
                "🔹 Génération VIII : Dynamax et formes de Galar\n\n" +
                "🔹 Génération IX : Monde ouvert à Paldea\n\n" +
                "💡 Chaque génération apporte de nouvelles mécaniques !",
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

            // Easter egg générationnel
            var random = new Random();
            if (random.Next(1, 10) == 1) // 10% de chance
            {
                var generationEggs = new[]
                {
                    "Un Pokémon légendaire vous observe ! ⚡",
                    "Vous trouvez une Master Ball ! 🏆",
                    "Le Professeur Chen vous salue ! 👨‍🔬",
                    "Un Pokémon Chromatique apparaît ! ✨",
                    "Vous découvrez une Pierre d'Évolution ! 💎",
                    "Un Champion de Ligue vous félicite ! 🥇",
                    "La Team Rocket s'envole vers d'autres cieux ! 🚀",
                    "Vous entendez le cri de Pikachu ! 🔊"
                };

                var randomEgg = generationEggs[random.Next(generationEggs.Length)];
                await DisplayAlert("✨ Découverte Légendaire !", randomEgg, "Incroyable !");
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

        // Gestion des couleurs thématiques par génération
        private Color GetGenerationColor(int generation)
        {
            switch (generation)
            {
                case 1: return Color.FromHex("#FF4500");
                case 2: return Color.FromHex("#DAA520");
                case 3: return Color.FromHex("#1E90FF");
                case 4: return Color.FromHex("#4B0082");
                case 5: return Color.FromHex("#2F4F4F");
                case 6: return Color.FromHex("#FF69B4");
                case 7: return Color.FromHex("#FF7F50");
                case 8: return Color.FromHex("#8B4513");
                case 9: return Color.FromHex("#32CD32");
                default: return Color.FromHex("#4169E1");
            }
        }

        // Informations sur les générations
        private string GetGenerationInfo(int generation)
        {
            switch (generation)
            {
                case 1: return "Les Pokémon originaux de Kanto avec Mew";
                case 2: return "Introduction des types Acier et Ténèbres à Johto";
                case 3: return "Les capacités spéciales font leur apparition à Hoenn";
                case 4: return "Division des statistiques spéciales à Sinnoh";
                case 5: return "Le plus grand nombre de nouveaux Pokémon à Unys";
                case 6: return "Introduction du type Fée à Kalos";
                case 7: return "Formes d'Alola et Capacités Z";
                case 8: return "Dynamax et formes de Galar/Hisui";
                case 9: return "Monde ouvert et nouvelles mécaniques à Paldea";
                default: return "Une génération pleine de surprises";
            }
        }

        // Obtenir le nombre de Pokémon par génération
        private int GetPokemonCount(int generation)
        {
            switch (generation)
            {
                case 1: return 151;
                case 2: return 100;
                case 3: return 135;
                case 4: return 107;
                case 5: return 156;
                case 6: return 72;
                case 7: return 81;
                case 8: return 89;
                case 9: return 120;
                default: return 0;
            }
        }

        // Obtenir les jeux principaux de la génération
        private string[] GetMainGames(int generation)
        {
            switch (generation)
            {
                case 1: return new[] { "Rouge", "Bleu", "Jaune" };
                case 2: return new[] { "Or", "Argent", "Cristal" };
                case 3: return new[] { "Rubis", "Saphir", "Émeraude" };
                case 4: return new[] { "Diamant", "Perle", "Platine" };
                case 5: return new[] { "Noir", "Blanc", "Noir 2", "Blanc 2" };
                case 6: return new[] { "X", "Y", "Légendes Z-A (Prochainement)" };
                case 7: return new[] { "Soleil", "Lune", "Ultra-Soleil", "Ultra-Lune" };
                case 8: return new[] { "Épée", "Bouclier", "Légendes Arceus" };
                case 9: return new[] { "Écarlate", "Violet" };
                default: return new string[0];
            }
        }

        // Méthode pour afficher les détails d'une génération
        private async Task ShowGenerationDetails(int generation)
        {
            var regionName = GetRegionName(generation);
            var pokemonCount = GetPokemonCount(generation);
            var mainGames = GetMainGames(generation);
            var info = GetGenerationInfo(generation);

            var gamesText = string.Join(", ", mainGames);

            await DisplayAlert($"🎮 Génération {generation} - {regionName}",
                $"📊 Pokémon : {pokemonCount}\n" +
                $"🎯 Jeux : {gamesText}\n" +
                $"ℹ️ Info : {info}",
                "OK");
        }

        // Obtenir le nom de la région
        private string GetRegionName(int generation)
        {
            switch (generation)
            {
                case 1: return "Kanto";
                case 2: return "Johto";
                case 3: return "Hoenn";
                case 4: return "Sinnoh";
                case 5: return "Unys";
                case 6: return "Kalos";
                case 7: return "Alola";
                case 8: return "Galar/Hisui";
                case 9: return "Paldea";
                default: return "Inconnue";
            }
        }

        // Animation spéciale pour les boutons de génération
        private async Task AnimateGenerationButton(Button button, int generation)
        {
            var color = GetGenerationColor(generation);

            // Animation de sélection
            await button.ScaleTo(0.9, 100);
            button.BackgroundColor = Color.White;
            await Task.Delay(100);
            button.BackgroundColor = color;
            await button.ScaleTo(1.1, 200);
            await button.ScaleTo(1, 100);

            // Effet de brillance
            await button.FadeTo(0.8, 100);
            await button.FadeTo(1, 100);
        }
    }
}