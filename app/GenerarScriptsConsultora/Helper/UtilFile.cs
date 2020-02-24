using System.IO;

namespace GenerarScriptsConsultora.Helper
{
    public class UtilFile
    {
        public bool DirectoryExists(string ruta)
        {
            return Directory.Exists(ruta);
        }

        public void CreateDirectory(string ruta)
        {
            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }
        }

        public string GetExtension(string @filename)
        {
            string ext = Path.GetExtension(@filename).ToLower();
            return ext;
        }

        public string GetFileName(string @filename)
        {
            string name = Path.GetFileName(@filename).ToLower();
            return name;
        }

        public string GroupFileToDirectory(string fileExtension, string getDirectory, string setDirectory)
        {
            string mensaje = "";
            try
            {
                string[] directories = System.IO.Directory.GetDirectories(getDirectory);
                if (directories.Length > 0)
                {
                    CopyFileByDirectory(getDirectory, fileExtension, getDirectory, setDirectory);
                }
                else
                {
                    CopyFileToDirectory(getDirectory, fileExtension, getDirectory, setDirectory);
                }
            }
            catch (System.Exception ex)
            {
                mensaje = ex.Message + " - " + ex.StackTrace;
            }

            return mensaje;
        }

        public void CopyFileByDirectory(string getDirectory, string fileExtension, string getDirectoryIni, string setDirectory)
        {
            string[] directories = System.IO.Directory.GetDirectories(getDirectory);

            foreach (string directory in directories)
            {
                CopyFileToDirectory(directory, fileExtension, getDirectoryIni, setDirectory);

                string[] directoriesCh = System.IO.Directory.GetDirectories(directory);
                if (directoriesCh.Length > 0)
                {
                    CopyFileByDirectory(directory, fileExtension, getDirectoryIni, setDirectory);
                }
            }
        }

        public void CopyFileToDirectory(string getDirectory, string fileExtension, string getDirectoryIni, string setDirectory)
        {
            foreach (string file in Directory.EnumerateFiles(getDirectory, fileExtension))
            {
                string newPathAndName = NewPathAndName(file, getDirectory, getDirectoryIni, setDirectory);
                if (!File.Exists(newPathAndName))
                    File.Copy(file, newPathAndName);
            }
        }

        public string NewPathAndName(string file, string getDirectory, string getDirectoryIni, string setDirectory)
        {
            string newNameFile = getDirectory.Replace(getDirectoryIni, "");
            newNameFile = newNameFile.Replace(@"/", @"_");
            newNameFile = newNameFile.Replace(@"\", @"_");
            
            if (newNameFile[0].ToString() == @"_")
            {
                newNameFile = newNameFile.Substring(1);
            }

            string name = GetFileName(file);
            string newPathAndName = Path.Combine(setDirectory, newNameFile + "_" + name);

            return newPathAndName;
        }
    }
}
