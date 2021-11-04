namespace DndBoard.Shared
{
    public static class BoardsHubContract
    {
        public const string BaseAddress = "/boardshub";

        public const string NotifyIconsModelsUpdate = nameof(NotifyIconsModelsUpdate);
        public const string RequestAllCoords = nameof(RequestAllCoords);
        public const string CoordsChanged = nameof(CoordsChanged);
        public const string IconInstanceRemoved = nameof(IconInstanceRemoved);
        public const string Connect = nameof(Connect);
        public const string Connected = nameof(Connected);
    }
}
