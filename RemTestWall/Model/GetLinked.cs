using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RemTestWall.Model
{
    internal class GetLinked
    {
        public static List<LinkedFileInfo> GetAllLinkedFiles(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            var linkedFiles = new List<LinkedFileInfo>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(RevitLinkInstance));

            foreach (var element in collector)
            {
                RevitLinkInstance linkInstance = element as RevitLinkInstance;
                if (linkInstance != null)
                {
                    Document linkedDoc = linkInstance.GetLinkDocument();
                    if (linkedDoc != null)
                    {
                        string fileName = linkedDoc.Title;
                        string parameterValue = linkInstance.get_Parameter(BuiltInParameter.RVT_LINK_INSTANCE_NAME)?.AsString();

                        linkedFiles.Add(new LinkedFileInfo
                        {
                            FileName = $"Link: {fileName}",
                            ParameterName = parameterValue,
                            LinkInstanceId = linkInstance.Id 
                        });
                    }
                }
            }
            return linkedFiles.OrderBy(file => file.FileName).ToList();
        }

        internal static IEnumerable<LinkedFileInfo> GetAllLinkedFiles(Document document)
        {
            throw new NotImplementedException();
        }
    }
}
