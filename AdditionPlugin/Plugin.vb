Option Explicit On
Option Strict On
Imports PluginAPI
Imports System.Xml.Serialization
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Net.Sockets
Imports System.Net

Public Class Plugin
    Implements IPlugin
    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '///                            SimTools Plugin - Edit the Setting below to provide support for you favorite game!                              ///
    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '////////////////////////////////////////////////
    '/// Per Game Settings - Change for Each Game ///
    '////////////////////////////////////////////////
    Private Const _PluginAuthorsName As String = "narthur157"
    Private Const _GameName As String = "Unity" 'The displayed Name for this Game. (DLL_Name ==> "Live for Speed.dll")
    Private Const _ProcessName As String = "Unity" 'Process_Name without the (".exe") for this game. Multiple Names Possible - ("LFS,notepad")
    Private Const _Port As String = "4123" 'Your Sending/Recieving UDP Port for this game - DEFAULT PORT 4123
    '////////////////////////////////////////////////
    '///           Memory Map Variables           ///
    '////////////////////////////////////////////////
    Private Const _Is_MemoryMap As Boolean = False 'Is a MemoryMap file Required for this game?
    Private Const _MMF_Name As String = "NULL" 'Set if using a Memory Map File - EXAMPLE("$gtr2$")
    '////////////////////////////////////////////////
    '///           MemoryHook Variables           ///
    '////////////////////////////////////////////////   
    Private Const _Is_MemoryHook As Boolean = False 'Is a Memory Hook Required for this game? 
    Private Const _MemHook_Roll As UInteger = 0 'Not Used = 0
    Private Const _MemHook_Pitch As UInteger = 0
    Private Const _MemHook_Heave As UInteger = 0
    Private Const _MemHook_Yaw As UInteger = 0
    Private Const _MemHook_Sway As UInteger = 0
    Private Const _MemHook_Surge As UInteger = 0
    Private Const _MemHook_Extra1 As UInteger = 0
    Private Const _MemHook_Extra2 As UInteger = 0
    Private Const _MemHook_Extra3 As UInteger = 0
    '////////////////////////////////////////////////
    '///    DOFs Used for Output for this Game    ///
    '////////////////////////////////////////////////
    Private Const _DOF_Support_Roll As Boolean = False
    Private Const _DOF_Support_Pitch As Boolean = True
    Private Const _DOF_Support_Heave As Boolean = False
    Private Const _DOF_Support_Yaw As Boolean = False
    Private Const _DOF_Support_Sway As Boolean = False
    Private Const _DOF_Support_Surge As Boolean = False
    Private Const _DOF_Support_Extra1 As String = "" 'Blank = False
    Private Const _DOF_Support_Extra2 As String = "" '"" = Not Used
    Private Const _DOF_Support_Extra3 As String = "" 'ADD THE FORCE NAME HERE
    '/////////////////////////////////////////////////
    '///       GameDash - Dash Board Support       ///
    '/////////////////////////////////////////////////
    Private Const _Is_DashBoard As Boolean = True 'Enable the DashBoard Output System?
    'EXAMPLES OF DASHOUTPUT (all variables are strings) - 20 DASH OUTPUTS MAX!!!
    'Use the Variables with either Process_PacketRecieved, Process_MemoryHook or Process_MemoryMap.
    'Variable = (Action, Value) as String
    'Dash_1_Output = "Speed," & OUTPUT_VALUE_HERE.ToString
    'Dash_2_Output = "Rpm," & OUTPUT_VALUE_HERE.ToString
    '...
    'Dash_20_Output = "Gear," & OUTPUT_VALUE_HERE.ToString
    '////////////////////////////////////////////////
    'dash stuff
    Private IsUsingDash As Boolean = False
    'Used by GameManager when the Game Starts.
    Public Sub StartupCommands() Implements IPlugin.StartupCommands
        'only collect dash data if its used
        IsUsingDash = IsDashUsed()
        If IsUsingDash = True Then
            DashClient = New System.Net.Sockets.UdpClient(4124) 'port
            DashThreadReceive = New System.Threading.Thread(AddressOf ProcessDash)
            DashThreadReceive.IsBackground = True
            DashThreadReceive.Start()
        End If
    End Sub

    'Used by GameMnnager when the Game Stops.
    Public Sub ShutDownCommands() Implements IPlugin.ShutDownCommands
        If IsUsingDash = True Then
            On Error Resume Next
            IsUsingDash = False
            DashThreadReceive.Abort()
            DashClient.Close() 'may not work twice in a row if its closed??         
        End If
    End Sub

    'Used by GameManager to Process a MemoryHook.
    Public Sub Process_MemoryHook() Implements IPlugin.Process_MemoryHook
        'DO SOMETHING HERE AT GAME START!
    End Sub

    'Used by GameManager to Process a MemoryMap.
    Public Sub Process_MemoryMap() Implements IPlugin.Process_MemoryMap
        'DO SOMETHING HERE AT GAME START!
    End Sub

    'Used by GameEngine to Process Incoming UDP Packets.
    Private MyOutsim_Internal As New OutSim
    Private Structure OutSim
        ' x coordinate
        Public PosX As Single
        ''   time in milliseconds
        'Public uintTime As UInteger
        ''   Angular Velocity
        'Public sngAVelocity0 As Single
        'Public sngAVelocity1 As Single
        'Public sngAVelocity2 As Single
        ''   Orientation
        'Public sngOrientation0 As Single
        'Public sngOrientation1 As Single
        'Public sngOrientation2 As Single
        ''   Acceleration 
        'Public sngAcceleration0 As Single
        'Public sngAcceleration1 As Single
        'Public sngAcceleration2 As Single
        ''   Velocity
        'Public sngVelocity0 As Single
        'Public sngVelocity1 As Single
        'Public sngVelocity2 As Single
        ''   Position
        'Public PosX As Single
        'Public PosY As Single
        'Public PosZ As Single
        ''   Game ID - if specified in cfg.txt
        'Public lngGameID As Long
    End Structure

    Private MyOutGaudge_Internal As New OutGaugePack
    Private Structure OutGaugePack
        Public uintTime As UInteger '  time in milliseconds (to check order)
        <MarshalAs(UnmanagedType.ByValTStr, sizeconst:=4)> _
        Public Car As String '         Car name
        Public Flags As UInt16 '       Info (see DL_x below)
        Public Gear As Byte '          Reverse:0, Neutral:1, First:2...
        Public PLID As Byte '          Unique ID of viewed player (0 = none)
        Public Speed As Single '       m/s
        Public RPM As Single '         RPM
        Public Turbo As Single '       bar
        Public EngTemp As Single '     °C
        Public Fuel As Single '        0 to 1
        Public OilPressure As Single ' BAR
        Public OilTemp As Single '     °C
        Public DashLights As UInt32 '  Dash lights available (see DL_x below)
        Public ShowLights As UInt32 '  Dash lights currently switched on
        Public Throttle As Single '    0 to 1
        Public Brake As Single '       0 to 1
        Public Clutch As Single '      0 to 1
        <MarshalAs(UnmanagedType.ByValTStr, sizeconst:=16)> _
        Public Display1 As String '    Usually Fuel
        <MarshalAs(UnmanagedType.ByValTStr, sizeconst:=16)> _
        Public Display2 As String '    Usually Settings
        Public ID As Int32 '           optional - only if OutGauge ID is specified
    End Structure

    Enum DL_x
        DL_SHIFT ' 			bit 0	- shift light
        DL_FULLBEAM '		bit 1	- full beam
        DL_HANDBRAKE '		bit 2	- handbrake
        DL_PITSPEED ' 		bit 3	- pit speed limiter
        DL_TC '				bit 4	- TC active or switched off
        DL_SIGNAL_L '		bit 5	- left turn signal
        DL_SIGNAL_R '		bit 6	- right turn signal
        DL_SIGNAL_ANY '		bit 7	- shared turn signal
        DL_OILWARN '		bit 8	- oil pressure warning
        DL_BATTERY '		bit 9	- battery warning
        DL_ABS '			bit 10	- ABS active or switched off
        DL_SPARE '			bit 11  - spare
        DL_NUM '            bit 12  - ?
    End Enum

    Sub Process_PacketRecieved(Text As String) Implements IPlugin.Process_PacketRecieved
        Try
            'Convert string to byte and copy to byte array
            Dim ByteArray() As Byte = System.Text.Encoding.Default.GetBytes(Text)
            'Create Gchandle instance and pin variable required
            Dim MyGC As System.Runtime.InteropServices.GCHandle = System.Runtime.InteropServices.GCHandle.Alloc(MyOutsim_Internal, System.Runtime.InteropServices.GCHandleType.Pinned)
            'get address of variable in pointer variable
            Dim AddofLongValue As IntPtr = MyGC.AddrOfPinnedObject()
            'Copy the memory space to my GCHandle
            System.Runtime.InteropServices.Marshal.Copy(ByteArray, 0, AddofLongValue, ByteArray.Length)
            'Direct Cast myGC to my Outsim Object
            MyOutsim_Internal = DirectCast(MyGC.Target, OutSim)
            'Free GChandle to avoid memory leaks
            MyGC.Free()
            Console.Out.WriteLine("received x position " + MyOutsim_Internal.PosX.ToString())
            'Get Proper Data out of UDP Packet
            With MyOutsim_Internal
                Roll_Output = 0
                Pitch_Output = .PosX
                Heave_Output = 0
                Yaw_Output = 0
                Sway_Output = 0
                Surge_Output = 0
                Extra1_Output = 0
            End With
        Catch ex As Exception
        End Try
    End Sub

    'Used by GameManager to Patch a Game.
    Function PatchGame(ByVal MyPath As String, ByVal MyIp As String) As Boolean Implements IPlugin.PatchGame
        'Change as Needed
        'If game is already patched - Unpatch first
        UnPatch(MyPath)
        Try
            MsgBox("Patch Installed!", MsgBoxStyle.OkOnly, "Patching info")
            Return True
        Catch ex As Exception
            MsgBox("You Must first Install and Run the Game Once Prior to Patching" + vbCrLf + "Game Not Patched!", MsgBoxStyle.OkOnly, "Patching info")
            Return False
        End Try
    End Function

    'Used by GameManager to UnPatch a Game.
    Sub UnPatchGame(MyPath As String) Implements IPlugin.UnPatchGame
        'Change as Needed
        UnPatch(MyPath)
        MsgBox("Patch Uninstalled!", MsgBoxStyle.OkOnly, "Patching info")
    End Sub

    Private Sub UnPatch(MyPath As String)
        If System.IO.File.Exists(MyPath & "cfg.BAK") Then 'Restore backup File
            If System.IO.File.Exists(MyPath & "cfg.txt") Then
                System.IO.File.Delete(MyPath & "cfg.txt")
            End If
            Rename(MyPath & "cfg.BAK", MyPath & "cfg.txt")
        End If
    End Sub

    Sub PatchPathInfo() Implements IPlugin.PatchPathInfo
        MsgBox("Please Select the Live for Speed's Installation Directory.", MsgBoxStyle.OkOnly, "Patching info")
    End Sub

    'Used by GameManager to Validate a Path befors Patching.
    Function ValidatePatchPath(MyPath As String) As Boolean Implements IPlugin.ValidatePatchPath
        'insert a simple validation of the patching path - let the user know he got it right
        If System.IO.File.Exists(MyPath & "\Unity.exe") = True Then
            Return True
        Else
            Return False
        End If
    End Function


    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '///                                                        DO NOT EDIT BELOW HERE!!!                                                           ///
    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#Region "///// BUILT IN METHODS - DO NOT CHANGE /////"
    'For MemoryHook
    Public Declare Function ReadProcessMemory Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByVal lpBuffer() As Byte, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Public Declare Function CloseHandle Lib "kernel32" Alias "CloseHandle" (ByVal hObject As Integer) As Integer
    Public Declare Function OpenProcess Lib "kernel32" Alias "OpenProcess" (ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Integer, ByVal dwProcessId As Integer) As Integer

    'Output Dash Vars
    Public Dash_1_Output As String = ""
    Public Dash_2_Output As String = ""
    Public Dash_3_Output As String = ""
    Public Dash_4_Output As String = ""
    Public Dash_5_Output As String = ""
    Public Dash_6_Output As String = ""
    Public Dash_7_Output As String = ""
    Public Dash_8_Output As String = ""
    Public Dash_9_Output As String = ""
    Public Dash_10_Output As String = ""
    Public Dash_11_Output As String = ""
    Public Dash_12_Output As String = ""
    Public Dash_13_Output As String = ""
    Public Dash_14_Output As String = ""
    Public Dash_15_Output As String = ""
    Public Dash_16_Output As String = ""
    Public Dash_17_Output As String = ""
    Public Dash_18_Output As String = ""
    Public Dash_19_Output As String = ""
    Public Dash_20_Output As String = ""

    'Output Vars
    Public Roll_Output As Double = 0
    Public Pitch_Output As Double = 0
    Public Heave_Output As Double = 0
    Public Yaw_Output As Double = 0
    Public Sway_Output As Double = 0
    Public Surge_Output As Double = 0
    Public Extra1_Output As Double = 0
    Public Extra2_Output As Double = 0
    Public Extra3_Output As Double = 0

    'MemHook Vars
    Public Roll_MemHook As Double = 0
    Public Pitch_MemHook As Double = 0
    Public Heave_MemHook As Double = 0
    Public Yaw_MemHook As Double = 0
    Public Sway_MemHook As Double = 0
    Public Surge_MemHook As Double = 0
    Public Extra1_MemHook As Double = 0
    Public Extra2_MemHook As Double = 0
    Public Extra3_MemHook As Double = 0

    'MemMap Vars
    Public Roll_MemMap As Double = 0
    Public Pitch_MemMap As Double = 0
    Public Heave_MemMap As Double = 0
    Public Yaw_MemMap As Double = 0
    Public Sway_MemMap As Double = 0
    Public Surge_MemMap As Double = 0
    Public Extra1_MemMap As Double = 0
    Public Extra2_MemMap As Double = 0
    Public Extra3_MemMap As Double = 0

    Public Function GetDOFsUsed() As String Implements IPlugin.GetDOFsUsed
        'Return DOF's Used (Roll,Pitch,Heave,Yaw,Sway,Surge)
        Return (_DOF_Support_Roll.ToString & "," & _DOF_Support_Pitch.ToString & "," & _DOF_Support_Heave.ToString & "," & _DOF_Support_Yaw.ToString & "," & _DOF_Support_Sway.ToString & "," & _DOF_Support_Surge.ToString & "," & _DOF_Support_Extra1.ToString & "," & _DOF_Support_Extra2.ToString & "," & _DOF_Support_Extra3.ToString)
    End Function

    Public Sub ResetDOFVars() Implements IPlugin.ResetDOFVars
        Roll_Output = 0
        Pitch_Output = 0
        Heave_Output = 0
        Yaw_Output = 0
        Sway_Output = 0
        Surge_Output = 0
        Extra1_Output = 0
        Extra2_Output = 0
        Extra3_Output = 0
    End Sub

    Public Sub ResetMapVars() Implements IPlugin.ResetMapVars
        Roll_MemMap = 0
        Pitch_MemMap = 0
        Heave_MemMap = 0
        Yaw_MemMap = 0
        Sway_MemMap = 0
        Surge_MemMap = 0
        Extra1_MemMap = 0
        Extra2_MemMap = 0
        Extra3_MemMap = 0
    End Sub

    Public Sub ResetHookVars() Implements IPlugin.ResetHookVars
        Roll_MemHook = 0
        Pitch_MemHook = 0
        Heave_MemHook = 0
        Yaw_MemHook = 0
        Sway_MemHook = 0
        Surge_MemHook = 0
        Extra1_MemHook = 0
        Extra2_MemHook = 0
        Extra3_MemHook = 0
    End Sub

    Public Sub ResetDashVars() Implements IPlugin.ResetDashVars
        Dash_1_Output = ""
        Dash_2_Output = ""
        Dash_3_Output = ""
        Dash_4_Output = ""
        Dash_5_Output = ""
        Dash_6_Output = ""
        Dash_7_Output = ""
        Dash_8_Output = ""
        Dash_9_Output = ""
        Dash_10_Output = ""
        Dash_11_Output = ""
        Dash_12_Output = ""
        Dash_13_Output = ""
        Dash_14_Output = ""
        Dash_15_Output = ""
        Dash_16_Output = ""
        Dash_17_Output = ""
        Dash_18_Output = ""
        Dash_19_Output = ""
        Dash_20_Output = ""
    End Sub

    Public ReadOnly Property PluginAuthorsName() As String Implements IPlugin.PluginAuthorsName
        Get
            Return _PluginAuthorsName
        End Get
    End Property

    Public ReadOnly Property Name() As String Implements IPlugin.Name
        Get
            Return _GameName
        End Get
    End Property

    Public ReadOnly Property ProcessName() As String Implements IPlugin.ProcessName
        Get
            Return _ProcessName
        End Get
    End Property

    Public ReadOnly Property Port() As String Implements IPlugin.Port
        Get
            Return _Port
        End Get
    End Property

    Public ReadOnly Property Is_MemoryMap() As Boolean Implements IPlugin.Is_MemoryMap
        Get
            Return _Is_MemoryMap
        End Get
    End Property

    Public ReadOnly Property Is_MemoryHook() As Boolean Implements IPlugin.Is_MemoryHook
        Get
            Return _Is_MemoryHook
        End Get
    End Property

    Public ReadOnly Property Is_DashBoard() As Boolean Implements IPlugin.Is_DashBoard
        Get
            Return _Is_DashBoard
        End Get
    End Property

    Public Function Get_RollOutput() As Double Implements IPlugin.Get_RollOutput
        Return Roll_Output
    End Function

    Public Function Get_Dash_1_Output() As String Implements IPlugin.Get_Dash1_Output
        Return Dash_1_Output
    End Function


    Public Function Get_Dash_2_Output() As String Implements IPlugin.Get_Dash2_Output
        Return Dash_2_Output
    End Function


    Public Function Get_Dash_3_Output() As String Implements IPlugin.Get_Dash3_Output
        Return Dash_3_Output
    End Function


    Public Function Get_Dash_4_Output() As String Implements IPlugin.Get_Dash4_Output
        Return Dash_4_Output
    End Function


    Public Function Get_Dash_5_Output() As String Implements IPlugin.Get_Dash5_Output
        Return Dash_5_Output
    End Function


    Public Function Get_Dash_6_Output() As String Implements IPlugin.Get_Dash6_Output
        Return Dash_6_Output
    End Function


    Public Function Get_Dash_7_Output() As String Implements IPlugin.Get_Dash7_Output
        Return Dash_7_Output
    End Function


    Public Function Get_Dash_8_Output() As String Implements IPlugin.Get_Dash8_Output
        Return Dash_8_Output
    End Function


    Public Function Get_Dash_9_Output() As String Implements IPlugin.Get_Dash9_Output
        Return Dash_9_Output
    End Function


    Public Function Get_Dash_10_Output() As String Implements IPlugin.Get_Dash10_Output
        Return Dash_10_Output
    End Function


    Public Function Get_Dash_11_Output() As String Implements IPlugin.Get_Dash11_Output
        Return Dash_11_Output
    End Function


    Public Function Get_Dash_12_Output() As String Implements IPlugin.Get_Dash12_Output
        Return Dash_12_Output
    End Function


    Public Function Get_Dash_13_Output() As String Implements IPlugin.Get_Dash13_Output
        Return Dash_13_Output
    End Function


    Public Function Get_Dash_14_Output() As String Implements IPlugin.Get_Dash14_Output
        Return Dash_14_Output
    End Function


    Public Function Get_Dash_15_Output() As String Implements IPlugin.Get_Dash15_Output
        Return Dash_15_Output
    End Function


    Public Function Get_Dash_16_Output() As String Implements IPlugin.Get_Dash16_Output
        Return Dash_16_Output
    End Function


    Public Function Get_Dash_17_Output() As String Implements IPlugin.Get_Dash17_Output
        Return Dash_17_Output
    End Function


    Public Function Get_Dash_18_Output() As String Implements IPlugin.Get_Dash18_Output
        Return Dash_18_Output
    End Function


    Public Function Get_Dash_19_Output() As String Implements IPlugin.Get_Dash19_Output
        Return Dash_19_Output
    End Function


    Public Function Get_Dash_20_Output() As String Implements IPlugin.Get_Dash20_Output
        Return Dash_20_Output
    End Function


    Public Function Get_PitchOutput() As Double Implements IPlugin.Get_PitchOutput
        Return Pitch_Output
    End Function

    Public Function Get_HeaveOutput() As Double Implements IPlugin.Get_HeaveOutput
        Return Heave_Output
    End Function

    Public Function Get_YawOutput() As Double Implements IPlugin.Get_YawOutput
        Return Yaw_Output
    End Function

    Public Function Get_SwayOutput() As Double Implements IPlugin.Get_SwayOutput
        Return Sway_Output
    End Function

    Public Function Get_SurgeOutput() As Double Implements IPlugin.Get_SurgeOutput
        Return Surge_Output
    End Function

    Public Function Get_Extra1Output() As Double Implements IPlugin.Get_Extra1Output
        Return Extra1_Output
    End Function

    Public Function Get_Extra2Output() As Double Implements IPlugin.Get_Extra2Output
        Return Extra2_Output
    End Function

    Public Function Get_Extra3Output() As Double Implements IPlugin.Get_Extra3Output
        Return Extra3_Output
    End Function

    Public Function Get_RollMemHook() As Double Implements IPlugin.Get_RollMemHook
        Return Roll_MemHook
    End Function

    Public Function Get_PitchMemHook() As Double Implements IPlugin.Get_PitchMemHook
        Return Pitch_MemHook
    End Function

    Public Function Get_HeaveMemHook() As Double Implements IPlugin.Get_HeaveMemHook
        Return Heave_MemHook
    End Function

    Public Function Get_YawMemHook() As Double Implements IPlugin.Get_YawMemHook
        Return Yaw_MemHook
    End Function

    Public Function Get_SwayMemHook() As Double Implements IPlugin.Get_SwayMemHook
        Return Sway_MemHook
    End Function

    Public Function Get_SurgeMemHook() As Double Implements IPlugin.Get_SurgeMemHook
        Return Surge_MemHook
    End Function

    Public Function Get_Extra1MemHook() As Double Implements IPlugin.Get_Extra1MemHook
        Return Extra1_MemHook
    End Function

    Public Function Get_Extra2MemHook() As Double Implements IPlugin.Get_Extra2MemHook
        Return Extra2_MemHook
    End Function

    Public Function Get_Extra3MemHook() As Double Implements IPlugin.Get_Extra3MemHook
        Return Extra3_MemHook
    End Function

    Public Function Get_RollMemMap() As Double Implements IPlugin.Get_RollMemMap
        Return Roll_MemMap
    End Function

    Public Function Get_PitchMemMap() As Double Implements IPlugin.Get_PitchMemMap
        Return Pitch_MemMap
    End Function

    Public Function Get_HeaveMemMap() As Double Implements IPlugin.Get_HeaveMemMap
        Return Heave_MemMap
    End Function

    Public Function Get_YawMemMap() As Double Implements IPlugin.Get_YawMemMap
        Return Yaw_MemMap
    End Function

    Public Function Get_SwayMemMap() As Double Implements IPlugin.Get_SwayMemMap
        Return Sway_MemMap
    End Function

    Public Function Get_SurgeMemMap() As Double Implements IPlugin.Get_SurgeMemMap
        Return Surge_MemMap
    End Function

    Public Function Get_Extra1MemMap() As Double Implements IPlugin.Get_Extra1MemMap
        Return Extra1_MemMap
    End Function

    Public Function Get_Extra2MemMap() As Double Implements IPlugin.Get_Extra2MemMap
        Return Extra2_MemMap
    End Function

    Public Function Get_Extra3MemMap() As Double Implements IPlugin.Get_Extra3MemMap
        Return Extra3_MemMap
    End Function

    'File Editor - Replaces a line in a txt/cfg file
    Private Sub MyFileEditor(FileName As String, LinetoEdit As String, StingReplacment As String)
        Dim lines As New List(Of String)
        Dim x As Integer = 0
        Using sr As New System.IO.StreamReader(FileName)
            While Not sr.EndOfStream
                lines.Add(sr.ReadLine)
            End While
        End Using

        For Each line As String In lines
            If line.Contains(LinetoEdit) Then
                lines.RemoveAt(x)
                lines.Insert(x, StingReplacment)
                Exit For 'must exit as we changed the iteration   
            End If
            x = x + 1
        Next

        Using sw As New System.IO.StreamWriter(FileName)
            For Each line As String In lines
                sw.WriteLine(line)
            Next
        End Using
        lines.Clear()
    End Sub

    'DeSerialize
    Public Function DeSerializeString(InputString As String) As Object
        On Error Resume Next
        Dim xml_serializer As New XmlSerializer(GetType(GenericOutput))
        Dim string_reader As New StringReader(InputString)
        Dim DeserializedOutput As GenericOutput = DirectCast(xml_serializer.Deserialize(string_reader), GenericOutput)
        string_reader.Close()
        Return DeserializedOutput
    End Function

    'Generic In/Output structure
    Public Structure GenericOutput
        Public _Roll As Double
        Public _Pitch As Double
        Public _Heave As Double
        Public _Yaw As Double
        Public _Sway As Double
        Public _Surge As Double
        Public _Extra1 As Double
        Public _Extra2 As Double
        Public _Extra3 As Double
    End Structure

    'memory hook function #1
    Public Function ReadSingle(hProcess As String, dwAddress As UInteger) As String
        Try
            Dim proc As Process = Process.GetProcessesByName(hProcess)(0)
            Dim winhandle As Integer = CInt(CType(OpenProcess(&H1F0FFF, 1, proc.Id), IntPtr))
            Dim buffer As Byte() = New Byte(3) {}
            buffer.ToArray()
            ReadProcessMemory(winhandle, CInt(dwAddress), buffer, 4, 0)
            Dim MySingle As Single = BitConverter.ToSingle(buffer, 0)
            Return MySingle.ToString
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    'memory hook function #2
    Public Function ReadInt32(hProcess As String, dwAddress As UInteger) As String
        Try
            Dim proc As Process = Process.GetProcessesByName(hProcess)(0)
            Dim winhandle As Integer = CInt(CType(OpenProcess(&H1F0FFF, 1, proc.Id), IntPtr))
            Dim buffer As Byte() = New Byte(3) {}
            buffer.ToArray()
            ReadProcessMemory(winhandle, CInt(dwAddress), buffer, 4, 0)
            Dim MyInt32 As Integer = BitConverter.ToInt32(buffer, 0)
            Return MyInt32.ToString
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
#End Region

    'Is the DashOutput Being Used? - returns True or false
    Private Function IsDashUsed() As Booleanf
        Dim SavePath As String = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) & "\SimTools\GameManager\UseDash.cfg"
        Return System.IO.File.Exists(SavePath)
    End Function

    Private DashClient As UdpClient
    Public DashRemoteIpEndPoint As New IPEndPoint(System.Net.IPAddress.Any, 4124) 'any ip we dont care
    Public DashThreadReceive As System.Threading.Thread
    Public Sub ProcessDash()
        Try
        Dim OutGaugeData As New OutGaugePack
            Dim ptr As New GCHandle
            Dim OutGaugeBytes() As Byte = DashClient.Receive(DashRemoteIpEndPoint)
            Dim OutGaugeSize As Integer = Marshal.SizeOf(OutGaugeData)
            'Dim CodemasterSize As Integer = 68
            ptr = GCHandle.Alloc(OutGaugeBytes, GCHandleType.Pinned)
            MyOutGaudge_Internal = CType(Marshal.PtrToStructure(ptr.AddrOfPinnedObject, GetType(OutGaugePack)), OutGaugePack)
            ptr.Free() '<< make sure you free the gc or you get memory problems

            'Get Proper Data out of UDP Packet
            With MyOutGaudge_Internal
                Dash_1_Output = "Speed [m/s]," & Int(.Speed + 0.5)
                Dash_2_Output = "RPM," & Int(.RPM + 0.5)
                Dash_3_Output = "Gear (-1 = Reverse)," & .Gear - 1
                Dash_4_Output = "EngineWaterTemp [°C]," & .EngTemp
                Dash_5_Output = "EngineOilTemp [°C]," & .OilTemp
                Dash_6_Output = "Fuel [%]," & .Fuel
                Dash_7_Output = "Engine Overheating," & (.Flags And CInt(2 ^ DL_x.DL_OILWARN))
                Dash_8_Output = "Turbo [bar]," & .Turbo
                Dash_9_Output = "Headlights," & (.Flags And CInt(2 ^ DL_x.DL_FULLBEAM))
                Dash_10_Output = "ShiftUp," & (.Flags And CInt(2 ^ DL_x.DL_SHIFT))  ' shift up light
                Dash_11_Output = "Oil Pressure [bar]," & .OilPressure
                'Dash_12_Output = " xxx," & 
                Dash_13_Output = "InPit," & (.Flags And CInt(2 ^ DL_x.DL_PITSPEED))
                Dash_14_Output = "Left turn signal," & (.Flags And CInt(2 ^ DL_x.DL_SIGNAL_L))
                Dash_15_Output = "Right turn signal," & (.Flags And CInt(2 ^ DL_x.DL_SIGNAL_R))
                Dash_16_Output = "Shared turn signal," & (.Flags And CInt(2 ^ DL_x.DL_SIGNAL_ANY))
                Dash_17_Output = "Battery warning," & (.Flags And CInt(2 ^ DL_x.DL_BATTERY))
                Dash_18_Output = "ABS," & (.Flags And CInt(2 ^ DL_x.DL_ABS))
                Dash_19_Output = "Handbrake," & (.Flags And CInt(2 ^ DL_x.DL_HANDBRAKE))
            End With
        Catch ex As Exception
        End Try

        'Startup a new thread listener
        If IsUsingDash = True Then
            DashThreadReceive = New System.Threading.Thread(AddressOf ProcessDash)
            DashThreadReceive.IsBackground = True
            DashThreadReceive.Start()
        End If
    End Sub
End Class
