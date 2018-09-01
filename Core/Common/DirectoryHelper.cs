using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.Common
{
    public class DirHelper
    {
        public static void CopyDirectory(string srcPath, string destPath)
        {
            if (!Directory.Exists(srcPath))
                return;

            DirectoryInfo dir = new DirectoryInfo(srcPath);
            //获取目录下（不包含子目录）的文件和子目录
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                //判断是否文件夹
                if (i is DirectoryInfo)
                {
                    if (!Directory.Exists(destPath + "\\" + i.Name))
                    {
                        //目标目录下不存在此文件夹即创建子文件夹
                        Directory.CreateDirectory(destPath + "\\" + i.Name);
                    }
                    //递归调用复制子文件夹
                    CopyDirectory(i.FullName, destPath + "\\" + i.Name);
                }
                else
                {
                    //不是文件夹即复制文件，true表示可以覆盖同名文件
                    File.Copy(i.FullName, destPath + "\\" + i.Name, true);
                }
            }
        }

        public static string FindFile(string path, string fileName, bool isRecursive)
        {
            if (!Directory.Exists(path))
                return string.Empty;

            DirectoryInfo dir = new DirectoryInfo(path);
            //获取目录下的文件和子目录
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                //判断是否文件夹
                if (i is DirectoryInfo)
                {
                    if (isRecursive)
                    {
                        //递归查找
                        var content = FindFile(i.FullName, fileName, isRecursive);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            return content;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    continue;
                }

                if (i.Name == fileName)
                    return File.ReadAllText(i.FullName);
            }

            return string.Empty;
        }
    }
}
