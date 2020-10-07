using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("1. 鼠标并不能很好地捕获，一边快速移动一边在中途点鼠标估计是没法命中的\n2. 左键可以点击程序，右键拖动窗口\n3. 不支持非置顶、不支持改变窗口大小\n4. 关闭本程序可以用关闭此黑窗口的方式等5.网页加载较慢，不过第二次以后就有缓存了。没加载出来窗口是蓝色的\n回车确认");
            Console.ReadLine();
            AppDomain.CurrentDomain.AssemblyResolve += HuiDesktop.DirectComposition.CefStartupInitialize.Resolver;
            HuiDesktop.DirectComposition.CefStartupInitialize.InitializeCefSharp();
            HuiDesktop.DirectComposition.CefApplication application = new HuiDesktop.DirectComposition.CefApplication();
            application.Run();
        }
    }
}
