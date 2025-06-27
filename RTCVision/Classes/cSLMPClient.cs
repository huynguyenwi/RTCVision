
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SLMPTcp
{
    public class SLMPClient
    {
        #region Variables
        object LockCommunication = new object();
        public int Port { get; set; }
        public string IP { get; set; }
        public int BitMode { get; set; } //16 or 32
        private bool _IsConnected;

        public bool IsConnected
        {
            get
            {
                if (mcTCP != null && mcTCP.Client != null)
                    return mcTCP.Client.Connected;
                else
                    return false;
            }
            set => _IsConnected = value;
        }

        public bool IsWriting { get; set; }
        public string ErrMessage { get; set; }

        private int _connectTimeOut = 3000;
        public int ConnectTimeOut
        {
            get => _connectTimeOut;
            set => _connectTimeOut = value;
        }

        private McProtocolTcp mcTCP;
        public SLMPClient(string ip, int port)
        {
            IP = ip; Port = port;
            //MC3E dùng cho các dòng PLC mới Q,FX
            IsWriting = false;
            mcTCP = new McProtocolTcp(ip, port);
            IsConnected = false;
        }
        #endregion Variables

        #region Convert Data Type
        public static double ObToDouble(object x)
        {
            try
            {
                return Convert.ToDouble(x);
            }
            catch
            {
                return 0;
            }
        }
        public static int ObToInt(object x)
        {
            try
            {
                return Convert.ToInt32(x);
            }
            catch
            {
                return 0;
            }
        }
        public static float ObToFloat(object x)
        {
            try
            {
                return Convert.ToSingle(x);
            }
            catch
            {
                return 0;
            }
        }
        public static decimal ObToDecimal(object x)
        {
            try
            {
                return Convert.ToDecimal(x);
            }
            catch
            {
                return 0;
            }
        }
        public static string ObToString(object x)
        {
            if (x != null)
            {
                return x.ToString().Trim();
            }
            return "";
        }
        public static bool ObToBool(object x)
        {
            try
            {
                return Convert.ToBoolean(x);
            }
            catch
            {
                return false;
            }

        }
        public static int[] DwordToArrayInt(int dataIN)
        {
            byte[] BufferDwordByte;
            int[] intputPLC = new int[2];
            BufferDwordByte = BitConverter.GetBytes(System.Convert.ToInt32(dataIN));
            intputPLC[0] = BitConverter.ToInt16(BufferDwordByte, 0);
            intputPLC[1] = BitConverter.ToInt16(BufferDwordByte, 2);
            return intputPLC;
        }
        public int ArrayIntToDword(int[] dataIN)
        {
            byte[] byarrBufferByte = new byte[4];
            byte[] byarrTemp;
            int iNumber;
            for (iNumber = 0; iNumber <= 2 - 1; iNumber++)
            {
                byarrTemp = BitConverter.GetBytes(dataIN[iNumber]);
                byarrBufferByte[iNumber * 2] = byarrTemp[0];
                byarrBufferByte[iNumber * 2 + 1] = byarrTemp[1];
            }
            int outputPLC = System.Convert.ToInt32(BitConverter.ToInt32(byarrBufferByte, 0));
            return outputPLC;
        }
        public float ArrayIntToFloat(int[] registers)
        {
            if (registers.Length != 2)
                throw new ArgumentException("Input Array length invalid - Array langth must be '2'");
            int highRegister = registers[1];
            int lowRegister = registers[0];
            byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
            byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
            byte[] floatBytes = {
                                    lowRegisterBytes[0],
                                    lowRegisterBytes[1],
                                    highRegisterBytes[0],
                                    highRegisterBytes[1]
                                };
            return BitConverter.ToSingle(floatBytes, 0);
        }
        public static string ArrayIntToString(int[] dataIN, int lenghtWord)
        {
            byte[] BufferStringByte = new byte[lenghtWord * 2];
            byte[] byarrTemp;
            int iNumber;
            for (iNumber = 0; iNumber <= lenghtWord - 1; iNumber++)
            {
                byarrTemp = BitConverter.GetBytes(dataIN[iNumber]);
                BufferStringByte[iNumber * 2] = byarrTemp[0];
                BufferStringByte[iNumber * 2 + 1] = byarrTemp[1];
            }
            string outputPLC = Encoding.Default.GetString(BufferStringByte);
            return outputPLC;
        }
        public int[] FloatToArrayInt(float floatValue)
        {
            byte[] floatBytes = BitConverter.GetBytes(floatValue);
            byte[] highRegisterBytes =
            {
                floatBytes[2],
                floatBytes[3],
                0,
                0
            };
            byte[] lowRegisterBytes =
            {

                floatBytes[0],
                floatBytes[1],
                0,
                0
            };
            int[] returnValue =
            {
                BitConverter.ToInt32(lowRegisterBytes,0),
                BitConverter.ToInt32(highRegisterBytes,0)
            };
            return returnValue;
        }
        public int[] StringToArrayInt(string dataIN, int lenghtWord)
        {
            byte[] BufferStringByte;
            int[] intputPLC = new int[lenghtWord];
            int iLengthOfBuffer;
            int iNumber;
            BufferStringByte = Encoding.Default.GetBytes(dataIN);
            iLengthOfBuffer = Math.Min(BufferStringByte.Length, lenghtWord * 2);

            for (iNumber = 0; iNumber <= iLengthOfBuffer - 2; iNumber += 2)
            {
                intputPLC[iNumber / 2] = BitConverter.ToInt16(BufferStringByte, iNumber);
            }
            if ((iLengthOfBuffer % 2) == 1)
            {
                intputPLC[(iLengthOfBuffer / 2)] = BufferStringByte[iLengthOfBuffer - 1];
            }
            return intputPLC;
        }
        #endregion Convert Data Type

        #region Communication

        public void Connect()
        {
            try
            {
                ErrMessage = string.Empty;
                mcTCP.ConnectTimeOut = ConnectTimeOut;
                mcTCP.Open();
                IsConnected = mcTCP.Client != null && mcTCP.Client.Connected;
                //ErrMessage = IsConnected ? string.Empty : cStrings.Disconnect;
            }
            catch (Exception ex)
            {
                ErrMessage = ex.Message;
            }
        }
        public void Disconnect()
        {
            if (mcTCP != null)
                mcTCP.Close();
            IsConnected = false;
        }
        public int ReadBit(string address, out bool success)
        {
            success = false;
            try
            {
                int[] dataBit = new int[1];
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.GetBitDevice(address, 1, dataBit);
                }
                success = true;
                if (dataBit[0] == 1) return 1; else return 0;
            }
            catch
            {
                return 0;
            }
        }
        public int[] ReadBitBlock(string address, int length, out bool success)
        {
            success = false;
            try
            {
                int[] dataBit = new int[length];
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.GetBitDevice(address, length, dataBit);
                }
                success = true;
                return dataBit;
            }
            catch
            {
                return new int[] { };
            }
        }
        public int ReadBitRegister(string address, out bool success)
        {
            success = false;
            try
            {
                if (!address.Contains(".")) return 0;
                int addressBit = ObToInt(address.Split('.')[1]);
                string addressInt = address.Split('.')[0];
                int[] data = new int[1];
                if (!IsConnected)
                    mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(addressInt, 1, data);
                }
                success = true;
                var result = Convert.ToString((data[0]), 2);
                int outData = ObToInt(result.Substring(result.Length - addressBit - 1, 1));
                if (outData == 0) return 0;
                else return 1;
            }
            catch
            {
                return 0;
            }
        }
        public static string ReplaceChar(int index, string originString, string ReplaceString)
        {
            var aStringBuilder = new StringBuilder(originString);
            if (index >= originString.Length)
            {
                int condition = index - originString.Length;
                for (int i = 0; i <= condition; i++)
                {
                    if (i == condition) aStringBuilder.Insert(0, ReplaceString); else aStringBuilder.Insert(0, 0);
                }
            }
            else
            {
                aStringBuilder.Remove(originString.Length - index - 1, 1);
                aStringBuilder.Insert(originString.Length - index - 1, ReplaceString);
            }
            return aStringBuilder.ToString();
        }
        public bool WriteBit(string address, int data)
        {

            try
            {
                if (!IsConnected)
                    mcTCP.Open();
                int result = -1;
                lock (LockCommunication)
                {
                    result = mcTCP.SetDevice(address, data);
                }
                if (result == 0) return true; else return false;
            }
            catch
            {
                return false;
            }
        }
        public bool WriteBitBlock(string address, int length, int[] data)
        {

            try
            {
                if (!IsConnected)
                    mcTCP.Open();
                int result = -1;
                lock (LockCommunication)
                {
                    result = mcTCP.SetBitDevice(address, length, data);
                }
                if (result == 0) return true; else return false;
            }
            catch
            {
                return false;
            }
        }
        private string Reserve(string source)
        {
            char[] charArray = source.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        private string Replace(string source, int index, int length, string replace)
        {
            return source.Remove(index, Math.Min(length, source.Length - index)).Insert(index, replace);
        }
        public bool WriteBitRegister(string address, int data)
        {
            try
            {
                if (!address.Contains(".")) return false;
                int addressBit = ObToInt(address.Split('.')[1]);
                string addressInt = address.Split('.')[0];
                if (!IsConnected)
                    mcTCP.Open();

                int[] currentData = new int[1];

                lock (LockCommunication)
                {
                    int result = -1;
                    try
                    {
                        mcTCP.ReadDeviceBlock(addressInt, 1, currentData);
                    }
                    catch
                    {
                        return false;
                    }

                    int dataRead = currentData[0];

                    var resultBinary = Convert.ToString(dataRead, 2).PadLeft(BitMode, '0');
                    string newValue = Reserve(resultBinary);

                    if (data == 1)
                        newValue = Replace(newValue, addressBit, 1, "1");
                    else
                        newValue = Replace(newValue, addressBit, 1, "0");

                    newValue = Reserve(newValue);

                    int dataBit = Convert.ToInt32(newValue, 2);

                    result = mcTCP.SetDevice(addressInt, dataBit);

                    if (result == 0)
                        return true;
                    else
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool WriteMultiBitRegister(string address, int[] data)
        {
            try
            {
                if (address.Length <= 0) return false;
                //Lay dia chi o nho
                string addressInt = address.Split('.')[0];

                int[] currentData = new int[1];

                lock (LockCommunication)
                {
                    try
                    {
                        mcTCP.ReadDeviceBlock(addressInt, 1, currentData);
                    }
                    catch
                    {
                        return false;
                    }

                    int dataRead = currentData[0];

                    // Chuyen ve day nhi phan
                    var resultBinary = Convert.ToString(dataRead, 2).PadLeft(BitMode, '0');
                    string newValue = Reserve(resultBinary);
                    string[] addressBit = address.Replace(addressInt + ".", "").Split(';');
                    for (int i = 0; i < addressBit.Length; i++)
                    {
                        if (int.TryParse(addressBit[i], out int index))
                            if (data[i] == 1)
                                newValue = Replace(newValue, index, 1, "1");
                            else
                                newValue = Replace(newValue, index, 1, "0");
                    }
                    newValue = Reserve(newValue);
                    int dataBit = Convert.ToInt32(newValue, 2);
                    if (!IsConnected) mcTCP.Open();
                    int result = mcTCP.SetDevice(addressInt, dataBit);
                    if (result == 0)
                        return true;
                    else
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public int ReadWord(string address, out bool success)
        {
            success = false;
            try
            {
                int[] data = new int[1];
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(address, 1, data);
                }
                success = true;
                return data[0];
            }
            catch
            {
                return -1;
            }
        }
        public int[] ReadWordBlock(string address, int length, out bool success)
        {
            success = false;
            try
            {
                int[] data = new int[length];
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(address, length, data);
                }
                success = true;
                return data;
            }
            catch
            {
                return new int[] { };
            }
        }
        public bool WriteWordBlock(string address, int lenght, int[] data)
        {
            try
            {
                if (!IsConnected)
                    mcTCP.Open();
                lock (LockCommunication)
                {
                    var result = mcTCP.WriteDeviceBlock(address, lenght, data);
                    if (result == 0) return true; else return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public int ReadDword(string address, out bool success)
        {
            success = false;
            try
            {
                int[] data = new int[2];
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(address, 2, data);
                }
                success = true;
                return ArrayIntToDword(data);
            }
            catch
            {
                return -1;
            }
        }
        public int[] ReadDwordBlock(string address, int length, out bool success)
        {
            success = false;
            try
            {
                int[] data = new int[2 * length];
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(address, 2 * length, data);
                }
                success = true;
                int[] result = new int[length];
                int j = 0;
                for (int i = 0; i < 2 * length; i += 2)
                {
                    result[j] = ArrayIntToDword(new[] { data[i], data[i + 1] });
                    j += 1;
                }
                return result;
            }
            catch
            {
                return new int[] { };
            }
        }
        public bool WriteDWordBlock(string address, int lenght, int[] data)
        {
            try
            {
                if (!IsConnected)
                    mcTCP.Open();
                lock (LockCommunication)
                {
                    var result = mcTCP.WriteDeviceBlock(address, lenght, data);
                    if (result == 0) return true; else return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool WriteDword(string address, int data)
        {
            try
            {
                if (!IsConnected)
                    mcTCP.Open();
                int[] dataInt = DwordToArrayInt(data);
                int result = -1;
                lock (LockCommunication)
                {
                    result = mcTCP.WriteDeviceBlock(address, dataInt.Length, dataInt);
                }
                if (result == 0) return true; else return false;
            }
            catch
            {
                return false;
            }
        }
        public bool WriteInt(string address, int data)
        {
            try
            {
                if (!IsConnected) mcTCP.Open();
                int result = -1;
                lock (LockCommunication)
                {
                    result = mcTCP.SetDevice(address, data);
                }
                if (result == 0) return true; else return false;
            }
            catch
            {
                return false;
            }
        }
        public bool WriteIntBlock(string address, int[] data)
        {
            try
            {
                if (!IsConnected)
                    mcTCP.Open();
                lock (LockCommunication)
                    foreach (var dataItem in data)
                    {
                        var result = mcTCP.SetDevice(address, dataItem);
                        if (result == 0) return true; else return false;
                    }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public float ReadFloat(string address, out bool success)
        {
            success = false;
            int[] data = new int[2];
            try
            {
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(address, 2, data);
                }
                float floatData = ArrayIntToFloat(data);
                success = true;
                return floatData;
            }
            catch
            {
                return 0;
            }
        }
        public float[] ReadFloatBlock(string address, int length, out bool success)
        {
            success = false;
            int[] data = new int[2 * length];
            try
            {
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(address, 2 * length, data);
                }
                float[] result = new float[length];
                int j = 0;
                for (int i = 0; i < 2 * length; i += 2)
                {
                    result[j] = ArrayIntToFloat(new[] { data[i], data[i + 1] });
                    j += 1;
                }

                success = true;
                return result;
            }
            catch
            {
                return new float[] { };
            }
        }
        public bool WriteFloat(string address, float data)
        {
            try
            {
                if (!IsConnected) mcTCP.Open();
                int[] dataInt = FloatToArrayInt(data);
                int result = -1;
                lock (LockCommunication)
                {
                    result = mcTCP.WriteDeviceBlock(address, dataInt.Length, dataInt);
                }
                if (result == 0) return true; else return false;
            }
            catch
            {
                return false;
            }
        }
        public string ReadString(string address, int number, out bool success)
        {
            success = false;
            try
            {
                int[] data = new int[number];
                if (!IsConnected) mcTCP.Open();
                lock (LockCommunication)
                {
                    mcTCP.ReadDeviceBlock(address, number, data);
                }
                string sData = ArrayIntToString(data, number);
                success = true;
                return sData;
            }
            catch
            {
                return "";
            }
        }
        public bool WriteString(string address, string data)
        {
            try
            {
                if (!IsConnected)
                    mcTCP.Open();
                int[] dataInt = StringToArrayInt(data, data.Length);
                int result = -1;
                lock (LockCommunication)
                {
                    result = mcTCP.WriteDeviceBlock(address, dataInt.Length, dataInt);
                }
                if (result == 0) return true; else return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion Communication
    }
    public enum McFrame
    {
        MC3E
        , MC4E
    }
    public enum PlcDeviceType
    {
        // PLC用デバイス
        M = 0x90
      , SM = 0x91
      , L = 0x92
      , F = 0x93
      , V = 0x94
      , S = 0x98
      , X = 0x9C
      , Y = 0x9D
      , B = 0xA0
      , SB = 0xA1
      , DX = 0xA2
      , DY = 0xA3
      , D = 0xA8
      , SD = 0xA9
      , R = 0xAF
      , ZR = 0xB0
      , W = 0xB4
      , SW = 0xB5
      , TC = 0xC0
      , TS = 0xC1
      , TN = 0xC2
      , CC = 0xC3
      , CS = 0xC4
      , CN = 0xC5
      , SC = 0xC6
      , SS = 0xC7
      , SN = 0xC8
      , Z = 0xCC
      , TT
      , TM
      , CT
      , CM
      , A
      , Max
    }
    interface Plc : IDisposable
    {
        int Open();
        int Close();
        int SetBitDevice(string iDeviceName, int iSize, int[] iData);
        int SetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] iData);
        int GetBitDevice(string iDeviceName, int iSize, int[] oData);
        int GetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] oData);
        int WriteDeviceBlock(string iDeviceName, int iSize, int[] iData);
        int WriteDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] iData);
        int ReadDeviceBlock(string iDeviceName, int iSize, int[] oData);
        int ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] oData);
        int SetDevice(string iDeviceName, int iData);
        int SetDevice(PlcDeviceType iType, int iAddress, int iData);
        int GetDevice(string iDeviceName, out int oData);
        int GetDevice(PlcDeviceType iType, int iAddress, out int oData);
    }
    abstract class McProtocolApp : Plc
    {
        // ====================================================================================
        public McFrame CommandFrame { get; set; }   // 使用フレーム
        public string HostName { get; set; }   // ホスト名またはIPアドレス
        public int PortNumber { get; set; }    // ポート番号
                                               // ====================================================================================
                                               // コンストラクタ
        protected McProtocolApp(string iHostName, int iPortNumber)
        {
            CommandFrame = McFrame.MC3E;
            HostName = iHostName;
            PortNumber = iPortNumber;
        }

        // ====================================================================================
        // 後処理
        public void Dispose()
        {
            Close();
        }

        // ====================================================================================
        public int Open()
        {
            DoConnect();
            Command = new McCommand(CommandFrame);
            return 0;
        }
        // ====================================================================================
        public int Close()
        {
            DoDisconnect();
            return 0;
        }
        // ====================================================================================
        public int SetBitDevice(string iDeviceName, int iSize, int[] iData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return SetBitDevice(type, addr, iSize, iData);
        }
        // ====================================================================================
        public int SetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] iData)
        {
            var type = iType;
            var addr = iAddress;
            var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
            var d = (byte)iData[0];
            var i = 0;
            while (i < iData.Length)
            {
                if (i % 2 == 0)
                {
                    d = (byte)iData[i];
                    d <<= 4;
                }
                else
                {
                    d |= (byte)(iData[i] & 0x01);
                    data.Add(d);
                }
                ++i;
            }
            if (i % 2 != 0)
            {
                data.Add(d);
            }
            byte[] sdCommand = Command.SetCommand(0x1401, 0x0001, data.ToArray());
            byte[] rtResponse = TryExecution(sdCommand);
            int rtCode = Command.SetResponse(rtResponse);
            return rtCode;
        }
        // ====================================================================================
        public int GetBitDevice(string iDeviceName, int iSize, int[] oData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return GetBitDevice(type, addr, iSize, oData);
        }
        // ====================================================================================
        public int GetBitDevice(PlcDeviceType iType, int iAddress, int iSize, int[] oData)
        {
            PlcDeviceType type = iType;
            int addr = iAddress;
            var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
            byte[] sdCommand = Command.SetCommand(0x0401, 0x0001, data.ToArray());
            byte[] rtResponse = TryExecution(sdCommand);
            int rtCode = Command.SetResponse(rtResponse);
            byte[] rtData = Command.Response;
            for (int i = 0; i < iSize; ++i)
            {
                if (i % 2 == 0)
                {
                    oData[i] = (rtCode == 0) ? ((rtData[i / 2] >> 4) & 0x01) : 0;
                }
                else
                {
                    oData[i] = (rtCode == 0) ? (rtData[i / 2] & 0x01) : 0;
                }
            }
            return rtCode;
        }
        // ====================================================================================
        public int WriteDeviceBlock(string iDeviceName, int iSize, int[] iData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return WriteDeviceBlock(type, addr, iSize, iData);
        }
        // ====================================================================================
        public int WriteDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] iData)
        {
            PlcDeviceType type = iType;
            int addr = iAddress;
            var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
            foreach (int t in iData)
            {
                data.Add((byte)t);
                data.Add((byte)(t >> 8));
            }
            byte[] sdCommand = Command.SetCommand(0x1401, 0x0000, data.ToArray());
            byte[] rtResponse = TryExecution(sdCommand);
            int rtCode = Command.SetResponse(rtResponse);
            return rtCode;
        }
        // ====================================================================================
        public int ReadDeviceBlock(string iDeviceName, int iSize, int[] oData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return ReadDeviceBlock(type, addr, iSize, oData);
        }
        // ====================================================================================
        public int ReadDeviceBlock(PlcDeviceType iType, int iAddress, int iSize, int[] oData)
        {
            PlcDeviceType type = iType;
            int addr = iAddress;
            var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , (byte) iSize
                      , (byte) (iSize >> 8)
                    };
            byte[] sdCommand = Command.SetCommand(0x0401, 0x0000, data.ToArray());
            byte[] rtResponse = TryExecution(sdCommand);
            int rtCode = Command.SetResponse(rtResponse);
            byte[] rtData = Command.Response;
            for (int i = 0; i < iSize; ++i)
            {
                oData[i] = (rtCode == 0) ? BitConverter.ToInt16(rtData, i * 2) : 0;
            }
            return rtCode;
        }
        // ====================================================================================
        public int SetDevice(string iDeviceName, int iData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            int result = SetDevice(type, addr, iData);
            Stopwatch timeOut = new Stopwatch();
            timeOut.Start();
            while (GetDevice(iDeviceName, out int data) == 0)
            {
                if (data == iData)
                    break;
                if (timeOut.ElapsedMilliseconds > 300)
                    break;
            }

            timeOut = null;
            return result;
            //return SetDevice(type, addr, iData);
        }
        // ====================================================================================
        public int SetDevice(PlcDeviceType iType, int iAddress, int iData)
        {
            PlcDeviceType type = iType;
            int addr = iAddress;
            var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , 0x01
                      , 0x00
                      , (byte) iData
                      , (byte) (iData >> 8)
                    };
            byte[] sdCommand = Command.SetCommand(0x1401, 0x0000, data.ToArray());
            byte[] rtResponse = TryExecution(sdCommand);
            int rtCode = Command.SetResponse(rtResponse);
            return rtCode;
        }
        // ====================================================================================
        public int GetDevice(string iDeviceName, out int oData)
        {
            PlcDeviceType type;
            int addr;
            GetDeviceCode(iDeviceName, out type, out addr);
            return GetDevice(type, addr, out oData);
        }
        // ====================================================================================
        public int GetDevice(PlcDeviceType iType, int iAddress, out int oData)
        {
            PlcDeviceType type = iType;
            int addr = iAddress;
            var data = new List<byte>(6)
                    {
                        (byte) addr
                      , (byte) (addr >> 8)
                      , (byte) (addr >> 16)
                      , (byte) type
                      , 0x01
                      , 0x00
                    };
            byte[] sdCommand = Command.SetCommand(0x0401, 0x0000, data.ToArray());
            byte[] rtResponse = TryExecution(sdCommand);
            int rtCode = Command.SetResponse(rtResponse);
            if (0 < rtCode)
            {
                oData = 0;
            }
            else
            {
                byte[] rtData = Command.Response;
                oData = BitConverter.ToInt16(rtData, 0);
            }
            return rtCode;
        }
        // ====================================================================================
        //public int GetCpuType(out string oCpuName, out int oCpuType)
        //{
        //    int rtCode = Command.Execute(0x0101, 0x0000, new byte[0]);
        //    oCpuName = "dummy";
        //    oCpuType = 0;
        //    return rtCode;
        //}
        // ====================================================================================
        public static PlcDeviceType GetDeviceType(string s)
        {
            return (s == "M") ? PlcDeviceType.M :
                   (s == "SM") ? PlcDeviceType.SM :
                   (s == "L") ? PlcDeviceType.L :
                   (s == "F") ? PlcDeviceType.F :
                   (s == "V") ? PlcDeviceType.V :
                   (s == "S") ? PlcDeviceType.S :
                   (s == "X") ? PlcDeviceType.X :
                   (s == "Y") ? PlcDeviceType.Y :
                   (s == "B") ? PlcDeviceType.B :
                   (s == "SB") ? PlcDeviceType.SB :
                   (s == "DX") ? PlcDeviceType.DX :
                   (s == "DY") ? PlcDeviceType.DY :
                   (s == "D") ? PlcDeviceType.D :
                   (s == "SD") ? PlcDeviceType.SD :
                   (s == "R") ? PlcDeviceType.R :
                   (s == "ZR") ? PlcDeviceType.ZR :
                   (s == "W") ? PlcDeviceType.W :
                   (s == "SW") ? PlcDeviceType.SW :
                   (s == "TC") ? PlcDeviceType.TC :
                   (s == "TS") ? PlcDeviceType.TS :
                   (s == "TN") ? PlcDeviceType.TN :
                   (s == "CC") ? PlcDeviceType.CC :
                   (s == "CS") ? PlcDeviceType.CS :
                   (s == "CN") ? PlcDeviceType.CN :
                   (s == "SC") ? PlcDeviceType.SC :
                   (s == "SS") ? PlcDeviceType.SS :
                   (s == "SN") ? PlcDeviceType.SN :
                   (s == "Z") ? PlcDeviceType.Z :
                   (s == "TT") ? PlcDeviceType.TT :
                   (s == "TM") ? PlcDeviceType.TM :
                   (s == "CT") ? PlcDeviceType.CT :
                   (s == "CM") ? PlcDeviceType.CM :
                   (s == "A") ? PlcDeviceType.A :
                                 PlcDeviceType.Max;
        }

        // ====================================================================================
        public static bool IsBitDevice(PlcDeviceType type)
        {
            return !((type == PlcDeviceType.D)
                  || (type == PlcDeviceType.SD)
                  || (type == PlcDeviceType.Z)
                  || (type == PlcDeviceType.ZR)
                  || (type == PlcDeviceType.R)
                  || (type == PlcDeviceType.W));
        }

        // ====================================================================================
        public static bool IsHexDevice(PlcDeviceType type)
        {
            return (type == PlcDeviceType.X)
                || (type == PlcDeviceType.Y)
                || (type == PlcDeviceType.B)
                || (type == PlcDeviceType.W);
        }

        // ====================================================================================
        public static void GetDeviceCode(string iDeviceName, out PlcDeviceType oType, out int oAddress)
        {
            string s = iDeviceName.ToUpper();
            string strAddress;

            // 1文字取り出す
            string strType = s.Substring(0, 1);
            switch (strType)
            {
                case "A":
                case "B":
                case "D":
                case "F":
                case "L":
                case "M":
                case "R":
                case "V":
                case "W":
                case "X":
                case "Y":
                    // 2文字目以降は数値のはずなので変換する
                    strAddress = s.Substring(1);
                    break;
                case "Z":
                    // もう1文字取り出す
                    strType = s.Substring(0, 2);
                    // ファイルレジスタの場合     : 2
                    // インデックスレジスタの場合 : 1
                    strAddress = s.Substring(strType.Equals("ZR") ? 2 : 1);
                    break;
                case "C":
                    // もう1文字取り出す
                    strType = s.Substring(0, 2);
                    switch (strType)
                    {
                        case "CC":
                        case "CM":
                        case "CN":
                        case "CS":
                        case "CT":
                            strAddress = s.Substring(2);
                            break;
                        default:
                            throw new Exception("Invalid format.");
                    }
                    break;
                case "S":
                    // もう1文字取り出す
                    strType = s.Substring(0, 2);
                    switch (strType)
                    {
                        case "SD":
                        case "SM":
                            strAddress = s.Substring(2);
                            break;
                        default:
                            throw new Exception("Invalid format.");
                    }
                    break;
                case "T":
                    // もう1文字取り出す
                    strType = s.Substring(0, 2);
                    switch (strType)
                    {
                        case "TC":
                        case "TM":
                        case "TN":
                        case "TS":
                        case "TT":
                            strAddress = s.Substring(2);
                            break;
                        default:
                            throw new Exception("Invalid format.");
                    }
                    break;
                default:
                    throw new Exception("Invalid format.");
            }

            oType = GetDeviceType(strType);
            oAddress = IsHexDevice(oType) ? Convert.ToInt32(strAddress, BlockSize) :
                                            Convert.ToInt32(strAddress);
        }
        // &&&&& protected &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        abstract protected bool DoConnect();
        abstract protected void DoDisconnect();
        abstract protected byte[] Execute(byte[] iCommand);
        // &&&&& private &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        private const int BlockSize = 0x0010;
        private McCommand Command { get; set; }

        //private bool _Connected;

        //public bool Connected
        //{
        //    get 
        //    {
        //        if (Cli)
        //        {

        //        }
        //        return _Connected; 
        //    }
        //    set { _Connected = value; }
        //}

        // ================================================================================
        private byte[] TryExecution(byte[] iCommand)
        {
            byte[] rtResponse;
            int tCount = 10;
            do
            {
                rtResponse = Execute(iCommand);
                --tCount;
                if (tCount < 0)
                {
                    throw new Exception("PLCから正しい値が取得できません.");
                }
            } while (Command.IsIncorrectResponse(rtResponse));
            return rtResponse;
        }
        // ####################################################################################
        // 通信に使用するコマンドを表現するインナークラス
        class McCommand
        {
            private McFrame FrameType { get; set; }  // フレーム種別
            private uint SerialNumber { get; set; }  // シリアル番号
            private uint NetwrokNumber { get; set; } // ネットワーク番号
            private uint PcNumber { get; set; }      // PC番号
            private uint IoNumber { get; set; }      // 要求先ユニットI/O番号
            private uint ChannelNumber { get; set; } // 要求先ユニット局番号
            private uint CpuTimer { get; set; }      // CPU監視タイマ
            private int ResultCode { get; set; }     // 終了コード
            public byte[] Response { get; private set; }    // 応答データ
                                                            // ================================================================================
                                                            // コンストラクタ
            public McCommand(McFrame iFrame)
            {
                FrameType = iFrame;
                SerialNumber = 0x0001u;
                NetwrokNumber = 0x0000u;
                PcNumber = 0x00FFu;
                IoNumber = 0x03FFu;
                ChannelNumber = 0x0000u;
                CpuTimer = 0x0010u;
            }
            // ================================================================================
            public byte[] SetCommand(uint iMainCommand, uint iSubCommand, byte[] iData)
            {
                var dataLength = (uint)(iData.Length + 6);
                var ret = new List<byte>(iData.Length + 20);
                uint frame = (FrameType == McFrame.MC3E) ? 0x0050u :
                             (FrameType == McFrame.MC4E) ? 0x0054u : 0x0000u;
                ret.Add((byte)frame);
                ret.Add((byte)(frame >> 8));
                if (FrameType == McFrame.MC4E)
                {
                    ret.Add((byte)SerialNumber);
                    ret.Add((byte)(SerialNumber >> 8));
                    ret.Add(0x00);
                    ret.Add(0x00);
                }
                ret.Add((byte)NetwrokNumber);
                ret.Add((byte)PcNumber);
                ret.Add((byte)IoNumber);
                ret.Add((byte)(IoNumber >> 8));
                ret.Add((byte)ChannelNumber);
                ret.Add((byte)dataLength);
                ret.Add((byte)(dataLength >> 8));
                ret.Add((byte)CpuTimer);
                ret.Add((byte)(CpuTimer >> 8));
                ret.Add((byte)iMainCommand);
                ret.Add((byte)(iMainCommand >> 8));
                ret.Add((byte)iSubCommand);
                ret.Add((byte)(iSubCommand >> 8));
                ret.AddRange(iData);
                return ret.ToArray();
            }
            // ================================================================================
            public int SetResponse(byte[] iResponse)
            {
                int min = (FrameType == McFrame.MC3E) ? 11 : 15;
                if (min <= iResponse.Length)
                {
                    var btCount = new[] { iResponse[min - 4], iResponse[min - 3] };
                    var btCode = new[] { iResponse[min - 2], iResponse[min - 1] };
                    int rsCount = BitConverter.ToUInt16(btCount, 0);
                    ResultCode = BitConverter.ToUInt16(btCode, 0);
                    Response = new byte[rsCount - 2];
                    Buffer.BlockCopy(iResponse, min, Response, 0, Response.Length);
                }
                return ResultCode;
            }
            // ================================================================================
            public bool IsIncorrectResponse(byte[] iResponse)
            {
                var min = (FrameType == McFrame.MC3E) ? 11 : 15;
                var btCount = new[] { iResponse[min - 4], iResponse[min - 3] };
                var btCode = new[] { iResponse[min - 2], iResponse[min - 1] };
                var rsCount = BitConverter.ToUInt16(btCount, 0) - 2;
                var rsCode = BitConverter.ToUInt16(btCode, 0);
                return (rsCode == 0 && rsCount != (iResponse.Length - min));
            }
        }
    }
    class McProtocolTcp : McProtocolApp
    {
        // ====================================================================================
        // コンストラクタ
        public string ErrMessage { get; set; }
        public int ConnectTimeOut { get; set; }

        public McProtocolTcp() : this("", 0)
        {
            ErrMessage = String.Empty;
            ConnectTimeOut = 3000;
        }
        public McProtocolTcp(string iHostName, int iPortNumber)
            : base(iHostName, iPortNumber)
        {
            Client = new TcpClient();
            ErrMessage = String.Empty;
            ConnectTimeOut = 3000;
        }

        // &&&&& protected &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        override protected bool DoConnect()
        {
            DoDisconnect();
            TcpClient c = Client;
            try
            {
                if (c == null)
                    c = new TcpClient();
                if (!c.Connected)
                {
                    // Keep Alive機能の実装
                    c = new TcpClient();
                    var ka = new List<byte>(sizeof(uint) * 3);
                    ka.AddRange(BitConverter.GetBytes(1u));
                    ka.AddRange(BitConverter.GetBytes(45000u));
                    ka.AddRange(BitConverter.GetBytes(5000u));
                    c.Client.IOControl(IOControlCode.KeepAliveValues, ka.ToArray(), null);
                    //c.Connect(HostName, PortNumber);

                    var result = c.BeginConnect(HostName, PortNumber, null, null);
                    double timeOut = ConnectTimeOut / 1000;
                    if (timeOut < 1)
                        timeOut = 1;
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeOut));
                    if (!success)
                        return false;
                    Stream = c.GetStream();
                }
            }
            catch (Exception ex)
            {
                ErrMessage = ex.Message;
                //return false;
            }
            Client = c;
            return c.Connected;
        }
        // ====================================================================================
        override protected void DoDisconnect()
        {
            if (Client != null)
            {
                try
                {
                    Stream?.Close();
                    Stream?.Dispose();
                    //Client.GetStream()?.Close();
                    //Client.GetStream()?.Dispose();
                }
                catch
                {
                }
                Client.Close();
                Client.Dispose();
                Client = null;
            }
        }
        // ================================================================================
        override protected byte[] Execute(byte[] iCommand)
        {
            NetworkStream ns = Client.GetStream();
            ns.Write(iCommand, 0, iCommand.Length);
            ns.Flush();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (var ms = new MemoryStream())
            {
                ns.ReadTimeout = 100;
                ns.WriteTimeout = 100;
                var buff = new byte[256];
                do
                {
                    int sz = ns.Read(buff, 0, buff.Length);
                    if (sz == 0)
                        throw new Exception("Disconnected");
                    ms.Write(buff, 0, sz);

                    if (sw.ElapsedMilliseconds > 3000)
                    {
                        sw.Stop();
                        sw = null;
                        throw new TimeoutException();
                    }

                } while (ns.DataAvailable);
                sw.Stop();
                sw = null;
                return ms.ToArray();
            }
        }
        // &&&&& private &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        public TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }
    }
    class McProtocolUdp : McProtocolApp
    {
        // ====================================================================================
        // コンストラクタ
        public McProtocolUdp(int iPortNumber) : this("", iPortNumber) { }
        public McProtocolUdp(string iHostName, int iPortNumber)
            : base(iHostName, iPortNumber)
        {
            Client = new UdpClient(iPortNumber);
        }

        // &&&&& protected &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        override protected bool DoConnect()
        {
            UdpClient c = Client;
            try
            {
                c.Connect(HostName, PortNumber);
            }
            catch
            {
                return false;
            }

            if (c.Client != null)
                return c.Client.Connected;

            return false;
        }
        // ====================================================================================
        override protected void DoDisconnect()
        {
            // UDPでは何もしない
        }
        // ================================================================================
        override protected byte[] Execute(byte[] iCommand)
        {
            UdpClient c = Client;
            // 送信
            c.Send(iCommand, iCommand.Length);

            using (var ms = new MemoryStream())
            {
                IPAddress ip = IPAddress.Parse(HostName);
                var ep = new IPEndPoint(ip, PortNumber);
                do
                {
                    // 受信
                    byte[] buff = c.Receive(ref ep);
                    ms.Write(buff, 0, buff.Length);
                } while (0 < c.Available);
                return ms.ToArray();
            }
        }
        // &&&&& private &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        private UdpClient Client { get; set; }
    }
}
