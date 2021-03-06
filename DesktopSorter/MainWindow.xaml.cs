using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DesktopSorter

{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool iscurrentpathcorrect = true;
        readonly Sortiermachine machine = new Sortiermachine(); //Backend Klasse

        //Connection und Dataadapter für die Tabellen
        SQLiteDataAdapter destDa;
        SQLiteConnection destCon;
        SQLiteDataAdapter whiteDa;
        SQLiteConnection whiteCon;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MyWindow_Loaded;

        }

        public void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //init Desktoppfad
            path.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //init deleteColumn
            var template = new DataTemplate();
            FrameworkElementFactory deleteButton = new FrameworkElementFactory(typeof(Button));
            deleteButton.SetValue(Button.ContentProperty, "X");
            deleteButton.SetValue(Button.CommandProperty, System.Windows.Input.ApplicationCommands.Delete);
            template.VisualTree = deleteButton;

            var deleteColumn1 = new DataGridTemplateColumn();
            deleteColumn1.Header = "Delete";
            deleteColumn1.CellTemplate = template;

            var deleteColumn2 = new DataGridTemplateColumn();
            deleteColumn2.Header = "Delete";
            deleteColumn2.CellTemplate = template;

            //init destinationTable
            destinationTable.ItemsSource = machine.GetTable("SELECT * FROM Destinations", ref destCon, ref destDa).DefaultView;
            destinationTable.Columns.Add(deleteColumn1);

            //init whitelistTable
            whitelistTable.ItemsSource = machine.GetTable("SELECT * FROM Whitelist", ref whiteCon, ref whiteDa).DefaultView;
            whitelistTable.Columns.Add(deleteColumn2);
        }

        public void sort_Click(object sender, RoutedEventArgs e)
        {
            //Directories und Whitelist sicher, falls vom user nicht gemacht wurde
            saveDirectories();
            saveWhitelist();

            //Rückbgabemeldung
            string message;
            
            //Start sort, falls Sorting path correct
            if (iscurrentpathcorrect)
            {
                message = machine.Sort(path.Text, progressbar, progressbartext);
            }
            else
            {
                message = "Error:\nDirectory path is not correct!\n\"" + path.Text + "\"";
            }

            // MessageBox ausgeben 
            MessageBox.Show(message);

            // progressbar zurücksetzen
            progressbar.Value = 0;
            progressbartext.Text = "";
        }

        private void pathchange_Click(object sender, RoutedEventArgs e)
        {
            var BrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            BrowserDialog.UseDescriptionForTitle = true;
            BrowserDialog.Description = "Please choose your folder to sort";
            BrowserDialog.ShowDialog();

            // Falls Abbrechen gedrückt wird -> keine Änderung am Pfad
            if (BrowserDialog.SelectedPath != "")
            {
                path.Text = BrowserDialog.SelectedPath;
            }

        }

        private void path_TextChanged(object sender, TextChangedEventArgs e)
        {

            //Check if Path exists
            if (Directory.Exists(path.Text))
            {
                iscurrentpathcorrect = true;
                pathincorrect.Text = ""; 
            }
            else
            {
                iscurrentpathcorrect = false;

                if (pathincorrect != null)
                {
                    pathincorrect.Text = "Error: Directory does not exist!";
                }
            }
        }

        private void saveDirectories_Click(object sender, RoutedEventArgs e)
        {
            saveDirectories();
        }

        private void saveWhitelist_Click(object sender, RoutedEventArgs e)
        {
            saveWhitelist();
        }

        private void saveDirectories()
        {
            machine.SaveData((destinationTable.ItemsSource as DataView).Table, "Destinations", ref destCon, ref destDa);
        }

        private void saveWhitelist()
        {
            machine.SaveData((whitelistTable.ItemsSource as DataView).Table, "Whitelist", ref whiteCon, ref whiteDa);
        }
    }
}
