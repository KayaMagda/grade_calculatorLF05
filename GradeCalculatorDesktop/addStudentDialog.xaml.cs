using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GradeCalculatorDesktop
{
    /// <summary>
    /// Interaction logic for addStudentDialog.xaml
    /// </summary>
    public partial class addStudentDialog : Window
    {
        private List<Specialty> specialtys;
        public Student? student { get; set; }

        private Specialty? selectedSpecialty;
        private bool specialtySelected = false;

        private bool studentNumberValid = false;
        private bool firstNameValid = false;
        private bool lastNameValid = false;

        public addStudentDialog(List<Specialty> specialtys)
        {
            InitializeComponent();
            this.specialtys = specialtys;
            studentSpecialty.ItemsSource = specialtys;
            studentSpecialty.DisplayMemberPath = "specialtyName";
            DataObject.AddPastingHandler(studentNumber, PasteHandler);
        }

        public void onlyNumbersAllowed(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            bool isNumber = regex.IsMatch(e.Text);
            if (!isNumber)
            {
                MessageBox.Show("Bitte nur Zahlen eingeben.");
            }
            else
            {
                if (e.Text.Length > 7)
                {
                    MessageBox.Show("Schülernummern bestehen aus höchstens sieben Ziffern.");
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSpecialty = studentSpecialty.SelectedItem as Specialty;
            specialtySelected = true;
        }

        public void noNumbersAllowed(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = new Regex(@" ^\\p{ L } + $");
            bool isAlphabetical = regex.IsMatch(e.Text);
            if (!isAlphabetical)
            {
                MessageBox.Show("Bitte nur Buchstaben eingeben.");
                
            }
            else if (e.Text.Length > 1)
            {
                e.Handled = true;
                if (tb.Name == "firstName")
                {
                    firstNameValid = true;
                }
                else
                {
                    lastNameValid = true;
                }
            }
        }

        public void PasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            bool textOK = false;

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                // Allow pasting only numericallly
                Regex regex = new Regex("[^0-9.-]+");
                string pasteText = e.DataObject.GetData(typeof(string)) as string;
                if (regex.IsMatch(pasteText) && pasteText.Length <= 7)
                    textOK = true;
            }

            if (!textOK)
            {
                e.CancelCommand();
                MessageBox.Show("Bitte nur Zahlen eingeben. Schülernummer ist höchstens 7 Ziffern lang.");
            }
            else
            {
                e.Handled = true;
            }
        }

        private void noSpaceAllowed(object sender, KeyEventArgs e)
        {
            // Prohibit space
            if (e.Key == Key.Space)
                e.Handled = false;
        }

        private void spaceAllowed(object sender, KeyEventArgs e)
        {
            // Prohibit space
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void saveStudent(object sender, RoutedEventArgs e)
        {
            if (lastNameValid && firstNameValid && studentNumberValid && specialtySelected)
            {
                student = new Student()
                {
                    studentId = int.Parse(studentNumber.Text),
                    firstName = firstName.Text,
                    lastName = lastName.Text,
                    specialty = selectedSpecialty!.specialtyId
                };               
            }
            else
            {
                MessageBox.Show("Bitte gib valide Schüler Daten ein.");
            }
        }
    }
}