/*using System;
using System.Collections.Generic;
using System.Globalization;
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

        // Normalise un nom de Pokémon en supprimant les accents et caractères spéciaux
        private string NormalizePokemonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            string normalized = name.Trim().ToLower();

            // Étape 1: Supprimer les accents en utilisant la normalisation Unicode
            normalized = RemoveAccents(normalized);

            // Étape 2: Remplacements spécifiques pour certains caractères
            normalized = normalized
                .Replace("♀", "-f")        // Nidoran♀ → nidoran-f
                .Replace("♂", "-m")        // Nidoran♂ → nidoran-m
                .Replace("é", "e")
                .Replace("è", "e")
                .Replace("ê", "e")
                .Replace("ë", "e")
                .Replace("à", "a")
                .Replace("â", "a")
                .Replace("ä", "a")
                .Replace("ù", "u")
                .Replace("û", "u")
                .Replace("ü", "u")
                .Replace("ô", "o")
                .Replace("ö", "o")
                .Replace("î", "i")
                .Replace("ï", "i")
                .Replace("ç", "c")
                .Replace("ÿ", "y")
                .Replace("ñ", "n")
                .Replace("æ", "ae")
                .Replace("œ", "oe")
                .Replace("'", "-")         // Apostrophes vers tirets
                .Replace(" ", "-")        // Espaces vers tirets
                .Replace(".", "")         // Supprimer les points
                .Replace(",", "");        // Supprimer les virgules

            // Étape 3: Nettoyer les tirets multiples
            while (normalized.Contains("--"))
            {
                normalized = normalized.Replace("--", "-");
            }

            // Étape 4: Supprimer les tirets en début et fin
            normalized = normalized.Trim('-');

            return normalized;
        }

        // Supprime les accents d'une chaîne
        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Décomposer les caractères (séparer les lettres des marques diacritiques)
            string normalized = text.Normalize(NormalizationForm.FormD);

            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                // Ne garder que les caractères qui ne sont pas des marques diacritiques
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            // Recomposer les caractères
            return result.ToString().Normalize(NormalizationForm.FormC);
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
}*/
using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// Normalise un nom de Pokémon en supprimant les accents et caractères spéciaux
        /// Méthode universelle qui fonctionne pour tous les Pokémons
        /// </summary>
        private string NormalizePokemonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            string normalized = name.Trim().ToLower();

            // Étape 1: Supprimer les accents en utilisant la normalisation Unicode
            normalized = RemoveAccents(normalized);

            // Étape 2: Remplacements spécifiques pour certains caractères
            normalized = normalized
                .Replace("♀", "-f")        // Nidoran♀ → nidoran-f
                .Replace("♂", "-m")        // Nidoran♂ → nidoran-m
                .Replace("é", "e")
                .Replace("è", "e")
                .Replace("ê", "e")
                .Replace("ë", "e")
                .Replace("à", "a")
                .Replace("â", "a")
                .Replace("ä", "a")
                .Replace("ù", "u")
                .Replace("û", "u")
                .Replace("ü", "u")
                .Replace("ô", "o")
                .Replace("ö", "o")
                .Replace("î", "i")
                .Replace("ï", "i")
                .Replace("ç", "c")
                .Replace("ÿ", "y")
                .Replace("ñ", "n")
                .Replace("æ", "ae")
                .Replace("œ", "oe")
                .Replace("'", "-")         // Apostrophes vers tirets
                .Replace(" ", "-")        // Espaces vers tirets
                .Replace(".", "")         // Supprimer les points
                .Replace(",", "");        // Supprimer les virgules

            // Étape 3: Nettoyer les tirets multiples
            while (normalized.Contains("--"))
            {
                normalized = normalized.Replace("--", "-");
            }

            // Étape 4: Supprimer les tirets en début et fin
            normalized = normalized.Trim('-');

            return normalized;
        }

        /// <summary>
        /// Supprime tous les accents d'une chaîne en utilisant la normalisation Unicode
        /// Méthode universelle qui fonctionne avec tous les caractères accentués
        /// </summary>
        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Décomposer les caractères (séparer les lettres des marques diacritiques)
            string normalized = text.Normalize(NormalizationForm.FormD);

            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                // Ne garder que les caractères qui ne sont pas des marques diacritiques
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            // Recomposer les caractères
            return result.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Effectue une recherche intelligente avec plusieurs stratégies de fallback
        /// </summary>
        private async Task PerformSearch()
        {
            try
            {
                _isSearching = true;
                await ShowLoadingState();

                if (_httpClient == null)
                {
                    InitializeHttpClient();
                }

                await access.ScaleTo(0.95, 100);
                await access.ScaleTo(1, 100);

                string originalSearchTerm = search.Text.Trim();

                // Stratégie de recherche multiple
                var searchStrategies = GetSearchStrategies(originalSearchTerm);

                Pokemon foundPokemon = null;
                string successfulUrl = null;

                // Essayer chaque stratégie jusqu'à en trouver une qui fonctionne
                foreach (var strategy in searchStrategies)
                {
                    try
                    {
                        string apiUrl = $"https://tyradex.vercel.app/api/v1/pokemon/{Uri.EscapeDataString(strategy)}";
                        System.Diagnostics.Debug.WriteLine($"Tentative avec: '{strategy}' -> URL: {apiUrl}");

                        var response = await _httpClient.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();

                            if (!string.IsNullOrWhiteSpace(jsonResponse) && jsonResponse != "null")
                            {
                                try
                                {
                                    var pokemon = JsonConvert.DeserializeObject<Pokemon>(jsonResponse);

                                    if (pokemon != null && !string.IsNullOrWhiteSpace(pokemon.name?.fr))
                                    {
                                        foundPokemon = pokemon;
                                        successfulUrl = apiUrl;
                                        System.Diagnostics.Debug.WriteLine($"✅ Pokémon trouvé avec la stratégie: '{strategy}'");
                                        break;
                                    }
                                }
                                catch (JsonException ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"❌ Erreur JSON pour '{strategy}': {ex.Message}");
                                    continue;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erreur pour '{strategy}': {ex.Message}");
                        continue;
                    }
                }

                // Si un Pokémon a été trouvé
                if (foundPokemon != null)
                {
                    await HideLoadingState();

                    // Animation de succès
                    access.BackgroundColor = Color.FromHex("#32CD32");
                    access.Text = "✅ POKÉMON TROUVÉ !";
                    await Task.Delay(1000);

                    // Navigation vers la page de détails
                    await Navigation.PushAsync(new PokemonDetailPage(foundPokemon));

                    // Réinitialiser le bouton
                    access.BackgroundColor = Color.FromHex("#FF6B35");
                    access.Text = "🔍 RECHERCHER";
                    return;
                }

                // Aucune stratégie n'a fonctionné
                await ShowNotFoundError(originalSearchTerm, searchStrategies);
            }
            catch (ObjectDisposedException)
            {
                InitializeHttpClient();
                await HideLoadingState();
                await DisplayAlert("🔄 Reconnexion", "Reconnexion en cours, veuillez réessayer.", "OK");
            }
            catch (HttpRequestException httpEx)
            {
                await HideLoadingState();
                System.Diagnostics.Debug.WriteLine($"Erreur HTTP: {httpEx.Message}");
                await DisplayAlert("🌐 Erreur de connexion",
                    "Impossible de se connecter au serveur. Vérifiez votre connexion Internet.", "Réessayer");
                await AnimateErrorState();
            }
            catch (Exception ex)
            {
                await HideLoadingState();
                System.Diagnostics.Debug.WriteLine($"Erreur générale: {ex}");
                await DisplayAlert("❌ Erreur",
                    $"Une erreur inattendue s'est produite : {ex.Message}", "OK");
                await AnimateErrorState();
            }
            finally
            {
                _isSearching = false;
            }
        }

        /// <summary>
        /// Génère plusieurs stratégies de recherche pour maximiser les chances de succès
        /// </summary>
        private List<string> GetSearchStrategies(string originalTerm)
        {
            var strategies = new List<string>();

            if (string.IsNullOrWhiteSpace(originalTerm))
                return strategies;

            // Stratégie 1: Terme original (au cas où il fonctionnerait)
            strategies.Add(originalTerm.Trim());

            // Stratégie 2: Terme normalisé (sans accents)
            string normalized = NormalizePokemonName(originalTerm);
            if (!strategies.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                strategies.Add(normalized);
            }

            // Stratégie 3: En minuscules
            string lowercase = originalTerm.Trim().ToLower();
            if (!strategies.Contains(lowercase, StringComparer.OrdinalIgnoreCase))
            {
                strategies.Add(lowercase);
            }

            // Stratégie 4: Normalized + lowercase
            string normalizedLowercase = NormalizePokemonName(lowercase);
            if (!strategies.Contains(normalizedLowercase, StringComparer.OrdinalIgnoreCase))
            {
                strategies.Add(normalizedLowercase);
            }

            // Stratégie 5: Première lettre en majuscule
            string capitalized = CapitalizeFirst(normalized);
            if (!strategies.Contains(capitalized, StringComparer.OrdinalIgnoreCase))
            {
                strategies.Add(capitalized);
            }

            // Stratégie 6: Gestion des espaces et tirets
            if (originalTerm.Contains(" ") || originalTerm.Contains("-"))
            {
                // Remplacer espaces par tirets
                string withDashes = originalTerm.Replace(" ", "-").Trim().ToLower();
                string withDashesNormalized = NormalizePokemonName(withDashes);
                if (!strategies.Contains(withDashesNormalized, StringComparer.OrdinalIgnoreCase))
                {
                    strategies.Add(withDashesNormalized);
                }

                // Sans espaces ni tirets
                string withoutSpaces = originalTerm.Replace(" ", "").Replace("-", "").Trim().ToLower();
                string withoutSpacesNormalized = NormalizePokemonName(withoutSpaces);
                if (!strategies.Contains(withoutSpacesNormalized, StringComparer.OrdinalIgnoreCase))
                {
                    strategies.Add(withoutSpacesNormalized);
                }
            }

            // Enlever les doublons et les termes vides
            strategies = strategies
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"📋 Stratégies générées pour '{originalTerm}': {string.Join(", ", strategies.Select(s => $"'{s}'"))}");

            return strategies;
        }

        /// <summary>
        /// Met la première lettre en majuscule
        /// </summary>
        private string CapitalizeFirst(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return char.ToUpper(text[0]) + text.Substring(1);
        }

        private async Task ShowNotFoundError(string originalTerm, List<string> triedStrategies)
        {
            await HideLoadingState();

            access.BackgroundColor = Color.FromHex("#FF4444");
            access.Text = "❌ POKÉMON INTROUVABLE";

            string strategiesText = triedStrategies.Count > 0
                ? $"\n\n🔍 Variantes essayées :\n• {string.Join("\n• ", triedStrategies.Take(5))}"
                : "";

            await DisplayAlert("🔍 Pokémon introuvable",
                $"Aucun Pokémon trouvé pour '{originalTerm}'.\n\n" +
                "Le système a automatiquement essayé plusieurs variantes :\n" +
                "• Suppression des accents\n" +
                "• Conversion en minuscules\n" +
                "• Gestion des espaces et tirets\n" +
                "• Normalisation Unicode\n\n" +
                "Conseils :\n" +
                "• Vérifiez l'orthographe de base\n" +
                "• Essayez le nom en français ou anglais\n" +
                "• Utilisez le nom complet du Pokémon" +
                strategiesText, "OK");

            await AnimateSearchField();

            await Task.Delay(2000);
            access.BackgroundColor = Color.FromHex("#FF6B35");
            access.Text = "🔍 RECHERCHER";
        }

        // Le reste du code reste inchangé...
        private async void InitializeAnimations()
        {
            await Task.Delay(100);

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
            if (access.BackgroundColor != Color.FromHex("#FF6B35"))
            {
                access.BackgroundColor = Color.FromHex("#FF6B35");
                access.Text = "🔍 RECHERCHER";
            }
        }

        private async void OnSearchCompleted(object sender, EventArgs e)
        {
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
                "🔹 Recherche intelligente :\n" +
                "• Le système gère automatiquement les accents\n" +
                "• Noms français et anglais acceptés\n" +
                "• Gestion des espaces et tirets automatique\n" +
                "• Plusieurs variantes sont essayées\n\n" +
                "🔹 Exemples qui fonctionnent :\n" +
                "• Salamèche, salameche, SALAMÈCHE\n" +
                "• Nidoran♀, Nidoran-F, nidoran-f\n" +
                "• Ho-Oh, Ho Oh, hooh\n\n" +
                "🔹 Tapez simplement le nom :\n" +
                "• Le système s'occupe du reste\n" +
                "• Plusieurs stratégies sont automatiquement testées\n" +
                "• Appuyez sur Entrée pour rechercher",
                "Compris");
        }

        private async void OnGridItemTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;

            var originalColor = frame.BackgroundColor;
            await frame.ScaleTo(1.2, 100, Easing.CubicOut);
            frame.BackgroundColor = Color.White;
            await frame.ScaleTo(1, 100, Easing.CubicIn);

            await Task.Delay(200);
            frame.BackgroundColor = originalColor;

            var random = new Random();
            if (random.Next(1, 15) == 1)
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

        ~NamePage()
        {
            _httpClient?.Dispose();
        }

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

        private bool IsValidPokemonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'' || c == '♀' || c == '♂' || c == '.');
        }
    }
}