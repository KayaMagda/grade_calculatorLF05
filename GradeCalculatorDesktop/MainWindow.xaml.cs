using System;
using System.Collections.Generic;
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

namespace GradeCalculatorDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Student focusedStudent;
        Student[] addedStudents;
        Specialties specialties;

        public MainWindow()
        {
            InitializeComponent();

             public Dictionary<int, string> allSpecialties = new Dictionary<int, string>()
             {
                 [0] = "IT-System-Elektroniker_in",
                 [1] = "Kaufmann/ Kauffrau für Digitalisierungsmanagement",
                 [2] = "Kaufmann/ Kauffrau für IT-System-Management",
                 [3] = "Fachinformatiker_in/ Anwendungsentwicklung",
                 [4] = "Fachinformatiker_in/ Systemintegration",
                 [5] = "Fachinformatiker_in/ Digitale Vernetzung"
             };

        //adds a new oneStudentRow to the studentTable
        //public addStudentRow

        //adds specialties to ComboBox specialtyFilter


    }
    }
}
