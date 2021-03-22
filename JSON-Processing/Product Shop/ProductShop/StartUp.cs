using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.DAtaTransferObjects;
using ProductShop.Models;
using Remotion.Linq.Clauses;

namespace ProductShop
{
    public class StartUp
    {
        private static IMapper mapper;

        public static void Main(string[] args)
        {
            var productShopContext = new ProductShopContext();

            //productShopContext.Database.EnsureDeleted();
            //productShopContext.Database.EnsureCreated();

            //string userJson = File.ReadAllText("../../../Datasets/users.json");
            //ImportUsers(productShopContext, userJson);

            //string productJson = File.ReadAllText("../../../Datasets/products.json");
            //ImportProducts(productShopContext, productJson);

            //string categoryJson = File.ReadAllText("../../../Datasets/categories.json");
            //ImportCategories(productShopContext, categoryJson);

            //string categoryProductJson = File.ReadAllText("../../../Datasets/categories-products.json");
            var result = GetUsersWithProducts(productShopContext);

            Console.WriteLine(result);

        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Include(x => x.ProductsSold)
                .ToList()
                .Where(x => x.ProductsSold.Any(b => b.BuyerId != null))
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Where(x => x.BuyerId != null).Count(),
                        products = u.ProductsSold.Where(x => x.BuyerId != null)
                            .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        })
                    }
                })
                .OrderByDescending(x => x.soldProducts.products.Count())
                .ToList();

            var resultObjects = new
            {
                usersCount = context.Users
                    .Where(x => x.ProductsSold.Any(b => b.BuyerId != null)).Count(),
                users = users
            };

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var resultJson = JsonConvert
                .SerializeObject(resultObjects, Formatting.Indented, jsonSerializerSettings);

            return resultJson;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categorisInfo = context.Categories
                .Select(x => new
                {
                    category = x.Name,
                    productsCount = x.CategoryProducts.Count,
                    averagePrice = x.CategoryProducts.Average(a => a.Product.Price)
                        .ToString("F2"),
                    totalRevenue = x.CategoryProducts.Sum(s => s.Product.Price)
                        .ToString("F2")

                })
                .OrderByDescending(x => x.productsCount)
                .ToList();

            var result = JsonConvert.SerializeObject(categorisInfo, Formatting.Indented);
            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Any(y => y.BuyerId != null))
                .Select(x => new
                {
                    firstName = x.FirstName,
                    lastName = x.LastName,
                    soldProducts = x.ProductsSold.Where(p => p.BuyerId != null)
                        .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price,
                            buyerFirstName = p.Buyer.FirstName,
                            buyerLastName = p.Buyer.LastName
                        })
                        .ToList()
                })
                .OrderBy( x => x.lastName)
                .ThenBy(x => x.firstName)
                .ToList();

            var result = JsonConvert.SerializeObject(users, Formatting.Indented);
            return result;
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new
                {
                    name = x.Name,
                    price = x.Price,
                    seller = x.Seller.FirstName + " " + x.Seller.LastName
                })
                .OrderBy(x => x.price)
                .ToArray();

            var result = JsonConvert.SerializeObject(products,Formatting.Indented);

            return result;
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();

            var dtoCategoriesProducts = JsonConvert.DeserializeObject<IEnumerable<CategoryProductInputModel>>(inputJson);

            var categorProduct = mapper.Map<IEnumerable<CategoryProduct>>(dtoCategoriesProducts);

            context.CategoryProducts.AddRange(categorProduct);
            context.SaveChanges();

            return $"Successfully imported {categorProduct.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();
            var dtoCategory = JsonConvert.DeserializeObject<IEnumerable<CategoryInputModel>>(inputJson).Where(c => c.Name != null)
                .ToList();

            var category = mapper.Map<IEnumerable<Category>>(dtoCategory);
            context.Categories.AddRange(category);
            context.SaveChanges();

            return $"Successfully imported {category.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();

            var dtoProducts = JsonConvert.DeserializeObject<IEnumerable<ProductInputModel>>(inputJson);

            var products = mapper.Map<IEnumerable<Product>>(dtoProducts);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            InitializeAutomapper();

            var dtoUsers = JsonConvert.DeserializeObject<IEnumerable<UserInputModel>>(inputJson);

            var users = mapper.Map<IEnumerable<User>>(dtoUsers);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count()}";
        }

        private static void InitializeAutomapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            mapper = config.CreateMapper();
        }
    }
}