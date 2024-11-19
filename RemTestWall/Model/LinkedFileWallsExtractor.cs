using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace RemTestWall.Model
{
    public class LinkedFileWallsExtractor
    {
        public static List<WallInfo> GetWallsInRangeForLink(Document document, LinkedFileInfo linkedFile, double minElevation, double maxElevation, double tolerance = 0.03)
        {
            List<WallInfo> wallsInRange = new List<WallInfo>();

            var linkInstance = new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .FirstOrDefault(li => li.Id == linkedFile.LinkInstanceId);

            if (linkInstance != null)
            {
                var linkDoc = linkInstance.GetLinkDocument();
                if (linkDoc == null)
                {
                    TaskDialog.Show("Error", "Related file is not uploaded");
                    return wallsInRange;
                }

                Transform transform = linkInstance.GetTransform();

                var wallElements = new FilteredElementCollector(linkDoc)
                    .OfClass(typeof(Wall))
                    .Cast<Wall>();

                foreach (var wall in wallElements)
                {
                    var boundingBox = wall.get_BoundingBox(null);
                    if (boundingBox == null) continue;

                    var transformedBoundingBox = TransformBoundingBox(boundingBox, transform);

                    double wallMinZ = transformedBoundingBox.Min.Z;
                    double wallMaxZ = transformedBoundingBox.Max.Z;

                    if (wallMinZ >= minElevation - tolerance && wallMaxZ <= maxElevation + tolerance)
                    {
                        wallsInRange.Add(new WallInfo
                        {
                            WallId = wall.Id.Value,
                            ModelName = linkDoc.Title,
                            LinkInstanceId = linkInstance.Id.Value
                        });
                    }
                }
            }
            else
            {
                TaskDialog.Show("Error", "It was not possible to find the chosen connection.");
            }

            return wallsInRange;
        }

        public static List<WallInfo> GetWallsInRange(Document document, double minElevation, double maxElevation, double tolerance = 0.03)
        {
            List<WallInfo> wallsInRange = new List<WallInfo>();

            var linkInstances = new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>();

            foreach (var linkInstance in linkInstances)
            {
                var linkDoc = linkInstance.GetLinkDocument();
                if (linkDoc == null) continue;

                Transform transform = linkInstance.GetTransform();

                var wallElements = new FilteredElementCollector(linkDoc)
                    .OfClass(typeof(Wall))
                    .Cast<Wall>();

                foreach (var wall in wallElements)
                {
                    var boundingBox = wall.get_BoundingBox(null);
                    if (boundingBox == null) continue;

                    var transformedBoundingBox = TransformBoundingBox(boundingBox, transform);

                    double wallMinZ = transformedBoundingBox.Min.Z;
                    double wallMaxZ = transformedBoundingBox.Max.Z;

                    if (wallMinZ >= minElevation - tolerance && wallMaxZ <= maxElevation + tolerance)
                    {
                        wallsInRange.Add(new WallInfo
                        {
                            WallId = wall.Id.Value,
                            ModelName = linkDoc.Title,
                            LinkInstanceId = linkInstance.Id.Value
                        });
                    }
                }
            }

            return wallsInRange;
        }

        private static BoundingBoxXYZ TransformBoundingBox(BoundingBoxXYZ boundingBox, Transform transform)
        {
            return new BoundingBoxXYZ
            {
                Min = transform.OfPoint(boundingBox.Min),
                Max = transform.OfPoint(boundingBox.Max)
            };
        }
    }
}
