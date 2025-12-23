using System.ComponentModel;
using System.Text.Json;

namespace ContosoBikestore.Agent.Host.Tools;

public static class ProductInventoryTools
{
    private static readonly string HardcodedBikes = """
    [
      {
        "id": 4821,
        "ProductName": "Contoso Roadster 200",
        "Description": "Lightweight road bike for speed and endurance.",
        "PriceUSD": 899.99,
        "Tags": [ "road", "lightweight", "racing" ],
        "City": "Sydney",
        "Branch": "CBD",
        "StockAvailability": 12
      },
      {
        "id": 1937,
        "ProductName": "Contoso Mountain X1",
        "Description": "Durable mountain bike for tough terrains.",
        "PriceUSD": 1199.99,
        "Tags": [ "mountain", "durable", "off-road" ],
        "City": "Sydney",
        "Branch": "Parramatta",
        "StockAvailability": 8
      },
      {
        "id": 7052,
        "ProductName": "Contoso City Cruiser",
        "Description": "Comfortable bike for daily city commuting.",
        "PriceUSD": 499.99,
        "Tags": [ "city", "commuter", "comfort" ],
        "City": "Melbourne",
        "Branch": "CBD",
        "StockAvailability": 15
      },
      {
        "id": 8640,
        "ProductName": "Contoso Kids Sprint",
        "Description": "Safe and colorful bike designed for children.",
        "PriceUSD": 299.99,
        "Tags": [ "kids", "safety", "colorful" ],
        "City": "Melbourne",
        "Branch": "Fitzroy",
        "StockAvailability": 20
      },
      {
        "id": 1204,
        "ProductName": "Contoso E-Bike Pro",
        "Description": "Electric bike with long battery life and fast charge.",
        "PriceUSD": 1799.99,
        "Tags": [ "electric", "battery", "commute" ],
        "City": "Brisbane",
        "Branch": "CBD",
        "StockAvailability": 5
      },
      {
        "id": 3378,
        "ProductName": "Contoso Gravel 360",
        "Description": "Versatile gravel bike for mixed surfaces.",
        "PriceUSD": 1099.99,
        "Tags": [ "gravel", "versatile", "hybrid" ],
        "City": "Brisbane",
        "Branch": "South Bank",
        "StockAvailability": 10
      },
      {
        "id": 5291,
        "ProductName": "Contoso Folding Go",
        "Description": "Compact folding bike for easy storage and travel.",
        "PriceUSD": 649.99,
        "Tags": [ "folding", "compact", "travel" ],
        "City": "Perth",
        "Branch": "CBD",
        "StockAvailability": 7
      },
      {
        "id": 6143,
        "ProductName": "Contoso Tandem Duo",
        "Description": "Tandem bike for two riders, fun for couples.",
        "PriceUSD": 1499.99,
        "Tags": [ "tandem", "couples", "fun" ],
        "City": "Perth",
        "Branch": "Fremantle",
        "StockAvailability": 3
      },
      {
        "id": 2785,
        "ProductName": "Contoso Fat Tire FX",
        "Description": "Fat tire bike for sand and snow adventures.",
        "PriceUSD": 1299.99,
        "Tags": [ "fat tire", "adventure", "snow" ],
        "City": "Adelaide",
        "Branch": "Glenelg",
        "StockAvailability": 6
      },
      {
        "id": 4920,
        "ProductName": "Contoso Classic 50",
        "Description": "Vintage-style bike with modern components.",
        "PriceUSD": 799.99,
        "Tags": [ "vintage", "classic", "stylish" ],
        "City": "Adelaide",
        "Branch": "CBD",
        "StockAvailability": 9
      }
    ]
    """;

    [Description("Get all available bikes from the Contoso bike store")]
    public static string GetAvailableBikes()
    {
        return HardcodedBikes;
    }

    [Description("Get full details for a bike by its name or ID, including price, description, and stock information")]
    public static string GetBikeDetails(
        [Description("The name or ID of the bike to find (e.g., 'Contoso Mountain X1' or '1937')")]
        string bikeNameOrId)
    {
        try
        {
            var bikes = JsonSerializer.Deserialize<List<BikeData>>(HardcodedBikes);
            if (bikes == null) return JsonSerializer.Serialize(new { error = "Failed to load bike data" });

            BikeData? bike = null;

            // Try to parse as ID first
            if (int.TryParse(bikeNameOrId, out int id))
            {
                bike = bikes.FirstOrDefault(b => b.id == id);
            }

            // If not found by ID, search by name (case-insensitive, partial match)
            if (bike == null)
            {
                bike = bikes.FirstOrDefault(b => 
                    b.ProductName.Contains(bikeNameOrId, StringComparison.OrdinalIgnoreCase));
            }

            if (bike == null)
            {
                return JsonSerializer.Serialize(new { error = $"Bike not found: {bikeNameOrId}" });
            }

            return JsonSerializer.Serialize(bike);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Error retrieving bike details: {ex.Message}" });
        }
    }

    private class BikeData
    {
        public int id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PriceUSD { get; set; }
        public List<string> Tags { get; set; } = new();
        public string City { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public int StockAvailability { get; set; }
    }
}
