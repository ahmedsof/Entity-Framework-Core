using System;
using System.Globalization;
using System.Linq;
using System.Text;
using BookShop.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookShop
{
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            //DbInitializer.ResetDatabase(db);

            Console.WriteLine(RemoveBooks(db));
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.Copies < 4200)
                .ToList();

            context.Books.RemoveRange(books);
            context.SaveChanges();

            var result = books.Count;
            return result;
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categoriBooks = context.Categories
                .Select(x => new
                {
                    CatName = x.Name,
                    Books = x.CategoryBooks.Select(x => new
                        {
                            x.Book.Title,
                            x.Book.ReleaseDate.Value
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(3)
                        .ToArray()
                })
                .OrderBy(x => x.CatName)
                .ToArray();

            var sb = new StringBuilder();
            foreach (var category in categoriBooks)
            {
                sb.AppendLine($"--{category.CatName}");
                foreach (var item in category.Books)
                {
                    sb.AppendLine($"{item.Title} ({item.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context.Categories
                .Select(x => new
                {
                    x.Name,
                    Profit = x.CategoryBooks.Sum(x => x.Book.Price * x.Book.Copies)
                })
                .OrderByDescending(x => x.Profit)
                .ThenBy(x => x.Name)
                .ToArray();
            var result = string.Join(Environment.NewLine, categories
                .Select(x => $"{x.Name} ${x.Profit:f2}"));

            return result;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var autors = context.Authors
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    sum = x.Books.Sum(x => x.Copies)
                })
                .OrderByDescending(x => x.sum)
                .ToList();

            var result = string.Join(Environment.NewLine, autors
                .Select(x => $"{x.FirstName} {x.LastName} - {x.sum}"));

            return result;
        }


        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var bookscount = context.Books
                .Where(x => x.Title.Length > lengthCheck)
                .Count();

            return bookscount;
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .Include(x => x.Author)
                .Where(x => 
                    EF.Functions.Like(x.Author.LastName, $"{input}%"))
                .Select(x => new
                {
                    x.Title,
                    x.Author.FirstName,
                    x.Author.LastName,
                    x.BookId
                })
                .OrderBy(x => x.BookId)
                .ToList();

            var result = string.Join(Environment.NewLine, books
                .Select(x => $"{x.Title} ({x.FirstName} {x.LastName})"));

            return result;

        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var searchInput = input.ToLower();
            var books = context.Books
                .Where(x => x.Title.ToLower().Contains(searchInput))
                .Select(x => x.Title)
                .OrderBy(x => x)
                .ToList();

            var result = string.Join(Environment.NewLine, books);
            return result;
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var autors = context.Authors
                .Where(x => x.FirstName.EndsWith(input))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            var result = string.Join(Environment.NewLine, autors
                .Select(x => $"{x.FirstName} {x.LastName}"));
            return result;
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var targetDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var books = context.Books
                .OrderByDescending(x => x.ReleaseDate)
                .Where(x => x.ReleaseDate.Value < targetDate)
                .Select(x => new
                {
                    x.Title,
                    x.EditionType,
                    x.Price
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categoris = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLower())
                .ToArray();

            var books = context.Books
                .Include(x => x.BookCategories)
                .ThenInclude(x => x.Category)
                .ToArray()
                .Where(x => x.BookCategories
                    .Any(x => categoris.Contains(x.Category.Name.ToLower())))
                .Select(x => x.Title)
                .OrderBy(x => x)
                .ToArray();

            var result = string.Join(Environment.NewLine, books);
            return result;


        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .OrderBy(x => x.BookId)
                .Where(x => x.ReleaseDate.HasValue && x.ReleaseDate.Value.Year != year)
                .Select(x => x.Title)
                .ToArray();

            var result = string.Join(Environment.NewLine, books);
            return result;
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.Price > 40)
                .Select(x => new
                {
                    x.Title,
                    x.Price
                })
                .OrderByDescending(x => x.Price)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.EditionType == EditionType.Gold && x.Copies < 5000)
                .Select(x => new
                {
                    x.BookId,
                    x.Title
                })
                .OrderBy(x => x.BookId)
                .ToArray();

            var result = string.Join(Environment.NewLine, books.Select(x => x.Title));
            return result;
        }
        

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
