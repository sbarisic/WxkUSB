using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WxkUSB;

namespace LedFan {
	public static unsafe class Program {
		public static void Main(string[] Args) {
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

			MainSafe(Args);
		}

		static Assembly OnAssemblyResolve(object S, ResolveEventArgs A) {
			string FolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LIB");

			string AssemblyPath = Path.Combine(FolderPath, new AssemblyName(A.Name).Name + ".dll");
			if (!File.Exists(AssemblyPath))
				return null;

			return Assembly.LoadFrom(AssemblyPath);
		}

		static void MainSafe(string[] Args) {
			Console.Write("Finding USB fan device ... ");

			int Dev = USB.OpenUSBDevice(3141, 29024);
			if (Dev == -1) {
				Console.WriteLine("FAIL");
				Console.ReadLine();
				return;
			} else
				Console.WriteLine("OK");

			string[] DataLines = File.ReadAllLines("fage.txt");

			foreach (var DataLine in DataLines) {
				byte[] DataArray = DataLine.Split(';').Where((In) => !string.IsNullOrWhiteSpace(In)).Select((In) => byte.Parse(In)).ToArray();

				fixed (byte* DataArrayPtr = DataArray) {
					USB.WriteUSB(Dev, (IntPtr)DataArrayPtr, DataArray.Length, out int Written);
					USB.ReadUSB(Dev, (IntPtr)DataArrayPtr, 3, out int Read);
				}
			}

			USB.CloseUSBDevice(Dev);
			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}
