using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeCalculatorDesktop
{
    public class Specialties
    {
        public int specialtyId { get; set; }
        public string specialtyName { get; set; }
        public string[] specialtyVariableAssesments { get; set; }

        public specialty(int id, string name, string[] assessments)
        {
            specialtyId = id;
            specialtyName = name;
            specialtyVariableAssesments = assessments;
        }

    }
}
