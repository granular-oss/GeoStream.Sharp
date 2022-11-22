using PeterO.Cbor;
using NetTopologySuite.Geometries;

namespace GeoStream.Sharp;

public struct Feature
{
    public Feature(Geometry geometry, CBORObject properties)
    {
        this.Geometry = geometry;
        this.Properties = properties;
    }

    public Geometry Geometry { get; set; }
    public int SRID {
        get {
            return this.Geometry.SRID;
        }
    }

    public string WKT
    {
        get
        {
            return this.Geometry.ToString();
        }
    }
    public byte[] WKB
    {
        get
        {
            return this.Geometry.ToBinary();
        }
    }

    public CBORObject Properties { get; set; }
}
