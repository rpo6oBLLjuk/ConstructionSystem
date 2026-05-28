using System.Collections.Generic;
using System.Linq;

public class ProjectDataSaver : AbstractSaver<ProjectData>
{
    public ProjectDataSaver() : base("S3/Blueprints") { }

    public List<ProjectData> LoadAllBlueprints() => GetAllSaveNames()
        .Select((saveName) => Load(saveName))
        .ToList();

    public string GetSaveNameByUserId(int id, string saveName) => $"{id}/{saveName}";

    public override bool Save(ProjectData obj, string saveName)
    {
        return base.Save(obj, saveName);
    }

    public override ProjectData Load(string saveName)
    {
        return base.Load(saveName);
    }

    public override bool Rename(string oldName, string newName)
    {
        return base.Rename(oldName, newName);
    }

    public override bool DeleteSave(string saveName)
    {
        return base.DeleteSave(saveName);
    }
}
