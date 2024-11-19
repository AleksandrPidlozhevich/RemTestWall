#region Usings
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RemTestWall.Views;
using System;
using System.IO;
using System.Windows.Media.Imaging;
#endregion

namespace RemTestWall
{
    [Transaction(TransactionMode.Manual)]
    public class App : IExternalApplication, IExternalCommand
    {
        public static string assemblyPath;
        public static string assemblyFolder;

        public Result OnStartup(UIControlledApplication application)
        {
            assemblyPath = typeof(App).Assembly.Location;
            assemblyFolder = Path.GetDirectoryName(assemblyPath);
            try
            {
                CreateAddInsButton(application, "RemTestWall");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void CreateAddInsButton(UIControlledApplication application, string assemblyName)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("TestWallCommand");
            PushButtonData buttonData = new PushButtonData("TestWallCommand", "Show Levels", assemblyPath, "RemTestWall.App");
            buttonData.LargeImage = LoadImage(Path.Combine(assemblyFolder, "Resources", assemblyName + "_large.png"));
            buttonData.Image = LoadImage(Path.Combine(assemblyFolder, "Resources", assemblyName + "_small.png"));
            ribbonPanel.AddItem(buttonData);
        }

        private BitmapImage LoadImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            return null;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var mainWindow = new MainWindow(commandData.Application);
                mainWindow.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }
        }
    }
}
