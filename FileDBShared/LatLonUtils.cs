using System;

namespace FileDBShared;

public static class LatLonUtils
{
    public static double CalculateDistance(double point1Lat, double point1Lon, double point2Lat, double point2Long)
    {
        var d1 = point1Lat * (Math.PI / 180.0);
        var num1 = point1Lon * (Math.PI / 180.0);
        var d2 = point2Lat * (Math.PI / 180.0);
        var num2 = point2Long * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                 Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }
}
