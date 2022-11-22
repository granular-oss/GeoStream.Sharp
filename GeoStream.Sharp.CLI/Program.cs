// See https://aka.ms/new-console-template for more information
using GeoStream.Sharp;

namespace GeoStream.Sharp.CLI;
class Program
{
    public static int Main(string[] args)
    {
        using (var fileStream = File.Open(args[0], FileMode.Open))
        {
            var binaryReader = new BinaryReader(fileStream);
            var geoStream = new GeoStreamReader(binaryReader);
            foreach (var feature in geoStream.Features())
            {
                var jsonStr = feature.AsGeoJson();
                Console.WriteLine(jsonStr);
            }
        }

        return 0;
    }
}

