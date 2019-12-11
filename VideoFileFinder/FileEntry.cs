using System;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Shell;

namespace VideoFileFinder
{
    public struct FileEntry : IEquatable<FileEntry>
    {
        public string FilePath { get; }

        public ImageSource Thumbnail { get; }

        public string Tags { get; }

        public FileEntry(string path)
        {
            FilePath = path;
            var file = ShellFile.FromFilePath(path);
            Thumbnail = file.Thumbnail.BitmapSource;
            var tags = file.Properties.System.Keywords.Value;
            Tags = tags == null ? string.Empty : string.Join("; ", file.Properties.System.Keywords.Value);
        }

        public bool Equals(FileEntry other)
        {
            return FilePath == other.FilePath;
        }

        public override bool Equals(object obj)
        {
            return obj is FileEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (FilePath != null ? FilePath.GetHashCode() : 0);
        }
    }
}