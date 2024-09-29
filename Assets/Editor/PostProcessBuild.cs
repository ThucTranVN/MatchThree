using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Assertions;

public class WindowCleanupPostProcessorBuild
{
    [PostProcessBuild(2000)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
#if UNITY_EDITOR
        string outputPath = path;
        try
        {
            string applicationName = Path.GetFileNameWithoutExtension(outputPath);
            string outputFolder = Path.GetDirectoryName(outputPath);
            Assert.IsNotNull(outputFolder);

            outputFolder = Path.GetFullPath(outputFolder);

            //Delete Burst Debug Folder
            string burstDebugInformationDirectoryPath = Path.Combine(outputFolder, $"{applicationName}_BurstDebugInformation_DoNotShip");

            if (Directory.Exists(burstDebugInformationDirectoryPath))
            {
                Debug.Log($" > Deleting Burst debug information folder at path '{burstDebugInformationDirectoryPath}'...");

                Directory.Delete(burstDebugInformationDirectoryPath, true);
            }

            //Delete il2cpp Debug Folder
            string il2cppDebugInformationDirectoryPath = Path.Combine(outputFolder, $"{applicationName}_BackUpThisFolder_ButDontShipItWithYourGame");

            if (Directory.Exists(il2cppDebugInformationDirectoryPath))
            {
                Debug.Log($" > Deleting BackUp debug information folder at path '{il2cppDebugInformationDirectoryPath}'...");

                Directory.Delete(il2cppDebugInformationDirectoryPath, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"An unexpected exception occurred while performing build cleanup: {e}");
        }
#endif
    }

}