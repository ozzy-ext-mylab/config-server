﻿using System.IO;
using System.Threading.Tasks;

namespace MyLab.ConfigServer.Tools
{
    interface IIncludesProvider
    {
        Task<string> GetInclude(string id);
    }

    class DefaultIncludeProvider : IIncludesProvider
    {
        public string BasePath { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultIncludeProvider"/>
        /// </summary>
        public DefaultIncludeProvider(string basePath)
        {
            BasePath = basePath;
        }
        public async Task<string> GetInclude(string id)
        {
            var filePath = Path.Combine(BasePath, id + ".json");
            return File.Exists(filePath) ? 
                await File.ReadAllTextAsync(filePath) :
                await Task.FromResult<string>(null);
        }
    }
}