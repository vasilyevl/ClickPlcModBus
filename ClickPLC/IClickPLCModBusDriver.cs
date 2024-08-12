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


namespace Grumpy.ClickPLC.Net.Driver
{
    public interface IClickModBusDriver
    {
        /// <summary>
        /// Gets a value indicating whether the connection to the PLC is currently open.
        /// </summary>
        /// <value>
        /// <c>true</c> if the connection is open; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property checks the status of the connection to determine if it is active. 
        /// Use this property to verify if the PLC is ready for communication.
        /// </remarks>
        Boolean IsOpen { get; }

        /// <summary>
        /// Gets a value of the last error record made by the driver..
        /// </summary>
        /// <value>
        /// <c>ILogRecord</c> if record available; otherwise, <c>null</c>.
        /// </value>
        /// <remarks>
        /// Use this property to check if any additional information available in case of transaction failure. 
        /// This property returns the last error record made by the driver. It is not guaranteed 
        /// to provide info regarding the latest error if the code fails to update the record.
        /// </remarks>
        ILogRecord? LastRecord { get; }

        /// <summary>
        /// Opens a connection to the PLC.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the connection was successfully opened; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to establish a connection to the PLC using the configured network parameters 
        /// set using<see cref="Init"/>method or provided during driver instantiation. 
        /// Ensure that the network settings are correct before calling this method.
        /// If the connection is already open, this method will return <c>true</c> without attempting to reconnect.
        /// </remarks>
        Boolean Open();


        /// <summary>
        /// Closes a connection with the PLC.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the connection was successfully closed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to close current connection to the PLC. 
        /// Ensure that the network settings are correct before calling this method.
        /// If the connection is already Closed, this method will return <c>true</c> without attempting to disconnect.
        /// </remarks>
        Boolean Close();

        /// <summary>
        /// Initializes the connection settings for the PLC using a JSON configuration string.
        /// </summary>
        /// <param name="configJsonString">
        /// A JSON-formatted string containing the configuration parameters for the PLC connection.
        /// </param>
        /// <returns>
        /// <c>true</c> if the initialization was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method parses the provided JSON string to configure the connection parameters, such as IP address, port, and other necessary settings.
        /// The JSON string must be correctly formatted and include all required parameters.
        /// Call this method before attempting to open a connection using the <see cref="Open"/> method.
        /// </remarks>
        Boolean Init(String configJsonString);


        /// <summary>
        /// Reads the state of a discrete control from the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the discrete control to read. (String) For example, "X1" "Y1", "C10", "T1", "CT20".
        /// </param>
        /// <param name="state">
        /// When this method returns, contains the <see cref="SwitchState"/> of the specified control. 
        /// The parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the discrete control was successfully read; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves the current state of a discrete control identified by its name. 
        /// The state is returned as an output parameter of type <see cref="SwitchState"/>.
        /// The method returns <c>false</c> if the specified control could not be read, possibly due to a communication error or invalid control name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean ReadDiscreteControl(String name, out SwitchState state);



        /// <summary>
        /// Writes a value to a discrete control on the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the discrete control to write to. (String) For example,  "Y1", "C10".
        /// </param>
        /// <param name="sw">
        /// The <see cref="SwitchCtrl"/> value to set for the specified control.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value was successfully written to the discrete control; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method sets the state of a discrete control identified by its name to the specified <see cref="SwitchCtrl"/> value.
        /// The method returns <c>false</c> if the value could not be written, which may occur due to a communication error or an invalid control name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean WriteDiscreteControl(String name, SwitchCtrl sw);



        /// <summary>
        /// Reads the states of multiple consecutive discrete controls from the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the first discrete control to read. (String) For example, "X1" "Y1", "C10", "T1", "CT20".
        /// </param>
        /// <param name="numberOfIosToRead">
        /// The number of consecutive discrete controls to read starting from the specified control.
        /// </param>
        /// <param name="status">
        /// When this method returns, contains an array of <see cref="SwitchState"/> values representing the states of the read controls. 
        /// The array will have a length equal to <paramref name="numberOfIosToRead"/>. The parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the states of the discrete controls were successfully read; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method reads the states of a specified number of consecutive discrete controls, starting from the control identified by <paramref name="name"/>.
        /// The states are returned as an array of <see cref="SwitchState"/> values via the <paramref name="status"/> output parameter.
        /// The method returns <c>false</c> if the states could not be read, which may occur due to a communication error or an invalid control name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean ReadDiscreteControls(String name, int numberOfIosToRead, out SwitchState[] status);



        ///<summary>
        /// Writes values to multiple consecutive discrete controls on the PLC.
        /// </summary> 
        ///<param name="startName">
        /// The name of the first discrete control to write to. (String) For example,  "Y1", "C10".
        /// </param> 
        /// <param name="SwitchCtrl">
        /// An array of `SwitchCtrl` values representing the states to set for the consecutive discrete controls.
        /// </param>
        /// <returns>
        /// <c>true</c> if the values were successfully written to the discrete controls; otherwise, returns <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method sets the states of multiple consecutive discrete controls starting from the control identified by `startName` to the specified  <see cref="SwitchCtrl"/> values in the "controls" array.
        /// Before calling this method, ensure that the connection to the PLC is open by checking the `IsOpen` property of the `IClickModBusDriver` interface.
        /// </remarks>
        /// 
        Boolean WriteDiscreteControls(String startName, SwitchCtrl[] controls);

        /// <summary>
        /// Reads the value of a 16-bit signed integer register from the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the register to read. (String) For example,  "DS1", "DS123".
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value of the specified register. The parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the register value was successfully read; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves the current value of a 16-bit signed integer register identified by its name.
        /// The value is returned as an output parameter of type <see cref="short"/>.
        /// The method returns <c>false</c> if the specified register could not be read, possibly due to a communication error or an invalid register name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean ReadInt16Register(String name, out short value);


        /// <summary>
        /// Writes a value to a 16-bit signed integer register on the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the register to write to. (String) For example,  "DS1", "DS123".
        /// </param>
        /// <param name="value">
        /// The value to set for the specified register.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value was successfully written to the register; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method sets the value of a 16-bit signed integer register identified by its name to the specified value.
        /// The method returns <c>false</c> if the value could not be written, which may occur due to a communication error or an invalid register name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean WriteInt16Register(String name, short value);

        /// <summary>
        /// Reads the value of a 16-bit signed integer register from the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the register to read. (String) For example,  "DS1", "DS123".
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value of the specified register as 16-bit unsigned value. The parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the register value was successfully read; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves the current value of a 16-bit unsigned integer register identified by its name.
        /// The value is returned as an output parameter of type <see cref="ushort"/>.
        /// The method returns <c>false</c> if the specified register could not be read, possibly due to a communication error or an invalid register name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean ReadUInt16Register(String name, out ushort value);


        /// <summary>
        /// Writes a value to a 16-bit unsigned integer register on the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the register to write to. (String) For example,  "DS1", "DS123".
        /// </param>
        /// <param name="value">
        /// The value to set for the specified register.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value was successfully written to the register; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method sets the value of a 16-bit unsigned integer register identified by its name to the specified value.
        /// The method returns <c>false</c> if the value could not be written, which may occur due to a communication error or an invalid register name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean WriteUInt16Register(String name, ushort value);


        /// <summary>
        /// Reads the value of a 16-bit signed integer register from the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the register to read.(String) For example,  "DF1", "DF123".
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value of the specified register as 32-bit float value. The parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the register value was successfully read; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves the current value of a 32-bit float register identified by its name.
        /// The value is returned as an output parameter of type <see cref="float"/>.
        /// The method returns <c>false</c> if the specified register could not be read, possibly due to a communication error or an invalid register name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean ReadFloat32Register(String name, out float value);


        /// <summary>
        /// Writes a value to a 32-bit float register on the PLC.
        /// </summary>
        /// <param name="name">
        /// The name of the register to write to. (String) For example,  "DF1", "DF123".
        /// </param>
        /// <param name="value">
        /// The value to set for the specified register. For example,  1.0f, 3.14f.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value was successfully written to the register; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method sets the value of a 32-bit float register identified by its name to the specified value.
        /// The method returns <c>false</c> if the value could not be written, which may occur due to a communication error or an invalid register name.
        /// Ensure that the connection is open by checking the <see cref="IsOpen"/> property before calling this method.
        /// </remarks>
        Boolean WriteFloat32Register(String name, float value);

    }
}