using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReadAndWriteCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            
			var importer = new Importer();
			importer.Mappings = new List<KeyValuePair<string, string>>{
			new KeyValuePair<string,string>("UserId","UserId"),
			new KeyValuePair<string,string>("First Last Name","FirstLastName"),
			new KeyValuePair<string,string>("Version","Version"),
			new KeyValuePair<string,string>("Insurance","Insurance"),
			};

			var list = importer.Import<User>(AppDomain.CurrentDomain.BaseDirectory +"\\CSVFile\\Book1.csv");


			var listForExport = importer.OrderAndRemoveDuplicate(list);

			var exportCsv = new ExportToCSV();

			exportCsv.Export<User>(listForExport, AppDomain.CurrentDomain.BaseDirectory + "\\CSVFile\\Book1.csv");

		}


		
	}

    public class User
    {
        public string UserId { get; set; }
        public string FirstLastName { get; set; }
        public int Version { get; set; }
        public string Insurance { get; set; }
    }

	public class Importer
	{
		public List<KeyValuePair<string, string>> Mappings;

		public List<T> Import<T>(string file)
		{
			List<T> list = new List<T>();
			List<string> lines = System.IO.File.ReadAllLines(file).ToList();
			string headerLine = lines[0];
			var headerInfo = headerLine.Split(',').ToList().Select((v, i) => new { ColName = v, ColIndex = i });

			Type type = typeof(T);
			var properties = type.GetProperties();

			var dataLines = lines.Skip(1);
			dataLines.ToList().ForEach(line => {
				var values = line.Split(',');
				T obj = (T)Activator.CreateInstance(type);

				//set values to obj properties from csv columns
				foreach (var prop in properties)
				{
					//find mapping for the prop
					var mapping = Mappings.SingleOrDefault(m => m.Value == prop.Name);
					var colName = mapping.Key;
					var colIndex = headerInfo.SingleOrDefault(s => s.ColName == colName).ColIndex;
					var value = values[colIndex];
					var propType = prop.PropertyType;
					prop.SetValue(obj, Convert.ChangeType(value, propType));
				}

				list.Add(obj);
			});

			return list;
		}

		public List<User> OrderAndRemoveDuplicate(List<User> param)
		{
			var listSorted = param.OrderBy(x => x.FirstLastName).ToList();

			return listSorted.OrderByDescending(e => e.Version)
			 .GroupBy(e => e.UserId)
			 .Select(g => g.First()).ToList();

        }
    }

    public class ExportToCSV 
	{

		public void Export<T>(List<T> list, string file)
		{
			var lines = GetLines(list);
			System.IO.File.WriteAllLines(file, lines);
		}

		private List<string> GetLines<T>(List<T> list)
		{
			var type = typeof(T);
			var props = type.GetProperties();

			string header = "";
			var firstDone = false;
			foreach (var prop in props)
			{
				if (!firstDone)
				{
					header += prop.Name;
					firstDone = true;
				}
				else
				{
					header += "," + prop.Name;
				}
			}


			List<string> lines = new List<string>();
			lines.Add(header);
			foreach (var obj in list)
			{
				firstDone = false;
				string line = "";
				foreach (var prop in props)
				{
					var value = prop.GetValue(obj).ToString();
					if (typeof(string) == prop.PropertyType)
					{
						value = "\"" + value + "\"";
					}

					if (!firstDone)
					{
						line += value;
						firstDone = true;
					}
					else
					{
						line += "," + value;
					}
				}
				lines.Add(line);
			}

			return lines;
		}
	}





}
