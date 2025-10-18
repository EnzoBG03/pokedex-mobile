using AppPokedex.Classes;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPokedex.RecherchePages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DailyPage : ContentPage
    {
        private Pokemon _currentPokemon;
        private HttpClient _httpClient;

        public DailyPage()
        {
            InitializeComponent();
            InitializeHttpClient();
            LoadPokemonDuJour();
        }

        private void InitializeHttpClient()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        private async void LoadPokemonDuJour()
        {
            try
            {
                // Afficher la date actuelle
                DateLabel.Text = DateTime.Now.ToString("dddd d MMMM yyyy",
                    System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));

                // Calculer l'ID du Pokémon basé sur la date
                int pokemonId = GetDailyPokemonId();

                // Charger le Pokémon depuis l'API
                await LoadPokemon(pokemonId);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur",
                    "Impossible de charger le Pokémon du jour : " + ex.Message,
                    "OK");
            }
        }

        private int GetDailyPokemonId()
        {
            // Utiliser la date comme seed pour garantir le même Pokémon chaque jour
            DateTime today = DateTime.Today;
            int seed = today.Year * 10000 + today.Month * 100 + today.Day;
            Random random = new Random(seed);

            // Générer un ID entre 1 et 1025
            return random.Next(1, 1026);
        }

        private async Task LoadPokemon(int pokemonId)
        {
            try
            {
                if (_httpClient == null)
                {
                    InitializeHttpClient();
                }

                string apiUrl = $"https://tyradex.vercel.app/api/v1/pokemon/{pokemonId}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    _currentPokemon = JsonConvert.DeserializeObject<Pokemon>(jsonResponse);

                    await DisplayPokemon();
                }
                else
                {
                    throw new Exception("Pokémon introuvable");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur",
                    "Erreur lors du chargement : " + ex.Message,
                    "Réessayer",
                    "Annuler");
            }
        }

        private async Task DisplayPokemon()
        {
            if (_currentPokemon == null) return;

            try
            {
                // Masquer l'indicateur de chargement
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;

                // Remplir les informations du Pokémon
                PokemonNumber.Text = $"#{_currentPokemon.pokedex_id:D3}";
                PokemonName.Text = _currentPokemon.name?.fr ?? _currentPokemon.name?.en ?? "Pokémon Mystérieux";

                // Charger l'image
                if (!string.IsNullOrEmpty(_currentPokemon.sprites?.regular))
                {
                    PokemonImage.Source = _currentPokemon.sprites.regular;
                }

                // Afficher les types
                DisplayTypes();

                // Afficher une description basée sur les catégories ou générer une description
                string description = GetPokemonDescription();
                PokemonDescription.Text = description;

                // Animation d'apparition
                await AnimateContent();

                // Afficher le contenu et les boutons
                PokemonContent.IsVisible = true;
                ActionButtons.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur d'affichage", ex.Message, "OK");
            }
        }

        private void DisplayTypes()
        {
            TypesContainer.Children.Clear();

            if (_currentPokemon.types != null)
            {
                foreach (var type in _currentPokemon.types)
                {
                    var typeFrame = CreateTypeImage(type);
                    TypesContainer.Children.Add(typeFrame);
                }
            }
        }

        private Image CreateTypeImage(Classes.PokemonType type)
        {
            var typeImage = new Image
            {
                Source = type.image,
                WidthRequest = 40,
                HeightRequest = 40
            };

            return typeImage;
        }

        /*private Color GetTypeColor(string typeName)
        {
            // Couleurs basées sur les types Pokémon
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
                default: return Color.FromHex("#68A090");
            }
        }*/

        private string GetPokemonDescription()
        {
            if (_currentPokemon.category != null)
            {
                return $"Le {_currentPokemon.category} vous accompagne aujourd'hui ! " +
                       "Découvrez ses capacités et caractéristiques uniques.";
            }

            return "Ce Pokémon mystérieux a été choisi spécialement pour vous aujourd'hui ! " +
                   "Explorez ses secrets dans sa fiche détaillée.";
        }

        private async Task AnimateContent()
        {
            // Animation d'entrée du contenu
            PokemonCard.Scale = 0.8;
            PokemonCard.Opacity = 0;

            await Task.WhenAll(
                PokemonCard.ScaleTo(1, 500, Easing.BounceOut),
                PokemonCard.FadeTo(1, 500)
            );
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            if (_currentPokemon != null)
            {
                await AnimateButtonClick(sender as Button);
                await Navigation.PushAsync(new PokemonDetailPage(_currentPokemon));
            }
        }

        /*private async void OnShareClicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);

            if (_currentPokemon != null)
            {
                string shareText = $"🌟 Pokémon du jour ({DateTime.Now:dd/MM/yyyy}) : " +
                                  $"{_currentPokemon.name?.fr ?? _currentPokemon.name?.en} " +
                                  $"#{_currentPokemon.pokedex_id:D3} ! " +
                                  $"Découvrez-le dans votre Pokédex ! 🌟";

                await Share.RequestAsync(new ShareTextRequest
                {
                    Text = shareText,
                    Title = "Pokémon du Jour"
                });
            }
        }*/

        private async void OnReturnClicked(object sender, EventArgs e)
        {
            await AnimateButtonClick(sender as Button);
            await Navigation.PopAsync();
        }

        private async Task AnimateButtonClick(Button button)
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
            _httpClient?.Dispose();
            base.OnDisappearing();
        }
    }
}