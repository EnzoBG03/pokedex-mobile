using AppPokedex.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPokedex.RecherchePages.FormPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormPage2 : ContentPage
    {
        private ObservableCollection<Pokemon> _regionalPokemon;
        private HttpClient _httpClient;
        private string _region;
        private bool isSystemActive = true;

        public FormPage2(string region)
        {
            InitializeComponent();
            _regionalPokemon = new ObservableCollection<Pokemon>();
            _httpClient = new HttpClient();
            _region = region;

            PokemonCollectionView.ItemsSource = _regionalPokemon;

            SetupRegionalTheme();
            InitializeStatusUpdates();
            LoadPokemonData();
        }

        private void SetupRegionalTheme()
        {
            // Configuration des couleurs et icônes selon la région
            var regionConfig = GetRegionConfiguration(_region);

            // Mettre à jour les éléments visuels
            RegionColorIndicator.Color = regionConfig.PrimaryColor;
            RegionColorCenter.Color = regionConfig.PrimaryColor;
            RegionTitle.Text = $"FORMES DE {_region.ToUpper()}";
            RegionSubtitle.Text = regionConfig.Subtitle;
            RegionIcon.Text = regionConfig.Icon;
            LoadingIndicator.Color = regionConfig.PrimaryColor;
            RetryButton.BackgroundColor = regionConfig.PrimaryColor;

            // Informations sur la région
            RegionInfoIcon.Text = regionConfig.Icon;
            RegionInfoTitle.Text = $"Région de {_region}";
            RegionInfoDescription.Text = regionConfig.Description;
            StatusRegionIcon.Text = regionConfig.Icon;

            // LEDs de statut avec couleur régionale
            StatusLed2.BackgroundColor = regionConfig.PrimaryColor;

            // Icône de panel vide
            EmptyIcon.Text = regionConfig.Icon;
        }

        private RegionConfiguration GetRegionConfiguration(string region)
        {
            switch (region)
            {
                case "Alola":
                    return new RegionConfiguration
                    {
                        PrimaryColor = Color.FromHex("#FF7F50"),
                        Icon = "🏝️",
                        Subtitle = "Paradis tropical",
                        Description = "Formes adaptées au climat tropical et à la culture polynésienne"
                    };
                case "Galar":
                    return new RegionConfiguration
                    {
                        PrimaryColor = Color.FromHex("#4169E1"),
                        Icon = "🏰",
                        Subtitle = "Royaume industriel",
                        Description = "Formes influencées par l'industrialisation et la culture britannique"
                    };
                case "Hisui":
                    return new RegionConfiguration
                    {
                        PrimaryColor = Color.FromHex("#8FBC8F"),
                        Icon = "⛩️",
                        Subtitle = "Terre ancienne",
                        Description = "Formes primitives de l'époque féodale japonaise"
                    };
                case "Paldea":
                    return new RegionConfiguration
                    {
                        PrimaryColor = Color.FromHex("#DAA520"),
                        Icon = "🎓",
                        Subtitle = "Terre d'apprentissage",
                        Description = "Formes diversifiées de la péninsule ibérique"
                    };
                default:
                    return new RegionConfiguration
                    {
                        PrimaryColor = Color.FromHex("#9370DB"),
                        Icon = "🌍",
                        Subtitle = "Région mystérieuse",
                        Description = "Formes uniques et mystérieuses"
                    };
            }
        }

        private void InitializeStatusUpdates()
        {
            // Animation des LEDs de statut
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (isSystemActive)
                    {
                        await StatusLed1.ScaleTo(1.2, 200);
                        await StatusLed1.ScaleTo(1, 200);

                        await Task.Delay(500);

                        await StatusLed2.ScaleTo(1.2, 200);
                        await StatusLed2.ScaleTo(1, 200);
                    }
                });
                return isSystemActive;
            });

            // Mise à jour du statut de scan
            Device.StartTimer(TimeSpan.FromSeconds(8), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (isSystemActive)
                    {
                        var statuses = new[] { "ANALYSE EN COURS...", "SCAN EN COURS...", "TERMINÉ" };
                        var colors = new[] { "#FFD700", "#FF7F50", "#32CD32" };

                        var random = new Random();
                        int index = random.Next(statuses.Length);

                        ScanStatus.Text = statuses[index];
                        ScanStatus.TextColor = Color.FromHex(colors[index]);

                        Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                        {
                            ScanStatus.Text = "TERMINÉ";
                            ScanStatus.TextColor = Color.FromHex("#32CD32");
                            return false;
                        });
                    }
                });
                return isSystemActive;
            });
        }

        private async Task LoadPokemonData()
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                ErrorPanel.IsVisible = false;
                EmptyPanel.IsVisible = false;
                PokemonCollectionView.IsVisible = false;

                // Animation de chargement
                ScanStatus.Text = "ANALYSE...";
                ScanStatus.TextColor = Color.FromHex("#FFD700");

                // Configuration du timeout
                _httpClient.Timeout = TimeSpan.FromSeconds(45);

                // Étape 1: Récupérer la liste de tous les Pokémon pour identifier ceux avec des formes régionales
                string apiUrl = "https://tyradex.vercel.app/api/v1/pokemon";
                string jsonResponse = await _httpClient.GetStringAsync(apiUrl);
                var allPokemon = JsonConvert.DeserializeObject<List<Pokemon>>(jsonResponse);

                if (allPokemon == null || allPokemon.Count == 0)
                {
                    throw new Exception("Aucune donnée Pokémon reçue");
                }

                _regionalPokemon.Clear();

                // Étape 2: Identifier les Pokémon avec des formes dans la région spécifiée
                var pokemonWithRegionalForms = new List<(Pokemon basePokemon, string formeName)>();

                foreach (var pokemon in allPokemon)
                {
                    if (pokemon.formes != null && pokemon.formes.Count > 0)
                    {
                        foreach (var forme in pokemon.formes)
                        {
                            if (string.Equals(forme.region, _region, StringComparison.OrdinalIgnoreCase))
                            {
                                // Obtenir le nom de la forme régionale
                                string regionName = _region.ToLower();
                                pokemonWithRegionalForms.Add((pokemon, regionName));
                                break;
                            }
                        }
                    }
                }

                // Étape 3: Charger les données spécifiques de chaque forme régionale
                await LoadRegionalForms(pokemonWithRegionalForms);

                // Mise à jour de l'interface
                await UpdateUIAfterLoad();

            }
            catch (HttpRequestException httpEx)
            {
                await ShowError("Erreur de connexion", "Impossible de se connecter au serveur. Vérifiez votre connexion Internet.");
                System.Diagnostics.Debug.WriteLine($"Erreur HTTP: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                await ShowError("Erreur de données", "Les données reçues sont incorrectes. Veuillez réessayer.");
                System.Diagnostics.Debug.WriteLine($"Erreur JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                await ShowError("Erreur inattendue", $"Une erreur s'est produite : {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement: {ex}");
            }
        }

        private async Task LoadRegionalForms(List<(Pokemon basePokemon, string formeName)> pokemonWithForms)
        {
            int totalForms = pokemonWithForms.Count;
            int loadedCount = 0;

            foreach (var (basePokemon, formeName) in pokemonWithForms)
            {
                try
                {
                    // Mise à jour du statut de chargement
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScanStatus.Text = $"CHARGEMENT {loadedCount + 1}/{totalForms}";
                        ScanStatus.TextColor = Color.FromHex("#FFD700");
                    });

                    // Construire l'URL pour la forme régionale
                    // Format: https://tyradex.vercel.app/api/v1/pokemon/{nom}/{région}
                    string pokemonName = basePokemon.name?.en?.ToLower().Replace(" ", "") ??
                                        basePokemon.name?.fr?.ToLower().Replace(" ", "");

                    if (string.IsNullOrEmpty(pokemonName))
                    {
                        System.Diagnostics.Debug.WriteLine($"Impossible d'obtenir le nom pour le Pokémon #{basePokemon.pokedex_id}");
                        continue;
                    }

                    string regionalFormUrl = $"https://tyradex.vercel.app/api/v1/pokemon/{pokemonName}/{formeName}";

                    System.Diagnostics.Debug.WriteLine($"Tentative de chargement: {regionalFormUrl}");

                    string regionalFormResponse = await _httpClient.GetStringAsync(regionalFormUrl);
                    var regionalPokemon = JsonConvert.DeserializeObject<Pokemon>(regionalFormResponse);

                    if (regionalPokemon != null)
                    {
                        // S'assurer que les informations de base sont présentes
                        if (regionalPokemon.pokedex_id == 0)
                            regionalPokemon.pokedex_id = basePokemon.pokedex_id;

                        if (regionalPokemon.generation == 0)
                            regionalPokemon.generation = basePokemon.generation;

                        _regionalPokemon.Add(regionalPokemon);
                        loadedCount++;

                        System.Diagnostics.Debug.WriteLine($"Forme régionale chargée: {regionalPokemon.name?.fr} (#{regionalPokemon.pokedex_id})");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur HTTP pour {basePokemon.name?.fr}: {httpEx.Message}");

                    // Essayer avec le nom français si le nom anglais a échoué
                    if (!basePokemon.name?.en?.Equals(basePokemon.name?.fr, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        try
                        {
                            string frenchName = basePokemon.name?.fr?.ToLower().Replace(" ", "");
                            if (!string.IsNullOrEmpty(frenchName))
                            {
                                string alternativeUrl = $"https://tyradex.vercel.app/api/v1/pokemon/{frenchName}/{formeName}";
                                string alternativeResponse = await _httpClient.GetStringAsync(alternativeUrl);
                                var regionalPokemon = JsonConvert.DeserializeObject<Pokemon>(alternativeResponse);

                                if (regionalPokemon != null)
                                {
                                    if (regionalPokemon.pokedex_id == 0)
                                        regionalPokemon.pokedex_id = basePokemon.pokedex_id;

                                    if (regionalPokemon.generation == 0)
                                        regionalPokemon.generation = basePokemon.generation;

                                    _regionalPokemon.Add(regionalPokemon);
                                    loadedCount++;
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            System.Diagnostics.Debug.WriteLine($"Échec également avec le nom français pour {basePokemon.name?.fr}: {ex2.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur générale pour {basePokemon.name?.fr}: {ex.Message}");
                }

                // Petite pause pour éviter de surcharger l'API
                await Task.Delay(50);
            }

            // Trier par numéro du Pokédex
            var sortedRegionalPokemon = _regionalPokemon.OrderBy(p => p.pokedex_id).ToList();
            _regionalPokemon.Clear();
            foreach (var pokemon in sortedRegionalPokemon)
            {
                _regionalPokemon.Add(pokemon);
            }
        }

        private async Task UpdateUIAfterLoad()
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            if (_regionalPokemon.Count > 0)
            {
                CounterLabel.Text = $"{_regionalPokemon.Count} forme{(_regionalPokemon.Count > 1 ? "s" : "")} régionale{(_regionalPokemon.Count > 1 ? "s" : "")} de {_region}";
                StatusCounter.Text = _regionalPokemon.Count.ToString();
                ScanStatus.Text = "TERMINÉ";
                ScanStatus.TextColor = Color.FromHex("#32CD32");

                PokemonCollectionView.IsVisible = true;

                // Animation d'apparition de la liste
                PokemonCollectionView.Opacity = 0;
                await PokemonCollectionView.FadeTo(1, 500);
            }
            else
            {
                CounterLabel.Text = $"Aucune forme régionale trouvée pour {_region}";
                StatusCounter.Text = "0";
                EmptyPanel.IsVisible = true;

                // Personnaliser le message selon la région
                var regionConfig = GetRegionConfiguration(_region);
                EmptyMessage.Text = $"La région de {_region} ne contient pas encore de formes alternatives dans notre base de données.";

                // Animation d'apparition du panel vide
                EmptyPanel.Opacity = 0;
                await EmptyPanel.FadeTo(1, 500);
            }
        }

        private async Task ShowError(string title, string message)
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            ErrorPanel.IsVisible = true;
            PokemonCollectionView.IsVisible = false;
            EmptyPanel.IsVisible = false;

            ScanStatus.Text = "ERREUR";
            ScanStatus.TextColor = Color.FromHex("#FF4444");

            // Animation d'apparition du panel d'erreur
            ErrorPanel.Opacity = 0;
            await ErrorPanel.FadeTo(1, 300);

            // Afficher l'alerte après un court délai
            await Task.Delay(500);
            await DisplayAlert(title, message, "OK");
        }

        private async void OnPokemonSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Pokemon selectedPokemon)
            {
                // Désélectionner immédiatement l'élément
                ((CollectionView)sender).SelectedItem = null;

                // Animation de sélection
                await AnimateSelection();

                try
                {
                    // Navigation vers la page de détails du Pokémon (forme régionale)
                    await Navigation.PushAsync(new PokemonDetailPage(selectedPokemon));
                }
                catch (Exception ex)
                {
                    // Fallback - afficher une alerte avec informations détaillées
                    string regionalInfo = $"Cette forme régionale de {_region} possède des caractéristiques uniques qui la distinguent de sa forme originale.";

                    await DisplayAlert($"{GetRegionConfiguration(_region).Icon} {selectedPokemon.name?.fr} de {_region}",
                        $"Pokémon #{selectedPokemon.pokedex_id:D3}\n\n{regionalInfo}",
                        "OK");
                }
            }
        }

        private async Task AnimateSelection()
        {
            // Animation de pulsation des LEDs
            await Task.WhenAll(
                StatusLed1.ScaleTo(1.3, 100),
                StatusLed2.ScaleTo(1.3, 100)
            );

            await Task.WhenAll(
                StatusLed1.ScaleTo(1, 100),
                StatusLed2.ScaleTo(1, 100)
            );
        }

        private async void OnRetryClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await AnimateButton(button);
            await LoadPokemonData();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await AnimateButton(button);

            isSystemActive = false;
            await Navigation.PopAsync();
        }

        private async Task AnimateButton(Button button)
        {
            if (button == null) return;

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
            _httpClient?.Dispose();
        }

        // Classe pour la configuration régionale
        private class RegionConfiguration
        {
            public Color PrimaryColor { get; set; }
            public string Icon { get; set; }
            public string Subtitle { get; set; }
            public string Description { get; set; }
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

        // Statistiques régionales
        private async void ShowRegionStats()
        {
            if (_regionalPokemon.Count > 0)
            {
                var stats = $"Statistiques de {_region}:\n\n";
                stats += $"• Total des formes: {_regionalPokemon.Count}\n";

                // Grouper par génération si possible
                var genGroups = _regionalPokemon.GroupBy(p => p.generation);
                foreach (var group in genGroups.OrderBy(g => g.Key))
                {
                    if (group.Key > 0)
                        stats += $"• Génération {group.Key}: {group.Count()}\n";
                }

                await DisplayAlert($"{GetRegionConfiguration(_region).Icon} Statistiques", stats, "OK");
            }
        }
    }
}