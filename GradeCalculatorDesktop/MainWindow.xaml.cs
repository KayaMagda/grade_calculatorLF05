using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private List<Student>? addedStudents;
        private List<Specialty> specialtys;
        private Specialty? selectedSpecialty;
        private TextBox[] allTextBoxes;

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
            allTextBoxes = new TextBox[] { student_AP_Teil_1_Procent, student_Project_Procent, student_Presentation_Procent, student_Theory1_Procent, student_Theory2_Procent, student_Economy_Procent, student_Verbal_Procent };

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSpecialty = specialtyFilter.SelectedItem as Specialty;
        }

        private void addStudent(object sender, RoutedEventArgs e)
        {
            var ownedAddStudent = new addStudentDialog(specialtys);

            //ShowDialog only returns if Window was closed
            System.Nullable<bool> studentAdded = ownedAddStudent.ShowDialog();
            //if Window was closed student property of addStudentDialog Class now holds the new student
            if (studentAdded != null && studentAdded == true)
            {
                if(addedStudents != null && ownedAddStudent.student != null)
                {
                    addedStudents.Add(ownedAddStudent.student);
                    studentTable.ItemsSource = addedStudents;
                }
                else if(ownedAddStudent.student != null)
                {
                    addedStudents = new List<Student> { ownedAddStudent.student };
                    studentTable.ItemsSource = addedStudents;
                }

            }

        }
        //opens a .prf file to read Students from
        public void readStudentsFromFile()
        {

        }
        public void focusStudent(object sender, SelectionChangedEventArgs e)
        {
            focusedStudent = studentTable.SelectedValue as Student;

            Specialty focusedStudentSpecialty = specialtys.Find(specialty => specialty.specialtyId == focusedStudent.specialty);
            string[] assessmentNames = focusedStudentSpecialty.specialtyVariableAssesments;

            standardAssessmentTwo.Content = assessmentNames[0];
            variableAssessmentOne.Content = assessmentNames[1];
            variableAssessmentTwo.Content = assessmentNames[2];

            if (focusedStudent != null)
            {
                selectedStudentName.Content = focusedStudent.firstName + " " + focusedStudent.lastName;
                if (focusedStudent.gradeData != null)
                {
                    GradeData gD = focusedStudent.gradeData;
                    if(gD.percentagePB1 != null)
                    {
                        student_AP_Teil_1_Procent.Text = gD.percentagePB1;
                    }
                    if(gD.percentageProject != null)
                    {
                        student_Project_Procent.Text = gD.percentageProject;
                    }
                    if (gD.percentagePresentation != null)
                    {
                        student_Presentation_Procent.Text = gD.percentagePresentation;
                    }
                    if(gD.percentageVariableOne != null)
                    {
                        student_Theory1_Procent.Text = gD.percentageVariableOne;
                    }
                    if(gD.percentageVariableTwo != null)
                    {
                        student_Theory2_Procent.Text = gD.percentageVariableTwo;
                    }
                    if(gD.percentageWiSo != null)
                    {
                        student_Economy_Procent.Text = gD.percentageWiSo;
                    }
                    if(gD.percentageOralAssessment != null)
                    {
                        student_Verbal_Procent.Text = gD.percentageOralAssessment;
                    }

                    Specialty focusedStudentSpecialty = specialtys.Find(specialty => specialty.specialtyId == focusedStudent.specialty);
                    string[] assessmentNames = focusedStudentSpecialty.specialtyVariableAssesments;

                    standardAssessmentTwo.Content = assessmentNames[0];
                    variableAssessmentOne.Content = assessmentNames[1];
                    variableAssessmentTwo.Content = assessmentNames[2];   
                    
                    foreach(TextBox box in allTextBoxes)
                    {
                        if (box.Text != "")
                        {
                            int percentage = int.Parse(box.Text);
                            writeMarkToLabel(percentage);
                        }
                    }
                    
                }
            }
        }
        public void writeMarkToLabel(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int percentage = int.Parse(tb.Text);
            int grade = Calculations.calculateGrade(percentage);
            switch (tb.Name)
            {
                case "student_AP_Teil_1_Procent":
                    student_AP_Teil_1_Mark.Content = grade;
                    break;
            }

        }
        public void writeMarkToLabel(int percentage)
        {
            int grade = Calculations.calculateGrade(percentage);
        }

        //opens a .prf file to save a Student into
        public void saveStudent(object sender, RoutedEventArgs e)
        {
            string filePath = "";

            OpenFileDialog chooseFile = new OpenFileDialog();
            chooseFile.Title = "Datei wählen.";
            chooseFile.InitialDirectory = @"C:\";
            chooseFile.RestoreDirectory = true;
            chooseFile.Filter = "prf files (*.prf)|*.prf|All files (*.*)|*.*";
            chooseFile.FilterIndex = 0;
            chooseFile.DefaultExt = "prf";
            
            if ((bool)chooseFile.ShowDialog())
            {
                filePath = chooseFile.FileName;
                Student.serializeStudent(focusedStudent, filePath);
            }
            
        }


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