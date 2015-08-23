using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoHorizontalScrollIssue
{

    public enum Gender { Male, Female };
    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public string Department { get; set; }
    }

    public partial class MainWindow : Window
    {
        CollectionViewSource cvSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadGridData();
            MyDataGrid.Focus();
        }

        private void btnAddGrouping_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(MyDataGrid.ItemsSource);
            if (view == null)
                return;

            if (view.GroupDescriptions.Count == 0)
                view.GroupDescriptions.Add(new PropertyGroupDescription("Department"));
            else if (view.GroupDescriptions.Count == 1)
                view.GroupDescriptions.Add(new PropertyGroupDescription("Gender"));

            if (MyDataGrid.GroupStyle == null || MyDataGrid.GroupStyle.Count == 0)
            {
                GroupStyle groupStyle = TryFindResource("GroupHeaderStyle") as GroupStyle;
                MyDataGrid.GroupStyle.Add(groupStyle);
            }

            MyDataGrid.UpdateLayout();
            ScrollToSelection();
            MyDataGrid.Focus();
        }

        private void btnClearGrouping_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(MyDataGrid.ItemsSource);
            if (view == null)
                return;

            if (view.GroupDescriptions.Count == 2)
            {
                view.GroupDescriptions.RemoveAt(1);
            }
            else if (view.GroupDescriptions.Count == 1)
            {
                view.GroupDescriptions.RemoveAt(0);
            }

            ScrollToSelection();
            MyDataGrid.Focus();
        }

        private void loadGridData()
        {
            // add columns
            ObservableCollection<DataGridColumn> columns = createColumns();
            foreach (DataGridColumn col in columns)
                MyDataGrid.Columns.Add(col);

            // add rows
            cvSource = new CollectionViewSource();
            cvSource.Source = createRows();

            // set datagrid items
            Binding itemsSourceBinding = new Binding();
            itemsSourceBinding.Source = cvSource;
            MyDataGrid.SetBinding(DataGrid.ItemsSourceProperty, itemsSourceBinding);
        }

        private ObservableCollection<DataGridColumn> createColumns()
        {
            System.Int64 colNo = 100;

            // generate columns
            ObservableCollection<DataGridColumn> columns = new ObservableCollection<DataGridColumn>();

            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = "First Name";
            textColumn.Binding = new Binding("FirstName");
            columns.Add(textColumn);

            DataGridComboBoxColumn comboColumn = new DataGridComboBoxColumn();
            comboColumn.Header = "Gender";
            comboColumn.TextBinding = new Binding("Gender");
            comboColumn.ItemsSource = typeof(Gender).GetEnumValues();
            columns.Add(comboColumn);

            DataGridTextColumn depColumn = new DataGridTextColumn();
            depColumn.Header = "Department";
            depColumn.Binding = new Binding("Department");
            columns.Add(depColumn);

            for (System.Int64 i = 2; i < colNo; i++)
            {
                textColumn = new DataGridTextColumn();
                textColumn.Header = "Last Name" + (i + 1).ToString("D10");
                textColumn.Binding = new Binding("LastName");
                columns.Add(textColumn);
            }

            return columns;
        }

        private List<Customer> createRows()
        {
            System.Int64 rowNo = 100;
            System.Int64 noOfGroups = 80;

            // generate rows
            List<Customer> customers = new List<Customer>();
            for (System.Int64 i = 0; i < rowNo; i++)
            {
                Gender gender;
                if (i % 2 == 0)
                    gender = Gender.Male;
                else
                    gender = Gender.Female;

                string deparment = "Dept" + (i % noOfGroups + 1).ToString("D10");

                customers.Add(new Customer()
                {
                    FirstName = "FirstName" + (i + 1).ToString("D10"),
                    LastName = "LastName" + i.ToString("D10"),
                    Gender = gender,
                    Department = deparment
                });
            }

            return customers;
        }

        private void ScrollToSelection()
        {
            MyDataGrid.Items.MoveCurrentTo(MyDataGrid.SelectedItem);
            MyDataGrid.ScrollIntoView(MyDataGrid.SelectedItem);
        }
    }
}