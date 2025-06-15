using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class RunPath
    {
        #region//按照游戏格式的路径
        public static string AndroidFilePath = "android:Survivalcraft2.4_API1.8";
        #endregion
        /// <summary>
        ///获取实际运行路径 
        /// </summary>
        public static string GetOperatingPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        /// <summary>
        /// 获取 EXE 或 dll 所在路径(包含文件自身路径)
        /// </summary>
        public static string GetExecutablePath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }
        /// <summary>
        /// 获取运行入口路径(用命令行或者其他程序调用时调用者目录)
        /// </summary>
        public static string GetEntryPath()
        {
            return AppContext.BaseDirectory;
        }
        /// <summary>
        /// 获取环境变量 path 多个路径用分号分隔
        /// </summary>
        public static string GetEnvironmentPath()
        {
            return Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
        }
    }
}
//跑路[doge]