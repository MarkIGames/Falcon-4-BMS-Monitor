using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.Win32;

namespace F4toA3Monitor
{
    [ComVisible(true)]
    public enum FalconDataFormats
    {
        BMS4 = 0
    }
    
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class Reader : IDisposable
    {
        private FalconDataFormats _dataFormat;
        private string _primarySharedMemoryAreaFileName = "FalconSharedMemoryArea";
        private IntPtr _hPrimarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpPrimarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private string _secondarySharedMemoryFileName = "FalconSharedMemoryArea2";
        private IntPtr _hSecondarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpSecondarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private string _OsbSharedMemoryAreaFileName = "FalconSharedOsbMemoryArea";
        private IntPtr _hOsbSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpOsbSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private bool _disposed = false;
        
        public Reader()
        {
        }
        
        public Reader(FalconDataFormats dataFormat)
        {
            _dataFormat = dataFormat;
        }

        public bool IsFalconRunning
        {
            get
            {
                try
                {
                    ConnectToFalcon();
                    if (_lpPrimarySharedMemoryAreaBaseAddress != IntPtr.Zero)
                    {
                        return true;
                    }
                    else
                    {
                       return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        
        public FalconDataFormats DataFormat
        {
            get
            {
                return _dataFormat;
            }
            set
            {
                _dataFormat = value;
            }
        }
        
        [ComVisible(false)]
        public byte[] GetRawOSBData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            List<byte> bytesRead = new List<byte>();
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpOsbSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(Headers.OSBData))) fileSizeBytes = Marshal.SizeOf(typeof(Headers.OSBData));
                for (int i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpOsbSharedMemoryAreaBaseAddress, i));
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
            }
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            else
            {
                return toReturn;
            }
        }
        
        private long GetMaxMemFileSize(IntPtr pMemAreaBaseAddr)
        {
            NativeMethods.MEMORY_BASIC_INFORMATION mbi = new NativeMethods.MEMORY_BASIC_INFORMATION();
            NativeMethods.VirtualQuery(ref pMemAreaBaseAddr, ref mbi, new IntPtr(Marshal.SizeOf(mbi)));
            return mbi.RegionSize.ToInt64();
        }
        
        [ComVisible(false)]
        public byte[] GetRawFlightData2()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            List<byte> bytesRead = new List<byte>();
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpSecondarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(Headers.BMS4FlightData2))) fileSizeBytes = Marshal.SizeOf(typeof(Headers.BMS4FlightData2));
                for (int i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpSecondarySharedMemoryAreaBaseAddress, i));
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
            }
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            else
            {
                return toReturn;
            }
        }

        [ComVisible(false)]
        public byte[] GetRawPrimaryFlightData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            List<byte> bytesRead = new List<byte>();
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpPrimarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(Headers.BMS4FlightData))) fileSizeBytes = Marshal.SizeOf(typeof(Headers.BMS4FlightData));
                for (int i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpPrimarySharedMemoryAreaBaseAddress, i));
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
            }
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            else
            {
                return toReturn;
            }
        }
        
        public FlightData GetCurrentData()
        {
            Type dataType=null;
            switch(_dataFormat) {

                case FalconDataFormats.BMS4:
                    dataType = typeof(Headers.BMS4FlightData);
                    break;
                default:
                    break;
            }
            if (dataType == null)
            {
                return null;
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals (IntPtr.Zero)) {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals (IntPtr.Zero)) {
                return null;
            }
            object data = Convert.ChangeType(Marshal.PtrToStructure(_lpPrimarySharedMemoryAreaBaseAddress, dataType), dataType);

            FlightData toReturn=null;
            switch(_dataFormat) {

                case FalconDataFormats.BMS4:
                    toReturn = new FlightData((Headers.BMS4FlightData)data);
                    break;
                default:
                    break;
            }
            if (toReturn == null)
            {
                return null;
            }
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                if (_dataFormat == FalconDataFormats.BMS4)
                {
                    data = (Marshal.PtrToStructure(_lpSecondarySharedMemoryAreaBaseAddress, typeof(Headers.BMS4FlightData2)));
                }
                else
                {
                    data = (Marshal.PtrToStructure(_lpSecondarySharedMemoryAreaBaseAddress, typeof(Headers.FlightData2)));
                }
                toReturn.PopulateFromStruct(data);
            }
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (Marshal.PtrToStructure(_lpOsbSharedMemoryAreaBaseAddress, typeof(Headers.OSBData)));
                toReturn.PopulateFromStruct(data);
            }
            toReturn.DataFormat = _dataFormat;
            return toReturn;
        }
        
        private void ConnectToFalcon()
        {
            Disconnect();
            _hPrimarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false, _primarySharedMemoryAreaFileName);
            _lpPrimarySharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hPrimarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
            if (_dataFormat == FalconDataFormats.BMS4)
            {
                _hSecondarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false, _secondarySharedMemoryFileName);
                _lpSecondarySharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hSecondarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
                _hOsbSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false, _OsbSharedMemoryAreaFileName);
                _lpOsbSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hOsbSharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
            }
        }
        
        private void Disconnect()
        {
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpPrimarySharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hPrimarySharedMemoryAreaFileMappingObject);
            }
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpSecondarySharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hSecondarySharedMemoryAreaFileMappingObject);
            }
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpOsbSharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hOsbSharedMemoryAreaFileMappingObject);
            }
        }
        
        internal void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                _disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
