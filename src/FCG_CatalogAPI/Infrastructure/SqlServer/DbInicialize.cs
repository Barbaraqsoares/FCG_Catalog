using FCG_CatalogAPI.Domain.Entities;

namespace FCG_CatalogAPI.Infrastructure.SqlServer;

public static class DbInicialize
{
    public static async Task InitialyzeCatalogAsync(CatalogDbContext context)
    {
        var games = new List<Game>
        {
            new Game { Name = "Balatro", Description = "Um jogo de cartas do gênero roguelike de construção de baralhos (deckbuilder)", Price = 20.0m, IsMultiplayer = false, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Brawlhalla", Description = "Um jogo de luta 2D do gênero plataforma, onde os jogadores competem em batalhas para derrubar seus oponentes de uma arena", Price = 50.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Call of Duty: Modern Warfare", Description = "Um jogo de tiro em primeira pessoa (FPS) da série Call of Duty, desenvolvido pela Infinity Ward e publicado pela Activision", Price = 120.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Counter-Strike: Global Offensive", Description = "Um jogo de tiro em primeira pessoa (FPS) do gênero tático, desenvolvido pela Valve Corporation e Hidden Path Entertainment", Price = 100.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Dota", Description = "Uma série de jogos do gênero MOBA (Multiplayer Online Battle Arena) onde duas equipes de cinco jogadores competem para destruir a base central do inimigo, chamada de Ancestral", Price = 80.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Grounded", Description = "É um jogo de sobrevivência, ação e aventura", Price = 150.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Minecraft", Description = "Um jogo de estilo sandbox (mundo aberto) e sobrevivência desenvolvido pela Mojang Studios. No jogo, os jogadores exploram um mundo tridimensional feito inteiramente de blocos cúbicos", Price = 2530.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Peak", Description = "O PEAK é um jogo cooperativo de escalada e sobrevivência desenvolvido pela Team PEAK", Price = 56.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Project Zomboid", Description = "Um jogo de sobrevivência em mundo aberto, desenvolvido pela The Indie Stone, onde os jogadores devem sobreviver em um mundo pós-apocalíptico infestado por zumbis", Price = 180.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Red Dead Redemption 2", Description = "um clássico de ação e aventura em mundo aberto, com temática de faroeste, que retrata o fim do Velho Oeste americano e a transição para a modernidade", Price = 350.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Roller Coaster Tycoon 3", Description = "É um jogo de sobrevivência, ação e aventura", Price = 150.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "StarDew Valley", Description = "Um popular jogo independente de RPG e simulação de vida no campo, desenvolvido por Eric ConcernedApe Barone", Price = 98.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "Super Mario World", Description = "Um clássico jogo de plataforma 2D desenvolvido pela Nintendo e lançado em 1990", Price = 30.0m, IsMultiplayer = true, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "The Legend of Zelda: Breath of the Wild", Description = "Um jogo de ação e aventura em mundo aberto, desenvolvido e publicado pela Nintendo", Price = 250.0m, IsMultiplayer = false, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "The Sims 4", Description = "Um jogo de simulação social e de vida onde os jogadores criam, personalizam e controlam pessoas virtuais chamadas Sims.", Price = 100.0m, IsMultiplayer = false, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Game { Name = "The Witcher 3: Wild Hunt", Description = "Um jogo de RPG de ação em mundo aberto, desenvolvido pela CD Projekt Red", Price = 200.0m, IsMultiplayer = false, IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };

        await context.Games.AddRangeAsync(games);
        await context.SaveChangesAsync();
    }
}