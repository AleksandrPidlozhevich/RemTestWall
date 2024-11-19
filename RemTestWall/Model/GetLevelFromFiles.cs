using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RemTestWall.Model
{
    public class GetLevelsFromFiles
    {
        public static List<Level> GetLevels(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(level => level.Elevation)
                .ToList();

            return levels;
        }
    }
}

