using System;
using System.Windows.Forms;
using System.IO;


namespace JSRF_ModTool
{
    class FileExplorer
    {

        public static string directory { get; set; }

        public void set_dir(string path)
        {
            directory = path;
        }

        public bool CreateTree(TreeView treeView)
        {

            bool returnValue = false;

            try
            {
                TreeNode files = new TreeNode();
                files.Text = directory;
                files.Tag = "Files";
                files.Nodes.Add("");
                treeView.Nodes.Add(files);
                files.Expand();
            }
            catch
            {
                returnValue = false;
            }
            return returnValue;

        }

        public TreeNode EnumerateDirectory(TreeNode parentNode)
        {

            try
            {
                DirectoryInfo rootDir;

                Char[] arr = { '\\' };
                string[] nameList = parentNode.FullPath.Split(arr);
                string path = directory;

                if (nameList.GetValue(0).ToString() == "Files")
                {
                    path = directory;

                    for (int i = 1; i < nameList.Length; i++)
                    {
                        path = path + nameList[i] + "\\";
                    }

                    rootDir = new DirectoryInfo(path);
                }
                // for other Directories
                else
                {

                    rootDir = new DirectoryInfo(parentNode.FullPath + "\\");
                }

                parentNode.Nodes[0].Remove();
                foreach (DirectoryInfo dir in rootDir.GetDirectories())
                {

                    TreeNode node = new TreeNode();
                    node.Text = dir.Name;
                    node.Nodes.Add("");
                    parentNode.Nodes.Add(node);
                }

                // Fill files
                foreach (FileInfo file in rootDir.GetFiles())
                {
                    if ((file.Extension == ".dat") || (file.Extension == ".bin"))
                    {
                        TreeNode node = new TreeNode();
                        node.Text = file.Name;
                        node.ImageIndex = 2;
                        node.SelectedImageIndex = 2;
                        parentNode.Nodes.Add(node);
                    }
                }
            }

            catch
            {
                return null;
            }

            return parentNode;
        }
    }
}
