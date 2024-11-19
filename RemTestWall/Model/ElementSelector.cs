using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace RemTestWall.Model
{
    public class ElementSelector
    {
        private readonly UIApplication _app;

        public ElementSelector(UIApplication app)
        {
            _app = app;
        }

        public void SelectElements(List<Reference> references)
        {
            if (references == null || references.Count == 0) return;

            try
            {
                _app.ActiveUIDocument.Selection.SetReferences(references);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
            }
        }

        public Reference CreateWallReference(RevitLinkInstance linkInstance, Wall wall, Document doc)
        {
            try
            {
                if (linkInstance != null)
                {
                    Reference wallRef = new Reference(wall);
                    string stableRepresentation = wallRef
                        .CreateLinkReference(linkInstance)
                        .ConvertToStableRepresentation(doc)
                        .Replace(":RVTLINK", ":0:RVTLINK");

                    return Reference.ParseFromStableRepresentation(doc, stableRepresentation);
                }
                else
                {
                    return new Reference(wall);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error occurred while creating wall reference: {ex.Message}");
                return null;
            }
        }
    }
}

