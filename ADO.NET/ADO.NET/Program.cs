using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace ADO.NET
{
    public class Program
    {
        private const string SQLConectionString = "Server=.;Database = MinionsDB;Integrated Security = true";
       public static void Main(string[] args)
        {
            using (var connection = new SqlConnection(SQLConectionString))
            {
                connection.Open();
                ;
                
            }
        }

       private static void RemoveVillain(SqlConnection connection)
       {
           int value = int.Parse(Console.ReadLine());
           string evilNameQuery = "SELECT Name FROM Villains WHERE Id = @villainId";

           using var sqlCommand = new SqlCommand(evilNameQuery, connection);
           sqlCommand.Parameters.AddWithValue("@villainId", value);
           var name = (string) sqlCommand.ExecuteScalar();

           if (name == null)
           {
               Console.WriteLine("No such villain was found.");
               return;
           }

           var deleteMinionsVillainsQuery = @"DELETE FROM MinionsVillains 
                        WHERE VillainId = @villainId";

           using var sqlDeleteMVCommand = new SqlCommand(deleteMinionsVillainsQuery, connection);
           sqlCommand.Parameters.AddWithValue("@villainId", value);
           var affectedRows = sqlDeleteMVCommand.ExecuteNonQuery();

           var deleteVillainsQuery = @"DELETE FROM Villains
                    WHERE Id = @villainId";

           using var sqlDeleteVCommand = new SqlCommand(deleteVillainsQuery, connection);
           sqlCommand.Parameters.AddWithValue("@villainId", value);
           sqlDeleteVCommand.ExecuteNonQuery();

           Console.WriteLine($"{name} was deleted.");
           Console.WriteLine($"{affectedRows} minions were released.");
       }

       private static void ChangeTownNamesCasing(SqlConnection connection)
       {
           string countriName = Console.ReadLine();

           string updateTownQuery = @"UPDATE Towns
                                                SET Name = UPPER(Name)
                WHERE CountryCode = (SELECT c.Id FROM Countries AS c WHERE c.Name = @countryName)";

           string selectTownNamesQuery = @"SELECT t.Name 
                       FROM Towns as t
                       JOIN Countries AS c ON c.Id = t.CountryCode
                      WHERE c.Name = @countryName";
           using var updateCommand = new SqlCommand(updateTownQuery, connection);
           updateCommand.Parameters.AddWithValue("@countryName", countriName);
           var affectedRows = updateCommand.ExecuteNonQuery();

           if (affectedRows == 0)
           {
               Console.WriteLine("No town names were affected.");
           }
           else
           {
               Console.WriteLine($"{affectedRows} town names were affected.");

               using var selectCommand = new SqlCommand(selectTownNamesQuery, connection);
               selectCommand.Parameters.AddWithValue("@countryName", countriName);

               using (var reader = selectCommand.ExecuteReader())
               {
                   var towns = new List<string>();

                   while (reader.Read())
                   {
                       towns.Add((string) reader[0]);
                   }

                   Console.WriteLine($"[{string.Join(", ", towns)}]");
               }
           }
       }

       private static void AddMinion4(SqlConnection connection)
       {
           string[] minionInfo = Console.ReadLine().Split(' ');
           string minionName = minionInfo[1];
           int age = int.Parse(minionInfo[2]);
           string townName = minionInfo[3];

           string[] villiainInfo = Console.ReadLine().Split(' ');


           int? townId = GetTownId(connection, townName);

           if (townId == null)
           {
               string createTownQuery = "INSERT INTO Towns(Name) VALUES(@townName)";
               using var sqlCommand = new SqlCommand(createTownQuery, connection);
               sqlCommand.Parameters.AddWithValue(@townName, townName);
               //sqlCommand.ExecuteNonQuery();
               townId = GetTownId(connection, townName);
               Console.WriteLine($"Town {townName} was added to the database");
           }

           string villainName = villiainInfo[1];
           int? villainId = GetVilainId(connection, villainName);

           if (villainId == null)
           {
               string createVillan = "INSERT INTO Villains(Name, EvilnessFactorId)  VALUES(@villainName, 4)";
               using var sqlCommand = new SqlCommand(createVillan, connection);
               sqlCommand.Parameters.AddWithValue(@villainName, villainName);
               //sqlCommand.ExecuteNonQuery();
               villainId = GetVilainId(connection, villainName);
               Console.WriteLine($"Villain {villainName} was added to the database.");
           }

           CreateMinion(connection, minionName, age, townId);

           var minionId = GetMinionId(connection, minionName);
           InsertMinVill(connection, villainId, minionId);
           Console.WriteLine($"Successfully added {minionName} to be minion of {villainName}.");
       }

       private static void InsertMinVill(SqlConnection connection, int? villainId, int? minionId)
       {
           var insertIntoMinVill =
               "INSERT INTO MinionsVillains (MinionId, VillainId) VALUES (@villainId, @minionId)";
           var sqlCommand = new SqlCommand(insertIntoMinVill, connection);
           sqlCommand.Parameters.AddWithValue("@villainId", minionId);
           sqlCommand.Parameters.AddWithValue("@minionId", villainId);
           sqlCommand.ExecuteNonQuery();
           
       }

       private static int? GetMinionId(SqlConnection connection, string minionName)
       {
           var minionQuery = "SELECT Id FROM Minions WHERE Name = @Name";
           var sqlCommand = new SqlCommand(minionQuery, connection);
           
           sqlCommand.Parameters.AddWithValue("@Name", minionName);
           var minionId = sqlCommand.ExecuteScalar();
           return (int?)minionId;
       }

       private static void CreateMinion(SqlConnection connection, string minionName, int age, int? townId)
       {
           string createMinion = "INSERT INTO Minions (Name, Age, TownId) VALUES (@name, @age, @townId)";
           using var sqlCommand = new SqlCommand(createMinion, connection);
           sqlCommand.Parameters.AddWithValue("@name", minionName);
           sqlCommand.Parameters.AddWithValue("@age", age);
           sqlCommand.Parameters.AddWithValue("@townId", townId);
           sqlCommand.ExecuteNonQuery();
       }

       private static int? GetVilainId(SqlConnection connection, string villainName)
       {
           string query = "SELECT Id FROM Villains WHERE Name = @Name";
           using var sqlCommand = new SqlCommand(query, connection);

           sqlCommand.Parameters.AddWithValue("@Name", villainName);
           var villanId = sqlCommand.ExecuteScalar();
           return (int?)villanId;
        }

       private static int? GetTownId(SqlConnection connection, string townName)
       {
           string townIdQuery = "SELECT Id FROM Towns WHERE Name = @townName";
           using var sqlCommand = new SqlCommand(townIdQuery, connection);

           sqlCommand.Parameters.AddWithValue("@townName", townName);
           var townId = sqlCommand.ExecuteScalar();
           return (int?)townId;
       }

       private static void MinionNames(SqlConnection connection)
       {
           int id = int.Parse(Console.ReadLine());

           string villianNameQuery = "SELECT Name FROM Villains WHERE Id = @Id";
           //command.Parameters.AddWithValue("@villainId", villainId);


           using (var command = new SqlCommand(villianNameQuery, connection))
           {
               command.Parameters.AddWithValue("@Id", id);


               var result = command.ExecuteScalar();

               string minionsQuery = @"SELECT ROW_NUMBER() OVER(ORDER BY m.Name) as RowNum,
                    m.Name, 
                    m.Age
                        FROM MinionsVillains AS mv
                        JOIN Minions As m ON mv.MinionId = m.Id
                    WHERE mv.VillainId = @Id
                    ORDER BY m.Name";

               if (result == null)
               {
                   Console.WriteLine($"No villain with ID {id} exists in the database.");
               }
               else
               {
                   Console.WriteLine($"Villain: {result}.");

                   using (var minionCommand = new SqlCommand(minionsQuery, connection))
                   {
                       minionCommand.Parameters.AddWithValue("@Id", id);

                       using (var reader = minionCommand.ExecuteReader())
                       {
                           if (!reader.HasRows)
                           {
                               Console.WriteLine("(no minions)");
                           }

                           while (reader.Read())
                           {
                               Console.WriteLine($"{reader[0]}. {reader[1]} {reader[2]}");
                           }
                       }
                   }
               }
           }
       }

       private static object ExecuteScalar(SqlConnection connection, string query, params KeyValuePair<string,string>[] keyValuePairs)
       {
           using (var command = new SqlCommand(query, connection))
           {
               

               foreach (var kvp in keyValuePairs)
               {
                   command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
                var result = command.ExecuteScalar();
               return result;
           }
       }

       private static void VillainNames(SqlConnection connection)
       {
           string query = @"SELECT v.Name, COUNT(mv.VillainId) AS MinionsCount
                FROM Villains AS v
                JOIN MinionsVillains AS mv ON v.Id = mv.VillainId
                GROUP BY v.Id, v.Name
                    HAVING COUNT(mv.VillainId) > 3
                ORDER BY COUNT(mv.VillainId)";

           using (var command = new SqlCommand(query, connection))
           {
               using (var reader = command.ExecuteReader())
               {
                   while (reader.Read())
                   {
                       var name = reader[0];
                       var count = reader[1];
                       Console.WriteLine($"{name} - {count}");
                   }
               }
           }
       }

       private static void InitialSetup(SqlConnection connection)
       {
           //string createDAtabase = "CREATE DATABASE MinionsDB";


           var createTableStatements = GetCreateTableStatements();

           foreach (var query in createTableStatements)
           {
               ExecuteNonQuery(connection, query);
           }

           var insertStatements = GetInsertDataStatemens();
           foreach (var query in insertStatements)
           {
               ExecuteNonQuery(connection, query);
           }
        }

       private static void ExecuteNonQuery(SqlConnection connection, string query)
       {
           using (var command = new SqlCommand(query, connection))
           {
               
              command.ExecuteNonQuery();
           }
        }

        private static string[] GetInsertDataStatemens()
        {
            var result = new string[]
            {
                "INSERT INTO Countries ([Name]) VALUES('Bulgaria'),('England'),('Cyprus'),('Germany'),('Norway')",

                "INSERT INTO Towns([Name], CountryCode) VALUES('Plovdiv', 1),('Varna', 1),('Burgas', 1),('Sofia', 1),('London', 2),('Southampton', 2),('Bath', 2),('Liverpool', 2),('Berlin', 3),('Frankfurt', 3),('Oslo', 4)",

                "INSERT INTO Minions(Name, Age, TownId) VALUES('Bob', 42, 3),('Kevin', 1, 1),('Bob ', 32, 6),('Simon', 45, 3),('Cathleen', 11, 2),('Carry ', 50, 10),('Becky', 125, 5),('Mars', 21, 1),('Misho', 5, 10),('Zoe', 125, 5),('Json', 21, 1)",

                "INSERT INTO EvilnessFactors(Name) VALUES('Super good'),('Good'),('Bad'), ('Evil'),('Super evil')",

                "INSERT INTO Villains(Name, EvilnessFactorId) VALUES('Gru', 2),('Victor', 1),('Jilly', 3),('Miro', 4),('Rosen', 5),('Dimityr', 1),('Dobromir', 2)",

                "INSERT INTO MinionsVillains(MinionId, VillainId) VALUES(4, 2),(1, 1),(5, 7),(3, 5),(2, 6),(11, 5),(8, 4),(9, 7),(7, 1),(1, 3),(7, 3),(5, 3),(4, 3),(1, 2),(2, 1),(2, 7)"
            };

            return result;
        }

        private static string[] GetCreateTableStatements()
        {
            var result = new string[]
            {
                "CREATE TABLE Countries (Id INT PRIMARY KEY IDENTITY,Name VARCHAR(50))",

                "CREATE TABLE Towns(Id INT PRIMARY KEY IDENTITY,Name VARCHAR(50), CountryCode INT FOREIGN KEY REFERENCES Countries(Id))",

                "CREATE TABLE Minions(Id INT PRIMARY KEY IDENTITY,Name VARCHAR(30), Age INT, TownId INT FOREIGN KEY REFERENCES Towns(Id))",

                "CREATE TABLE EvilnessFactors(Id INT PRIMARY KEY IDENTITY, Name VARCHAR(50))",

                "CREATE TABLE Villains (Id INT PRIMARY KEY IDENTITY, Name VARCHAR(50), EvilnessFactorId INT FOREIGN KEY REFERENCES EvilnessFactors(Id))",

                "CREATE TABLE MinionsVillains (MinionId INT FOREIGN KEY REFERENCES Minions(Id),VillainId INT FOREIGN KEY REFERENCES Villains(Id),CONSTRAINT PK_MinionsVillains PRIMARY KEY (MinionId, VillainId))"
            };
            return result;
        }
    }
}
