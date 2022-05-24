using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GradeCalculatorDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Student? focusedStudent;
        private List<Student> addedStudents;
        private List<Specialty> specialtys;
        private Specialty? selectedSpecialty;

        public MainWindow()
        {
            InitializeComponent();

            //all current specialties
            specialtys = new List<Specialty>()
             {
                new Specialty(0, "IT-System-Elektroniker_in", new string[] { "Erstellen, Ändern oder Erweitern von IT-Systemen und von deren Infrastruktur", "Installation von und Service an IT-Geräten, IT-Systemen und IT-Infrastrukturen", "Anbindung von Geräten, Systemen und Betriebsmitteln an die Stromversorgung" }),
                new Specialty(1, "Kaufmann/ Kauffrau für Digitalisierungsmanagement", new string[] { "Digitale Entwicklung von Prozessen", "Entwicklung eines digitalen Geschäftsmodells", "Kaufmännische Unterstützungsprozesse"}),
                new Specialty(2, "Kaufmann/ Kauffrau für IT-System-Management", new string[] { "Abwicklung eines Kundenauftrages", "Einführen einer IT-Systemlösung", "Kaufmännische Unterstützungsprozesse"}),
                new Specialty(3, "Fachinformatiker_in/ Anwendungsentwicklung", new string[] { "Planen und Umsetzen eines Softwareprojektes", "Planen eines Softwareproduktes", "Entwicklung und Umsetzung von Algorithmen"}),
                new Specialty(4, "Fachinformatiker_in/ Systemintegration", new string[] { "Planen und Umsetzen eines Projektes der Systemintegration", "Konzeption und Administration von IT-Systemen", "Analyse und Entwicklung von Netzwerken"}),
                new Specialty(5, "Fachinformatiker_in/ Digitale Vernetzung", new string[] { "Planen und Durchführen eines Projektes der Datenanalyse", "Durchführen einer Prozessanalyse", "Sicherstellen der Datenqualität"}),                
             };
            specialtyFilter.Items.Clear();
            specialtyFilter.ItemsSource = specialtys;
            specialtyFilter.DisplayMemberPath = "specialtyName";
            


        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSpecialty = specialtyFilter.SelectedItem as Specialty;
        }

        private void addStudent(object sender, RoutedEventArgs e)
        {
            var ownedAddStudent = new addStudentDialog(specialtys);
            //ShowDialog only returns if Window was closed
            var studentAdded = ownedAddStudent.ShowDialog();
            //if Window was closed student property of addStudentDialog Class now holds the new student
            if (studentAdded != null)
            {
                if(addedStudents != null && ownedAddStudent.student != null)
                {
                    addedStudents.Add(ownedAddStudent.student);
                    //addStudentRow
                }
                else if(ownedAddStudent.student != null)
                {
                    addedStudents = new List<Student> { ownedAddStudent.student };
                }

            }

        }
        //opens a .prf file to read Students from
        public void readStudentsFromFile()
        {

        }

        //opens a .prf file to save a Student into



        //adds a new oneStudentRow to the studentTable
        //public addStudentRow


        //validatesInputs into Grade Fields
        public void onlyNumbersAllowed(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            bool isNumber = regex.IsMatch(e.Text);
            int asNumber = int.Parse(e.Text);
            if (!isNumber)
            {
                MessageBox.Show("Bitte nur Zahlen eingeben.");
            }
            else
            {
                if (asNumber < 0 || asNumber > 100)
                {
                    MessageBox.Show("Die Punktzahl muss mindesten Null und darf höchstens 100 sein.");
                }
                else
                {
                    e.Handled = true;
                }
            }
        }
    }
}