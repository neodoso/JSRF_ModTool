using System.IO;

namespace JSRF_Tool_2.Functions
{
    class IO
    {
        public static void DeleteDirectoryContent(string target_dir)
        {
            if (Directory.Exists(target_dir))
            {

                System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(target_dir);

                foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                    }
                }
                foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
