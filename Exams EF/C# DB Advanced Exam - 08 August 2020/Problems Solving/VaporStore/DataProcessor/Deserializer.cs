using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VaporStore.Data.Models;
using VaporStore.DataProcessor.Dto.Import;

namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Data;

	public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var output = new StringBuilder();

            var data = JsonConvert.DeserializeObject<IEnumerable<GameJsonImportModel>>(jsonString);
            foreach (var games in data)
            {

                if (!IsValid(games) || games.Tags.Count() == 0 )
                {
                    output.AppendLine("Invalid Data");
					continue;
                }

                var genre = context.Genres.FirstOrDefault(x => x.Name == games.Genre)
                            ?? new Genre { Name = games.Genre };

                var developer = context.Developers.FirstOrDefault(x => x.Name == games.Developer) 
                                ?? new Developer {Name = games.Developer};

                var game = new Game
                {
					Name = games.Name,
					Genre = genre,
					Developer = developer,
					Price = games.Price,
					ReleaseDate = games.ReleaseDate.Value,
                };

                foreach (var jsonTag in games.Tags)
                {
                    var tag = context.Tags.FirstOrDefault(x => x.Name == jsonTag)
                              ?? new Tag {Name = jsonTag};
					game.GameTags.Add(new GameTag {Tag = tag});
                }

                context.Games.Add(game);
                context.SaveChanges();
                output.AppendLine($"Added {games.Name} ({games.Genre}) with {games.Tags.Count()} tags"!);
            }

            return output.ToString().TrimEnd();
        }

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
            var output = new StringBuilder();
            var users = JsonConvert.DeserializeObject<IEnumerable<UsersImportModel>>(jsonString);

            foreach (var jsonUsers in users)
            {


                if (!IsValid(jsonUsers) || !jsonUsers.Cards.All(IsValid))
                {
                    output.AppendLine($"Invalid Data");
                    continue;
                }

                var user = new User
                {
                    FullName = jsonUsers.FullName,
                    Username = jsonUsers.Username,
                    Email = jsonUsers.Email,
                    Age = jsonUsers.Age,
                    Cards = jsonUsers.Cards.Select(x => new Card
                    {
                        Number = x.Number,
                        Cvc = x.CVC,
                        Type = x.Type.Value
                    }).ToArray()

                };
                context.Users.Add(user);
                context.SaveChanges();
                output.AppendLine($"Imported {jsonUsers.Username} with {jsonUsers.Cards.Count()} cards");
            }

            return output.ToString().TrimEnd();
        }

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
            var output = new StringBuilder();

            var inputData = XmlConverter.Deserializer<PurchasesInputModel>(xmlString, "Purchases");
            foreach (var input in inputData)
            {
                if (!IsValid(input))
                {
                    output.AppendLine("Invalid Data");
                    continue;
                }

                var parsedDate = DateTime
                    .TryParseExact(input.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);

                 var purches = new Purchase
                 {
                     Date = date,
                     Type = input.Type.Value,
                     ProductKey = input.Key,
                 };
                 purches.Card = context.Cards.FirstOrDefault(x => x.Number == input.Card);
                 purches.Game = context.Games.FirstOrDefault(x => x.Name == input.Title);

                 context.Purchases.Add(purches);
                 context.SaveChanges();

                 var userName = context.Users.Where(x => x.Id == purches.Card.UserId).Select(x => x.Username).FirstOrDefault();

                 output.AppendLine($"Imported {input.Title} for {userName}");
            }

            return output.ToString().TrimEnd();
        }

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}