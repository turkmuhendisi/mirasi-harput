using System;

[Serializable]
public class LocationQrRegistryData
{
    public int schemaVersion = 1;
    public string appPayloadPrefix = "mirasi-harput://v1/location/";
    public string qrImageFolder = "QrImages";
    public LocationQrEntry[] locations;
}
