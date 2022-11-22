using PeterO.Cbor;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace GeoStream.Sharp;

public struct Feature
{
    public Feature(Geometry geometry, CBORObject properties)
    {
        if (geometry == null)
        {
            throw new ArgumentNullException("geometry");
        }
        if (properties == null)
        {
            throw new ArgumentNullException("properties");
        }
        this.Geometry = geometry;
        this.Properties = properties;
    }

    public Geometry Geometry { get; } public CBORObject Properties { get; } public int SRID
    {
        get
        {
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

    public string AsGeoJson()
    {
        var geom = GeoJSON.Net.Contrib.Wkb.Conversions.WkbDecode.Decode(this.WKB);
        var props = this.Properties.ToObject<IDictionary<string,object>>();
        var obj = new GeoJSON.Net.Feature.Feature(geom);
        var geometryString = JsonConvert.SerializeObject(obj);
        var cborObject = CBORObject.FromJSONString(geometryString);
        cborObject["properties"] = this.Properties;
        var asString = cborObject.ToJSONString();
        return asString;
    }

    public Feature? FromGeoJson(string geoJsonString)
    {
        var geoJsonFeature = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.Feature>(geoJsonString);
        if (geoJsonFeature == null)
        {
            return null;
        }

        var instance = new NetTopologySuite.NtsGeometryServices(
            NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
            new NetTopologySuite.Geometries.PrecisionModel(NetTopologySuite.Geometries.PrecisionModels.Floating),
            (int)this.SRID,
            NetTopologySuite.Geometries.GeometryOverlay.Legacy,
            new NetTopologySuite.Geometries.CoordinateEqualityComparer()
        );
        var rdr = new NetTopologySuite.IO.WKBReader(instance);

        var wkbBytes = GeoJSON.Net.Contrib.Wkb.Conversions.WkbEncode.Encode(geoJsonFeature.Geometry);
        var geom = rdr.Read(wkbBytes);
        var cborObj = CBORObject.FromJSONString(geoJsonString);
        var feature = new Feature(geom, cborObj["properties"]);

        return feature;
    }

}
