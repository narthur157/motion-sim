Public Interface IPlugin
    ReadOnly Property Name() As String
    ReadOnly Property ProcessName() As String
    ReadOnly Property PluginAuthorsName() As String
    ReadOnly Property Port() As String
    ReadOnly Property Is_MemoryMap() As Boolean
    ReadOnly Property Is_DashBoard() As Boolean
    ReadOnly Property Is_MemoryHook() As Boolean
    Sub Process_MemoryHook()
    Sub Process_MemoryMap()

    'run at start/stop
    Sub StartupCommands()
    Sub ShutDownCommands()

    'patching
    Function PatchGame(ByVal MyPath As String, ByVal MyIp As String) As Boolean
    Sub PatchPathInfo()
    Sub UnPatchGame(ByVal MyPath As String)
    Function ValidatePatchPath(ByVal MyPath As String) As Boolean

    'GameEngine - process info
    Sub Process_PacketRecieved(ByVal Text As String)

    'Game_Manager
    Function GetDOFsUsed() As String

    'output
    Function Get_RollOutput() As Double
    Function Get_PitchOutput() As Double
    Function Get_HeaveOutput() As Double
    Function Get_YawOutput() As Double
    Function Get_SwayOutput() As Double
    Function Get_SurgeOutput() As Double
    Function Get_Extra1Output() As Double
    Function Get_Extra2Output() As Double
    Function Get_Extra3Output() As Double
    Function Get_RollMemHook() As Double
    Function Get_PitchMemHook() As Double
    Function Get_HeaveMemHook() As Double
    Function Get_YawMemHook() As Double
    Function Get_SwayMemHook() As Double
    Function Get_SurgeMemHook() As Double
    Function Get_Extra1MemHook() As Double
    Function Get_Extra2MemHook() As Double
    Function Get_Extra3MemHook() As Double
    Function Get_RollMemMap() As Double
    Function Get_PitchMemMap() As Double
    Function Get_HeaveMemMap() As Double
    Function Get_YawMemMap() As Double
    Function Get_SwayMemMap() As Double
    Function Get_SurgeMemMap() As Double
    Function Get_Extra1MemMap() As Double
    Function Get_Extra2MemMap() As Double
    Function Get_Extra3MemMap() As Double
    Function Get_Dash1_Output() As String
    Function Get_Dash2_Output() As String
    Function Get_Dash3_Output() As String
    Function Get_Dash4_Output() As String
    Function Get_Dash5_Output() As String
    Function Get_Dash6_Output() As String
    Function Get_Dash7_Output() As String
    Function Get_Dash8_Output() As String
    Function Get_Dash9_Output() As String
    Function Get_Dash10_Output() As String
    Function Get_Dash11_Output() As String
    Function Get_Dash12_Output() As String
    Function Get_Dash13_Output() As String
    Function Get_Dash14_Output() As String
    Function Get_Dash15_Output() As String
    Function Get_Dash16_Output() As String
    Function Get_Dash17_Output() As String
    Function Get_Dash18_Output() As String
    Function Get_Dash19_Output() As String
    Function Get_Dash20_Output() As String

    'reset
    Sub ResetDOFVars()
    Sub ResetMapVars()
    Sub ResetHookVars()
    Sub ResetDashVars()
End Interface
