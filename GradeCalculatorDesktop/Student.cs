using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GradeCalculatorDesktop
{
    public class Student
    {
        public int studentId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public int specialty { get; set; }
        public GradeData? gradeData { get; set; }

        public void serializeStudent(Student student, FileStream file)
        {

        }

    }
}
