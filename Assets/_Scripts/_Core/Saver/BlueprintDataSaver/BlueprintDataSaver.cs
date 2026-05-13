using System;
using System.Collections.Generic;
using System.Linq;

public class BlueprintDataSaver : AbstractSaver<BlueprintData>
{
    public BlueprintDataSaver() : base("Blueprints") { }

    public override bool Save(BlueprintData obj, string saveName)
    {
        obj.name = saveName;
        obj.editTime = DateTime.Now.ToString();
        return base.Save(obj, saveName);
    }

    public override bool Rename(string oldName, string newName)
    {
        bool result = base.Rename(oldName, newName);

        BlueprintData data = Load(newName);
        data.name = newName;
        Save(data, data.name);

        return result;
    }

    public List<BlueprintData> LoadAllBlueprints() => GetAllSaveNames()
        .Select((saveName) => Load(saveName))
        .ToList();
}
