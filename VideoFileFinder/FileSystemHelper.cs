using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAPICodePack.Shell;

namespace VideoFileFinder
{
    public static class FileSystemHelper
    {
        public static IEnumerable<string> GetDriveFiles(string driveRootPath, IEnumerable<string> supportedFileTypes)
        {
            var driveFiles = new List<string>();
            ApplyAllFiles(driveRootPath, filePath =>
            {
                if (supportedFileTypes.Contains(Path.GetExtension(filePath)))
                    driveFiles.Add(filePath);
            });
            return driveFiles;
        }

        public static IEnumerable<DriveItemData> GetVolumes()
        {
            return DriveInfo.GetDrives().Select(driveInfo => new DriveItemData(driveInfo, false));
        }

        public static IEnumerable<string> GetFileTags(string file)
        {
            try
            {
                var shellFile = ShellFile.FromFilePath(file);
                return shellFile.Properties.System.Keywords.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool CheckFileFilter(IEnumerable<string> fileTags, IReadOnlyCollection<string> searchTags, bool logicalOr)
        {
            if (searchTags == null || !searchTags.Any() || (searchTags.Count == 1 && searchTags.First() == string.Empty))
                return true;

            if (logicalOr)
            {
                if (fileTags.Intersect(searchTags).Any())
                    return true;
            }
            else
            {
                //if (tags.OrderBy(x => x).SequenceEqual(filters.OrderBy(x => x)))
                if (fileTags.ContainsAllItems(searchTags))
                    return true;
            }

            return false;
        }

        private static void ApplyAllFiles(string folder, Action<string> fileAction)
        {
            if (folder.Contains("$RECYCLE.BIN"))
                return;
            foreach (var file in Directory.GetFiles(folder))
            {
                fileAction(file);
            }
            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                {
                    ApplyAllFiles(subDir, fileAction);
                }
                catch
                {
                }
            }
        }
    }
}
