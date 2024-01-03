using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static async Task  Main(string[] args)

    {
        var url = "https://www.daviva.lt/en/2-pagrindinis?q=Make-Audi-BMW-Bentley-CPI-Cadillac-Chevrolet-Citroen-DAF-Dacia-Fiat-Ford/Model-1+Series-2-3-4+Series-4.107-6-7+Series-9--3-25-57-106/Year-1957-1960-1963-1965-1967-1968-1969-1970-1971-1980-2017/Fuel+type-Diesel-Electric+Motor-Full+Hybrid-Full+Hybrid+%28Petrol%29-Mild+Hybrid-Mild+Hybrid+%28Petrol%29-Petrol-Plug--In+Hybrid-Range+Extender-Wankel-Wankel+%28Petrol%29/Engine+displacement+-0.1+%28110--125cm%C2%B3%29-0.6+%28569--650cm%C2%B3%29-0.7+%28652--748cm%C2%B3%29-0.8+%28750--848cm%C2%B3%29-0.9+%28870--948cm%C2%B3%29-1.0+%28954--1050cm%C2%B3%29-1.1+%281051--1149cm%C2%B3%29-1.2+%281150--1250cm%C2%B3%29-1.4+%281351--1449cm%C2%B3%29-1.5+%281451--1548cm%C2%B3%29-1.6+%281555--1647cm%C2%B3%29";
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var activeSearchFilterSection = htmlDocument.DocumentNode.SelectNodes("//section[@id='products']//div[@class='hidden-sm-down']//section[@id='js-active-search-filters']/ul/li[contains(@class, 'filter-block')]");

        if (activeSearchFilterSection != null)
        {
            List<CarItem> carItems = new List<CarItem>();

            int numItems = activeSearchFilterSection.Count / 5; 

            for (int i = 0; i < numItems; i++)
            {
                var make = CleanUpText(activeSearchFilterSection[i]?.InnerText);
                var model = CleanUpText(activeSearchFilterSection[i + numItems]?.InnerText.Trim());
                var year = CleanUpText(activeSearchFilterSection[i + 2 * numItems]?.InnerText.Trim());
                var fuelType = CleanUpText(activeSearchFilterSection[i + 3 * numItems]?.InnerText.Trim());
                var engine = CleanUpText(activeSearchFilterSection[i + 4 * numItems]?.InnerText.Trim());

                var carItem = new CarItem
                {
                    Make = ExtractValueFromColonSeparatedString(make),
                    Model = ExtractValueFromColonSeparatedString(model),
                    Year = ExtractValueFromColonSeparatedString(year),
                    FuelType = ExtractValueFromColonSeparatedString(fuelType),
                    Engine = ExtractValueFromColonSeparatedString(engine)
                };

                carItems.Add(carItem);
            }

           
            SaveToCsv(carItems, "output.csv");
        }
        else
        {
            Console.WriteLine("Active search filter section not found.");
        }
    }
    static string CleanUpText(string input)
    {
       
        return input?.Trim().Replace("\n", "");
    }

    static void SaveToCsv(List<CarItem> data, string filePath)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
              
                sw.WriteLine("Make,Model,Year,FuelType,Engine Power");

                foreach (var carItem in data)
                {
                   
                    sw.WriteLine($"{carItem.Make},{carItem.Model},{carItem.Year},{carItem.Engine}");
                }

                Console.WriteLine($"Data saved to {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
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
    public string FuelType { get; set; }
    public string Engine { get; set; }
}

