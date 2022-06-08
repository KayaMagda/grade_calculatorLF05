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

        public addStudentDialog(List<Specialty> specialtys, Student? sentStudent)
        {
            InitializeComponent();
            this.specialtys = specialtys;
            studentSpecialty.ItemsSource = specialtys;
            studentSpecialty.DisplayMemberPath = "specialtyName";
            DataObject.AddPastingHandler(studentNumber, PasteHandler);
            if (sentStudent != null)
            {
                student = sentStudent;
                firstName.Text = student.firstName;
                firstNameValid = true;
                lastName.Text = student.lastName;
                lastNameValid = true;
                studentNumber.Text = student.studentId.ToString();
                studentNumberValid = true;
                foreach (Specialty specialty in specialtys)
                {
                    if (specialty.specialtyId == student.specialty)
                    {
                        studentSpecialty.SelectedItem = specialty;
                        specialtySelected = true;
                    }
                }
            }
        }

        public void onlyNumbersAllowed(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9]+$");
            bool isNumber = regex.IsMatch(e.Text);
            if (!isNumber)
            {
                MessageBox.Show("Bitte nur Zahlen eingeben.");
                e.Handled = true;
            }
            if (tb.Text.Length == 6)
            {
                studentNumberValid = true;
            }
            else
            {
                studentNumberValid = false;
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

            Regex regex = new Regex(@"^[\p{L}- ]+$");
            bool isAlphabetical = regex.IsMatch(e.Text);
            if (!isAlphabetical)
            {
                MessageBox.Show("Bitte nur Buchstaben eingeben.");
                e.Handled = true;
            }
            if (tb.Text.Length > 2)
            {
                if (tb.Name == "firstName")
                {
                    firstNameValid = true;
                }
                else if (tb.Name == "lastName")
                {
                    lastNameValid = true;
                }
            }
            else
            {
               if (tb.Name == "firstName")
                {
                    firstNameValid = false;
                } else if (tb.Name == "lastName")
                {
                    lastNameValid= false;
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
                DialogResult = true;
            }
            else if (!lastNameValid || !firstNameValid)
            {
                MessageBox.Show("Bitte gib einen Namen ein der länger als ein Buchstabe ist");
            }
            else if (!studentNumberValid)
            {
                MessageBox.Show("Bitte gib eine Nummer mit sieben Ziffern ein");
            }
            else if (!specialtySelected)
            {
                MessageBox.Show("Bitte wähle einen Fachbereich.");
            }
        }

        private void closeDialog(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}