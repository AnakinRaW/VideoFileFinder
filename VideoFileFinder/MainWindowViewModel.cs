using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using ModernApplicationFramework.Input.Command;

namespace VideoFileFinder
{
    public class MainWindowViewModel : Screen
    {
        private static readonly IEnumerable<string> SupportedFileTypes = new List<string> { ".mp4", ".avi", ".wmv" };

        private IEnumerable<DriveItemData> _drives;
        private bool _logicalOr = true;
        private string _tagInput = string.Empty;

        private readonly Dictionary<(char DriveLetter, string sortedFilter, bool logicalOr), IEnumerable<string>> _newSearchCache =
            new Dictionary<(char DriveLetter, string sortedFilter, bool logicalOr), IEnumerable<string>>();

        private string _countRandom = 5.ToString();
        private bool _useCsv;


        public ICommand PickFilesCommand => new UICommand(CreateRandomEntry, () => true);
        public ICommand GenerateCsvCommand => new UICommand(() => CsvTable.GenerateCsv(SupportedFileTypes.ToList(), Drives), () => true);
        
        public IEnumerable<DriveItemData> Drives
        {
            get => _drives;
            set
            {
                if (Equals(value, _drives)) return;
                _drives = value;
                NotifyOfPropertyChange();
            }
        }

        public bool LogicalOr
        {
            get => _logicalOr;
            set
            {
                if (value == _logicalOr) return;
                _logicalOr = value;
                NotifyOfPropertyChange();
            }
        }

        public string TagInput
        {
            get => _tagInput;
            set
            {
                if (value == _tagInput) return;
                _tagInput = value;
                NotifyOfPropertyChange();
            }
        }

        public string CountRandom
        {
            get => _countRandom;
            set
            {
                if (value == _countRandom) return;
                _countRandom = value;
                NotifyOfPropertyChange();
            }
        }

        public bool UseCsv
        {
            get => _useCsv;
            set
            {
                if (value == _useCsv) return;
                _useCsv = value;
                NotifyOfPropertyChange();
            }
        }

        public IObservableCollection<RandomEntry> SearchResults { get; } = new BindableCollection<RandomEntry>();

        public MainWindowViewModel()
        {
            Drives = FileSystemHelper.GetVolumes().ToList();
        }

        public void OnKeyDown(KeyEventArgs e, FileEntry entry)
        {
            if (e.Key != Key.Enter)
                return;
            ItemActivated(entry);
        }

        public void OnListEntryRemove(RandomEntry entry)
        {
            SearchResults.Remove(entry);
        }

        public void ItemActivated(FileEntry entry)
        {
            Process.Start(entry.FilePath);
        }
        
        private IEnumerable<string> GetMatchingFilesFromFilter(char driveLetter, IReadOnlyCollection<string> files, string sortedFilters)
        {
            var filters = sortedFilters.Split(';');
            
            var searchCacheKey = (driveLetter, sortedFilters, LogicalOr);

            if (_newSearchCache.ContainsKey(searchCacheKey))
                return _newSearchCache[searchCacheKey];

            if (string.IsNullOrEmpty(sortedFilters))
            {
                _newSearchCache.Add(searchCacheKey, files);
                return files;
            }

            var filteredFiles = new List<string>();

            foreach (var file in files)
            {
                var tags = FileSystemHelper.GetFileTags(file).ToList();
                if (FileSystemHelper.CheckFileFilter(tags, filters, LogicalOr))
                    filteredFiles.Add(file);
            }
            _newSearchCache.Add(searchCacheKey, filteredFiles);
            return filteredFiles;
        }
        
        private void CreateRandomEntry()
        {
            var searchResult = new List<string>();

            var filterText = TagInput;
            var filter = filterText.Split(';');
            var sortedFilters = filter.OrderBy(x => x).Select(x => x.Trim()).ToList();
            var sortedFilterText = string.Join(";", sortedFilters);

            switch (UseCsv)
            {
                case true:
                    searchResult.AddRange(CsvTable.GetFilesFromCsv(sortedFilterText, LogicalOr));
                    break;
                default:
                    searchResult.AddRange(GetSearchResultFromDisk(sortedFilterText));
                    break;
            }

            var randomFiles = PickRandom(searchResult).ToList();

            var drives = string.Join(";", Drives.Where(x => x.Selected).ToList().Select(x => x.DriveInfo.Name.First()));

            var entry = new RandomEntry(randomFiles, sortedFilterText, drives, LogicalOr);
            if (!entry.Files.Any())
                return;
            SearchResults.Add(entry);
        }
        
        private IEnumerable<string> GetSearchResultFromDisk(string filter)
        {
            var searchResult = new List<string>();

            var selectedDrives = Drives.Where(x => x.Selected).ToList();
            foreach (var drive in selectedDrives)
            {
                var driveFiles = FileSystemHelper.GetDriveFiles(drive.DriveInfo.RootDirectory.FullName, SupportedFileTypes).ToList();
                var filteredFiles = GetMatchingFilesFromFilter(drive.DriveInfo.Name[0], driveFiles, filter);
                searchResult.AddRange(filteredFiles);
            }

            return searchResult;
        }
        
        private IEnumerable<string> PickRandom(IReadOnlyCollection<string> files)
        {
            var indexes = new HashSet<int>();
            var count = 0;
            var targetNumber = int.Parse(CountRandom);
            var random = new Random();

            var maxFiles = Math.Min(targetNumber, files.Count);

            while (count != maxFiles)
            {
                var i = random.Next(0, files.Count);
                if (indexes.Contains(i))
                    continue;
                indexes.Add(i);
                count++;
            }

            var result = new List<string>();
            foreach (var index in indexes)
            {
                result.Add(files.ElementAt(index));
            }

            return result;
        }
    }
}
