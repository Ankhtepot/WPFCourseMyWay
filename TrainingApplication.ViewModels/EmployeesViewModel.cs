using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TrainingApplication.Models;

namespace TrainingApplication.ViewModels
{
    public class EmployeesViewModel : INotifyPropertyChanged
    {
        private Employee itemSelected;

        #region Employee binding properties

        private string userId;
        private string jobTitle;
        private string firstName;
        private string lastName;
        private string fullName;
        private string employeeCode;
        private string region;
        private string phoneNo;
        private string emailId;
        private int id;

        public string UserId
        {
            get => userId;
            set
            {
                userId = value;
                OnPropertyChanged();
            }
        }

        public string JobTitle
        {
            get => jobTitle;
            set
            {
                jobTitle = value;
                OnPropertyChanged();
            }
        }

        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                OnPropertyChanged();
            }
        }

        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }

        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged();
            }
        }

        public string EmployeeCode
        {
            get => employeeCode;
            set
            {
                employeeCode = value;
                OnPropertyChanged();
            }
        }

        public string Region
        {
            get => region;
            set
            {
                region = value;
                OnPropertyChanged();
            }
        }

        public string PhoneNo
        {
            get => phoneNo;
            set
            {
                phoneNo = value;
                OnPropertyChanged();
            }
        }

        public string EmailId
        {
            get => emailId;
            set
            {
                emailId = value;
                OnPropertyChanged();
            }
        }

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ObservableCollection<Employee> Employees { get; set; }

        public Employee ItemSelected
        {
            get => itemSelected;
            set
            {
                itemSelected = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddEmployee { get; set; }

        public EmployeesViewModel()
        {
            Employees = new ObservableCollection<Employee>();
            AddEmployee = new RelayCommand(AddNewEmployee);
            PupulateStaticData();
        }

        private void AddNewEmployee()
        {
            string errorMessage = "Found problems:\n";
            bool isError = false;

            if (string.IsNullOrEmpty(FirstName))
            {
                errorMessage += "First name is null or empty.\n";
                isError = true;
            }

            if (string.IsNullOrEmpty(LastName))
            {
                errorMessage += "Last name is null or empty.\n";
                isError = true;
            }

            if (isError)
            {
                MessageBox.Show(errorMessage);
                return;
            }

            Employees.Add(new Employee());
        }

        private void PupulateStaticData()
        {
            Assembly assembly = typeof(Organisation).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("TrainingApplication.Models.Data.Employees.json"))
                if (stream != null)
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string employeeJson = reader.ReadToEnd();

                        Organisation organization = JsonSerializer.Deserialize<Organisation>(employeeJson);

                        foreach (Employee employee in organization.Employees)
                        {
                            Employees.Add(employee);
                        }
                    }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}