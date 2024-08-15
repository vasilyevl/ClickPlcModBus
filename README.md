# ClickPlc.NET

Overview:

This C# Dynamic Link Library (DLL) is specifically developed for communication with "Automation Direct" CLICK PLCs using the MODBUS TCP protocol. The code has undergone testing with the  CLICK PLUS PLC configuration described further in the text and is intended exclusively for use in ModBus over TCP mode.

A supplied test program demonstrates basic communication examples. It is important to note that functionalities beyond these test cases have not been validated. Users should adjust network parameters to match their specific network configurations.

The test PLC code is located in the PLC_CLICK_PLUS subfolder.

Implementation Details:

This DLL employs PLC relay and register names instead of direct ModBus addresses to dynamically generate addresses. For instance, to check the state of a timer, the following method is used:

csharp
Copy code
bool result = _driver.ReadDiscreteControl("T1", out switchState state);
In this context, switchState is an enumerator that can hold the values "On," "Off," or "Unknown." The method returns true if the read operation is successful, accompanied by the actual timer state. Otherwise, it returns false and an "Unknown" state. In the event of an error, detailed information can be accessed via the LastRecord property.

For comprehensive API documentation, please refer to the IClickPLCModBusDriver.cs file.

This particular solution is configured for .NET8.

Test Configuration:
PLC:
CPU: C2-01CPU-2
Plug-in Slot #1: C2-14DR
Plug-in Slot #2: C2-08D1-VC

I/O Connections Established for Testing:

C2-14DR:
Output #1 to Input #1
Output #2 to Input #2
Output #3 to Input #3
Output #4 to Input #4
Output #5 to Input #5
Output #6 to Input #6
Commons (C1, C2, C3, C4): Connected to PLC power 0V

C2-08D1-VC:
DA1V connected to AD1V
DA2V connected to AD2V

The PLC program used for testing is located in the ClickPlcModBus\PLC_CLICK_PLUS folder.

General Program Flow:

Create Communication Driver:
    Utilize the static method:
        ClickDriver.CreateDriver(configuration, out IClickModBusDriver _driver);
    Alternatively:
        ClickDriver.CreateDriver(out IClickModBusDriver driver);
        driver.Init(configuration);

    The configuration parameter should be a JSON string representing the network configuration. An example is provided within the code.

Open Connection:
    Establish a connection with the PLC using deriver.Open() method. 

Do Read/Write Operations:
    Commence read and write operations on the PLC controls.

Close connection:
    Close connection using deriver.Close() method. 