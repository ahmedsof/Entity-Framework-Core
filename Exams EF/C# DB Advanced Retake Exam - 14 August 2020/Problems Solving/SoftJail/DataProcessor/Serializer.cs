using System.Linq;
using Newtonsoft.Json;
using SoftJail.DataProcessor.ExportDto;

namespace SoftJail.DataProcessor
{

    using Data;
    using System;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var people = context.Prisoners
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.FullName,
                    CellNumber = x.Cell.CellNumber,
                    Officers = x.PrisonerOfficers.Select(o => new
                    {
                        OfficerName = o.Officer.FullName,
                        Department = o.Officer.Department.Name

                    }).ToList(),
                    TotalOfficerSalary = 
                        decimal.Parse(x.PrisonerOfficers.Sum(x => x.Officer.Salary).ToString("F2"))
                        
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToList();

            string result = JsonConvert.SerializeObject(people, Formatting.Indented);
            return result;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var names = prisonersNames.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var prisioners = context.Prisoners
                .Where(x => names.Contains(x.FullName))
                .Select(x => new PrisonerViewModel
                {
                    Id = x.Id,
                    Name = x.FullName,
                    IncarcerationDate = x.IncarcerationDate.ToString("yyyy-MM-dd"),
                    EncryptedMessages = x.Mails.Select(m => new EncryptedMessagesViewModel
                    {
                        Description = string.Join("", m.Description.Reverse())
                    }).ToArray()
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToList();

            var xml = XmlConverter.Serialize(prisioners, "Prisoners");

            return xml;
        }
    }
}