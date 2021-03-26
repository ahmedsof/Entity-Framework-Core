using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CarDealer.Data;
using CarDealer.DataTransferObjects.Input;
using CarDealer.Models;
using CarDealer.XMLHelper;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {

            var context = new CarDealerContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            /*(../../../suppliers.xml)*/
            var supplierXml = File.ReadAllText("./Datasets/suppliers.xml");
            ImportSuppliers(context, supplierXml);

            var partXml = File.ReadAllText("./Datasets/parts.xml");
            var result = ImportParts(context, partXml);

            Console.WriteLine(result);
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