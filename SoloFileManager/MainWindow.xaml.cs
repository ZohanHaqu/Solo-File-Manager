using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SoloFileManager
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileItem> Files { get; set; } = new ObservableCollection<FileItem>();
        private string currentPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private Stack<string> history = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadDrives();
            LoadFiles(currentPath);
            FileList.ItemsSource = Files;
            AddressBar.Text = currentPath;
        }

        // Load all drives into the TreeView
        private void LoadDrives()
        {
            FolderTree.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                TreeViewItem driveItem = new TreeViewItem
                {
                    Header = drive.Name,
                    Tag = drive.Name
                };
                driveItem.Items.Add(null);
                driveItem.Expanded += Folder_Expanded;
                FolderTree.Items.Add(driveItem);
            }
        }

        // Load files and folders in the ListView
        private void LoadFiles(string path)
        {
            Files.Clear();
            currentPath = path;
            AddressBar.Text = path;

            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    Files.Add(new FileItem
                    {
                        Name = System.IO.Path.GetFileName(dir),
                        Type = "Folder",
                        Path = dir,
                        Icon = "folder.png"
                    });
                }

                foreach (var file in Directory.GetFiles(path))
                {
                    Files.Add(new FileItem
                    {
                        Name = System.IO.Path.GetFileName(file),
                        Type = System.IO.Path.GetExtension(file),
                        Path = file,
                        Icon = "file.png"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}");
            }
        }

        // Navigate to folder on double-click
        private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileList.SelectedItem is FileItem item)
            {
                if (item.Type == "Folder")
                {
                    history.Push(currentPath);
                    LoadFiles(item.Path);
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(item.Path) { UseShellExecute = true });
                }
            }
        }

        // Handle "Go" button click
        private void Go_Click(object sender, RoutedEventArgs e)
        {
            string path = AddressBar.Text;
            if (Directory.Exists(path))
            {
                history.Push(currentPath);
                LoadFiles(path);
            }
            else
            {
                MessageBox.Show("Invalid directory.");
            }
        }

        // Handle Enter key in address bar
        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Go_Click(sender, e);
            }
        }

        // Handle Back button click
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (history.Count > 0)
            {
                LoadFiles(history.Pop());
            }
        }

        // Handle TreeView navigation
        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (FolderTree.SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                LoadFiles(path);
            }
        }

        // Populate subdirectories when a folder is expanded
        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                try
                {
                    foreach (var dir in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subItem = new TreeViewItem
                        {
                            Header = System.IO.Path.GetFileName(dir),
                            Tag = dir
                        };
                        subItem.Items.Add(null);
                        subItem.Expanded += Folder_Expanded;
                        item.Items.Add(subItem);
                    }
                }
                catch { }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }
    }

    public class FileItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
    }
}
