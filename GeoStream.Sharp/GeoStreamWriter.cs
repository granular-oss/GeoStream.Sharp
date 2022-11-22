using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using PeterO.Cbor;

namespace GeoStream.Sharp;

public class GeoStreamWriter
{
    private BinaryWriter stream;

    public GeoStreamWriter(BinaryWriter writer, CBORObject? properties = null)
    {
        this.stream = writer;
        this.stream.Write((uint)4U); // version 4
        this.stream.Write((uint)4326); // SRID 4326
        if (properties == null)
        {
            this.stream.Write((uint)0);
        }
        else
        {
            var propBytes = properties.EncodeToBytes();
            this.stream.Write((uint)propBytes.Length);
            this.stream.Write(propBytes);
        }
        this.stream.Flush();
    }

    public void WriteFeature(Feature feature)
    {
        if (feature.SRID != 4326 || feature.SRID == -1)
        {
            throw new ArgumentException(
                string.Format("feature has invalid SRID {0}", feature.SRID)
            );
        }
        var cborToDump = CBORObject
            .NewMap()
            .Add(Feature.GEOMETRY, feature.WKB)
            .Add(Feature.PROPERTIES, feature.Properties);
        var cborBytes = cborToDump.EncodeToBytes();
        var zippedStream = new MemoryStream();
        var deflaterStream = new DeflaterOutputStream(zippedStream);
        deflaterStream.Write(cborBytes);
        deflaterStream.Finish();
        var bytesToWrite = zippedStream.ToArray();
        var lengthOfBytesToWrite = (uint)bytesToWrite.Length;
        this.stream.Write(lengthOfBytesToWrite);
        this.stream.Write(bytesToWrite);
        this.stream.Write(lengthOfBytesToWrite);
        this.stream.Flush();
    }

    public void WriteFeatures(IEnumerable<Feature> features)
    {
        foreach (var feature in features)
        {
            this.WriteFeature(feature);
        }
    }
}
