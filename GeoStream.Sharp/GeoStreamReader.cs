using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using PeterO.Cbor;


namespace GeoStream.Sharp;
public class GeoStreamReader
{
    private BinaryReader stream;
    // Header Info
    public uint Version { get; private set; }
    public uint SRID { get; private set; }
    public uint PropertiesLength { get; private set; }

    public GeoStreamReader(BinaryReader binaryReader)
    {
        this.stream = binaryReader;
        this.readHeader();
        if (this.SRID != 4326)
        {
            // probably something better but this works
            throw new InvalidDataException("SRID not 4326");
        }
        if (this.Version != 4)
        {
            // probably something better but this works
            throw new InvalidDataException("Version not 4");
        }
    }

    private void readHeader()
    {
        this.Version = this.stream.ReadUInt32();
        this.SRID = this.stream.ReadUInt32();
        this.PropertiesLength = this.stream.ReadUInt32();
        if (this.PropertiesLength > 0)
        {
            this.stream.ReadBytes((int)this.PropertiesLength);
        }
    }

    public IEnumerable<Feature> Features()
    {
        var instance = new NetTopologySuite.NtsGeometryServices(
            NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
            new NetTopologySuite.Geometries.PrecisionModel(NetTopologySuite.Geometries.PrecisionModels.Floating),
            (int)this.SRID,
            NetTopologySuite.Geometries.GeometryOverlay.Legacy,
            new NetTopologySuite.Geometries.CoordinateEqualityComparer()
        );
        var rdr = new NetTopologySuite.IO.WKBReader(instance);
        while (true)
        {
            uint next_length, _;
            byte[] zippedBytes;
            try
            {
                next_length = this.stream.ReadUInt32();
                zippedBytes = this.stream.ReadBytes((int)next_length);
                _ = this.stream.ReadUInt32(); // same length for reverse reading
            }
            catch (EndOfStreamException)
            {
                yield break;
            }

            var zippedStream = new MemoryStream(zippedBytes);
            var decompressor = new InflaterInputStream(zippedStream);
            var cborFeature = CBORObject.Read(decompressor);
            var properties = cborFeature["properties"];
            var geometry = rdr.Read(cborFeature["geometry"].GetByteString());
            var feature = new Feature(geometry, properties);
            yield return feature;
        }
    }
}