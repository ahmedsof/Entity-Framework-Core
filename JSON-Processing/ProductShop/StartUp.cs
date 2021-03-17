using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.DAtaTransferObjects;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        private static IMapper mapper;

        public static void Main(string[] args)
        {
            var productShopContext = new ProductShopContext();

            productShopContext.Database.EnsureDeleted();
            productShopContext.Database.EnsureCreated();

            string userJson = File.ReadAllText("../../../Datasets/users.json");
            ImportUsers(productShopContext, userJson);

            string productJson = File.ReadAllText("../../../Datasets/products.json");
            ImportProducts(productShopContext, productJson);

            string categoryJson = File.ReadAllText("../../../Datasets/categories.json");
            ImportCategories(productShopContext, categoryJson);

            string categoryProductJson = File.ReadAllText("../../../Datasets/categories-products.json");
            var result = ImportCategoryProducts(productShopContext, categoryProductJson);

            Console.WriteLine(result);

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