﻿using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.Pins
{
	[Guid("31B5051D-09D5-45E7-BF67-4FEC1E28BF35"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGenericIO: INodeIOBase
	{
		object GetSlice(int slice);
	}
	
	public class GenericIO : IGenericIO
	{
		public object GetSlice(int slice)
		{
			return null;
		}
	}

}
