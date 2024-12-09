using SQLite_Insight.Interfaces;
using SQLite_Insight.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SQLite_Insight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDatabaseAction
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainWindowViewModel(this);
            DataContext = vm;
        }

        public void FillDataGrid()
        {
            if (!(DataContext is MainWindowViewModel vm))
            {
                MessageBox.Show("Error while filling data grid.", "Error");
                return;
            }

            databaseDataGrid.Columns.Clear();
            databaseDataGrid.ItemsSource = null;
            
            if (vm.CurrentDatabase.SelectionMode == false)
            {
                selectionModeButton.Content = "⚎";
                var database = vm.CurrentDatabase;

                if (database.Rows.Count == 0)
                {
                    var columnNames = database.GetColumnNames();
                    if (columnNames.Count != 0)
                    {
                        foreach (var columnName in columnNames)
                        {
                            DataGridTextColumn col = new DataGridTextColumn
                            {
                                Header = columnName
                            };
                            databaseDataGrid.Columns.Add(col);
                        }
                    }

                    return;
                }

                foreach (var key in database.Rows[0].Keys)
                {
                    DataGridTextColumn col = new DataGridTextColumn
                    {
                        Header = key,
                        Binding = new Binding($"[{key}]")
                    };
                    databaseDataGrid.Columns.Add(col);
                }

                databaseDataGrid.ItemsSource = database.Rows;
            }
            else
            {
                selectionModeButton.Content = "☶";
                var dataCollection = vm.CurrentDatabase.CurrentSelection;

                if (dataCollection.Count == 0)
                {
                    return;
                }

                foreach (var key in dataCollection[0].Keys)
                {
                    DataGridTextColumn col = new DataGridTextColumn
                    {
                        Header = key,
                        Binding = new Binding($"[{key}]")
                    };
                    databaseDataGrid.Columns.Add(col);
                }

                databaseDataGrid.ItemsSource = dataCollection;
            }
        }

        public void SetSelectionButtonVisibility(bool state)
        {
            if (state)
            {
                selectionModeButton.Visibility = Visibility.Visible;
            }
            else
            {
                selectionModeButton.Visibility = Visibility.Hidden;
            }
        }
    }
}
