using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Input;

namespace AvaloniaApplication1;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Chooses Input or output folder
    private async void OnSelectFolderClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Name == "InputFolderButton")
            {
                await SelectFolderAndSetPath(InputFolder, "input");
            }
            else if (button.Name == "OutputFolderButton")
            {
                await SelectFolderAndSetPath(OutputFolder, "output");
            }
        }
    }

    // Set Folder paths
    private async Task SelectFolderAndSetPath(TextBlock folderPathText, string folderType)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel == null)
        {
            folderPathText.Text = "Unable to get TopLevel";
            return;
        }

        var selectedFolder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = $"Select {folderType} folder",
        });

        if (selectedFolder.Count >= 1)
        {
            var folderPath = selectedFolder[0].Path.LocalPath;
            folderPathText.Text = folderPath;
        }
        else
        {
            folderPathText.Text = $"No {folderType} folder selected";
        }
    }

    // Verify files in folders
    private async void StartVerification_OnClick(object? sender, RoutedEventArgs e)
    {
        //Get Path
        var inputFolderPath = InputFolder.Text;
        var outputFolderPath = OutputFolder.Text;

        if (string.IsNullOrWhiteSpace(inputFolderPath) || string.IsNullOrWhiteSpace(outputFolderPath))
        {
            Console.WriteLine("Input or output folder is not selected.");
            return;
        }
        
        // Get all files in folder and add to List
        try
        {
            var inputFiles = await GetFilesInFolder(inputFolderPath);
            var outputFiles = await GetFilesInFolder(outputFolderPath);

            if (inputFiles.Count == 0 || outputFiles.Count == 0)
            {
                Console.WriteLine("One or both folders are empty.");
                return;
            }
            // Verify if the file name is in both folders.
            await VerifyFiles(inputFiles, outputFiles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during verification: {ex.Message}");
        }
    }

    
    private async Task<List<IStorageFile>> GetFilesInFolder(string folderPath)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel == null)
        {
            throw new Exception("Unable to get TopLevel for retrieving files.");
        }
        // Create Uri filepath
        var folderUri = new Uri(folderPath, UriKind.Absolute);
        var folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(folderUri);

        if (folder == null)
        {
            throw new Exception($"Folder not found: {folderPath}");
        }

        var files = new List<IStorageFile>();

       
        await foreach (var item in folder.GetItemsAsync())
        {
            if (item is IStorageFile file)
            {
                files.Add(file); 
            }
        }

        return files; 
    }

    // Chech if the file is in both folders. 
    private async Task VerifyFiles(List<IStorageFile> inputFiles, List<IStorageFile> outputFiles)
    {
        foreach (var inputFile in inputFiles)
        {
            Console.WriteLine($"Verifying input file: {inputFile.Name}");

            var matchingOutputFile = outputFiles.FirstOrDefault(f => f.Name == inputFile.Name);
            if (matchingOutputFile != null)
            {
                Console.WriteLine($"Match found for {inputFile.Name} in output folder.");
            }
            else
            {
                Console.WriteLine($"No match found for {inputFile.Name} in output folder.");
            }
        }
    }
}
