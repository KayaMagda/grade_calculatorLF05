using Microsoft.Win32;
using System;
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
        private Label[] allFocusedLabels;
        private bool oralEnabled = false;
        private string previouslyOpenedFile;
        private int? selectedSpecialtyId;
        private ObservableCollection<NameAndValueStructure> forOralAssessment;
        private List<int> allPercentages = new List<int>();

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
                new Specialty(6, "Keine Auswahl", new string[] {"Projekt", "Theorie 1", "Theorie 2"}),
             };
            specialtyFilter.Items.Clear();
            specialtyFilter.ItemsSource = specialtys;
            specialtyFilter.DisplayMemberPath = "specialtyName";
            forOralAssessment = new ObservableCollection<NameAndValueStructure>();
            NameAndValueStructure initial = new NameAndValueStructure() { name = "", value = "Keine Schülerdaten vorhanden", percentage = 0 };
            forOralAssessment.Add(initial);
            oralAssessment.ItemsSource = forOralAssessment;
            oralAssessment.DisplayMemberPath = "value";
            oralAssessment.SelectedValuePath = "name";
            allTextBoxes = new TextBox[] { student_AP_Teil_1_Procent, student_Project_Procent, student_Presentation_Procent, student_Theory1_Procent, student_Theory2_Procent, student_Economy_Procent, student_Verbal_Procent };
            allFocusedLabels = new Label[] { student_Project_Total_Procent, student_Result_Procent, student_Total_Procent, student_AP_Teil_1_Mark, student_Project_Mark, student_Presentation_Mark, student_Project_Total_Mark, student_Theory1_Mark, student_Theory2_Mark, student_Economy_Mark, student_Verbal_Mark, student_Result_Mark, student_Total_Mark, grade_As_Word };
            student_Verbal_Procent.IsEnabled = oralEnabled;
            saveButton.IsEnabled = false;

            foreach (TextBox box in allTextBoxes)
            {
                box.IsEnabled = false;
            }
            studentTable.Items.Clear();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Student[] copyForSelection = null;
            selectedSpecialty = specialtyFilter.SelectedItem as Specialty;
            selectedSpecialtyId = selectedSpecialty.specialtyId;
            ObservableCollection<Student> observableStudentsOfSpecialty = new ObservableCollection<Student>();
            if (addedStudents != null)
            {
                if (selectedSpecialtyId != 6)
                {
                    copyForSelection = new Student[addedStudents.Count];
                    addedStudents.CopyTo(copyForSelection, 0);
                    IEnumerable<Student> studentsOfSpecialty = copyForSelection.Where(student => student.specialty == selectedSpecialtyId);
                    foreach (Student student in studentsOfSpecialty)
                    {
                        observableStudentsOfSpecialty.Add(student);
                    }
                    studentTable.ItemsSource = observableStudentsOfSpecialty;
                }
                else
                {
                    studentTable.ItemsSource = addedStudents;
                }
            }
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
                    saveButton.IsEnabled = true;
                }
                else if (ownedAddStudent.student != null)
                {
                    addedStudents = new ObservableCollection<Student> { ownedAddStudent.student };
                    studentTable.ItemsSource = addedStudents;
                    saveButton.IsEnabled = true;
                }
            }
        }

        //opens a .prf file to read Students from
        public void readStudentsFromFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog chooseFile = new OpenFileDialog();
            chooseFile.Title = "Datei wählen.";
            chooseFile.InitialDirectory = @"C:\";
            chooseFile.RestoreDirectory = true;
            chooseFile.Filter = "prf files (*.prf)|*.prf|All files (*.*)|*.*";
            chooseFile.FilterIndex = 0;
            chooseFile.DefaultExt = "prf";

            if ((bool)chooseFile.ShowDialog())
            {
                string filePath = chooseFile.FileName;
                MessageBoxResult result = MessageBoxResult.Cancel;
                if (filePath == previouslyOpenedFile)
                {
                    result = MessageBox.Show("Diese Daten wurden bereits geladen. Möchtest du sie neu laden?", "Daten neu laden.", MessageBoxButton.YesNoCancel);
                }
                else
                {
                    previouslyOpenedFile = filePath;
                    string fileContent = File.ReadAllText(filePath);
                    string[] toRemove = new string[] { "\r", "\n" };
                    foreach (string charToRemove in toRemove)
                    {
                        fileContent = fileContent.Replace(charToRemove, string.Empty);
                    }

                    string[] asStudents = fileContent.Split(';');
                    if (result == MessageBoxResult.Yes)
                    {
                        addedStudents = new ObservableCollection<Student>();
                    }
                    foreach (string jsonStudent in asStudents)
                    {
                        if (jsonStudent != string.Empty)
                        {
                            Student? student = JsonSerializer.Deserialize<Student>(jsonStudent);

                            if (addedStudents != null && student != null)
                            {
                                if (selectedSpecialtyId != null && student.specialty == selectedSpecialtyId)
                                {
                                    addedStudents.Add(student);
                                }
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
                            saveButton.IsEnabled = true;
                        }
                    }
                }
            }
        }

        public void focusStudent(object sender, SelectionChangedEventArgs e)
        {
            if (studentTable.SelectedItem != null)
            {
                forOralAssessment = new ObservableCollection<NameAndValueStructure>();
                focusedStudent = studentTable.SelectedValue as Student;
                allPercentages.Clear();
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
                        foreach (Label label in allFocusedLabels)
                        {
                            label.Content = "-";
                        }
                        gradeData = focusedStudent.gradeData;
                        if (gradeData.percentagePB1 != null)
                        {
                            student_AP_Teil_1_Procent.Text = gradeData.percentagePB1;
                        }
                        else
                        {
                            student_AP_Teil_1_Procent.Text = "";
                        }
                        if (gradeData.percentageProject != null)
                        {
                            student_Project_Procent.Text = gradeData.percentageProject;
                        }
                        else
                        {
                            student_Project_Procent.Text = "";
                        }
                        if (gradeData.percentagePresentation != null)
                        {
                            student_Presentation_Procent.Text = gradeData.percentagePresentation;
                        }
                        else
                        {
                            student_Presentation_Procent.Text = "";
                        }
                        if (gradeData.percentageVariableOne != null && gradeData.percentageVariableOne != "")
                        {
                            student_Theory1_Procent.Text = gradeData.percentageVariableOne;
                            int grade = Calculations.calculateGrade(int.Parse(gradeData.percentageVariableOne));
                            if (grade >= 5)
                            {
                                NameAndValueStructure nameAndValueStructure = new NameAndValueStructure()
                                {
                                    name = student_Theory1_Procent.Name,
                                    value = assessmentNames[1],
                                    percentage = int.Parse(gradeData.percentageVariableOne)
                                };
                                forOralAssessment.Add(nameAndValueStructure);
                            }
                        }
                        else
                        {
                            student_Theory1_Procent.Text = "";
                        }
                        if (gradeData.percentageVariableTwo != null && gradeData.percentageVariableTwo != "")
                        {
                            student_Theory2_Procent.Text = gradeData.percentageVariableTwo;
                            allPercentages.Add(4);
                            int grade = Calculations.calculateGrade(int.Parse(gradeData.percentageVariableTwo));
                            if (grade >= 5)
                            {
                                NameAndValueStructure nameAndValueStructure = new NameAndValueStructure()
                                {
                                    name = student_Theory2_Procent.Name,
                                    value = assessmentNames[2],
                                    percentage = int.Parse(gradeData.percentageVariableTwo)
                                };
                                forOralAssessment.Add(nameAndValueStructure);
                            }
                        }
                        else
                        {
                            student_Theory2_Procent.Text = "";
                        }
                        if (gradeData.percentageWiSo != null && gradeData.percentageWiSo != "")
                        {
                            student_Economy_Procent.Text = gradeData.percentageWiSo;
                            int grade = Calculations.calculateGrade(int.Parse(gradeData.percentageWiSo));
                            if (grade >= 5)
                            {
                                NameAndValueStructure nameAndValueStructure = new NameAndValueStructure()
                                {
                                    name = student_Economy_Procent.Name,
                                    value = "Wirtschaft und Sozialkunde",
                                    percentage = int.Parse(gradeData.percentageWiSo)
                                };
                                forOralAssessment.Add(nameAndValueStructure);
                            }
                        }
                        else
                        {
                            student_Economy_Procent.Text = string.Empty;
                        }
                        if (gradeData.percentageOralAssessment != null)
                        {
                            student_Verbal_Procent.Text = gradeData.percentageOralAssessment;
                        }
                        else
                        {
                            student_Verbal_Procent.Text = "";
                            student_Verbal_Mark.Content = "-";
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
                        foreach (TextBox box in allTextBoxes)
                        {
                            box.Text = "";
                        }
                        foreach (Label label in allFocusedLabels)
                        {
                            label.Content = "-";
                        }
                    }
                }
                saveButton.IsEnabled = true;
                oralAssessment.ItemsSource = forOralAssessment;
            }
        }

        public void writeMarkToLabel(object sender, RoutedEventArgs e)
        {
            ObservableCollection<NameAndValueStructure> copyForChange = new ObservableCollection<NameAndValueStructure>();
            bool forOralDropdownFilled = forOralAssessment.Count != 0;
            Specialty focusedStudentSpecialty = specialtys.Find(specialty => specialty.specialtyId == focusedStudent.specialty);
            string[] assessmentNames = focusedStudentSpecialty.specialtyVariableAssesments;

            TextBox tb = sender as TextBox;
            bool isNumber = int.TryParse(tb.Text, out int asNumber);
            if (isNumber == true)
            {
                if (tb.Text != string.Empty && asNumber > 0 && asNumber <= 100)
                {
                    int grade = Calculations.calculateGrade(asNumber);
                    bool alreadyThere = false;
                    switch (tb.Name)
                    {
                        case "student_AP_Teil_1_Procent":
                            gradeData.percentagePB1 = tb.Text;
                            student_AP_Teil_1_Mark.Content = grade;
                            allPercentages.Add(1);
                            break;

                        case "student_Project_Procent":
                            gradeData.percentageProject = tb.Text;
                            string otherPart = student_Presentation_Procent.Text;
                            if (int.TryParse(otherPart, out int projectNumber))
                            {
                                totalProjectPart(asNumber, projectNumber);
                                allPercentages.Add(2);
                            }
                            else
                            {
                                totalProjectPart(asNumber, 0);
                                allPercentages.Add(2);
                            }
                            student_Project_Mark.Content = grade;
                            break;

                        case "student_Presentation_Procent":
                            gradeData.percentagePresentation = tb.Text;
                            student_Presentation_Mark.Content = grade;
                            string projectPart = student_Project_Procent.Text;
                            if (int.TryParse(projectPart, out int partNumber))
                            {
                                totalProjectPart(asNumber, partNumber);
                                allPercentages.Add(2);
                            }
                            else
                            {
                                totalProjectPart(asNumber, 0);
                                allPercentages.Add(2);
                            }
                            break;

                        case "student_Theory1_Procent":
                            gradeData.percentageVariableOne = tb.Text;
                            student_Theory1_Mark.Content = grade;
                            allPercentages.Add(3);
                            if (grade < 5)
                            {
                                if (forOralDropdownFilled)
                                {
                                    foreach (NameAndValueStructure option in forOralAssessment)
                                    {
                                        if (option.name != tb.Name)
                                        {
                                            copyForChange.Add(option);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                NameAndValueStructure nameAndValueStructure = new NameAndValueStructure()
                                {
                                    name = tb.Name,
                                    value = assessmentNames[1],
                                    percentage = int.Parse(gradeData.percentageVariableOne)
                                };
                                foreach (NameAndValueStructure option in forOralAssessment)
                                {
                                    if (option.name == tb.Name)
                                    {
                                        alreadyThere = true;
                                    }
                                }

                                if (!alreadyThere)
                                {
                                    forOralAssessment.Add(nameAndValueStructure);
                                }
                            }
                            break;

                        case "student_Theory2_Procent":
                            gradeData.percentageVariableTwo = tb.Text;
                            student_Theory2_Mark.Content = grade;
                            allPercentages.Add(4);
                            if (grade < 5)
                            {
                                if (forOralDropdownFilled)
                                {
                                    foreach (NameAndValueStructure option in forOralAssessment)
                                    {
                                        if (option.name != tb.Name)
                                        {
                                            copyForChange.Add(option);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                NameAndValueStructure nameAndValueStructure = new NameAndValueStructure()
                                {
                                    name = tb.Name,
                                    value = assessmentNames[1],
                                    percentage = int.Parse(gradeData.percentageVariableTwo)
                                };
                                foreach (NameAndValueStructure option in forOralAssessment)
                                {
                                    if (option.name == tb.Name)
                                    {
                                        alreadyThere = true;
                                    }
                                }

                                if (!alreadyThere)
                                {
                                    forOralAssessment.Add(nameAndValueStructure);
                                }
                            }
                            break;

                        case "student_Economy_Procent":
                            gradeData.percentageWiSo = tb.Text;
                            student_Economy_Mark.Content = grade;
                            allPercentages.Add(5);
                            if (allPercentages.Count >= 5)
                            {
                                calculateTotal();
                            }
                            if (grade < 5)
                            {
                                if (forOralDropdownFilled)
                                {
                                    foreach (NameAndValueStructure option in forOralAssessment)
                                    {
                                        if (option.name != tb.Name)
                                        {
                                            copyForChange.Add(option);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                NameAndValueStructure nameAndValueStructure = new NameAndValueStructure()
                                {
                                    name = tb.Name,
                                    value = "Wirtschaft und Sozialkunde",
                                    percentage = int.Parse(gradeData.percentageWiSo)
                                };
                                foreach (NameAndValueStructure option in forOralAssessment)
                                {
                                    if (option.name == tb.Name)
                                    {
                                        alreadyThere = true;
                                    }
                                }

                                if (!alreadyThere)
                                {
                                    forOralAssessment.Add(nameAndValueStructure);
                                }
                            }
                            break;

                        case "student_Verbal_Procent":
                            gradeData.percentageOralAssessment = tb.Text;
                            student_Verbal_Mark.Content = grade;
                            allPercentages.Add(6);
                            if (allPercentages.Count >= 5)
                            {
                                calculateTotal();
                            }
                            break;

                        default:
                            break;
                    }
                    if (copyForChange.Count != 0)
                    {
                        forOralAssessment.Clear();
                        foreach (NameAndValueStructure option in copyForChange)
                        {
                            forOralAssessment.Add(option);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Bitte gib nur Punktzahlen zwischen 0 und 100 ein.");
                    tb.Text = string.Empty;
                }
                if (allPercentages.Count >= 5)
                {
                    calculateTotal();
                }
            }
            else if (tb.Text == "")
            {
                switch (tb.Name)
                {
                    case "student_AP_Teil_1_Procent":
                        gradeData.percentagePB1 = tb.Text;
                        student_AP_Teil_1_Mark.Content = "-";
                        allPercentages.Remove(1);
                        break;

                    case "student_Project_Procent":
                        gradeData.percentageProject = tb.Text;
                        string otherPart = student_Presentation_Procent.Text;
                        if (int.TryParse(otherPart, out int projectNumber))
                        {
                            totalProjectPart(0, projectNumber);
                        }
                        else
                        {
                            student_Project_Total_Procent.Content = "-";
                            student_Project_Total_Mark.Content = "-";
                            allPercentages.Remove(2);
                        }
                        student_Project_Mark.Content = "-";
                        break;

                    case "student_Presentation_Procent":
                        gradeData.percentagePresentation = tb.Text;
                        student_Presentation_Mark.Content = "-";
                        string projectPart = student_Project_Procent.Text;
                        if (int.TryParse(projectPart, out int partNumber))
                        {
                            totalProjectPart(0, partNumber);
                        }
                        else
                        {
                            student_Project_Total_Procent.Content = "-";
                            student_Project_Total_Mark.Content = "-";
                            allPercentages.Remove(2);
                        }
                        break;

                    case "student_Theory1_Procent":
                        gradeData.percentageVariableOne = tb.Text;
                        student_Theory1_Mark.Content = "-";
                        allPercentages.Remove(3);
                        break;

                    case "student_Theory2_Procent":
                        gradeData.percentageVariableTwo = tb.Text;
                        student_Theory2_Mark.Content = "-";
                        allPercentages.Remove(4);
                        break;

                    case "student_Economy_Procent":
                        gradeData.percentageWiSo = tb.Text;
                        student_Economy_Mark.Content = "-";
                        allPercentages.Remove(5);
                        break;

                    case "student_Verbal_Procent":
                        gradeData.percentageOralAssessment = tb.Text;
                        student_Verbal_Mark.Content = "-";
                        allPercentages.Remove(6);
                        break;

                    default:
                        break;
                }
            }
        }

        public void oralAssessmentEnabler(object sender, SelectionChangedEventArgs e)
        {
            student_Verbal_Procent.IsEnabled = true;
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
                        allPercentages.Add(1);
                        break;

                    case "student_Project_Procent":
                        student_Project_Mark.Content = grade;
                        string otherPart = student_Presentation_Procent.Text;
                        if (int.TryParse(otherPart, out int asNumber))
                        {
                            totalProjectPart(percentage, asNumber);
                            if (!allPercentages.Contains(2))
                            { allPercentages.Add(2); }
                        }
                        else
                        {
                            totalProjectPart(percentage, 0);
                            if (!allPercentages.Contains(2))
                            { allPercentages.Add(2); }
                        }
                        totalPartTwo();
                        break;

                    case "student_Presentation_Procent":
                        student_Presentation_Mark.Content = grade;
                        string projectPart = student_Project_Procent.Text;
                        if (int.TryParse(projectPart, out int partNumber))
                        {
                            totalProjectPart(percentage, partNumber);
                            if (!allPercentages.Contains(2))
                            { allPercentages.Add(2); }
                        }
                        else
                        {
                            totalProjectPart(percentage, 0);
                            if (!allPercentages.Contains(2))
                            { allPercentages.Add(2); };
                        }
                        totalPartTwo();
                        break;

                    case "student_Theory1_Procent":
                        student_Theory1_Mark.Content = grade;
                        allPercentages.Add(3);
                        totalPartTwo();
                        break;

                    case "student_Theory2_Procent":
                        student_Theory2_Mark.Content = grade;
                        allPercentages.Add(4);
                        totalPartTwo();
                        break;

                    case "student_Economy_Procent":
                        student_Economy_Mark.Content = grade;
                        allPercentages.Add(5);
                        totalPartTwo();
                        break;

                    case "student_Verbal_Procent":
                        student_Verbal_Mark.Content = grade;
                        allPercentages.Add(6);
                        totalPartTwo();
                        break;

                    default:
                        break;
                }
                if (allPercentages.Count >= 5)
                {
                    calculateTotal();
                }
            }
            else
            {
                MessageBox.Show("Diese Punktzahl befindet sich nicht im Bereich 0-100.");
            }
        }

        //todo: write methods to calculate all the different totals and decide where to call
        //deletes the focused Student from List
        public void removeStudent(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Student> students = new ObservableCollection<Student>();
            foreach (Student student in addedStudents)
            {
                if (student.studentId != focusedStudent.studentId)
                {
                    students.Add(student);
                }
            }
            foreach (TextBox box in allTextBoxes)
            {
                box.Text = "";
            }
            foreach (Label label in allFocusedLabels)
            {
                label.Content = "-";
            }
            selectedStudentName.Content = "";
            addedStudents = students;
            studentTable.ItemsSource = addedStudents;
        }

        //opens a .prf file to save a List of students
        public async void saveList(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            string listString = "";

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
                if (addedStudents.Count != 0)
                {
                    foreach (Student student in addedStudents)
                    {
                        string studentString = JsonSerializer.Serialize(student);
                        listString += studentString;
                        listString += ";";
                    }
                    string[] toRemove = new string[] { "\r", "\n" };
                    foreach (string charToRemove in toRemove)
                    {
                        listString = listString.Replace(charToRemove, string.Empty);
                    }
                    await using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine(listString);
                    }
                    MessageBox.Show("Prüflingsdaten wurden gespeichert.");
                }
                else
                {
                    await using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine(listString);
                    }
                    MessageBox.Show("Prüflingsdaten wurden gespeichert.");
                }
            }
        }

        //opens a .prf file to save a single Student into
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

                if (fileContent != "")
                {
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
                else
                {
                    Student.serializeStudentAsync(focusedStudent, filePath);
                    MessageBox.Show("Prüflingsdaten wurden gespeichert.");
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

        private void totalProjectPart(int percentProject, int percentPresentation)
        {
            double total = (percentProject + percentPresentation) / 2;
            int totalRounded = (int)Math.Round(total);
            student_Project_Total_Procent.Content = totalRounded.ToString();
            int grade = Calculations.calculateGrade(totalRounded);
            student_Project_Total_Mark.Content = grade.ToString();
        }

        private void totalPartTwo()
        {
            string projectString = (string)student_Project_Total_Procent.Content;
            if (projectString != "-" && student_Theory1_Procent.Text != "" && student_Theory2_Procent.Text != "" && student_Economy_Procent.Text != "")
            {
                int project = int.Parse(projectString);
                int theoryOne = int.Parse(student_Theory1_Procent.Text);
                int theoryTwo = int.Parse(student_Theory2_Procent.Text);
                int wiPo = int.Parse(student_Economy_Procent.Text);
                double forTotal = (project + theoryOne + theoryTwo + wiPo) / 4;
                int total = (int)Math.Round(forTotal);
                student_Result_Procent.Content = total;
                int grade = Calculations.calculateGrade(total);
                student_Result_Mark.Content = grade;
            }
        }

        private void calculateTotal()
        {
            int partOne = int.Parse(student_AP_Teil_1_Procent.Text);
            string projectString = (string)student_Project_Total_Procent.Content;
            int project = int.Parse(projectString);
            int theoryOne = int.Parse(student_Theory1_Procent.Text);
            int theoryTwo = int.Parse(student_Theory2_Procent.Text);
            int wiPo = int.Parse(student_Economy_Procent.Text);

            int total = (int)Math.Round(partOne * 0.2 + project * 0.5 + theoryOne * 0.1 + theoryTwo * 0.1 + wiPo * 0.1);
            student_Total_Procent.Content = total;
            int grade = Calculations.calculateGrade(total);
            student_Total_Mark.Content = grade;
            bool totalMarkGoodEnough = grade <= 4;
            bool partTwoGoodEnough = int.Parse(student_Result_Mark.Content.ToString()) <= 4;
            Label[] marksPartTwo = new Label[] { student_Project_Total_Mark, student_Theory1_Mark, student_Theory2_Mark, student_Economy_Mark };
            int counter = 0;
            foreach (Label label in marksPartTwo)
            {
                if (label.Content is int)
                {
                    int asNumber = (int)label.Content;
                    if (asNumber >= 5)
                    {
                        counter++;
                    }
                }
            }
            bool notTooManyFivesInPartTwo = counter <= 1;
            if (totalMarkGoodEnough && partTwoGoodEnough && notTooManyFivesInPartTwo)
            {
                grade_As_Word.Content = "Bestanden";
            }
            else
            {
                grade_As_Word.Content = "Nicht Bestanden";
            }
        }

        private void calcWithOralAssessment(object sender, RoutedEventArgs e)
        {
            string selectedSubject = (string)oralAssessment.SelectedValue;
            TextBox tb = sender as TextBox;
            bool isNumber = int.TryParse(tb.Text, out int percentage);
            if (isNumber)
            {
                int grade = Calculations.calculateGrade(percentage);
                student_Verbal_Mark.Content = grade;
                foreach (TextBox box in allTextBoxes)
                {
                    if (box.Name == selectedSubject)
                    {
                        int currentPercentage = int.Parse(box.Text);
                        int newPercentage = Calculations.newPercentage(currentPercentage, percentage);
                        box.Text = newPercentage.ToString();
                        int newGrade = Calculations.calculateGrade(newPercentage);

                        switch (box.Name)
                        {
                            case "student_Theory1_Procent":
                                student_Theory1_Mark.Content = newGrade;
                                break;

                            case "student_Theory2_Procent":
                                student_Theory2_Mark.Content = newGrade;
                                break;

                            case "student_Economy_Procent":
                                student_Economy_Mark.Content = newGrade;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void selectedStudentModifyButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}