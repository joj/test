namespace PhxGauging.Common.IO
{
    public interface IFileManager
    {
        bool FileExists(string filePath);
        bool DirectoryExists(string directoryPath);
        void CreateFile(string filePath);
        void CreateDirectory(string directoryPath);
        void AppendToFile(string filePath, string contents);
        string DataDirectory { get; set; }

        void WriteAllBytes(string filePath, byte[] contents);
        void WriteAllText(string filePath, string serializeTestResult);
        byte[] ReadAllBytes(string filePath);
        void CopyFile(string sourcePath, string destinationPath, bool overwrite = true);
    }
}