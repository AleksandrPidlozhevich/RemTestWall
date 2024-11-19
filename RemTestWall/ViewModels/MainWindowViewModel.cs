using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RemTestWall.Model;
using RemTestWall.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace RemTestWall.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<Level> Levels { get; set; }
        public ObservableCollection<Level> LevelsMin { get; set; }
        public ObservableCollection<Level> LevelsMax { get; set; }
        public ObservableCollection<LinkedFileInfo> LinkedFiles { get; set; }
        public ObservableCollection<WallInfo> Walls { get; set; }

        private int _foundWallCount;
        public int FoundWallCount
        {
            get => _foundWallCount;
            set
            {
                _foundWallCount = value;
                OnPropertyChanged();
            }
        }

        private Level _selectedLevelMin;
        public Level SelectedLevelMin
        {
            get => _selectedLevelMin;
            set
            {
                _selectedLevelMin = value;
                OnPropertyChanged();
                ValidateLevelSelection();
            }
        }

        private Level _selectedLevelMax;
        public Level SelectedLevelMax
        {
            get => _selectedLevelMax;
            set
            {
                _selectedLevelMax = value;
                OnPropertyChanged();
                ValidateLevelSelection();
            }
        }

        public LinkedFileInfo SelectedLinkedFile { get; set; }

        public RelayCommand GetWallsCommand { get; }
        public RelayCommand ChooseWallCommand { get; }

        private readonly Document _document;
        private readonly UIApplication _app;
        private readonly ElementSelector _elementSelector;

        public MainWindowViewModel(UIApplication app)
        {
            _document = app.ActiveUIDocument.Document;
            _app = app;
            _elementSelector = new ElementSelector(app);

            var allLevels = new FilteredElementCollector(_document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(level => level.Elevation)
                .ToList();

            Levels = new ObservableCollection<Level>(allLevels);
            LevelsMin = new ObservableCollection<Level>(allLevels.Take(allLevels.Count - 1));
            LevelsMax = new ObservableCollection<Level>(allLevels.Skip(1).Reverse());

            LinkedFiles = new ObservableCollection<LinkedFileInfo>
            {
                new LinkedFileInfo { FileName = "The current model", LinkInstanceId = ElementId.InvalidElementId },
                new LinkedFileInfo { FileName = "Link models" },
                new LinkedFileInfo { FileName = "Current and link models", LinkInstanceId = ElementId.InvalidElementId }
            };

            var linkedFiles = GetLinked.GetAllLinkedFiles(app).OrderBy(file => file.FileName);
            foreach (var file in linkedFiles)
            {
                LinkedFiles.Add(file);
            }

            Walls = new ObservableCollection<WallInfo>();

            GetWallsCommand = new RelayCommand(
                execute: _ => GetWallsBetweenLevels(),
                canExecute: _ => SelectedLevelMin != null && SelectedLevelMax != null && IsCurrentFile()
            );

            ChooseWallCommand = new RelayCommand(ChooseWall);
        }

        private void ValidateLevelSelection()
        {
            if (SelectedLevelMin == null || SelectedLevelMax == null) return;

            var minIndex = Levels.IndexOf(SelectedLevelMin);
            var maxIndex = Levels.IndexOf(SelectedLevelMax);

            if (minIndex >= maxIndex)
            {
                MessageBox.Show("The selected Min level should be lower than the Max.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                SelectedLevelMin = null;
                SelectedLevelMax = null;
            }
        }

        private bool IsCurrentFile()
        {
            return SelectedLinkedFile != null &&
                   (SelectedLinkedFile.FileName == "The current model" ||
                    SelectedLinkedFile.FileName == "Link models" ||
                    SelectedLinkedFile.FileName == "Current and link models" ||
                    SelectedLinkedFile.LinkInstanceId != ElementId.InvalidElementId);
        }

        private void GetWallsBetweenLevels()
        {
            try
            {
                if (SelectedLevelMin == null || SelectedLevelMax == null)
                {
                    MessageBox.Show("Please select levels.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                double minElevation = SelectedLevelMin.Elevation;
                double maxElevation = SelectedLevelMax.Elevation;
                double tolerance = 0.03;
                List<WallInfo> walls = new List<WallInfo>();

                if (SelectedLinkedFile.FileName == "The current model")
                {
                    walls = GetWallFile.GetWallsBetweenLevelsByBoundingBox(_document, minElevation, maxElevation, tolerance);
                }
                else if (SelectedLinkedFile.FileName == "Link models")
                {
                    walls = LinkedFileWallsExtractor.GetWallsInRange(_document, minElevation, maxElevation, tolerance);
                }
                else if (SelectedLinkedFile.FileName == "Current and link models")
                {
                    var currentModelWalls = GetWallFile.GetWallsBetweenLevelsByBoundingBox(_document, minElevation, maxElevation, tolerance);
                    var linkedModelWalls = LinkedFileWallsExtractor.GetWallsInRange(_document, minElevation, maxElevation, tolerance);

                    walls.AddRange(currentModelWalls);
                    walls.AddRange(linkedModelWalls);
                }
                else if (SelectedLinkedFile.LinkInstanceId != ElementId.InvalidElementId)
                {
                    walls = LinkedFileWallsExtractor.GetWallsInRangeForLink(_document, SelectedLinkedFile, minElevation, maxElevation, tolerance);
                }
                else
                {
                    MessageBox.Show("The selected mode is not supported.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Walls.Clear();
                foreach (var wall in walls)
                {
                    Walls.Add(wall);
                }

                FoundWallCount = Walls.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred when receiving walls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChooseWall(object parameter)
        {
            if (parameter is WallInfo wallInfo)
            {
                try
                {
                    Document doc = _app.ActiveUIDocument.Document;

                    if (wallInfo.LinkInstanceId.HasValue)
                    {
                        var linkInstance = new FilteredElementCollector(doc)
                            .OfClass(typeof(RevitLinkInstance))
                            .Cast<RevitLinkInstance>()
                            .FirstOrDefault(li => li.Id.IntegerValue == wallInfo.LinkInstanceId.Value);

                        if (linkInstance != null)
                        {
                            Document linkedDoc = linkInstance.GetLinkDocument();
                            if (linkedDoc != null)
                            {
                                Element wall = linkedDoc.GetElement(new ElementId((int)wallInfo.WallId));
                                if (wall != null)
                                {
                                    Reference reference = _elementSelector.CreateWallReference(linkInstance, (Wall)wall, doc);
                                    if (reference != null)
                                    {
                                        _elementSelector.SelectElements(new List<Reference> { reference });
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Element wall = doc.GetElement(new ElementId((int)wallInfo.WallId));
                        if (wall != null)
                        {
                            _app.ActiveUIDocument.Selection.SetElementIds(new List<ElementId> { wall.Id });
                            return;
                        }
                    }

                    TaskDialog.Show("Error", "Unable to find or select the wall.");
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", $"An error occurred while selecting the wall: {ex.Message}");
                }
            }
        }
    }
}
