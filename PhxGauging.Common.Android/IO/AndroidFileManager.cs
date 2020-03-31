using System;
using System.IO;
using PhxGauging.Common.IO;

namespace PhxGauging.Common.Android.IO
{
    public class AndroidFileManager : IFileManager
    {
        private string dataDirectory;

        public string DataDirectory
        {
            get
            {
                if (dataDirectory == null)
                {
                    dataDirectory = global::Android.OS.Environment.GetExternalStoragePublicDirectory(
                        global::Android.OS.Environment.DirectoryDocuments).AbsolutePath;//Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                }

                return dataDirectory;
            }
            set
            {
                dataDirectory = value; 
                

            }
        }

        public AndroidFileManager()
        {
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public void CreateFile(string filePath)
        {
            var fileStream = File.Create(filePath);
            fileStream.Close();
            fileStream.Dispose();
        }

        public void CreateDirectory(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
        }

        public void AppendToFile(string filePath, string contents)
        {
            using (var fs = File.AppendText(filePath))
            {
                fs.WriteLine(contents);
            }
        }

        public void WriteAllBytes(string filePath, byte[] contents)
        {
            File.WriteAllBytes(filePath, contents);
        }

        public void WriteAllText(string filePath, string contents)
        {
            File.WriteAllText(filePath, contents);
        }

        public byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        
        public void CopyFile(string sourcePath, string destinationPath, bool overwrite = true)
        {
            File.Copy(sourcePath, destinationPath, overwrite);
        }
    }
}