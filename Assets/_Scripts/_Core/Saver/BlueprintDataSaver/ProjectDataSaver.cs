using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ProjectDataSaver : AbstractSaver<ProjectData>
{
    public ProjectDataSaver() : base("S3/ProjectsData") { }

    public List<ProjectData> LoadAllBlueprints() => GetAllSaveNames().Select((saveName) => Load(saveName)).ToList();

    public string GetSaveNameByUserId(int id, string saveName)
    {
        string pathWithUserId = Path.Combine(BaseDirectory, id.ToString());
        if (!Directory.Exists(pathWithUserId))
            Directory.CreateDirectory(pathWithUserId);

        return Path.Combine(id.ToString(), saveName);
    }
}
