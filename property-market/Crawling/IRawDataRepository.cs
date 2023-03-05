using Parsers;

namespace Crawling
{
    public interface IRawDataRepository
    {
        Task Add(IPropertyDataProvider provider, string content);
    }

    public class FileSystemRawDataRepository : IRawDataRepository
    {
        private readonly string folderPath;

        public FileSystemRawDataRepository(string folderPath)
        {
            this.folderPath = folderPath;
        }

        public async Task Add(IPropertyDataProvider provider, string content)
        {
            var filename = $"{provider.Id}_{DateTimeOffset.UtcNow:yyMMdd_HHmm}.txt";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            await File.WriteAllTextAsync(Path.Combine(folderPath, filename), content);
        }
    }
}
