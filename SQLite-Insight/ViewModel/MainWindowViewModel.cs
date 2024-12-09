using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SQLite_Insight.Interfaces;
using SQLite_Insight.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace SQLite_Insight.ViewModel
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        private const string mainWindowDefaultTitle = "SQLite-Insight";

        [ObservableProperty]
        private bool fileOpened = false;

        [ObservableProperty]
        private string mainWindowTitle = mainWindowDefaultTitle;

        public RelayCommand OpenFileCommand { get; }
        public RelayCommand NewFileCommand { get; }

        public RelayCommand ClearQueryCommand { get; }
        public RelayCommand ExecuteQueryCommand { get; }

        public RelayCommand QueryInsertCommand { get; }
        public RelayCommand QueryDeleteCommand { get; }
        public RelayCommand QuerySelectCommand { get; }
        public RelayCommand QueryAddCommand { get; }
        public RelayCommand QueryDropCommand { get; }
        public RelayCommand QueryRenameCommand { get; }

        public RelayCommand HelpCommand { get; }
        public RelayCommand ChangeSelectionModeCommand { get; }

        public RelayCommand SwitchTableCommand { get; }
        public RelayCommand CreateTableCommand { get; }
        public RelayCommand RemoveTableCommand { get; }

        public RelayCommand IncreaseDataGridLayoutScaleCommand { get; }
        public RelayCommand DecreaseDataGridLayoutScaleCommand { get; }
        public RelayCommand ResetDataGridLayoutScaleCommand { get; }

        [ObservableProperty]
        private string queryTextBoxContent;

        [ObservableProperty]
        public Database? currentDatabase;

        [ObservableProperty]
        public double dataGridLayoutScale = 1;

        private readonly IDatabaseAction databaseAction;


        public MainWindowViewModel(IDatabaseAction databaseAction)
        {
            OpenFileCommand = new RelayCommand(OnOpenFile);
            NewFileCommand = new RelayCommand(OnNewFile);

            ClearQueryCommand = new RelayCommand(OnClearQuery);
            ExecuteQueryCommand = new RelayCommand(OnExecuteQuery);

            QueryInsertCommand = new RelayCommand(OnQueryInsert);
            QueryDeleteCommand = new RelayCommand(OnQueryDelete);
            QuerySelectCommand = new RelayCommand(OnQuerySelect);
            QueryAddCommand = new RelayCommand(OnQueryAdd);
            QueryDropCommand = new RelayCommand(OnQueryDrop);
            QueryRenameCommand = new RelayCommand(OnQueryRename);

            HelpCommand = new RelayCommand(OnHelp);
            ChangeSelectionModeCommand = new RelayCommand(OnChangeSelectionMode);

            SwitchTableCommand = new RelayCommand(OnSwitchTable);
            CreateTableCommand = new RelayCommand(OnCreateTable);
            RemoveTableCommand = new RelayCommand(OnRemoveTable);

            IncreaseDataGridLayoutScaleCommand = new RelayCommand(OnIncreaseDataGridLayoutScale);
            DecreaseDataGridLayoutScaleCommand = new RelayCommand(OnDecreaseDataGridLayoutScale);
            ResetDataGridLayoutScaleCommand = new RelayCommand(OnResetDataGridLayoutScale);

            this.databaseAction = databaseAction;
        }


        private void UpdateMainWindowTitle()
        {
            if (CurrentDatabase != null)
            {
                MainWindowTitle = System.IO.Path.GetFileName(CurrentDatabase.Path) + ": "
                    + CurrentDatabase.TableName + " - " + mainWindowDefaultTitle;
            }
        }


        private void UpdateMainWindowTable(string tableName)
        {
            CurrentDatabase = new Database(CurrentDatabase.Path, false, tableName);
            MainWindowTitle = CurrentDatabase.TableName + " - " + mainWindowDefaultTitle;
            databaseAction.FillDataGrid();
            databaseAction.SetSelectionButtonVisibility(false);
            UpdateMainWindowTitle();
        }


        private void OnSwitchTable()
        {
            if (CurrentDatabase == null)
            {
                MessageBox.Show("No database opened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> tableNames = Database.GetTableNames(CurrentDatabase.Path);
            if (tableNames.Count() != 0)
            {
                string? selectedTable = SelectDialogStatic.ShowDialog("Table selection", "Select a table to be opened:", tableNames);
                if (selectedTable != null && CurrentDatabase.TableName != selectedTable) {
                    UpdateMainWindowTable(selectedTable);
                }
            }
        }
        

        private void OnCreateTable()
        {
            if (CurrentDatabase == null)
            {
                MessageBox.Show("No database opened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            while (true) {
                string? newTableName = InputDialogStatic.ShowDialog("New table", "Name of a new table:");
                if (newTableName == null) // User pressed 'cancel' or closed the dialog window
                {
                    return;
                }
                else if (newTableName.Count() == 0)
                {
                    MessageBox.Show("Table must have a name!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    List<string> tableNames = Database.GetTableNames(CurrentDatabase.Path);
                    if (tableNames.Contains(newTableName))
                    {
                        MessageBox.Show($"Table named {newTableName} already exists in the database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        try
                        {
                            CurrentDatabase.Execute($"CREATE TABLE {newTableName} (id INT PRIMARY KEY);");
                            UpdateMainWindowTable(newTableName);
                        }
                        catch (Exception ex) {
                            MessageBox.Show($"Failed to add the table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }
                }
            }
        }
        

        private void OnRemoveTable()
        {
            if (CurrentDatabase == null)
            {
                MessageBox.Show("No database opened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Database.GetTableNames(CurrentDatabase.Path).Count() == 1)
            {
                MessageBox.Show("You can not delete the only table in the database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            while (true)
            {
                string? toRemoveTableName = SelectDialogStatic.ShowDialog("Remove table", "Name of a table to remove:", Database.GetTableNames(CurrentDatabase.Path));
                if (toRemoveTableName == null) // User pressed 'cancel' or closed the dialog window
                {
                    return;
                }
                else if (toRemoveTableName.Count() == 0)
                {
                    MessageBox.Show("There is no table without a name!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    List<string> tableNames = Database.GetTableNames(CurrentDatabase.Path);
                    if (!tableNames.Contains(toRemoveTableName))
                    {
                        MessageBox.Show($"There is no table named {toRemoveTableName} in the database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        try
                        {
                            CurrentDatabase.Execute($"DROP TABLE {toRemoveTableName};");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to remove the table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (toRemoveTableName == CurrentDatabase.TableName)
                        {
                            UpdateMainWindowTable(Database.GetTableNames(CurrentDatabase.Path)[0]);
                        }
                        return;
                    }
                }
            }
        }


        private void OnOpenFile()
        {
            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "SQLite Database | *.sqlite; *.sqlite3; *.db; *.db3; *.s3db; *.sl3";
            fileDialog.Title = "Pick an SQLite database file";

            bool? opened = fileDialog.ShowDialog();
            if (opened == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                if (!Database.IsValidSqliteDatabase(path))
                {
                    return;
                };

                List<string> tableNames = Database.GetTableNames(path);

                string queryText = "Select a table to be opened:";
                string? tableName = SelectDialogStatic.ShowDialog("Select table", queryText, tableNames);
                while (tableName == null)
                {
                    MessageBox.Show("You must select a table!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    tableName = SelectDialogStatic.ShowDialog("Select table", queryText, tableNames);
                }

                CurrentDatabase = new Database(path, false, tableName);
                UpdateMainWindowTitle();
                databaseAction.FillDataGrid();
                databaseAction.SetSelectionButtonVisibility(false);
                FileOpened = true;
            }
        }


        private void OnExecuteQuery()
        {
            if (CurrentDatabase == null)
            {
                MessageBox.Show("No database opened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ObservableCollection<Dictionary<string, string>>? selectionResult;

            try
            {
                selectionResult = CurrentDatabase.Execute(QueryTextBoxContent);
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Query execution failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (selectionResult != null)
            {
                CurrentDatabase.CurrentSelection = selectionResult;
                CurrentDatabase.SelectionMode = true;
                databaseAction.SetSelectionButtonVisibility(true);
            }
            else
            {
                CurrentDatabase.Update();
            }

            this.databaseAction.FillDataGrid();
            return;
        }


        private void OnClearQuery()
        {
            QueryTextBoxContent = "";
        }


        private void OnHelp()
        {
            string url = "https://www.sqlitetutorial.net/";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening webpage: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnQueryInsert()
        {
            string sampleInsertQuery = "INSERT INTO table_name () VALUES ();";

            if (CurrentDatabase != null)
            {
                string columnNamesString = string.Join(", ", CurrentDatabase.GetColumnNames());
                QueryTextBoxContent = $"INSERT INTO {CurrentDatabase.TableName} ({columnNamesString}) VALUES (  );";
            }
            else {
                QueryTextBoxContent = sampleInsertQuery;
            }
        }


        private void OnQuerySelect()
        {
            string sampleSelectQuery = "SELECT () FROM table_name;";

            if (CurrentDatabase != null)
            {
                string columnNamesString = string.Join(", ", CurrentDatabase.GetColumnNames());
                QueryTextBoxContent = $"SELECT {columnNamesString} FROM {CurrentDatabase.TableName};";
            }
            else
            {
                QueryTextBoxContent = sampleSelectQuery;
            }
        }


        private void OnQueryDelete()
        {
            string sampleSelectQuery = "DELETE FROM table_name WHERE condition;";

            if (CurrentDatabase != null)
            {
                QueryTextBoxContent = $"DELETE FROM {CurrentDatabase.TableName} WHERE (  );";
            }
            else
            {
                QueryTextBoxContent = sampleSelectQuery;
            }
        }


        private void OnQueryAdd()
        {
            string sampleSelectQuery = "ALTER TABLE table_name ADD column_name datatype;";

            if (CurrentDatabase != null)
            {
                QueryTextBoxContent = $"ALTER TABLE {CurrentDatabase.TableName} ADD name type;";
            }
            else
            {
                QueryTextBoxContent = sampleSelectQuery;
            }
        }


        private void OnQueryDrop()
        {
            string sampleSelectQuery = "ALTER TABLE table_name DROP COLUMN column_name;";

            if (CurrentDatabase != null)
            {
                QueryTextBoxContent = $"ALTER TABLE {CurrentDatabase.TableName} DROP COLUMN name;";
            }
            else
            {
                QueryTextBoxContent = sampleSelectQuery;
            }
        }


        private void OnQueryRename()
        {
            string sampleSelectQuery = "ALTER TABLE table_name RENAME COLUMN old_name TO new_name;";

            if (CurrentDatabase != null)
            {
                QueryTextBoxContent = $"ALTER TABLE {CurrentDatabase.TableName} RENAME COLUMN old TO new;";
            }
            else
            {
                QueryTextBoxContent = sampleSelectQuery;
            }
        }


        private void OnNewFile()
        {
            SaveFileDialog fileDialog = new SaveFileDialog
            {
                Title = "Create a new database file",
                Filter = "SQLite Database (*.db)|*.db",
                DefaultExt = "db"
            };

            if (fileDialog.ShowDialog() != true) {
                return;
            }

            string? dialogResult;
            string tableName;
            do
            {
                dialogResult = InputDialogStatic.ShowDialog("Table name", "Name of a new table");
                if (dialogResult == null)
                {
                    return;
                }
            }
            while (dialogResult.IndexOf(' ') != -1);

            tableName = dialogResult;

            string filePath = fileDialog.FileName;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            string directoryPath = System.IO.Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (FileStream fs = File.Create(filePath)) { };

            CurrentDatabase = new Database(filePath, true, tableName);
            MainWindowTitle = currentDatabase.TableName + " - " + mainWindowDefaultTitle;

            this.databaseAction.FillDataGrid();
            databaseAction.SetSelectionButtonVisibility(false);

            FileOpened = true;
        }


        private void OnChangeSelectionMode()
        {
            if (CurrentDatabase == null) 
            {
                return;
            }
            CurrentDatabase.SelectionMode = !CurrentDatabase.SelectionMode;
            databaseAction.FillDataGrid();
        }


        private void OnIncreaseDataGridLayoutScale()
        {
            DataGridLayoutScale = DataGridLayoutScale * 1.25;
        }


        private void OnDecreaseDataGridLayoutScale()
        {
            DataGridLayoutScale = DataGridLayoutScale * 0.8;
        }


        private void OnResetDataGridLayoutScale()
        {
            DataGridLayoutScale = 1;
        }
    }
}
