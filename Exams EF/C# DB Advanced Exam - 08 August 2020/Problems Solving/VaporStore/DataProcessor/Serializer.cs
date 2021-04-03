using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using VaporStore.DataProcessor.Dto.Export;

namespace VaporStore.DataProcessor
{
	using System;
	using Data;

	public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var data = context.Genres.ToList()
                .Where(x => genreNames.Contains(x.Name))
                .Select(x => new
                {
                    Id = x.Id,
                    Genre = x.Name,
                    Games = x.Games.Select(x => new
                        {
                            Id = x.Id,
                            Title = x.Name,
                            Developer = x.Developer.Name,
                            Tags = string.Join(", ", x.GameTags.Select(x => x.Tag.Name)),
                            Players = x.Purchases.Count
                        })
                        .Where(g => g.Players > 0)
                        .OrderByDescending(x => x.Players)
                        .ThenBy(x => x.Id),
                    TotalPlayers = x.Games.Sum(x => x.Purchases.Count)
                })
                .OrderByDescending(x => x.TotalPlayers)
                .ThenBy(x => x.Id);
                

            var result = JsonConvert.SerializeObject(data, Formatting.Indented);
            return result;
        }

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            var data = context.Users.ToList()
                .Where(x => x.Cards
                    .Any(c => c.Purchases
                        .Any(p => p.Type.ToString()== storeType)))
                .Select(x => new UserXmlExportModel
                {
                    Username = x.Username,
                    TotalSpent = x.Cards
                        .Sum(p => p.Purchases.Where(c => c.Type.ToString() == storeType)
                            .Sum(x => x.Game.Price)),
                    Purchases = x.Cards
                        .SelectMany(c => c.Purchases)
                        .Where(p => p.Type.ToString() == storeType)
                        .Select(p => new PurchesXmlExportModel
                        {
                            Card = p.Card.Number,
                            Cvc = p.Card.Cvc,
                            Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            Game = new GameXmlExportModel
                            {
                                Title = p.Game.Name,
                                Price = p.Game.Price,
                                Genre = p.Game.Genre.Name
                            }
                        })
                        .OrderBy(x => x.Date)
                        .ToArray(),
                })
                .OrderByDescending(x => x.TotalSpent)
                .ThenBy(x => x.Username)
                .ToArray();

             //XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserXmlExportModel[]), new XmlRootAttribute("Users"));

             //var sw = StringWriter();
             //xmlSerializer.Serialize(sw, data);
             var result = XmlConverter.Serialize(data, "Users");

             return result;
        }
	}
}