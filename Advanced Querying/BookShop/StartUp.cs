using System;
using System.Linq;
using BookShop.Models.Enums;

namespace BookShop
{
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            Console.WriteLine(GetBooksByAgeRestriction(db, "teEN"));
        }

        //public static string GetGoldenBooks(BookShopContext context)
        

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var ageRestriction = Enum.Parse<AgeRestriction>(command, true);

            var books = context.Books
                .Where(books => books.AgeRestriction == ageRestriction)
                .Select(book => book.Title)
                .OrderBy(title => title)
                .ToArray();

            var result = string.Join(Environment.NewLine, books);

            return result;
        }
    }
}
