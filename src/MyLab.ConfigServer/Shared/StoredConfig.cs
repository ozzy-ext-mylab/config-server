﻿using System;
using System.IO;

namespace MyLab.ConfigServer.Shared
{
    public class StoredConfig
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public long Length { get; set; }
        public DateTime LastModified { get; set; }

        public static StoredConfig FromFile(FileInfo fi)
        {
            return new StoredConfig
            {
                Name = Path.GetFileNameWithoutExtension(fi.Name),
                Format = fi.Extension,
                Length = fi.Length,
                LastModified = fi.LastWriteTime
            };
        }
    }
}