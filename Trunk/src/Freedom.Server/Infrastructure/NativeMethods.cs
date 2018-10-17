using System;
using System.Runtime.InteropServices;

namespace Freedom.Server.Infrastructure
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetFileInformationByHandle(IntPtr handle,
            out FileInformation fileInformation);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FileInformation
    {
        private readonly uint _fileAttributes;
        private readonly ulong _creationTime;
        private readonly ulong _lastAccessTime;
        private readonly ulong _lastWriteTime;
        private readonly uint _volumeSerialNumber;
        private readonly uint _fileSizeHigh;
        private readonly uint _fileSizeLow;
        private readonly uint _numberOfLinks;
        private readonly uint _fileIndexHigh;
        private readonly uint _fileIndexLow;

        private bool Equals(FileInformation other)
        {
            return _volumeSerialNumber == other._volumeSerialNumber &&
                   _fileIndexHigh == other._fileIndexHigh &&
                   _fileIndexLow == other._fileIndexLow;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FileInformation && Equals((FileInformation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)_volumeSerialNumber;
                hashCode = (hashCode * 397) ^ (int)_fileIndexHigh;
                hashCode = (hashCode * 397) ^ (int)_fileIndexLow;
                return hashCode;
            }
        }

        public static bool operator ==(FileInformation left, FileInformation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FileInformation left, FileInformation right)
        {
            return !left.Equals(right);
        }
    }
}