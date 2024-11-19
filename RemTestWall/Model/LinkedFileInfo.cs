using Autodesk.Revit.DB;

namespace RemTestWall.Model
{
    public class LinkedFileInfo
    {
        public string FileName { get; set; }
        public string ParameterName { get; set; }
        public ElementId LinkInstanceId { get; set; }

        public string DisplayName => string.IsNullOrEmpty(ParameterName)
            ? FileName
            : $"{FileName} ({ParameterName})";
    }
}
