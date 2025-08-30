using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AppPokedex.Classes;

namespace AppPokedex.RecherchePages.GenPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GenPage2 : ContentPage
    {
        private ObservableCollection<Pokemon> _allPokemon;
        private ObservableCollection<Pokemon> _filteredPokemon;
        private HttpClient _httpClient;
        private int _generation;
        private string _regionName;
        private string _generationColor;
        private string _pokemonRange;
        private string _activeFilter = "Tous les types";

        public GenPage2(int generation, string regionName, string color, string range)
        {
            InitializeComponent();

            _generation = generation;
            _regionName = regionName;
            _generationColor = color;
            _pokemonRange = range;

            _allPokemon = new ObservableCollection<Pokemon>();
            _filteredPokemon = new ObservableCollection<Pokemon>();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            PokemonCollectionView.ItemsSource = _filteredPokemon;

            InitializeUI();
            LoadPokemonData();
        }

        private void InitializeUI()
        {
            // Configuration de l'interface selon la génération
            TitleLabel.Text = $"GÉNÉRATION {GetRomanNumeral(_generation)} - {_regionName.ToUpper()}";
            RangeLabel.Text = _pokemonRange;
            RegionLabel.Text = _regionName;
            GenLabel.Text = GetRomanNumeral(_generation);
            StatusGenLabel.Text = $"{GetRomanNumeral(_generation)} - {_regionName.ToUpper()}";

            // Couleur thématique
            var color = Color.FromHex(_generationColor);
            MainPokeball.BackgroundColor = color;
            StatusLed1.BackgroundColor = color;

            // Icône par génération
            GenIcon.Text = GetGenerationIcon(_generation);

            // Configuration du picker
            TypePicker.SelectedIndex = 0;
        }

        private async Task LoadPokemonData()
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                ErrorPanel.IsVisible = false;
                NoResultPanel.IsVisible = false;
                PokemonCollectionView.IsVisible = false;

                // Appel à l'API Tyradex pour récupérer tous les Pokémon
                string apiUrl = "https://tyradex.vercel.app/api/v1/pokemon";
                string jsonResponse = await _httpClient.GetStringAsync(apiUrl);

                var pokemonList = JsonConvert.DeserializeObject<List<Pokemon>>(jsonResponse);

                // Filtrer par génération
                var generationPokemon = FilterPokemonByGeneration(pokemonList, _generation);

                // Trier par numéro du Pokédex
                var sortedPokemon = generationPokemon.OrderBy(p => p.pokedex_id).ToList();

                _allPokemon.Clear();
                _filteredPokemon.Clear();

                foreach (var pokemon in sortedPokemon)
                {
                    // Ajouter la propriété Generation pour l'affichage
                    pokemon.generation = _generation;

                    // Calculer HasSecondType pour l'affichage
                    pokemon.HasSecondType = pokemon.types != null && pokemon.types.Count > 1;

                    _allPokemon.Add(pokemon);
                    _filteredPokemon.Add(pokemon);
                }

                CounterLabel.Text = _allPokemon.Count.ToString();

                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                PokemonCollectionView.IsVisible = true;

                // Animation d'apparition
                await AnimateListAppearance();
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                ErrorPanel.IsVisible = true;
                PokemonCollectionView.IsVisible = false;

                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement: {ex.Message}");
            }
        }

        private List<Pokemon> FilterPokemonByGeneration(List<Pokemon> pokemonList, int generation)
        {
            // Plages de numéros Pokédex par génération
            var ranges = GetGenerationRanges();

            if (ranges.ContainsKey(generation))
            {
                var range = ranges[generation];
                return pokemonList.Where(p => p.pokedex_id >= range.min && p.pokedex_id <= range.max).ToList();
            }

            return new List<Pokemon>();
        }

        private Dictionary<int, (int min, int max)> GetGenerationRanges()
        {
            return new Dictionary<int, (int min, int max)>
            {
                { 1, (1, 151) },
                { 2, (152, 251) },
                { 3, (252, 386) },
                { 4, (387, 493) },
                { 5, (494, 649) },
                { 6, (650, 721) },
                { 7, (722, 809) },
                { 8, (810, 905) },
                { 9, (906, 1025) }
            };
        }

        private void OnTypeFilterChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            var selectedType = picker.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedType) || selectedType == "Tous les types")
            {
                _activeFilter = "Tous les types";
                ActiveFilterLabel.Text = "TOUS";
                ApplyFilter();
                return;
            }

            _activeFilter = selectedType;
            ActiveFilterLabel.Text = selectedType.ToUpper();
            ApplyFilter();
        }

        private async void OnClearFilterClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.8, 100);
            await button.ScaleTo(1, 100);

            TypePicker.SelectedIndex = 0;
            _activeFilter = "Tous les types";
            ActiveFilterLabel.Text = "TOUS";
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            _filteredPokemon.Clear();

            IEnumerable<Pokemon> filtered = _allPokemon;

            if (_activeFilter != "Tous les types")
            {
                filtered = _allPokemon.Where(p =>
                    p.types != null &&
                    p.types.Any(t => string.Equals(t.name, _activeFilter, StringComparison.OrdinalIgnoreCase))
                );
            }

            foreach (var pokemon in filtered)
            {
                _filteredPokemon.Add(pokemon);
            }

            // Afficher le panneau "Aucun résultat" si nécessaire
            if (_filteredPokemon.Count == 0 && _activeFilter != "Tous les types")
            {
                NoResultPanel.IsVisible = true;
                NoResultMessage.Text = $"Aucun Pokémon de type {_activeFilter}\ndans la génération {GetRomanNumeral(_generation)}";
                PokemonCollectionView.IsVisible = false;
            }
            else
            {
                NoResultPanel.IsVisible = false;
                PokemonCollectionView.IsVisible = true;
            }

            // Mettre à jour le compteur
            CounterLabel.Text = _filteredPokemon.Count.ToString();
        }

        private async void OnPokemonSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Pokemon selectedPokemon)
            {
                // Désélectionner immédiatement l'élément
                ((CollectionView)sender).SelectedItem = null;

                try
                {
                    // Navigation vers la page de détails du Pokémon
                    await Navigation.PushAsync(new PokemonDetailPage(selectedPokemon));
                }
                catch (Exception ex)
                {
                    // Fallback avec alerte
                    await DisplayAlert("Pokémon sélectionné",
                        $"Vous avez sélectionné {selectedPokemon.name.fr} (#{selectedPokemon.pokedex_id:D3})\n" +
                        $"Génération {GetRomanNumeral(_generation)} - {_regionName}",
                        "OK");
                }
            }
        }

        private async void OnRetryClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.9, 100);
            await button.ScaleTo(1, 100);

            await LoadPokemonData();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.9, 100);
            await button.ScaleTo(1, 100);

            await Navigation.PopAsync();
        }

        private async void OnInfoClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(0.9, 100);
            await button.ScaleTo(1, 100);

            var info = GetGenerationDetailedInfo(_generation);
            await DisplayAlert($"ℹ️ Génération {GetRomanNumeral(_generation)} - {_regionName}", info, "OK");
        }

        private string GetGenerationDetailedInfo(int generation)
        {
            switch (generation)
            {
                case 1:
                    return "🔴 KANTO - Les origines\n" +
                           "• 150 Pokémon + Mew\n" +
                           "• Jeux : Rouge, Bleu, Jaune\n" +
                           "• Légendaires : Artikodin, Électhor, Sulfura, Mewtwo\n" +
                           "• Fabuleux : Mew\n" +
                           "• Première génération avec 15 types";

                case 2:
                    return "🟡 JOHTO - L'expansion\n" +
                           "• 100 nouveaux Pokémon\n" +
                           "• Jeux : Or, Argent, Cristal\n" +
                           "• Nouveaux types : Acier, Ténèbres\n" +
                           "• Introduction des bébés Pokémon\n" +
                           "• Légendaires : Raikou, Entei, Suicune, Lugia, Ho-Oh\n" +
                           "• Fabuleux : Celebi";

                case 3:
                    return "🔵 HOENN - Les capacités\n" +
                           "• 135 nouveaux Pokémon\n" +
                           "• Jeux : Rubis, Saphir, Émeraude\n" +
                           "• Introduction des capacités spéciales\n" +
                           "• Méga-Évolutions (remakes)\n" +
                           "• Légendaires : Regirock, Regice, Registeel, Latias, Latios, Kyogre, Groudon, Rayquaza\n" +
                           "• Fabuleux : Jirachi, Deoxys";

                case 4:
                    return "⚫ SINNOH - L'évolution\n" +
                           "• 107 nouveaux Pokémon\n" +
                           "• Jeux : Diamant, Perle, Platine\n" +
                           "• Division Attaque/Défense Spéciale\n" +
                           "• Évolutions de Pokémon précédents\n" +
                           "• Légendaires : Créhelf, Créfollet, Créfadet, Dialga, Palkia, Heatran, Regigigas, Giratina, Cresselia\n" +
                           "• Fabuleux : Phione, Manaphy, Darkrai, Shaymin, Arceus";

                case 5:
                    return "⚪ UNYS - Le renouveau\n" +
                           "• 156 nouveaux Pokémon (record)\n" +
                           "• Jeux : Noir, Blanc, Noir 2, Blanc 2\n" +
                           "• Aucun ancien Pokémon (Noir/Blanc)\n" +
                           "• Saisons et météo dynamique\n" +
                           "• Légendaires : Cobaltium, Terrakium, Viridium, Boréas, Fulguris, Reshiram, Zekrom, Démétéros, Kyurem\n" +
                           "• Fabuleux : Victini, Keldeo, Meloetta, Genesect";

                case 6:
                    return "🌸 KALOS - La révolution\n" +
                           "• 72 nouveaux Pokémon\n" +
                           "• Jeux : X, Y, Légendes Z-A (Prochainement)\n" +
                           "• Introduction du type Fée\n" +
                           "• Méga-Évolutions\n" +
                           "• Graphismes 3D\n" +
                           "• Légendaires : Xerneas, Yveltal, Zygarde\n" +
                           "• Fabuleux : Diancie, Hoopa, Volcanion";

                case 7:
                    return "🌺 ALOLA - L'exotisme\n" +
                           "• 81 nouveaux Pokémon\n" +
                           "• Jeux : Soleil, Lune, Ultra-Soleil, Ultra-Lune\n" +
                           "• Formes d'Alola\n" +
                           "• Capacités Z\n" +
                           "• Ultra-Chimères : Zéroïd, Mouscoto, Cancrelove, Câblifère, Bamboiselle, Katagami, Engloutyran, Necrozma, Vémini, Mandrillon, Ama-Ama, Pierroteknik\n" +
                           "• Légendaires : Type:0, Silvallié, Tokorico, Tokopiyon, Tokotoro, Tokopisco, Cosmog, Cosmovum, Solgaleo, Lunala\n" +
                           "• Fabuleux : Magearna, Marshadow, Zeraora, Meltan, Melmetal";

                case 8:
                    return "🏰 GALAR/HISUI - L'innovation\n" +
                           "• 89 nouveaux Pokémon\n" +
                           "• Jeux : Épée, Bouclier, Légendes Arceus\n" +
                           "• Dynamax et Gigamax\n" +
                           "• Formes de Galar et d'Hisui\n" +
                           "• Terres Sauvages\n" +
                           "• Légendaires : Zacian, Zamazenta, Éthernatos, Wushours, Shifours, Regieleki, Regidrago, Blizzeval, Spectreval, Sylveroy, Amovénus\n" +
                           "• Fabuleux : Zarude";

                case 9:
                    return "🎓 PALDEA - Le monde ouvert\n" +
                           "• 103+ nouveaux Pokémon\n" +
                           "• Jeux : Écarlate, Violet\n" +
                           "• Monde complètement ouvert\n" +
                           "• Téracristallisation\n" +
                           "• Multi-joueur en ligne\n" +
                           "• Légendaires : Chongjian, Baojian, Dinglu, Yuyu, Koraidon, Miraidon, Félicanis, Fortusimia, Favianos, Ogerpon, Terapagos\n" +
                           "• Fabuleux : Pêchaminus";

                default:
                    return "Informations non disponibles pour cette génération.";
            }
        }

        private string GetRomanNumeral(int number)
        {
            switch (number)
            {
                case 1: return "I";
                case 2: return "II";
                case 3: return "III";
                case 4: return "IV";
                case 5: return "V";
                case 6: return "VI";
                case 7: return "VII";
                case 8: return "VIII";
                case 9: return "IX";
                default: return number.ToString();
            }
        }

        private string GetGenerationIcon(int generation)
        {
            switch (generation)
            {
                case 1: return "🔴";
                case 2: return "🟡";
                case 3: return "🔵";
                case 4: return "⚫";
                case 5: return "⚪";
                case 6: return "🌸";
                case 7: return "🌺";
                case 8: return "🏰";
                case 9: return "🎓";
                default: return "⚡";
            }
        }

        private async Task AnimateListAppearance()
        {
            // Animation d'apparition de la liste
            PokemonCollectionView.Scale = 0.8;
            PokemonCollectionView.Opacity = 0;

            await Task.WhenAll(
                PokemonCollectionView.ScaleTo(1, 400, Easing.SpringOut),
                PokemonCollectionView.FadeTo(1, 300)
            );
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _httpClient?.Dispose();
        }

        // Méthodes utilitaires supplémentaires
        private Color GetTypeColor(string typeName)
        {
            switch (typeName?.ToLower())
            {
                case "normal": return Color.FromHex("#A8A878");
                case "feu": case "fire": return Color.FromHex("#F08030");
                case "eau": case "water": return Color.FromHex("#6890F0");
                case "électrik": case "electric": return Color.FromHex("#F8D030");
                case "plante": case "grass": return Color.FromHex("#78C850");
                case "glace": case "ice": return Color.FromHex("#98D8D8");
                case "combat": case "fighting": return Color.FromHex("#C03028");
                case "poison": return Color.FromHex("#A040A0");
                case "sol": case "ground": return Color.FromHex("#E0C068");
                case "vol": case "flying": return Color.FromHex("#A890F0");
                case "psy": case "psychic": return Color.FromHex("#F85888");
                case "insecte": case "bug": return Color.FromHex("#A8B820");
                case "roche": case "rock": return Color.FromHex("#B8A038");
                case "spectre": case "ghost": return Color.FromHex("#705898");
                case "dragon": return Color.FromHex("#7038F8");
                case "ténèbres": case "dark": return Color.FromHex("#705848");
                case "acier": case "steel": return Color.FromHex("#B8B8D0");
                case "fée": case "fairy": return Color.FromHex("#EE99AC");
                default: return Color.Gray;
            }
        }

        private async Task AnimateFilterChange()
        {
            // Animation lors du changement de filtre
            var currentTranslation = PokemonCollectionView.TranslationX;

            await PokemonCollectionView.TranslateTo(-30, 0, 150, Easing.CubicIn);
            await PokemonCollectionView.FadeTo(0.5, 100);

            // Appliquer le filtre ici

            await PokemonCollectionView.TranslateTo(30, 0, 0);
            await Task.WhenAll(
                PokemonCollectionView.TranslateTo(0, 0, 150, Easing.CubicOut),
                PokemonCollectionView.FadeTo(1, 100)
            );
        }

        private void StartStatusAnimation()
        {
            // Animation des LEDs de statut
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var leds = new[] { StatusLed1, StatusLed2, StatusLed3 };
                    var colors = new[] { "#32CD32", "#FFD700", "#FF6B35", _generationColor };

                    for (int i = 0; i < leds.Length; i++)
                    {
                        var randomColor = colors[new Random().Next(colors.Length)];
                        leds[i].BackgroundColor = Color.FromHex(randomColor);
                        await Task.Delay(200);
                    }
                });

                return true;
            });
        }

        private string GetGenerationDescription(int generation)
        {
            switch (generation)
            {
                case 1: return "Les premiers Pokémon découverts dans la région de Kanto";
                case 2: return "L'expansion vers Johto avec de nouveaux types";
                case 3: return "L'introduction des capacités spéciales à Hoenn";
                case 4: return "L'évolution des mécaniques de combat à Sinnoh";
                case 5: return "Le plus grand nombre de nouveaux Pokémon à Unys";
                case 6: return "La révolution 3D et le type Fée à Kalos";
                case 7: return "Les formes régionales exotiques d'Alola";
                case 8: return "L'innovation Dynamax de Galar et les formes d'Hisui";
                case 9: return "Le monde ouvert révolutionnaire de Paldea";
                default: return "Une génération pleine de découvertes";
            }
        }

        // Gestion des erreurs réseau
        private async Task HandleNetworkError(Exception ex)
        {
            if (ex is HttpRequestException)
            {
                await DisplayAlert("🌐 Erreur de connexion",
                    "Impossible de charger les Pokémon.\nVérifiez votre connexion Internet.",
                    "OK");
            }
            else if (ex is TaskCanceledException)
            {
                await DisplayAlert("⏱️ Délai dépassé",
                    "La requête a pris trop de temps.\nVeuillez réessayer.",
                    "OK");
            }
            else
            {
                await DisplayAlert("❌ Erreur",
                    $"Une erreur s'est produite :\n{ex.Message}",
                    "OK");
            }
        }

        // Méthode pour obtenir les statistiques de la génération
        private async Task ShowGenerationStats()
        {
            var totalPokemon = _allPokemon.Count;
            var filteredCount = _filteredPokemon.Count;
            var types = _allPokemon
                .SelectMany(p => p.types?.Select(t => t.name) ?? new string[0])
                .Distinct()
                .Count();

            await DisplayAlert($"📊 Statistiques - Génération {GetRomanNumeral(_generation)}",
                $"🎯 Pokémon total : {totalPokemon}\n" +
                $"📋 Actuellement affiché : {filteredCount}\n" +
                $"🏷️ Types différents : {types}\n" +
                $"🌍 Région : {_regionName}\n" +
                $"📱 Filtre actif : {_activeFilter}",
                "OK");
        }

        // Animation pour le changement de génération
        private async Task AnimateGenerationTransition()
        {
            var color = Color.FromHex(_generationColor);

            // Animation de la Pokéball principale
            await MainPokeball.ScaleTo(1.2, 200, Easing.CubicOut);
            MainPokeball.BackgroundColor = color;
            await MainPokeball.ScaleTo(1, 200, Easing.CubicIn);

            // Animation du titre
            await TitleLabel.FadeTo(0, 150);
            TitleLabel.Text = $"GÉNÉRATION {GetRomanNumeral(_generation)} - {_regionName.ToUpper()}";
            await TitleLabel.FadeTo(1, 150);
        }
    }
}