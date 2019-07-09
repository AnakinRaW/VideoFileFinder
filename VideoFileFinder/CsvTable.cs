using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.Win32;

namespace VideoFileFinder
{
    public class CsvTable
    {
        private static readonly string _delimiter = ",";
        private static readonly string _dialogFilter = "File table|*.csv";

        private static string _path;

        public static void GenerateCsv(IReadOnlyCollection<string> supportedFileTypes, IEnumerable<DriveItemData> drives)
        {
            var selectedDrives = drives.Where(x => x.Selected).ToList();

            var entries = new List<CsvEntry>();
            var dialog = new SaveFileDialog();
            dialog.CreatePrompt = true;
            dialog.OverwritePrompt = true;
            dialog.Filter = _dialogFilter;

            var dialogResult = dialog.ShowDialog();

            if (dialogResult != true)
                return;

            foreach (var drive in selectedDrives)
            {
                var files = FileSystemHelper.GetDriveFiles(drive.DriveInfo.RootDirectory.FullName, supportedFileTypes).ToList();

                foreach (var file in files)
                {
                    var tags = FileSystemHelper.GetFileTags(file);
                    var tagString = tags == null ? string.Empty : string.Join(";", tags);
                    var driveName = drive.DriveInfo.VolumeLabel;
                    var path = file.Substring(Path.GetPathRoot(file).Length);

                    entries.Add(new CsvEntry
                    {
                        Path = path,
                        Tags = tagString,
                        DriveName = driveName
                    });
                }
            }

            using (var writer = new StreamWriter(dialog.FileName))
            {
                var csvWriter = new CsvWriter(writer);
                csvWriter.Configuration.Delimiter = _delimiter;
                using (csvWriter)
                {
                    csvWriter.WriteRecords(entries);
                }
            }
        }

        public static IEnumerable<string> GetFilesFromCsv(string filter, bool logicalOr)
        {
            if (string.IsNullOrEmpty(_path))
            {
                var dialog = new OpenFileDialog { Multiselect = false, Filter = _dialogFilter };
                var result = dialog.ShowDialog();
                if (result != true)
                    return new List<string>();
                _path = dialog.FileName;
            }

            var searchResult = new List<string>();

            using (var reader = new StreamReader(_path))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.Delimiter = _delimiter;
                using (csv)
                {
                    var records = csv.GetRecords<CsvEntry>();

                    foreach (var entry in records)
                    {
                        var tags = entry.Tags.Split(';');
                        var searchFilters = filter.Split(';');

                        if (!FileSystemHelper.CheckFileFilter(tags, searchFilters, logicalOr))
                            continue;

                        var drive = DriveInfo.GetDrives().FirstOrDefault(x => x.VolumeLabel.Equals(entry.DriveName));
                        if (drive == null)
                            continue;
                        var realPath = Path.Combine(drive.Name, entry.Path);
                        searchResult.Add(realPath);
                    }
                }
            }

            return searchResult;
        }
    }
}
