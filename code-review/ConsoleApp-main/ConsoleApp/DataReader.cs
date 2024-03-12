using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp
{
    public class DataReader
    {
        // List to store imported objects
        public List<ImportedObject> ImportedObjects { get; } = new List<ImportedObject>();

        // Method to import data from file and print it
        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            try
            {
                // Using statement ensures proper stream disposal after usage
                using (var streamReader = new StreamReader(fileToImport))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var values = line.Split(';');

                        // Check if the line has the correct number of values
                        if (values.Length >= 7)
                        {
                            var importedObject = new ImportedObject
                            {
                                Type = values[0].Trim(),
                                Name = values[1].Trim(),
                                Schema = values[2].Trim(),
                                ParentName = values[3].Trim(),
                                ParentType = values[4].Trim(),
                                DataType = values[5].Trim(),
                                IsNullable = values[6].Trim()
                            };
                            ImportedObjects.Add(importedObject);
                        }
                    }
                }

                // Clean and correct data (remove whitespaces, convert to uppercase)
                ClearAndCorrectData();
                // Assign number of children for each object
                AssignNumberOfChildren();
                // Print the data
                PrintData();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Method to clear and correct data
        private void ClearAndCorrectData()
        {
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.Type = importedObject.Type.ToUpper().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Name = importedObject.Name.Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema.Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName.Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType.Replace(" ", "").Replace(Environment.NewLine, "");
            }
        }

        // Method to assign number of children for each object
        private void AssignNumberOfChildren()
        {
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.NumberOfChildren = ImportedObjects.Count(io => io.ParentType.Equals(importedObject.Type, StringComparison.OrdinalIgnoreCase) && io.ParentName == importedObject.Name);
            }
        }

        // Method to print the data
        private void PrintData()
        {
            foreach (var database in ImportedObjects.Where(io => io.Type.Equals("DATABASE", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                foreach (var table in ImportedObjects.Where(io => io.ParentType.Equals(database.Type, StringComparison.OrdinalIgnoreCase) && io.ParentName == database.Name))
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    foreach (var column in ImportedObjects.Where(io => io.ParentType.Equals(table.Type, StringComparison.OrdinalIgnoreCase) && io.ParentName == table.Name))
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }

            Console.ReadLine();
        }
    }

    // Class representing an imported object
    public class ImportedObject
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }
        public string ParentName { get; set; }
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; }
        public double NumberOfChildren { get; set; }
    }
}
