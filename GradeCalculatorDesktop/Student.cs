using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace GradeCalculatorDesktop
{
    public class Student
    {
        public int studentId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public int specialty { get; set; }
        public GradeData? gradeData { get; set; }

        public static async void serializeStudentAsync(Student student, string filePath)
        {
            await using (StreamWriter writer = File.AppendText(filePath))
            {
                String studentString = JsonSerializer.Serialize<Student>(student);
                writer.WriteLine(studentString + ";");
            }
        }

    }
}
