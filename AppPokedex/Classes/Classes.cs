using System;
using System.Collections.Generic;
using System.Text;

namespace AppPokedex.Classes
{
    // Modèles de données pour l'API Tyradex
    public class Pokemon
    {
        public int pokedex_id { get; set; }
        public int generation { get; set; }
        public string category { get; set; }
        public PokemonName name { get; set; }
        public PokemonSprites sprites { get; set; }
        public bool HasSecondType { get; set; }
        public List<PokemonType> types { get; set; }
        public PokemonStats stats { get; set; }
        public List<PokemonResistances> resistances { get; set; }
        public PokemonEvolution evolution { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public List<string> egg_groups { get; set; }
        public PokemonSexe sexe { get; set; }
        public string catch_rate { get; set; }
        public int? level_100 { get; set; }
        public List<PokemonFormes> formes { get; set; }
    }

    public class PokemonName
    {
        public string fr { get; set; }
        public string en { get; set; }
        public string jp { get; set; }
    }

    public class PokemonSprites
    {
        public string regular { get; set; }
        public string shiny { get; set; }
        public PokemonGmaxSprites gmax { get; set; }
    }

    public class PokemonType
    {
        public string name { get; set; }
        public string image { get; set; }
    }

    public class PokemonStats
    {
        public int hp { get; set; }
        public int atk { get; set; }
        public int def { get; set; }
        public int spe_atk { get; set; }
        public int spe_def { get; set; }
        public int vit { get; set; }
    }

    public class PokemonSexe
    {
        public double? male { get; set; }
        public double? female { get; set; }
    }

    public class PokemonFormes
    {
        public string region { get; set; }
        public PokemonName name { get; set; }
    }

    public class PokemonResistances
    {
        public string Name { get; set; }
        public double Multiplier { get; set; }
    }

    public class PokemonEvolution
    {
        public object pre { get; set; } // Peut être null ou un objet EvolutionInfo
        public List<EvolutionInfo> next { get; set; }
        public List<PokemonMega> mega { get; set; }
    }

    public class EvolutionInfo
    {
        public int pokedex_id { get; set; }
        public string name { get; set; }
        public string condition { get; set; }
    }

    public class PokemonGmaxSprites
    {
        public string regular { get; set; }
        public string shiny { get; set; }
    }

    public class PokemonMega
    {
        public string orbe { get; set; }
        public PokemonSprites sprites { get; set; }
    }
}
