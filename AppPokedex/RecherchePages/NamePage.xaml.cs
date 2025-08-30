using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AppPokedex.Classes;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPokedex.RecherchePages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NamePage : ContentPage
    {
        private HttpClient _httpClient;
        private bool _isSearching = false;

        public NamePage()
        {
            InitializeComponent();
            InitializeHttpClient();
            InitializeAnimations();
        }

        private void InitializeHttpClient()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        private async void InitializeAnimations()
        {
            // Animation d'entrée pour les éléments de l'interface
            await Task.Delay(100);

            // Animer l'apparition des panneaux
            var mainStack = ((ScrollView)Content).Content as StackLayout;
            if (mainStack != null)
            {
                foreach (var child in mainStack.Children)
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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Réinitialiser l'HttpClient si il a été disposé
            if (_httpClient == null)
            {
                InitializeHttpClient();
            }
        }

        private async void access_Clicked(object sender, EventArgs e)
        {
            if (_isSearching || string.IsNullOrWhiteSpace(search.Text))
            {
                if (string.IsNullOrWhiteSpace(search.Text))
                {
                    await DisplayAlert("⚠️ Champ vide", "Veuillez saisir le nom d'un Pokémon avant de rechercher.", "OK");
                    await AnimateSearchField();
                }
                return;
            }

            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            try
            {
                _isSearching = true;
                await ShowLoadingState();

                // Vérifier que l'HttpClient n'est pas null ou disposé
                if (_httpClient == null)
                {
                    InitializeHttpClient();
                }

                // Animation du bouton de recherche
                await access.ScaleTo(0.95, 100);
                await access.ScaleTo(1, 100);

                string searchTerm = search.Text.Trim().ToLower();

                // Appel à l'API Tyradex pour récupérer le Pokémon recherché
                string apiUrl = $"https://tyradex.vercel.app/api/v1/pokemon/{searchTerm}";

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var pokemon = JsonConvert.DeserializeObject<Pokemon>(jsonResponse);

                    if (pokemon != null)
                    {
                        await HideLoadingState();

                        // Animation de succès
                        access.BackgroundColor = Color.FromHex("#32CD32");
                        access.Text = "✅ POKÉMON TROUVÉ !";
                        await Task.Delay(1000);

                        // Navigation vers la page de détails
                        await Navigation.PushAsync(new PokemonDetailPage(pokemon));

                        // Réinitialiser le bouton
                        access.BackgroundColor = Color.FromHex("#FF6B35");
                        access.Text = "🔍 RECHERCHER";
                    }
                    else
                    {
                        await ShowNotFoundError();
                    }
                }
                else
                {
                    await ShowNotFoundError();
                }
            }
            catch (ObjectDisposedException)
            {
                // L'HttpClient a été disposé, le réinitialiser
                InitializeHttpClient();
                await HideLoadingState();
                await DisplayAlert("🔄 Reconnexion", "Reconnexion en cours, veuillez réessayer.", "OK");
            }
            catch (HttpRequestException httpEx)
            {
                await HideLoadingState();
                await DisplayAlert("🌐 Erreur de connexion",
                    "Impossible de se connecter au serveur. Vérifiez votre connexion Internet.", "Réessayer");
                await AnimateErrorState();
            }
            catch (JsonException jsonEx)
            {
                await HideLoadingState();
                await DisplayAlert("⚠️ Erreur de données",
                    "Les données reçues sont incorrectes. Veuillez réessayer.", "OK");
                await AnimateErrorState();
            }
            catch (Exception ex)
            {
                await HideLoadingState();
                await DisplayAlert("❌ Erreur",
                    $"Une erreur inattendue s'est produite : {ex.Message}", "OK");
                await AnimateErrorState();

                System.Diagnostics.Debug.WriteLine($"Erreur de recherche: {ex}");
            }
            finally
            {
                _isSearching = false;
            }
        }

        private async Task ShowNotFoundError()
        {
            await HideLoadingState();

            // Animation d'erreur
            access.BackgroundColor = Color.FromHex("#FF4444");
            access.Text = "❌ POKÉMON INTROUVABLE";

            await DisplayAlert("🔍 Pokémon introuvable",
                $"Aucun Pokémon trouvé pour '{search.Text}'.\n\n" +
                "Suggestions :\n" +
                "• Vérifiez l'orthographe\n" +
                "• Essayez le nom en français ou anglais\n" +
                "• Utilisez le nom complet", "OK");

            await AnimateSearchField();

            // Réinitialiser le bouton après un délai
            await Task.Delay(2000);
            access.BackgroundColor = Color.FromHex("#FF6B35");
            access.Text = "🔍 RECHERCHER";
        }

        private async Task ShowLoadingState()
        {
            LoadingPanel.IsVisible = true;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            access.IsEnabled = false;
            access.BackgroundColor = Color.FromHex("#FFA500");
            access.Text = "🔄 RECHERCHE...";

            await LoadingPanel.FadeTo(1, 300);
        }

        private async Task HideLoadingState()
        {
            await LoadingPanel.FadeTo(0, 300);
            LoadingPanel.IsVisible = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            access.IsEnabled = true;
        }

        private async Task AnimateSearchField()
        {
            var searchFrame = search.Parent as Frame;
            if (searchFrame != null)
            {
                var originalColor = searchFrame.BorderColor;
                searchFrame.BorderColor = Color.Red;

                await searchFrame.ScaleTo(1.05, 100);
                await searchFrame.ScaleTo(1, 100);

                await Task.Delay(1500);
                searchFrame.BorderColor = originalColor;
            }
        }

        private async Task AnimateErrorState()
        {
            await access.ScaleTo(0.9, 100);
            await access.ScaleTo(1, 100);
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            // Réinitialiser l'état du bouton quand l'utilisateur tape
            if (access.BackgroundColor != Color.FromHex("#FF6B35"))
            {
                access.BackgroundColor = Color.FromHex("#FF6B35");
                access.Text = "🔍 RECHERCHER";
            }
        }

        private async void OnSearchCompleted(object sender, EventArgs e)
        {
            // Permettre la recherche en appuyant sur Entrée
            if (!_isSearching && !string.IsNullOrWhiteSpace(search.Text))
            {
                await PerformSearch();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.9, 100);
            await button.ScaleTo(1, 100);

            await Navigation.PopAsync();
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.9, 100);
            await button.ScaleTo(1, 100);

            await DisplayAlert("❓ Aide - Recherche par nom",
                "🔹 Comment rechercher :\n" +
                "• Tapez le nom complet du Pokémon\n" +
                "• Noms français et anglais acceptés\n" +
                "• Respectez l'orthographe exacte\n\n" +
                "🔹 Exemples valides :\n" +
                "• Pikachu, pikachu, PIKACHU\n" +
                "• Dracaufeu ou Charizard\n" +
                "• Mewtwo, Mew, Celebi\n\n" +
                "🔹 Conseils :\n" +
                "• Évitez les abréviations\n" +
                "• Pas d'espaces en début/fin\n" +
                "• Appuyez sur Entrée pour rechercher",
                "Compris");
        }

        private async void OnGridItemTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;

            // Animation avec couleur temporaire
            var originalColor = frame.BackgroundColor;
            await frame.ScaleTo(1.2, 100, Easing.CubicOut);
            frame.BackgroundColor = Color.White;
            await frame.ScaleTo(1, 100, Easing.CubicIn);

            // Retour à la couleur originale
            await Task.Delay(200);
            frame.BackgroundColor = originalColor;

            // Easter egg aléatoire
            var random = new Random();
            if (random.Next(1, 15) == 1) // ~7% de chance
            {
                var easterEggs = new[]
                {
                    "Vous avez trouvé une Poké Ball !",
                    "Un Pokémon sauvage apparaît !",
                    "Vous trouvez une Baie Oran !",
                    "Vous découvrez une Pierre Évolutive !",
                    "Un Pokémon Chromatique vous observe !",
                    "Le Professeur Chen vous félicite !"
                };

                var randomEgg = easterEggs[random.Next(easterEggs.Length)];
                await DisplayAlert("✨ Découverte !", randomEgg, "Super !");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        // Disposer l'HttpClient seulement quand la page est vraiment fermée
        ~NamePage()
        {
            _httpClient?.Dispose();
        }

        // Méthode pour animer l'entrée des éléments de l'interface
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

        // Méthode pour créer un effet de pulsation sur un élément
        private async Task PulseElement(View element)
        {
            await element.ScaleTo(1.05, 200, Easing.CubicInOut);
            await element.ScaleTo(1, 200, Easing.CubicInOut);
        }

        // Gestion des suggestions de recherche
        private string[] GetSearchSuggestions(string input)
        {
            var commonPokemon = new[]
            {
                "Pikachu", "Dracaufeu", "Charizard", "Salamèche", "Charmander",
                "Carapuce", "Squirtle", "Tortank", "Blastoise", "Bulbizarre",
                "Venusaur", "Herbizarre", "Mewtwo", "Mew", "Celebi",
                "Lugia", "Ho-Oh", "Kyogre", "Groudon", "Rayquaza",
                "Dialga", "Palkia", "Giratina", "Arceus", "Lucario"
            };

            if (string.IsNullOrWhiteSpace(input))
                return new string[0];

            return commonPokemon
                .Where(p => p.ToLower().Contains(input.ToLower()))
                .Take(5)
                .ToArray();
        }

        // Validation du nom de Pokémon
        private bool IsValidPokemonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Vérifie que le nom contient uniquement des lettres, des tirets et des espaces
            return name.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'');
        }
    }
}