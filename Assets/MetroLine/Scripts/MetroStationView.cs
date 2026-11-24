using UnityEngine;

namespace MetroLine
{
    public sealed class MetroStationView : MonoBehaviour
    {
        public int StationId;
        public string StationName;
        public MetroShape Shape;
        public Color StationColor = Color.white;

        public void Configure(int id, string stationName, MetroShape shape, Color color)
        {
            StationId = id;
            StationName = stationName;
            Shape = shape;
            StationColor = color;
            name = $"Station_{id:00}_{stationName}_{shape}";
        }
    }
}
