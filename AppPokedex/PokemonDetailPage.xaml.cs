/*using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AppPokedex.Classes;

namespace AppPokedex
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PokemonDetailPage : ContentPage
    {
        private Pokemon _pokemon;
        private Dictionary<string, Color> _typeColors;

        public PokemonDetailPage(Pokemon pokemon)
        {
            InitializeComponent();
            _pokemon = pokemon;
            LoadPokemonDetails();
        }

        private void LoadPokemonDetails()
        {
            // En-tête
            PokemonNameLabel.Text = _pokemon.name.fr.ToUpper();
            PokemonNumberLabel.Text = $"#{_pokemon.pokedex_id:D3}";

            // Image principale
            PokemonImage.Source = _pokemon.sprites?.regular;

            // Informations de base
            CategoryLabel.Text = _pokemon.category ?? "Pokémon";
            HeightLabel.Text = _pokemon.height;
            WeightLabel.Text = _pokemon.weight;

            // Types
            LoadTypes();

            // Statistiques
            LoadStats();

            // Informations supplémentaires
            GenerationLabel.Text = GetGenerationRoman(_pokemon.generation);
            CatchRateLabel.Text = _pokemon.catch_rate + " %" ?? "Inconnu";
            EggGroupsLabel.Text = _pokemon.egg_groups != null ? string.Join(", ", _pokemon.egg_groups) : "Aucun"; ;

            // Résistances
            LoadResistances();
        }

        private void LoadTypes()
        {
            TypesContainer.Children.Clear();

            if (_pokemon.types != null)
            {
                foreach (var type in _pokemon.types)
                {
                    var typeImage = new Image 
                    { 
                        Source = type.image, 
                        WidthRequest = 30, 
                        HeightRequest = 30 
                    };
                    TypesContainer.Children.Add(typeImage);
                }
            }
        }

        private void LoadStats()
        {
            StatsContainer.Children.Clear();

            if (_pokemon.stats != null)
            {
                var stats = new Dictionary<string, int>
                {
                    { "PV", _pokemon.stats.hp },
                    { "ATTAQUE", _pokemon.stats.atk },
                    { "DÉFENSE", _pokemon.stats.def },
                    { "ATK SPÉ", _pokemon.stats.spe_atk },
                    { "DÉF SPÉ", _pokemon.stats.spe_def },
                    { "VITESSE", _pokemon.stats.vit }
                };

                var maxStat = 255; // Valeur maximale théorique pour une statistique

                foreach (var stat in stats)
                {
                    var statContainer = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 10
                    };

                    // Nom de la statistique
                    var statLabel = new Label
                    {
                        Text = stat.Key,
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromHex("#696969"),
                        WidthRequest = 80,
                        VerticalOptions = LayoutOptions.Center
                    };

                    // Valeur de la statistique
                    var valueLabel = new Label
                    {
                        Text = stat.Value.ToString(),
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromHex("#2F4F4F"),
                        WidthRequest = 35,
                        HorizontalTextAlignment = TextAlignment.End,
                        VerticalOptions = LayoutOptions.Center
                    };

                    // Barre de progression
                    var progressFrame = new Frame
                    {
                        BackgroundColor = Color.FromHex("#E0E0E0"),
                        CornerRadius = 8,
                        Padding = 0,
                        HasShadow = false,
                        HeightRequest = 16,
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    };

                    var progressBar = new BoxView
                    {
                        Color = GetStatColor(stat.Value),
                        CornerRadius = 8,
                        HeightRequest = 16,
                        HorizontalOptions = LayoutOptions.Start,
                        WidthRequest = Math.Max(10, (stat.Value * 200) / maxStat) // Largeur proportionnelle
                    };

                    progressFrame.Content = progressBar;

                    statContainer.Children.Add(statLabel);
                    statContainer.Children.Add(valueLabel);
                    statContainer.Children.Add(progressFrame);

                    StatsContainer.Children.Add(statContainer);
                }
            }
        }

        private Color GetStatColor(int statValue)
        {
            if (statValue >= 120) return Color.FromHex("#4CAF50"); // Vert
            if (statValue >= 90) return Color.FromHex("#FF9800");  // Orange
            if (statValue >= 60) return Color.FromHex("#FFC107");  // Jaune
            return Color.FromHex("#F44336"); // Rouge
        }

        private void LoadResistances()
        {
            ResistancesContainer.Children.Clear();

            if (_pokemon.resistances != null && _pokemon.resistances.Count > 0)
            {
                var resistancesGrid = new Grid();
                resistancesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                resistancesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                int row = 0;
                int col = 0;

                foreach (var resistance in _pokemon.resistances)
                {
                    var typeName = resistance.Name;
                    var multiplier = resistance.Multiplier;

                    var resistanceFrame = new Frame
                    {
                        BackgroundColor = GetResistanceColor(multiplier.ToString()),
                        CornerRadius = 8,
                        Padding = new Thickness(8, 4),
                        HasShadow = false,
                        Margin = new Thickness(2)
                    };

                    var resistanceLabel = new Label
                    {
                        Text = $"{typeName} {multiplier}",
                        TextColor = Color.White,
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    resistanceFrame.Content = resistanceLabel;

                    resistancesGrid.Children.Add(resistanceFrame, col, row);

                    col++;
                    if (col > 1)
                    {
                        col = 0;
                        row++;
                        resistancesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }
                }

                ResistancesContainer.Children.Add(resistancesGrid);
            }
            else
            {
                var noResistanceLabel = new Label
                {
                    Text = "Aucune résistance particulière",
                    FontSize = 12,
                    TextColor = Color.FromHex("#696969"),
                    HorizontalOptions = LayoutOptions.Center
                };
                ResistancesContainer.Children.Add(noResistanceLabel);
            }
        }

        private Color GetResistanceColor(string multiplier)
        {
            switch (multiplier)
            {
                case "0": return Color.FromHex("#424242");    // Immunité
                case "0.25": return Color.FromHex("#2E7D32"); // Très résistant
                case "0.5": return Color.FromHex("#388E3C");  // Résistant
                case "2": return Color.FromHex("#F57C00");    // Faible
                case "4": return Color.FromHex("#D32F2F");    // Très faible
                default: return Color.FromHex("#696969");     // Neutre
            }
        }

        private string GetGenerationRoman(int generation)
        {
            switch (generation)
            {
                case 1: return "1 - Kanto";
                case 2: return "2 - Johto";
                case 3: return "3 - Hoenn";
                case 4: return "4 - Sinnoh";
                case 5: return "5 - Unys";
                case 6: return "6 - Kalos";
                case 7: return "7 - Alola";
                case 8: return "8 - Galar";
                case 9: return "9 - Paldea";
                default: return generation.ToString();
            }
        }

        private void OnShinyToggled(object sender, ToggledEventArgs e)
        {
            if (_pokemon.sprites != null)
            {
                if (e.Value && !string.IsNullOrEmpty(_pokemon.sprites.shiny))
                {
                    PokemonImage.Source = _pokemon.sprites.shiny;
                }
                else
                {
                    PokemonImage.Source = _pokemon.sprites.regular;
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Plugin.SimpleAudioPlayer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AppPokedex.Classes;

namespace AppPokedex
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PokemonDetailPage : ContentPage
    {
        // ─── Propriétés bindables pour le dégradé (lues par GradientPageRenderer) ──

        public static readonly BindableProperty GradientStartColorProperty =
            BindableProperty.Create(nameof(GradientStartColor), typeof(Color), typeof(PokemonDetailPage), Color.FromHex("#DC143C"));

        public static readonly BindableProperty GradientEndColorProperty =
            BindableProperty.Create(nameof(GradientEndColor), typeof(Color), typeof(PokemonDetailPage), Color.FromHex("#DC143C"));

        public Color GradientStartColor
        {
            get => (Color)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public Color GradientEndColor
        {
            get => (Color)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        // ─── Couleurs par type (nom FR en minuscules) ────────────────────────────

        private static readonly Dictionary<string, Color> TypeColors = new Dictionary<string, Color>
        {
            { "normal",   Color.FromHex("#A8A878") },
            { "feu",      Color.FromHex("#F08030") },
            { "eau",      Color.FromHex("#6890F0") },
            { "plante",   Color.FromHex("#78C850") },
            { "électrik", Color.FromHex("#F8D030") },
            { "glace",    Color.FromHex("#98D8D8") },
            { "combat",   Color.FromHex("#C03028") },
            { "poison",   Color.FromHex("#A040A0") },
            { "sol",      Color.FromHex("#E0C068") },
            { "vol",      Color.FromHex("#A890F0") },
            { "psy",      Color.FromHex("#F85888") },
            { "insecte",  Color.FromHex("#A8B820") },
            { "roche",    Color.FromHex("#B8A038") },
            { "spectre",  Color.FromHex("#705898") },
            { "dragon",   Color.FromHex("#7038F8") },
            { "ténèbres", Color.FromHex("#705848") },
            { "acier",    Color.FromHex("#B8B8D0") },
            { "fée",      Color.FromHex("#EE99AC") },
        };

        private static readonly HttpClient _httpClient = new HttpClient();
        private const string CryBaseUrl = "https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/{0}.ogg";
        private ISimpleAudioPlayer _audioPlayer;
        private bool _isPlayingCry = false;
        private Pokemon _pokemon;

        public PokemonDetailPage(Pokemon pokemon)
        {
            InitializeComponent();
            _pokemon = pokemon;
            _audioPlayer = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            ApplyTypeTheme();
            LoadPokemonDetails();
        }

        // ─── Thème dynamique ─────────────────────────────────────────────────────

        private void ApplyTypeTheme()
        {
            var types = _pokemon.types;
            if (types == null || types.Count == 0) return;

            Color color1 = GetTypeColor(types[0].name);
            Color color2 = types.Count > 1 ? GetTypeColor(types[1].name) : color1;

            // Ces deux propriétés sont lues par GradientPageRenderer (projet Android)
            GradientStartColor = color1;
            GradientEndColor = color2;

            // Fallback couleur unie si le renderer n'est pas actif
            BackgroundColor = color1;
        }

        private Color GetTypeColor(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return Color.FromHex("#A8A878");
            return TypeColors.TryGetValue(typeName.ToLower().Trim(), out Color c) ? c : Color.FromHex("#A8A878");
        }

        // ─── Lecture du cri ──────────────────────────────────────────────────────

        private async void OnPokemonImageTapped(object sender, EventArgs e)
        {
            if (_isPlayingCry) return;
            _isPlayingCry = true;

            await AnimatePokemonImage();

            try
            {
                string cryUrl = string.Format(CryBaseUrl, _pokemon.pokedex_id);
                byte[] audioBytes = await _httpClient.GetByteArrayAsync(cryUrl);

                using (var stream = new MemoryStream(audioBytes))
                {
                    _audioPlayer.Load(stream);
                    _audioPlayer.Play();
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Cri indisponible",
                    $"Impossible de charger le cri de {_pokemon.name.fr}.\nVérifiez votre connexion.",
                    "OK");
            }
            finally
            {
                _isPlayingCry = false;
            }
        }

        private async Task AnimatePokemonImage()
        {
            await PokemonImageFrame.ScaleTo(0.90, 80, Easing.CubicIn);
            await PokemonImageFrame.ScaleTo(1.05, 100, Easing.CubicOut);
            await PokemonImageFrame.ScaleTo(1.00, 80, Easing.Linear);
        }

        // ─── Chargement des données ───────────────────────────────────────────────

        private void LoadPokemonDetails()
        {
            PokemonNameLabel.Text = _pokemon.name.fr.ToUpper();
            PokemonNumberLabel.Text = $"#{_pokemon.pokedex_id:D3}";
            PokemonImage.Source = _pokemon.sprites?.regular;
            CategoryLabel.Text = _pokemon.category ?? "Pokémon";
            HeightLabel.Text = _pokemon.height;
            WeightLabel.Text = _pokemon.weight;
            LoadTypes();
            LoadStats();
            GenerationLabel.Text = GetGenerationRoman(_pokemon.generation);
            CatchRateLabel.Text = _pokemon.catch_rate + " %" ?? "Inconnu";
            EggGroupsLabel.Text = _pokemon.egg_groups != null
                                     ? string.Join(", ", _pokemon.egg_groups)
                                     : "Aucun";
            LoadResistances();
        }

        private void LoadTypes()
        {
            TypesContainer.Children.Clear();
            if (_pokemon.types == null) return;
            foreach (var type in _pokemon.types)
            {
                TypesContainer.Children.Add(new Image
                {
                    Source = type.image,
                    WidthRequest = 30,
                    HeightRequest = 30
                });
            }
        }

        private void LoadStats()
        {
            StatsContainer.Children.Clear();
            if (_pokemon.stats == null) return;

            var stats = new Dictionary<string, int>
            {
                { "PV",      _pokemon.stats.hp      },
                { "ATTAQUE", _pokemon.stats.atk     },
                { "DÉFENSE", _pokemon.stats.def     },
                { "ATK SPÉ", _pokemon.stats.spe_atk },
                { "DÉF SPÉ", _pokemon.stats.spe_def },
                { "VITESSE", _pokemon.stats.vit     }
            };

            foreach (var stat in stats)
            {
                var row = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 10 };

                row.Children.Add(new Label
                {
                    Text = stat.Key,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromHex("#696969"),
                    WidthRequest = 80,
                    VerticalOptions = LayoutOptions.Center
                });
                row.Children.Add(new Label
                {
                    Text = stat.Value.ToString(),
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromHex("#2F4F4F"),
                    WidthRequest = 35,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalOptions = LayoutOptions.Center
                });
                row.Children.Add(new Frame
                {
                    BackgroundColor = Color.FromHex("#E0E0E0"),
                    CornerRadius = 8,
                    Padding = 0,
                    HasShadow = false,
                    HeightRequest = 16,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Content = new BoxView
                    {
                        Color = GetStatColor(stat.Value),
                        CornerRadius = 8,
                        HeightRequest = 16,
                        HorizontalOptions = LayoutOptions.Start,
                        WidthRequest = Math.Max(10, (stat.Value * 200) / 255)
                    }
                });

                StatsContainer.Children.Add(row);
            }
        }

        private Color GetStatColor(int v)
        {
            if (v >= 120) return Color.FromHex("#4CAF50");
            if (v >= 90) return Color.FromHex("#FF9800");
            if (v >= 60) return Color.FromHex("#FFC107");
            return Color.FromHex("#F44336");
        }

        private void LoadResistances()
        {
            ResistancesContainer.Children.Clear();

            if (_pokemon.resistances == null || _pokemon.resistances.Count == 0)
            {
                ResistancesContainer.Children.Add(new Label
                {
                    Text = "Aucune résistance particulière",
                    FontSize = 12,
                    TextColor = Color.FromHex("#696969"),
                    HorizontalOptions = LayoutOptions.Center
                });
                return;
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            int row = 0, col = 0;
            foreach (var r in _pokemon.resistances)
            {
                grid.Children.Add(new Frame
                {
                    BackgroundColor = GetResistanceColor(r.Multiplier.ToString()),
                    CornerRadius = 8,
                    Padding = new Thickness(8, 4),
                    HasShadow = false,
                    Margin = new Thickness(2),
                    Content = new Label
                    {
                        Text = $"{r.Name} {r.Multiplier}",
                        TextColor = Color.White,
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                }, col, row);

                if (++col > 1)
                {
                    col = 0;
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    row++;
                }
            }
            ResistancesContainer.Children.Add(grid);
        }

        private Color GetResistanceColor(string m)
        {
            switch (m)
            {
                case "0": return Color.FromHex("#424242");
                case "0.25": return Color.FromHex("#2E7D32");
                case "0.5": return Color.FromHex("#388E3C");
                case "2": return Color.FromHex("#F57C00");
                case "4": return Color.FromHex("#D32F2F");
                default: return Color.FromHex("#696969");
            }
        }

        private string GetGenerationRoman(int g)
        {
            switch (g)
            {
                case 1: return "1 - Kanto";
                case 2: return "2 - Johto";
                case 3: return "3 - Hoenn";
                case 4: return "4 - Sinnoh";
                case 5: return "5 - Unys";
                case 6: return "6 - Kalos";
                case 7: return "7 - Alola";
                case 8: return "8 - Galar";
                case 9: return "9 - Paldea";
                default: return g.ToString();
            }
        }

        // ─── Événements UI ───────────────────────────────────────────────────────

        private void OnShinyToggled(object sender, ToggledEventArgs e)
        {
            if (_pokemon.sprites == null) return;
            PokemonImage.Source = (e.Value && !string.IsNullOrEmpty(_pokemon.sprites.shiny))
                ? _pokemon.sprites.shiny
                : _pokemon.sprites.regular;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
