using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

/*
2    0 00001170 CloseUSBDevice
7    1 00001340 GetErrorMsgA
8    2 00001370 GetErrorMsgW
5    3 000012A0 GetInputLength
10    4 000013C0 GetInputReport
6    5 000012F0 GetOutputLength
1    6 00001010 OpenUSBDevice
3    7 00001210 ReadUSB
9    8 000013A0 SetOutputReport
4    9 00001180 WriteUSB
*/

namespace WxkUSB {
	public static class USB_Orig {
		const CallingConvention CConv = CallingConvention.Winapi;
		const string DllName = "WxkUSB_original";

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern int OpenUSBDevice(int VID, int PID);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern int CloseUSBDevice(int A);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GetErrorMsgA();

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GetErrorMsgW();

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GetInputLength();

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GetInputReport();

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GetOutputLength();

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern int ReadUSB(int Handle, IntPtr Data, int Len, out int NumberOfBytesRead);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void SetOutputReport();

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern int WriteUSB(int Handle, IntPtr Data, int Len, out int NumberOfBytesWritten);
	}

	public static class USB {
		[DllImport("kernel32")]
		static extern bool AllocConsole();

		static USB() {
			if (WriteTraces)
				AllocConsole();
			
			if (File.Exists("dump.txt"))
				File.Delete("dump.txt");
		}

		const bool WriteTraces = true;
		const bool SpoofUSB = true;
		const bool DumpWrite = true;
		const int SpoofHandle = 666;
		const CallingConvention CConv = CallingConvention.Winapi;
		
		static T PrintRet<T>(T In) {
			if (WriteTraces)
				Console.WriteLine(In);

			return In;
		}

		[DllExport(CConv)]
		public static int OpenUSBDevice(int VID, int PID) {
			if (WriteTraces)
				Console.Write("{0}({1}) = ", MethodBase.GetCurrentMethod().Name, string.Join(", ", VID, PID));


			if (!SpoofUSB)
				return PrintRet(USB_Orig.OpenUSBDevice(VID, PID));

			return PrintRet(SpoofHandle);
		}

		[DllExport(CConv)]
		public static int CloseUSBDevice(int Handle) {
			if (WriteTraces)
				Console.Write("{0}({1}) = ", MethodBase.GetCurrentMethod().Name, Handle);

			if (SpoofUSB && Handle == SpoofHandle)
				return PrintRet(0);

			return PrintRet(USB_Orig.CloseUSBDevice(Handle));
		}

		[DllExport(CConv)]
		public static int ReadUSB(int Handle, IntPtr Data, int Len, out int NumberOfBytesRead) {
			if (WriteTraces)
				Console.Write("{0}({1}) = ", MethodBase.GetCurrentMethod().Name, string.Join(", ", Handle, Data, Len));

			if (SpoofUSB && Handle == SpoofHandle) {
				byte[] DataResponse = new byte[] { 0, 0, 128 };
				for (int i = 0; i < Len; i++)
					Marshal.WriteByte(Data, i, DataResponse[i]);

				NumberOfBytesRead = Len;
				return PrintRet(1);
			}

			int Res = PrintRet(USB_Orig.ReadUSB(Handle, Data, Len, out NumberOfBytesRead));

			if (WriteTraces) {
				for (int i = 0; i < NumberOfBytesRead; i++) {
					Console.Write(Marshal.ReadByte(Data, i));
					if (i < NumberOfBytesRead - 1)
						Console.Write(", ");
				}
				Console.WriteLine();
			}

			return Res;
		}

		[DllExport(CConv)]
		public static int WriteUSB(int Handle, IntPtr Data, int Len, out int NumberOfBytesWritten) {
			if (WriteTraces)
				Console.Write("{0}({1}) = ", MethodBase.GetCurrentMethod().Name, string.Join(", ", Handle, Data, Len));

			byte[] DataArray = new byte[Len];
			for (int i = 0; i < DataArray.Length; i++)
				DataArray[i] = Marshal.ReadByte(Data, i);

			if (DumpWrite) {
				const string DumpName = "dump.txt";

				/*if (File.Exists(DumpName))
					File.Delete(DumpName);*/

				//File.AppendAllText(DumpName, Len.ToString() + ";");
				for (int i = 0; i < DataArray.Length; i++)
					File.AppendAllText(DumpName, DataArray[i].ToString() + ";");

				File.AppendAllText(DumpName, "\n");
			}
			
			if (SpoofUSB && Handle == SpoofHandle) {
				int ReturnCode = 1;
				NumberOfBytesWritten = Len;
				PrintRet(ReturnCode);

				if (WriteTraces) {
					for (int i = 0; i < DataArray.Length; i++) {
						Console.Write(DataArray[i]);
						if (i < DataArray.Length - 1)
							Console.Write(", ");
					}
					Console.WriteLine();
				}

				return ReturnCode;
			}

			return PrintRet(USB_Orig.WriteUSB(Handle, Data, Len, out NumberOfBytesWritten));
		}

		[DllExport(CConv)]
		public static void GetErrorMsgA() {
			if (WriteTraces)
				Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			USB_Orig.GetErrorMsgA();
		}

		[DllExport(CConv)]
		public static void GetErrorMsgW() {
			if (WriteTraces)
				Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			USB_Orig.GetErrorMsgW();
		}

		[DllExport(CConv)]
		public static void GetInputLength() {
			if (WriteTraces)
				Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			USB_Orig.GetInputLength();
		}

		[DllExport(CConv)]
		public static void GetInputReport() {
			if (WriteTraces)
				Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			USB_Orig.GetInputReport();
		}

		[DllExport(CConv)]
		public static void GetOutputLength() {
			if (WriteTraces)
				Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			USB_Orig.GetOutputLength();
		}

		[DllExport(CConv)]
		public static void SetOutputReport() {
			if (WriteTraces)
				Console.WriteLine(MethodBase.GetCurrentMethod().Name);

			USB_Orig.SetOutputReport();
		}
	}
}
