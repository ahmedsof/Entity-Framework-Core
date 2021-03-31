using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DataTransferObjects.Input;
using CarDealer.DataTransferObjects.Output;
using CarDealer.Models;
using CarDealer.XMLHelper;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {

            var context = new CarDealerContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();
            //
            //(../../../suppliers.xml)
            //var supplierXml = File.ReadAllText("./Datasets/suppliers.xml");
            //ImportSuppliers(context, supplierXml);
            //
            //var partXml = File.ReadAllText("./Datasets/parts.xml");
            //ImportParts(context, partXml);
            //
            //var carsXml = File.ReadAllText("./Datasets/cars.xml");
            //ImportCars(context, carsXml);
            //
            //var customersXml = File.ReadAllText("./Datasets/customers.xml");
            //ImportCustomers(context, customersXml);
            //
            //var salesXml = File.ReadAllText("./Datasets/sales.xml");
            //var result = ImportSales(context, salesXml);

            Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(x => new SalesOutputModel
                {
                    Car = new CarSaleOutputModel
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TravelledDistance = x.Car.TravelledDistance
                    },
                    Discount = x.Discount,
                    CustomerName = x.Customer.Name,
                    Price = x.Car.PartCars.Sum(x => x.Part.Price),
                    PriceWithDiscount = 
                        x.Car.PartCars.Sum(x => x.Part.Price) 
                        - x.Car.PartCars.Sum(x => x.Part.Price) * x.Discount/100

                }).ToList();

            var result = XmlConverter.Serialize(sales, "sales");
            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(x => x.Sales.Any())
                .Select(x => new TotalSalesCustomerOutputModel
                {
                    FulName = x.Name,
                    BoughtCars = x.Sales.Count,
                    SpentMoney = x.Sales
                        .Select(x => x.Car)
                        .SelectMany(x => x.PartCars)
                        .Sum(x => x.Part.Price)
                })
                .OrderByDescending(x => x.SpentMoney)
                .ToList();

            var result = XmlConverter.Serialize(customers, "customers");
            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(x => new CarPartOutputModel
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance,
                    Parts = x.PartCars.Select(p => new CarPartsInfoOutputModel
                        {
                            Name = p.Part.Name,
                            Price = p.Part.Price

                        })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                })
                .OrderByDescending(x => x.TravelledDistance)
                .ThenBy(x => x.Model)
                .Take(5)
                .ToList();

            var result = XmlConverter.Serialize(cars, "cars");
            return result;
        }
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var locSupliers = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new LocalSuppliersOutputModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                }).ToList();
            var result = XmlConverter.Serialize(locSupliers, "suppliers");
            return result;
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var carsBMW = context.Cars
                .Where(x => x.Make == "BMW")
                .Select(x => new BMWOutputModel
                {
                    Id = x.Id,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .ToList();

            var result = XmlConverter.Serialize(carsBMW, "cars");

            return result;
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(x => x.TravelledDistance > 2000000)
                .Select(c => new CarsWithDistanceOutputModel
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .OrderBy(x => x.Make)
                .ThenBy(x => x.Model)
                .Take(10)
                .ToArray();

            /*XmlSerializer xmlSerializer = 
                new XmlSerializer(typeof(CarsWithDistanceOutputModel[]), new XmlRootAttribute("cars"));
            var textWriter = new StringWriter();

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xmlSerializer.Serialize(textWriter, cars, ns);

            var result = textWriter.ToString();*/

            var result = XmlConverter.Serialize(cars, "cars");

            return result;
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            const string root = "Sales";

            var salesDto = XmlConverter.Deserializer<SalesInputModel>(inputXml, root);
            var carsId = context.Cars.Select(x => x.Id).ToList();

            var sales = salesDto
                .Where(x => carsId.Contains(x.CarId))
                .Select(s => new Sale
                {
                    CarId = s.CarId,
                    CustomerId = s.CustomerId,
                    Discount = s.Discount
                }).ToList();
            context.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            const string root = "Customers";
            var custDto = XmlConverter.Deserializer<CustomersInputModel>(inputXml, root);

            var customers = custDto.Select(x => new Customer
            {
                Name = x.Name,
                BirthDate = x.BirthDate,
                IsYoungDriver = x.IsYoungDriver
            }).ToList();

            context.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            const string root = "Cars";

            var cars = new List<Car>();
            var carsDto = XmlConverter.Deserializer<CarInputModel>(inputXml, root);

            var allParts = context.Parts.Select(x => x.Id).ToList();

            foreach (var currentCar in carsDto)
            {
                var distinctedParts = currentCar.CarPartsInputModel
                    .Select(x => x.Id)
                    .Distinct();
                var parts = distinctedParts.Intersect(allParts);

                var car = new Car
                {
                    Make = currentCar.Make,
                    Model = currentCar.Model,
                    TravelledDistance = currentCar.TraveledDistance,

                };

                foreach (var part in parts)
                {
                    var partCar = new PartCar
                    {
                        PartId = part
                    };
                    car.PartCars.Add(partCar);
                }

                cars.Add(car);
            }
            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";

        }
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            const string root = "Parts";
            var partsDto = XmlConverter.Deserializer<PartsInputModel>(inputXml, root);

            //var xmlSerializer =
            //     new XmlSerializer(typeof(PartsInputModel[]), new XmlRootAttribute(root));

            //var textRead = new StringReader(inputXml);
            //var partsDto = xmlSerializer.Deserialize(textRead) as PartsInputModel[];

            var supplierIds = context.Suppliers.Select(x => x.Id).ToList();

            var parts = partsDto
                .Where(s => supplierIds.Contains(s.SupplierId))
                .Select(x => new Part
            {
                Name = x.Name,
                Price = x.Price,
                Quantity = x.Quantity,
                SupplierId = x.SupplierId
            }
            ).ToList();

            context.Parts.AddRange(parts);
            context.SaveChanges();
            
            return $"Successfully imported {parts.Count}";
        }
        
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var suppliersDto = XmlConverter.Deserializer<SuppliersInputModel>(inputXml, "Suppliers");

            //XmlSerializer xmlSerializer = new XmlSerializer
            //    (typeof(SuppliersInputModel[]), new XmlRootAttribute("Suppliers"));

            //var textRead = new StringReader(inputXml);
            //var suppliersDto = xmlSerializer.Deserialize(textRead) as SuppliersInputModel[];


            var suppliers = suppliersDto.Select(x => new Supplier
            {
                Name = x.Name,
                IsImporter = x.IsImporter
            }).ToList();

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }
    }
}