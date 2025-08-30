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

namespace AppPokedex.RecherchePages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AllPage : ContentPage
    {
        private ObservableCollection<Pokemon> _allPokemon;
        private HttpClient _httpClient;

        public AllPage()
        {
            InitializeComponent();
            _allPokemon = new ObservableCollection<Pokemon>();
            _httpClient = new HttpClient();

            PokemonCollectionView.ItemsSource = _allPokemon;

            LoadPokemonData();
        }

        private async Task LoadPokemonData()
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                ErrorPanel.IsVisible = false;
                PokemonCollectionView.IsVisible = false;

                // Appel à l'API Tyradex pour récupérer tous les Pokémon
                string apiUrl = "https://tyradex.vercel.app/api/v1/pokemon";
                string jsonResponse = await _httpClient.GetStringAsync(apiUrl);

                var pokemonList = JsonConvert.DeserializeObject<List<Pokemon>>(jsonResponse);

                // Trier par numéro du Pokédex
                var sortedPokemon = pokemonList.OrderBy(p => p.pokedex_id).ToList();

                _allPokemon.Clear();

                foreach (var pokemon in sortedPokemon)
                {
                    _allPokemon.Add(pokemon);
                }

                CounterLabel.Text = $"{_allPokemon.Count} Pokémon au total";

                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                PokemonCollectionView.IsVisible = true;
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

        private async void OnPokemonSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Pokemon selectedPokemon)
            {
                // Désélectionner immédiatement l'élément
                ((CollectionView)sender).SelectedItem = null;

                try
                {
                    // Naviguer vers la page de détails du Pokémon
                    await Navigation.PushAsync(new PokemonDetailPage(selectedPokemon));
                }
                catch (Exception ex)
                {
                    // Pour l'instant, afficher une alerte simple
                    await DisplayAlert("Pokémon sélectionné",
                        $"Vous avez sélectionné {selectedPokemon.name.fr} (#{selectedPokemon.pokedex_id:D3})",
                        "OK");
                }
            }
        }

        private async void OnRetryClicked(object sender, EventArgs e)
        {
            await LoadPokemonData();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _httpClient?.Dispose();
        }
    }
}