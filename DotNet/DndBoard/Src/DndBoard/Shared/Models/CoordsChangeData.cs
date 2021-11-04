namespace DndBoard.Shared.Models
{
    public record CoordsChangeData
    {
        public string InstanceId { get; set; }
        public Coords Coords { get; set; }
        public string ModelId { get; set; }
    }
}
