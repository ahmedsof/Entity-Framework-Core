using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var inputSuppliersJson = File.ReadAllText("../../../Datasets/suppliers.json");
            //ImportSuppliers(context, inputSuppliersJson);

            //var inputPartsJson = File.ReadAllText("../../../DataSets/parts.json");
            //ImportParts(context, inputPartsJson);


            //var inputCarsJson = File.ReadAllText("../../../DataSets/cars.json");
            //ImportCars(context, inputCarsJson);

            //var inputCustomerJson = File.ReadAllText("../../../DataSets/customers.json");
            //ImportCustomers(context, inputCustomerJson);

            //var inputSaleJson = File.ReadAllText("../../../DataSets/sales.json");
            //ImportSales(context, inputSaleJson);

            var result = GetOrderedCustomers(context);

            Console.WriteLine(result);

        }

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(x => new
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToList();

            var result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var dtoSales = JsonConvert.DeserializeObject<IEnumerable<SalesInputModel>>(inputJson);

            var sales = dtoSales.Select(x => new Sale
            {
                CarId = x.CarId,
                CustomerId = x.CustomerId,
                Discount = x.Discount
            }).ToList();

            context.Sales.AddRange(sales);
            context.SaveChanges();
            return $"Successfully imported {sales.Count}.";
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var dtoCustomer = JsonConvert.DeserializeObject<IEnumerable<CustomerInputModel>>(inputJson);

            var customers = dtoCustomer.Select(x => new Customer
            {
                Name = x.Name,
                BirthDate = x.BirthDate,
                IsYoungDriver = x.IsYoungDriver
            }).ToList();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var carsDto = JsonConvert.DeserializeObject<IEnumerable<CarInputmodel>>(inputJson);
            var listOfCArs = new List<Car>();
            foreach (var car in carsDto)
            {
                var currentCar = new Car
                {
                    Make = car.Make,
                    Model = car.Model,
                    TravelledDistance = car.TravelledDistance

                };
                foreach (var partId in car?.PartsId.Distinct())
                {
                    currentCar.PartCars.Add(new PartCar
                    {
                        PartId = partId
                    });
                }
                listOfCArs.Add(currentCar);
            }
            context.Cars.AddRange(listOfCArs);
            context.SaveChanges();

            return $"Successfully imported {listOfCArs.Count}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var supplierIds = context.Suppliers
                .Select(x => x.Id)
                .ToArray();

            var parts = JsonConvert.DeserializeObject<IEnumerable<Part>>(inputJson)
                .Where(s => supplierIds.Contains(s.SupplierId))
                .ToList();

            
            var validParts = parts;

            context.Parts.AddRange(parts);
            context.SaveChanges();
            return $"Successfully imported {parts.Count}.";
        }

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var dtoSupliers = JsonConvert.DeserializeObject<IEnumerable<ImportSupplierInputModel>>(inputJson);

            var suplier = dtoSupliers.Select(x => new Supplier
                {
                    Name = x.Name,
                    IsImporter = x.IsImporter


                })
                .ToList();
            context.Suppliers.AddRange(suplier);
            context.SaveChanges();

            return $"Successfully imported {suplier.Count}.";
        }
    }
}