using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Entities;
using System.IO;
using Model = Tiled2ZXNext.Models;
using Entity = Tiled2ZXNext.Entities;
using System.Xml.Serialization;
using Tiled2ZXNext.Models;
using System.Reflection.Metadata;


namespace Tiled2ZXNext.Mapper
{
    public class ProjectMapper
    {
        public static void Map(Model.Project projectRaw, Entities.Project project, Options options)
        {
            project.Properties = PropertyMapper.Map(projectRaw.Properties);
            string inputPath = Path.GetDirectoryName(options.Input);
            project.RootFolder = inputPath;

            if (projectRaw.Properties.ExistProperty("Tables"))
            {
                project.Tables.Append<string, Table>(ResolveTables(projectRaw.Properties.GetProperty("Tables"), options.AppRoot));
            }

            if (projectRaw.Properties.ExistProperty("Includes"))
            {
                project.Includes.Append<string, Table>(ResolveTables(projectRaw.Properties.GetProperty("Includes"), options.AppRoot));
            }


            project.PropertyTypes = projectRaw.PropertyTypes.FindAll(f=>f.Type == "enum").ConvertAll<Entities.PropertyType>(x=> new Entities.PropertyType() { Id =x.Id, Name = x.Name, StorageType = x.StorageType, Type = x.Type, Values = x.Values, ValuesAsFlags = x.ValuesAsFlags});
        }

        /// <summary>
        /// check if tables are available and load definition
        /// </summary>
        /// <param name="props"></param>
        private static Dictionary<string, Table> ResolveTables(string prop, string appRoot)
        {

            string[] tablesRaw = prop.Split('\n');           
            Dictionary<string, Table> tables = new();
            foreach (string table in tablesRaw)
            {
                string[] tableParts = table.Split(':');
                Table tableSettings = new Table() { Name = tableParts[0], FilePath = tableParts[1] };
                tables.Add(tableSettings.Name, tableSettings);
                Console.WriteLine($"Table={tableSettings.Name} Path={tableSettings.FilePath}");
                // read the file content    
                List<string> tableData = new(File.ReadAllLines(tableSettings.FilePath.Replace("~", appRoot)));
                // find table begin
                int tableIndex = tableData.FindIndex(r => r.Contains("Table:" + tableSettings.Name, StringComparison.InvariantCultureIgnoreCase));
                // if table exists
                if (tableIndex > 0)
                {   // loop through content until find and empty line
                    for (int line = tableIndex + 1; line < tableData.Count; line++)
                    {
                        string item = tableData[line];
                        if (item == string.Empty)
                        {
                            break;
                        }
                        // get just the name
                        string[] itemData = item.Split(new char[] { ' ', '\t' });
                        tableSettings.Items.Add(itemData[0]);
                    }
                }
            }
            return tables;
        }
    }
}
