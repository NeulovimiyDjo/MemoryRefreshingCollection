using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DndBoard.Shared.Models;

namespace DndBoard.ServerCommon.Hubs
{
    public class Board
    {
        public ConcurrentDictionary<string, ModelFile> ModelsFiles { get; init; } = new();
        public ConcurrentDictionary<string, DndIcon> IconsInstances { get; init; } = new();
        public string BoardId { get; set; }


        public string AddIconInstance(DndIcon icon)
        {
            string id = Guid.NewGuid().ToString();
            icon.InstanceId = id;
            IconsInstances.TryAdd(id, icon);
            return id;
        }

        public void ChangeIconInstanceCoords(string instanceId, Coords newCoords)
        {
            IconsInstances[instanceId].Coords = newCoords;
        }


        public void DeleteModelFile(string modelId)
        {
            ModelsFiles.Remove(modelId, out _);
        }

        public string AddModelFile(ModelFile model)
        {
            string id = Guid.NewGuid().ToString();
            model.ModelId = id;
            ModelsFiles.TryAdd(id, model);
            return id;
        }
    }
}
