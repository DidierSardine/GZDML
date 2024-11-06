using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GZDML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string doomPath { get; set; } = "C:\\Example\\GZDoom\\gzdoom.exe";
        public string iWadPath { get; set; } = "C:\\Example\\GZDoom\\IWADS\\DOOM2.WAD";
        public string additionalArgs { get; set; }
        public ObservableCollection<ModItem> modItems { get; set; } = new ObservableCollection<ModItem>();
        public ObservableCollection<string> profileItems = new ObservableCollection<string>();
        public static string profilesPath = "GZDML-profiles";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            if (!System.IO.Directory.Exists(profilesPath))
            {
                System.IO.Directory.CreateDirectory(profilesPath);
            }
            if (System.IO.File.Exists(profilesPath + "\\GZDML_profile_default.json"))
            {
                string jsonString = System.IO.File.ReadAllText(profilesPath + "\\GZDML_profile_default.json");
                ModManagerData mmdata = JsonSerializer.Deserialize<ModManagerData>(jsonString);
                if (mmdata.DoomPath != null) doomPath = mmdata.DoomPath;
                if (mmdata.IWadPath != null) iWadPath = mmdata.IWadPath;
                if (mmdata.AdditionalArgs != null) additionalArgs = mmdata.AdditionalArgs;
                if (mmdata.ModItems != null) modItems = mmdata.ModItems;
            }
            else
            {
                ModManagerData data = new ModManagerData
                {
                    DoomPath = doomPath,
                    IWadPath = iWadPath,
                    AdditionalArgs = additionalArgs,
                    ModItems = modItems
                };
            }
            ExecutableTextBox.Text = doomPath;
            IWADTextBox.Text = iWadPath;
            RefreshProfiles();
        }

        public class ModManagerData
        {
            public string DoomPath { get; set; }
            public string IWadPath { get; set; }
            public string AdditionalArgs { get; set; }
            public ObservableCollection<ModItem> ModItems { get; set; } = new ObservableCollection<ModItem>();
        }
        public class ModItem
        {
            public string FullPath { get; set; }
            public string DisplayName
            {
                get
                {
                    return System.IO.Path.GetFileName(FullPath);
                }
            }
        }

        private void AddModButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Doom WAD or PK3|*.wad;*.pk3|All files (*.*)|*.*";
            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                foreach (var fileName in dialog.FileNames)
                { 
                    string filepath = fileName;
                    modItems.Add(new ModItem { FullPath = filepath });
                }
            }
        }

        private void ClearModListButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModListBox.Items.Count > 0)
            {
                modItems.Clear();
            }
        }

        private void RemoveModButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModListBox.SelectedItems.Count > 0)
            {
                var itemsToRemove = ModListBox.SelectedItems.Cast<ModItem>().ToList();
                foreach (var mod in itemsToRemove)
                {
                    modItems.Remove(mod);
                }
            }
        }

        private void ExecutableBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "GZDoom executable|*.exe|All files (*.*)|*.*";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                ExecutableTextBox.Text = dialog.FileName;
            }
        }

        private void IWADBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Doom IWAD|*.wad|All files (*.*)|*.*";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                IWADTextBox.Text = dialog.FileName;
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = ModListBox.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = modItems[selectedIndex];
                modItems.RemoveAt(selectedIndex);
                modItems.Insert(selectedIndex - 1, itemToMoveUp);
                ModListBox.SelectedIndex = selectedIndex - 1;
            }
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = ModListBox.SelectedIndex;

            if (selectedIndex + 1 < modItems.Count)
            {
                var itemToMoveDown = modItems[selectedIndex];
                modItems.RemoveAt(selectedIndex);
                modItems.Insert(selectedIndex + 1, itemToMoveDown);
                ModListBox.SelectedIndex = selectedIndex + 1;
            }
        }

        public bool SaveData(ModManagerData data, string filepath)
        {
            string jsonString = JsonSerializer.Serialize(data);
            try
            {
                System.IO.File.WriteAllText(profilesPath + "\\" + filepath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving profile: " + ex.Message, "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool LoadData(string filePath)
        {
            if (!System.IO.File.Exists(profilesPath + "\\" + filePath))
            {
                MessageBox.Show("Profile not found: " + filePath, "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            string jsonString = System.IO.File.ReadAllText(profilesPath + "\\" + filePath);
            ModManagerData mmdata = JsonSerializer.Deserialize<ModManagerData>(jsonString);
            if (mmdata.DoomPath != null) doomPath = mmdata.DoomPath;
            if (mmdata.IWadPath != null) iWadPath = mmdata.IWadPath;
            if (mmdata.AdditionalArgs != null) additionalArgs = mmdata.AdditionalArgs;
            if (mmdata.ModItems != null)
            {
                modItems.Clear();
                foreach (var mod in mmdata.ModItems)
                {
                    modItems.Add(mod);
                }
            }
            return true;
        }

        private void RefreshProfiles()
        {
            var profileFiles = System.IO.Directory.GetFiles(profilesPath + "\\", "GZDML_profile_*.json");
            profileItems.Clear();
            foreach (var profileFile in profileFiles)
            {
                string profileName = System.IO.Path.GetFileNameWithoutExtension(profileFile)
                    .Replace("GZDML_profile_", "");
                profileItems.Add(profileName);
            }

            ProfilesComboBox.ItemsSource = profileItems;
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mod in modItems)
            {
                if (!System.IO.File.Exists(mod.FullPath))
                {
                    MessageBox.Show("Mod file not found: " + mod.FullPath, "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            if (!System.IO.File.Exists(ExecutableTextBox.Text))
            {
                MessageBox.Show("Executable not found: " + ExecutableTextBox.Text, "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            StringBuilder args = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(IWADTextBox.Text) && System.IO.File.Exists(IWADTextBox.Text))
            {
                args.Append($" -iwad \"{IWADTextBox.Text}\"");
            }
            else if (string.IsNullOrWhiteSpace(IWADTextBox.Text))
            {
                MessageBox.Show("No IWAD file selected, the game will not launch automatically.", "GZDML Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("IWAD file not found: " + IWADTextBox.Text + "\nProcessing without, the game will not launch automatically.", "GZDML Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (var mod in modItems)
            {
                args.Append($" -file \"{mod.FullPath}\"");
            }
            if (additionalArgs != null)
            {
                args.Append(" " + additionalArgs);
            }
            try
            {
                System.Diagnostics.Process.Start(ExecutableTextBox.Text, args.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error launching GZDoom: " + ex.Message, "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SaveData(new ModManagerData { DoomPath = ExecutableTextBox.Text, IWadPath = IWADTextBox.Text, AdditionalArgs = additionalArgs, ModItems = modItems }, "GZDML_profile_default.json");
        }

        private void ProfilesSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem != null || !string.IsNullOrWhiteSpace(ProfilesComboBox.Text))
            {
                bool success = SaveData(new ModManagerData { DoomPath = ExecutableTextBox.Text, IWadPath = IWADTextBox.Text, AdditionalArgs = additionalArgs, ModItems = modItems }, $"GZDML_profile_{ProfilesComboBox.Text}.json");
                if (!success) return;
                MessageBox.Show("Profile saved: " + ProfilesComboBox.Text, "GZDML Info", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshProfiles();
            } 
            else
            {
                MessageBox.Show("Please enter a profile name.", "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfilesLoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem != null || !string.IsNullOrWhiteSpace(ProfilesComboBox.Text))
            {
                bool success = LoadData($"GZDML_profile_{ProfilesComboBox.Text}.json");
                if (!success) return;
                ExecutableTextBox.Text = doomPath;
                IWADTextBox.Text = iWadPath;
                ArgsTextBox.Text = additionalArgs;
                MessageBox.Show("Profile loaded: " + ProfilesComboBox.Text, "GZDML Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a profile.", "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfilesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem == null && string.IsNullOrWhiteSpace(ProfilesComboBox.Text))
            {
                MessageBox.Show("Please select a profile.", "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (System.IO.File.Exists(profilesPath + "\\GZDML_profile_" + ProfilesComboBox.Text + ".json"))
            {
                System.IO.File.Delete(profilesPath + "\\GZDML_profile_" + ProfilesComboBox.Text + ".json");
                MessageBox.Show("Profile deleted: " + ProfilesComboBox.Text, "GZDML Info", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshProfiles();
            }
            else
            {
                MessageBox.Show("Profile not found: " + ProfilesComboBox.Text, "GZDML Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}