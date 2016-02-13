Imports System.IO
Imports System.Reflection
Imports PluginAPI

Module PluginConsole

    Sub Main()
        Dim availablePlugins As String()
        availablePlugins = GetAvailablePlugins()
        Console.ReadLine()
    End Sub

    Private Function GetAvailablePlugins() As String()
        Dim appDir As String = Directory.GetCurrentDirectory()
        appDir = Replace(appDir, "PluginConsole\bin\Debug", "")
        appDir = appDir & "AdditionPlugin\bin\Debug"
        Dim availablePlugins As String() = Directory.GetFiles(appDir, "*Plugin.DLL")
        If availablePlugins.Count = 0 Then
            Console.WriteLine("No plugins found.")
            Return Nothing
        Else
            Console.WriteLine("Possible plugins found, now checking validity...")
        End If
        For Each possiblePlugin As String In availablePlugins
            Dim asm As Assembly = Assembly.LoadFrom(possiblePlugin)
            Dim myType As System.Type = asm.GetType(asm.GetName.Name & ".Plugin")
            Dim implementsIPlugin As Boolean = GetType(IPlugin).IsAssignableFrom(myType)
            If implementsIPlugin Then
                Console.WriteLine(asm.GetName.Name + " is a valid plugin!")
                Dim plugin As IPlugin = CType(Activator.CreateInstance(myType), IPlugin)
                Console.WriteLine(plugin.Name)
            End If
        Next
        Return Nothing
    End Function
End Module
