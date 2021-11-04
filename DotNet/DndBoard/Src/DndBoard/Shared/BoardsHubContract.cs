namespace DndBoard.Shared
{
    public static class BoardsHubContract
    {
        public const string BaseAddress = "/boardshub";

        public const string RequestAllModels = nameof(RequestAllModels);
        public const string RequestAllCoords = nameof(RequestAllCoords);

        public const string AddModels = nameof(AddModels);
        public const string ModelsAdded = nameof(ModelsAdded);
        public const string DeleteModel = nameof(DeleteModel);
        public const string ModelDeleted = nameof(ModelDeleted);

        public const string CoordsChanged = nameof(CoordsChanged);
        public const string IconInstanceRemoved = nameof(IconInstanceRemoved);

        public const string Connect = nameof(Connect);
        public const string Connected = nameof(Connected);
    }
}
