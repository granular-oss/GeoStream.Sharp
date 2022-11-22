// See https://aka.ms/new-console-template for more information
using GeoStream.Sharp;

namespace GeoStream.Sharp.CLI;

class Program
{
    public static int Main(string[] args)
    {
        using (var fileStream = File.Open(args[0], FileMode.Open))
        using (var openStream = File.Open(args[1], FileMode.Create))
        {
            // reader
            var binaryReader = new BinaryReader(fileStream);
            var geoStream = new GeoStreamReader(binaryReader);
            // writer
            var binaryWriter = new BinaryWriter(openStream);
            var geoStreamWrite = new GeoStreamWriter(binaryWriter);
            foreach (var feature in geoStream.Features())
            {
                var jsonStr = feature.AsGeoJson();
                Console.WriteLine(jsonStr);
                geoStreamWrite.WriteFeature(feature);
            }
        }

        return 0;
    }
}
