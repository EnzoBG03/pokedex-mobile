using System;
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
            InitializeTypeColors();
            LoadPokemonDetails();
        }

        private void InitializeTypeColors()
        {
            _typeColors = new Dictionary<string, Color>
            {
                { "Normal", Color.FromHex("#A8A878") },
                { "Feu", Color.FromHex("#F08030") },
                { "Eau", Color.FromHex("#6890F0") },
                { "Électrik", Color.FromHex("#F8D030") },
                { "Plante", Color.FromHex("#78C850") },
                { "Glace", Color.FromHex("#98D8D8") },
                { "Combat", Color.FromHex("#C03028") },
                { "Poison", Color.FromHex("#A040A0") },
                { "Sol", Color.FromHex("#E0C068") },
                { "Vol", Color.FromHex("#A890F0") },
                { "Psy", Color.FromHex("#F85888") },
                { "Insecte", Color.FromHex("#A8B820") },
                { "Roche", Color.FromHex("#B8A038") },
                { "Spectre", Color.FromHex("#705898") },
                { "Dragon", Color.FromHex("#7038F8") },
                { "Ténèbres", Color.FromHex("#705848") },
                { "Acier", Color.FromHex("#B8B8D0") },
                { "Fée", Color.FromHex("#EE99AC") }
            };
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
                    var typeFrame = new Frame
                    {
                        BackgroundColor = _typeColors.ContainsKey(type.name) ? _typeColors[type.name] : Color.Gray,
                        CornerRadius = 12,
                        Padding = new Thickness(12, 6),
                        HasShadow = false,
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    var typeLabel = new Label
                    {
                        Text = type.name.ToUpper(),
                        TextColor = Color.White,
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    typeFrame.Content = typeLabel;
                    TypesContainer.Children.Add(typeFrame);
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
}