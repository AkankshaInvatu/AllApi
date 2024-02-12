using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)

    {
        var url = "https://www.daviva.lt/en/2-pagrindinis?q=Make-Alfa+Romeo-Audi-BMW-Bentley-CPI-Cadillac/Model-1+Series-2-2+Series-3-3+Series-4+Series-4.107-5-5+Series-6-6+Series-7+Series-8-9--3-9--5-25-45-57-75-80-90-100-106-107-123-124-125-145-147-155-156-159-166-190-200-200SX-206-206+Van-207-207+Cc-207+Sw-208-240-300C-306-307-307+SW-308-308+CC-308+SW-323-400-400--Series-406-407-407+SW-500-508+SW-960-1007/Year-1957-1960-1963-1965-1967-1968-1969-1970-1971-1972-1973-1974-1975-1977-1978-1979-1980-1981-1982-1983-1984-1985-1986-1987-1988-1989-1990-1991-1992-1993-1994-1995-1996-1997-1998-1999-2000-2001-2002-2003-2004-2005-2006-2007-2008-2009-2010-2011-2012-2013-2014-2015-2016-2017-2018-2019-2020-2021-2022-2023";
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var activeSearchFilterSection = htmlDocument.DocumentNode.SelectNodes("//section[@id='products']//div[@class='hidden-sm-down']//section[@id='js-active-search-filters']/ul/li[contains(@class, 'filter-block')]");
        int num = activeSearchFilterSection.Count;
        if (activeSearchFilterSection != null)
        {
            List<CarItem> carItems = new List<CarItem>();

            int numElementsPerPair = num%10;
            int numyear = (numElementsPerPair * 10) + numElementsPerPair;
            for (int i = 0; i < numElementsPerPair; i++)
            {
                var make = CleanUpText(activeSearchFilterSection[i]?.InnerText);

          
                for (int j = 0; j < 10; j++)
                {
                    var modelIndex = i * 10 + j;
                    var model = CleanUpText(activeSearchFilterSection[modelIndex + numElementsPerPair]?.InnerText.Trim());
                    var year = CleanUpText(activeSearchFilterSection[modelIndex + numyear]?.InnerText.Trim());

                    var carItem = new CarItem
                    {
                        Make = ExtractValueFromColonSeparatedString(make),
                        Model = ExtractValueFromColonSeparatedString(model),
                        Year = ExtractValueFromColonSeparatedString(year)
                       
                    };
                    carItems.Add(carItem);

                  
                    Console.WriteLine($"Make: {carItem.Make}, Model: {carItem.Model}, Year: {carItem.Year}");
                }
            }
            WriteToCsv(carItems, "output.csv");

        }
    }
    static void WriteToCsv(IEnumerable<CarItem> carItems, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
         
            csv.WriteRecord(new { Make = "make", Model = "model", Year = "year" });
            csv.NextRecord();

         
            foreach (var carItem in carItems)
            {
               
                    csv.WriteRecord(new { Make = carItem.Make, Model = carItem.Model , Year = carItem.Year });
                    csv.NextRecord();
                
            }
        }
    }

   
    static string CleanUpText(string input)
    {
       
        return input?.Trim().Replace("\n", "");
    }

    ]
    static string ExtractValueFromColonSeparatedString(string input)
    {
       
        string withoutEntities = System.Net.WebUtility.HtmlDecode(input);

       
        string trimmedValue = Regex.Replace(withoutEntities, @"\s+", " ").Trim().Replace("", "").Trim();

       
        int colonIndex = trimmedValue.IndexOf(':');
        string result = colonIndex != -1 ? trimmedValue.Substring(colonIndex + 1).Trim() : string.Empty;

        return result;
    }
}
public class CarItem
{
    public string Make { get; set; }
    public string Model { get; set; }

    public string Year { get; set; }

}

