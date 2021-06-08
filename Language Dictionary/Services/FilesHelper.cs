using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Language_Dictionary.Models;

namespace Language_Dictionary.Services
{
    public class FilesHelper
    {
        private readonly DirectoryInfo _directory;

        public string Path => _directory.FullName;

        public FilesHelper()
        {
            _directory = new DirectoryInfo(Settings.Folder);

            if (!_directory.Exists)
                _directory.Create();
        }

        public List<FileInfo> GetAllFiles() => _directory.GetFiles().ToList();

        public async Task<List<string>> GetAllLines(string path)
        {
            using var streamReader = new StreamReader(path, Encoding.UTF8);

            var list = new List<string>();

            string line;
            while ((line = await streamReader.ReadLineAsync()) != null)
                list.Add(line);

            return list.Distinct().ToList();
        }
    }
}