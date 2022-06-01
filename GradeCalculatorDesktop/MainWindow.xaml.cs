using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        private GradeData? gradeData;
        private ObservableCollection<Student>? addedStudents;
        private List<Specialty> specialtys;
        private Specialty? selectedSpecialty;
        private TextBox[] allTextBoxes;
        private bool oralEnabled = false;
        private string previouslyOpenedFile;
        private int? selectedSpecialtyId;

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
            student_Verbal_Procent.IsEnabled = oralEnabled;
            foreach (TextBox box in allTextBoxes)
            {
                box.IsEnabled = false;
            }
            studentTable.Items.Clear();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSpecialty = specialtyFilter.SelectedItem as Specialty;
            selectedSpecialtyId = selectedSpecialty.specialtyId;
            if (addedStudents != null)
            {
                IEnumerable<Student>studentsOfSpecialty = addedStudents.Where(student => student.specialty == selectedSpecialtyId);
                //todo: keep former students somewhere
                addedStudents = new ObservableCollection<Student>();
                foreach (Student student in studentsOfSpecialty)
                {
                    addedStudents.Add(student);
                }               
                
            }
            studentTable.ItemsSource = addedStudents;
        }

        private void addStudent(object sender, RoutedEventArgs e)
        {
            var ownedAddStudent = new addStudentDialog(specialtys);

            //ShowDialog only returns if Window was closed
            System.Nullable<bool> studentAdded = ownedAddStudent.ShowDialog();
            //if Window was closed student property of addStudentDialog Class now holds the new student
            if (studentAdded != null && studentAdded == true)
            {
                if (addedStudents != null && ownedAddStudent.student != null)
                {
                    addedStudents.Add(ownedAddStudent.student);
                    studentTable.ItemsSource = addedStudents;
                }
                else if (ownedAddStudent.student != null)
                {
                    addedStudents = new ObservableCollection<Student> { ownedAddStudent.student };
                    studentTable.ItemsSource = addedStudents;
                }
            }
        }

        //opens a .prf file to read Students from
        public void readStudentsFromFile(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            if (filePath == previouslyOpenedFile)
            {
                MessageBox.Show("Diese Daten wurden bereits geladen.");
            }
            else
            {
                previouslyOpenedFile = filePath;
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
                    string fileContent = File.ReadAllText(filePath);
                    string[] toRemove = new string[] { "\r", "\n" };
                    foreach (string charToRemove in toRemove)
                    {
                        fileContent = fileContent.Replace(charToRemove, string.Empty);
                    }

                    string[] asStudents = fileContent.Split(';');
                    foreach (string jsonStudent in asStudents)
                    {
                        if (jsonStudent != string.Empty)
                        {
                            Student? student = JsonSerializer.Deserialize<Student>(jsonStudent);

                            if (addedStudents != null && student != null)
                            {
                                if (selectedSpecialtyId != null && student.specialty == selectedSpecialtyId)
                                { addedStudents.Add(student); }
                                else
                                {
                                    addedStudents.Add(student);
                                }
                            }
                            else if (student != null)
                            {
                                if (selectedSpecialtyId != null && student.specialty == selectedSpecialtyId)
                                {
                                    addedStudents = new ObservableCollection<Student>();
                                    studentTable.ItemsSource = addedStudents;
                                    addedStudents.Add(student);
                                }
                                else
                                {
                                    addedStudents = new ObservableCollection<Student>();
                                    studentTable.ItemsSource = addedStudents;
                                    addedStudents.Add(student);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void focusStudent(object sender, SelectionChangedEventArgs e)
        {
            if (studentTable.SelectedItem != null)
            {
                focusedStudent = studentTable.SelectedValue as Student;
                foreach (TextBox box in allTextBoxes)
                {
                    if (box.Name != "student_Verbal_Procent")
                    { box.IsEnabled = true; }
                }

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
                        gradeData = focusedStudent.gradeData;
                        if (gradeData.percentagePB1 != null)
                        {
                            student_AP_Teil_1_Procent.Text = gradeData.percentagePB1;
                        }
                        if (gradeData.percentageProject != null)
                        {
                            student_Project_Procent.Text = gradeData.percentageProject;
                        }
                        if (gradeData.percentagePresentation != null)
                        {
                            student_Presentation_Procent.Text = gradeData.percentagePresentation;
                        }
                        if (gradeData.percentageVariableOne != null)
                        {
                            student_Theory1_Procent.Text = gradeData.percentageVariableOne;
                        }
                        if (gradeData.percentageVariableTwo != null)
                        {
                            student_Theory2_Procent.Text = gradeData.percentageVariableTwo;
                        }
                        if (gradeData.percentageWiSo != null)
                        {
                            student_Economy_Procent.Text = gradeData.percentageWiSo;
                        }
                        if (gradeData.percentageOralAssessment != null)
                        {
                            student_Verbal_Procent.Text = gradeData.percentageOralAssessment;
                        }

                        standardAssessmentTwo.Content = assessmentNames[0];
                        variableAssessmentOne.Content = assessmentNames[1];
                        variableAssessmentTwo.Content = assessmentNames[2];

                        foreach (TextBox box in allTextBoxes)
                        {
                            if (box.Text != "")
                            {
                                int percentage = int.Parse(box.Text);
                                writeMarkToLabel(percentage, box.Name);
                            }
                        }
                    }
                    else
                    {
                        gradeData = new GradeData();
                        focusedStudent.gradeData = gradeData;
                    }
                }
            }
        }

        public void writeMarkToLabel(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int asNumber = int.Parse(tb.Text);
            if (tb.Text != string.Empty && asNumber > 0 && asNumber <= 100)
            {
                int percentage = int.Parse(tb.Text);
                int grade = Calculations.calculateGrade(percentage);
                switch (tb.Name)
                {
                    case "student_AP_Teil_1_Procent":
                        gradeData.percentagePB1 = tb.Text;
                        student_AP_Teil_1_Mark.Content = grade;
                        break;

                    case "student_Project_Procent":
                        gradeData.percentageProject = tb.Text;
                        student_Project_Mark.Content = grade;
                        break;

                    case "student_Presentation_Procent":
                        gradeData.percentagePresentation = tb.Text;
                        student_Presentation_Mark.Content = grade;
                        break;

                    case "student_Theory1_Procent":
                        gradeData.percentageVariableOne = tb.Text;
                        student_Theory1_Mark.Content = grade;
                        break;

                    case "student_Theory2_Procent":
                        gradeData.percentageVariableTwo = tb.Text;
                        student_Theory2_Mark.Content = grade;
                        break;

                    case "student_Economy_Procent":
                        gradeData.percentageWiSo = tb.Text;
                        student_Economy_Mark.Content = grade;
                        break;

                    case "student_Verbal_Procent":
                        gradeData.percentageOralAssessment = tb.Text;
                        student_Verbal_Mark.Content = grade;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Bitte gib nur Punktzahlen zwischen 0 und 100 ein.");
                tb.Text = string.Empty;
            }
        }

        public void writeMarkToLabel(int percentage, string textBoxName)
        {
            int grade = Calculations.calculateGrade(percentage);
            if (percentage > 0 && percentage <= 100)
            {
                switch (textBoxName)
                {
                    case "student_AP_Teil_1_Procent":
                        student_AP_Teil_1_Mark.Content = grade;
                        break;

                    case "student_Project_Procent":
                        student_Project_Mark.Content = grade;
                        break;

                    case "student_Presentation_Procent":
                        student_Presentation_Mark.Content = grade;
                        break;

                    case "student_Theory1_Procent":
                        student_Theory1_Mark.Content = grade;
                        break;

                    case "student_Theory2_Procent":
                        student_Theory2_Mark.Content = grade;
                        break;

                    case "student_Economy_Procent":
                        student_Economy_Mark.Content = grade;
                        break;

                    case "student_Verbal_Procent":
                        student_Verbal_Mark.Content = grade;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Diese Punktzahl befindet sich nicht im Bereich 0-100.");
            }
        }
        //todo: write methods to calculate all the different totals and decide where to call

        //opens a .prf file to save a Student into
        public async void saveStudent(object sender, RoutedEventArgs e)
        {
            string filePath = "";

            OpenFileDialog chooseFile = new OpenFileDialog();
            chooseFile.Title = "Datei wählen.";
            chooseFile.InitialDirectory = @"C:\";
            chooseFile.RestoreDirectory = true;
            chooseFile.Filter = "prf files (*.prf)|*.prf|All files (*.*)|*.*";
            chooseFile.FilterIndex = 0;
            chooseFile.DefaultExt = "prf";

            string studentString = JsonSerializer.Serialize<Student>(focusedStudent);

            if ((bool)chooseFile.ShowDialog())
            {
                filePath = chooseFile.FileName;
                string fileContent = File.ReadAllText(filePath);
                string[] toRemove = new string[] { "\r", "\n" };
                foreach (string charToRemove in toRemove)
                {
                    fileContent = fileContent.Replace(charToRemove, string.Empty);
                }
                string compareString = studentString.Substring(0, 21);

                string[] asStudents = fileContent.Split(';');

                foreach (string savedStudent in asStudents)
                {
                    if (savedStudent != string.Empty)
                    {
                        if (savedStudent.Contains(compareString))
                        {
                            fileContent = fileContent.Replace(savedStudent, studentString);
                            await using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                writer.WriteLine(fileContent);
                            }
                            MessageBox.Show("Prüflingsdaten wurden gespeichert.");
                        }
                        else
                        {
                            Student.serializeStudentAsync(focusedStudent, filePath);
                            MessageBox.Show("Prüflingsdaten wurden gespeichert.");
                        }
                    }
                }
            }
        }

        //validatesInputs into Grade Fields
        public void onlyNumbersAllowed(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9]+$");
            bool isNumber = regex.IsMatch(e.Text);
            int asNumber = 0;
            if (!isNumber)
            {
                MessageBox.Show("Bitte nur Zahlen eingeben.");
                e.Handled = true;
            }
            else if (tb.Text != string.Empty)
            {
                asNumber = int.Parse(tb.Text);

                if (asNumber < 0 || asNumber > 100)
                {
                    MessageBox.Show("Die Punktzahl muss mindesten Null und darf höchstens 100 sein.");
                }
            }
        }

        private void selectedStudentModifyButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}