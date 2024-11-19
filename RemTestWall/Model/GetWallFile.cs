using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace RemTestWall.Model
{
    public static class GetWallFile
    {
        public static List<WallInfo> GetWallsBetweenLevelsByBoundingBox(Document document, double minElevation, double maxElevation, double tolerance = 0.03)
        {
            var walls = new List<WallInfo>();

            var wallElements = new FilteredElementCollector(document)
                .OfClass(typeof(Wall))
                .Cast<Wall>();

            foreach (var wall in wallElements)
            {
                var boundingBox = wall.get_BoundingBox(null);
                if (boundingBox == null) continue;

                double wallMinZ = boundingBox.Min.Z;
                double wallMaxZ = boundingBox.Max.Z;

                if (wallMinZ >= minElevation - tolerance && wallMaxZ <= maxElevation + tolerance)
                {
                    walls.Add(new WallInfo
                    {
                        WallId = wall.Id.Value,
                        ModelName = document.Title,
                        LinkInstanceId = null
                    });
                }
            }
            return walls;
        }
    }
}
