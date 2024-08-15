/*
 Copyright (c) 2024 vasilyevl (Grumpy). Permission is hereby granted, 
free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"),to deal in the Software 
without restriction, including without limitation the rights to use, copy, 
modify, merge, publish, distribute, sublicense, and/or sell copies of the 
Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included 
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


using Grumpy.ClickPLC.Net.Driver.ModBus;

using Newtonsoft.Json;

using System.Runtime.CompilerServices;


namespace Grumpy.ClickPLC.Net.Driver
{
    public class ClickModBusDriver :  IClickModBusDriver
    {
        internal const string TimerPrefix = "T";
        internal const string CounterPrefix = "CT";
        internal const string TimerDataRegisterPrefix = "TD";
        internal const string CounterDataRegisterPrefix = "CTD";

        private readonly Dictionary<IOType, int> _StartAddresses;

        private Dictionary<string, object> Controls;

        private ILogRecord? _lastError;
        private ModbusClient? _mbClient;
        private ClickDriverConfiguration? _configuration;

        internal ClickModBusDriver() {

            _mbClient = new ModbusClient();
            _configuration = null;
            _lastError =null;
            Controls = new Dictionary<string, object>();


            _StartAddresses =
            new Dictionary<IOType, int>() {

                {IOType.Input, ClickAddressMap.XStartAddressHex},
                {IOType.Output, ClickAddressMap.YStartAddressHex},
                {IOType.ControlRelay, ClickAddressMap.CStartAddressHex},
                {IOType.RegisterInt16, ClickAddressMap.DSStartAddressHex},
                {IOType.Timer, ClickAddressMap.TStartAddressHex},
                {IOType.RegisterFloat32, ClickAddressMap.DFStartAddressHex},
                {IOType.SystemControlRelay, ClickAddressMap.SCStartAddressHex},
                {IOType.RegisterHex, ClickAddressMap.CStartAddressHex },
                {IOType.InputRegister, ClickAddressMap.XDStartAddressHex},
                {IOType.OutputRegister, ClickAddressMap.YDStartAddressHex},
                {IOType.TimerRegister, ClickAddressMap.TDStartAddressHex},
                {IOType.CounterRegister, ClickAddressMap.CTDStartAddressHex},
            };

        }


        /// <summary>
        /// Creates and a CLICK PLC ModBus driver instance.
        /// </summary>
        /// <returns>
        /// <c>IClickModBusDriver</c> if the driver was successfully created; otherwise, <c>null</c>.
        /// </returns>
        /// 
        public static IClickModBusDriver CreateDriver() => new ClickModBusDriver();

        /// <summary>
        /// Creates and initializes a CLICK PLC ModBus driver instance using the provided configuration.
        /// </summary>
        /// <param name="configuration">
        /// A JSON string representing the network configuration required to establish communication with the PLC.
        /// The configuration should include details such as the IP address, port number, and any other necessary parameters.
        /// </param>
        /// <param name="driver">
        /// When this method returns, contains an initialized instance of <see cref="IClickModBusDriver"/> if the creation is successful; 
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the driver was successfully created and initialized; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to parse the provided JSON configuration and instantiate a driver that can communicate 
        /// with a CLICK PLC using the ModBus TCP protocol. Ensure that the JSON configuration string is correctly formatted and 
        /// contains all necessary parameters.
        /// </remarks>
        /// 

        public static bool CreateDriver(string configuration,
            out IClickModBusDriver? driver) {

            driver = new ClickModBusDriver();
            if (!driver.Init(configuration)) {
                driver = null;
            }
            return driver is not null;
        }


        public bool Init(object cnfg) {


            if (IsOpen) {
                SaveLasttError(nameof(Init),
                                       ClickErrorCode.ProhibitedWhenControllerIsConnected, "");
                return false;
            }

            var configuration = cnfg as ClickDriverConfiguration;

            if (configuration == null) {
                SaveLasttError(nameof(Init),
                    ClickErrorCode.ConfigurationIsNotProvided,
                    "Provided configuration object is \"null\"");
                return false;
            }

            _configuration = configuration.Clone() as ClickDriverConfiguration;

            if (_configuration == null) {
                SaveLasttError(nameof(Init),
                                ClickErrorCode.ConfigurationNotSet,
                                "Failed to clone provided configuration object.");
            }

            return _configuration != null;
        }

        public bool Init(string configJsonString) {

            if (IsOpen) {
                SaveLasttError(nameof(Init),
                    ClickErrorCode.ProhibitedWhenControllerIsConnected, "");
                return false;
            }

            if (string.IsNullOrEmpty(configJsonString)) {
                SaveLasttError(nameof(Init),
                    ClickErrorCode.ConfigurationIsNotProvided,
                    "Provided configuration string is \"null\" or empty.");
                return false;
            }

            try {

                _configuration = JsonConvert.DeserializeObject<ClickDriverConfiguration>(configJsonString);
                return true;
            }
            catch (Exception ex) {
                SaveLasttError(nameof(Init),
                     ClickErrorCode.ConfigDeserializationError, ex.Message);
                return false;
            }
        }

        private bool _OpenTcpIp() {

            if (_configuration?.Interface?.Network is null) {
                SaveLasttError(nameof(Open),
                                       ClickErrorCode.OpenFailed,
                                                          "Network configuration is not provided.");
                return false;
            }
            else if (_configuration.Interface.Network.IpAddress is null
                               || _configuration.Interface.Network.Port < 0) {
                SaveLasttError(nameof(Open),
                                       ClickErrorCode.OpenFailed,
                                                          $"IP configuration is not valid.");
                return false;
            }

            _mbClient = new ModbusClient(
                               _configuration.Interface.Network.IpAddress,
                                              _configuration.Interface.Network.Port);

            return _mbClient.Connect();
        }

        private bool _DisconnectIfConnected([CallerMemberName] string callerMethodName = "") {

            if (_mbClient != null && _mbClient.IsConnected) {

                try {
                    _mbClient.Disconnect();
                    return true;
                }
                catch (Exception ex) {
                    SaveLasttError(callerMethodName,
                                                   ClickErrorCode.CloseFailed, ex.Message);
                    return false;
                }
            }

            return true;
        }

        public bool Open() {

            if (_configuration == null) {
                SaveLasttError(nameof(Open),
                    ClickErrorCode.ConfigurationNotSet);
                return false;
            }
            return _OpenTcpIp();
        }

        public bool Close() {
            try {
                _mbClient?.Disconnect();
                return true;
            }
            catch (Exception ex) {

                SaveLasttError(nameof(Close), ClickErrorCode.CloseFailed,
                    ClickPlcDriverErrors.GetErrorDescription(
                                                ClickErrorCode.CloseFailed)
                    + $" Exception: {ex.Message}");
                return false;
            }
        }

        public bool IsOpen => _mbClient?.IsConnected ?? false;

        public ILogRecord? LastRecord =>_lastError;
       

        public bool WriteDiscreteControl(string name, SwitchCtrl sw) {

            if (_CanReadWrite(nameof(WriteDiscreteControl))) {

                if (ClickAddressMap.GetModBusHexAddress(
                       ioFunction: IoFunction.SingleControlWrite,
                       control: name, out int address,
                       out int functionCode) == ClickErrorCode.NoError) {

                    try {

                        _mbClient!.WriteSingleCoil(address,
                            sw == SwitchCtrl.On, functionCode);
                        return true;
                    }
                    catch (Exception ex) {

                        SaveLasttError(nameof(WriteDiscreteControl),
                            ClickErrorCode.NotWritableControl,
                            $"\"{address}\" control is not writable. {ex.Message}");
                        return false;
                    }
                }
            }
            return false;
        }

        public bool WriteDiscreteControls(string startName, SwitchCtrl[] controls) {

            if (_CanReadWrite(nameof(WriteDiscreteControls))) {

                if (_DecodeControlName(startName, out IOType ioType,
                                   out int address, write: true)) {

                    try {

                        _mbClient!.WriteMultipleCoils(address,
                                  controls.Select((c) => c == SwitchCtrl.On).ToArray());
                        return true;
                    }
                    catch (Exception ex) {

                        SaveLasttError(nameof(WriteDiscreteControls),
                                        ClickErrorCode.GroupIoWriteFailed,
                                        ex.Message);
                        return false;
                    }
                }
            }
            return false;
        }

        public bool ReadDiscreteControl(string name, out SwitchState state) {

            bool r = ReadDiscreteIO(name, out SwitchSt st);
            state = new SwitchState(st);
            return r;
        }

        internal bool ReadDiscreteIO(string name, out SwitchSt readback) {

            if (_CanReadWrite(nameof(ReadDiscreteIO))) {

                if (ClickAddressMap.GetModBusHexAddress(
                    ioFunction: IoFunction.SingleControlRead,
                    control: name, out int address,
                    out int functionCode) == ClickErrorCode.NoError) {

                    try {

                        _mbClient!.ReadCoils(address, 1, out bool[]? data, functionCode);
                        if (data is not null) {
                            readback = data[0] ? SwitchSt.On : SwitchSt.Off;

                            return true;
                        }
                        readback = SwitchSt.Unknown;
                        return false;
                    }
                    catch (Exception ex) {

                        SaveLasttError(nameof(ReadDiscreteControl),
                            ClickErrorCode.GroupIoWriteFailed,
                            ex.Message);
                        readback = SwitchSt.Unknown;
                        return false;
                    }
                }
            }

            readback = SwitchSt.Unknown;
            return false;
        }

        public bool ReadDiscreteControls(string name, int numberOfIosToRead, out SwitchState[] readback) {

            if (_CanReadWrite(nameof(ReadDiscreteControls))) {

                if (ClickAddressMap.GetModBusHexAddress(ioFunction: IoFunction.MultipleControlRead,
                    control: name, out int address, out int functionCode) == ClickErrorCode.NoError) {

                    try {

                        if (_mbClient!.ReadCoils(address,

                            Math.Max(1, numberOfIosToRead), out bool[]? data, functionCode)) {

                            if (data is not null) {

                                readback =
                                    data.Select((st) => new SwitchState(st ? SwitchSt.On : SwitchSt.Off)).ToArray();
                                return true;
                            }
                        }
                    }
                    catch (Exception ex) {

                        SaveLasttError(nameof(ReadDiscreteControl),
                            ClickErrorCode.GroupIoWriteFailed,
                            ex.Message);
                    }
                }
            }

            readback = Enumerable.Repeat(new SwitchState(SwitchSt.Unknown), numberOfIosToRead).ToArray();
            return false;
        }


        public bool ReadInt16Register(string name, out short value) {

            value = -1;

            if (_CanReadWrite(nameof(ReadInt16Register))) {

                int address = -1;
                try {
                    if ((ClickAddressMap.GetModBusHexAddress(ioFunction: IoFunction.SingleControlRead,
                               control: name, out address, out int functionCode) == ClickErrorCode.NoError)
                               &&
                            _mbClient!.ReadInputRegisters(address, 1, out int[]? response, functionCode)) {

                        value = (short)(response![0]);
                        return true;
                    }
                }
                catch (Exception ex){
                    SaveLasttError(nameof(ReadInt16Register),
                                           ClickErrorCode.NotWritableControl,
                                          $"\"{name}\"\"{address}\" control is not " +
                                          $"writable. {ex.Message}");
                }
            }
            return false;
   
            
        }

        public bool ReadUInt16Register(string name, out ushort value) {

            if (_CanReadWrite(nameof(ReadUInt16Register))) {

                if (ReadInt16Register(name, out short valueInt16)) {

                    value = (ushort)valueInt16;
                    return true;
                }
            }

            value = 0xFFFF;
            return false;
        }

        public bool WriteUInt16Register(string name, ushort value) {

            if (_CanReadWrite(nameof(WriteUInt16Register))) {

                int address = -1;
                try {
                    return _DecodeControlName(name, out IOType ioType,
                                              out address, write: true)
                           && (_mbClient?.WriteSingle16bitRegister(address, value) ?? false);
                } 
                catch (Exception ex) {
                    SaveLasttError(nameof(WriteUInt16Register),
                                  ClickErrorCode.NotWritableControl, 
                                  $"\"{name}\"\"{address}\" control is not " +
                                          $"writable. {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        public bool WriteInt16Register(string name, short value) {

            if (_CanReadWrite(nameof(WriteInt16Register))){

                return _DecodeControlName(name, out IOType ioType,
                                           out int address, write: true)
                     && (_mbClient?.WriteSingle16bitRegister(address, (ushort)value) ?? false);
            }

            return false;
        }


        public bool WriteFloat32Register(string name, float value) {

            if (_DecodeControlName(name, out IOType ioType, out int address, write: true)) {
                try {
                    var res = Utilities.ConvertFloatToRegisters(value);
                    _mbClient?.WriteMultipleRegisters(address, res);
                    return true;
                }
                catch (Exception ex) {
                    SaveLasttError(nameof(WriteFloat32Register),
                                               ClickErrorCode.InvalidControlName, ex.Message);
                    return false;
                }
            }

            return false;
        }

        public bool ReadFloat32Register(string name, out float value) {

            if (_DecodeControlName(name, out IOType ioType, out int address, write: false)) {
                try {

                    int[]? data = null;
                    Boolean res = _mbClient?.ReadInputRegisters(address, 2, out data) ?? false;

                    string? error = null;
                    if (res && Utilities.ConvertRegistersToFloat(data, RegisterOrder.LowHigh, out value, out error)) {

                        return true;
                    }
                    else {
                        SaveLasttError(nameof(ReadFloat32Register), ClickErrorCode.FailedTConvertRegistersToFloat,
                                    $"{nameof(ReadFloat32Register)}  {error ?? string.Empty}");
                    }
                }
                catch (Exception ex) {
                    SaveLasttError(nameof(ReadFloat32Register), ClickErrorCode.InvalidControlName, ex.Message);
                }
            }

            value = float.NaN;
            return false;
        }


        #region Private Methods

        private bool _CanReadWrite(string caller = "") {

            if (_mbClient == null || !_mbClient.IsConnected) {

                SaveLasttError(nameof(_CanReadWrite),
                     ClickErrorCode.NotConnected, $"Can't read/write when" +
                     $" {(_mbClient == null? 
                            "ModBas client is not instantiated" : 
                            "not connected")}.");
                return false;
            }
            return true;
        }   

        private bool _DecodeControlName(string name, out IOType ioType,
        out int address, bool write) {
            ioType = IOType.Unknown;
            address = -1;

            var prefix =
                ChannelConstants.ValidControlNamePrefixes
                .FirstOrDefault((x) => name.ToUpper().StartsWith(x.ToUpper()));

            if (string.IsNullOrEmpty(prefix)) {

                SaveLasttError(nameof(_DecodeControlName), ClickErrorCode.InvalidControlNamePrefix);
                return false;
            }

            IOType tp = ChannelConstants.IoTypes[prefix];

            if (_StartAddresses.ContainsKey(tp)) {
                try {

                    address = _StartAddresses[tp];
                    int idx = Int32.Parse(name.Substring(name.IndexOf(prefix) + prefix.Length));
                    address += _CalculateAddressOffset(tp, idx);
                    ioType = tp;
                    return true;
                }
                catch (Exception ex) {

                    SaveLasttError(nameof(_DecodeControlName), ClickErrorCode.InvalidControlName, ex.Message);
                    return false;
                }
            }

            SaveLasttError(nameof(_DecodeControlName), ClickErrorCode.IoNotSupported, $"IO {tp} not supported.");
            return false;
        }

        private static int _CalculateAddressOffset(IOType type, int idx) {

            switch (type) {
                case IOType.RegisterFloat32:
                case IOType.RegisterInt32:
                    return Math.Max(0, idx - 1) * 2;

                default:
                    return Math.Max(0, idx - 1);
            }
        }

        private bool SaveLasttError(string methodName,
                                    ClickErrorCode code, string? details = null) {
            try {
                _lastError = new LogRecord(LogLevel.Error, methodName, details!, (int)code, DateTime.Now);
                return true;
            }
#if !DEBUG
            catch {
                return false;
            }
#else
            catch (Exception ex) {
                string msg = $"Failed to add error record to the error " +
                    $"history stack. Exception: {ex.Message}";
                Console.WriteLine(msg);
                return false;
            }
#endif
        }

        #endregion Private Methods
    }
}
