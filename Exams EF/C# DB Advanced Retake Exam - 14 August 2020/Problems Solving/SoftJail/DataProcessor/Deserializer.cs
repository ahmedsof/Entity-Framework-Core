using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SoftJail.Data.Models;
using SoftJail.Data.Models.Enums;
using SoftJail.DataProcessor.ImportDto;

namespace SoftJail.DataProcessor
{

    using Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var departments = new List<Department>();

            var departmentsCells = JsonConvert.DeserializeObject<IEnumerable<DepartmentCellInputModel>>(jsonString);

            foreach (var departmentsCell in departmentsCells)
            {
                if (!IsValid(departmentsCell) 
                    || !departmentsCell.Cells.All(IsValid) 
                    || !departmentsCell.Cells.Any())
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }
                var department = new Department
                {
                    Name = departmentsCell.Name,
                    Cells = departmentsCell.Cells.Select(x => new Cell
                    {
                        CellNumber = x.CellNumber,
                        HasWindow = x.HasWindow
                    }).ToList()
                };
            departments.Add(department);

            sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");

            }
            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var prisioners = new List<Prisoner>();

            var prisonerMails = JsonConvert.DeserializeObject<IEnumerable<PrisonersMailsInputModel>>(jsonString);

            foreach (var currentPrisoner in prisonerMails)
            {
                if (!IsValid(currentPrisoner) || !currentPrisoner.Mails.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var isValidReleaseDate = 
                    DateTime.TryParseExact
                    (currentPrisoner.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime releaseDate);

                var incanserationDate =
                    DateTime.ParseExact(currentPrisoner.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var prisioner = new Prisoner
                {
                    FullName = currentPrisoner.FullName,
                    Nickname = currentPrisoner.Nickname,
                    Age = currentPrisoner.Age,
                    IncarcerationDate = incanserationDate,
                    ReleaseDate = isValidReleaseDate ? (DateTime?)releaseDate : null,
                    Bail = currentPrisoner.Bail,
                    CellId = currentPrisoner.CellId,
                    Mails = currentPrisoner.Mails.Select(m => new Mail
                    {
                        Sender = m.Sender,
                        Address = m.Address,
                        Description = m.Description
                    }).ToList()

                };

                prisioners.Add(prisioner);
                sb.AppendLine($"Imported {prisioner.FullName} {prisioner.Age} years old");
            }
            context.Prisoners.AddRange(prisioners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var validOfficers = new List<Officer>();

            var officersPrisioners =
                XmlConverter.Deserializer<OfficerPrisonerInputModel>(xmlString, "Officers");

            foreach (var officersPrisioner in officersPrisioners)
            {
                if (!IsValid(officersPrisioner))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }
                var officer = new Officer
                {
                    FullName = officersPrisioner.Name,
                    Salary = officersPrisioner.Money,
                    Position = Enum.Parse<Position>(officersPrisioner.Position),
                    Weapon = Enum.Parse<Weapon>(officersPrisioner.Weapon),
                    DepartmentId = officersPrisioner.DepartmentId,
                    OfficerPrisoners = officersPrisioner.Prisoners.Select(x => new OfficerPrisoner
                    {
                        PrisonerId = x.Id
                    }).ToList()
                };
                validOfficers.Add(officer);
                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }

            context.Officers.AddRange(validOfficers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}