using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeCalculatorDesktop
{
    public class GradeData
    {
        public int? percentagePB1 { get; set; }
        public int? percentageProject { get; set; }
        public int? percentagePresentation { get; set; }
        public int? percentageVariableOne { get; set; }
        public int? percentageVariableTwo { get; set; }
        public int? percentageWiSo { get; set; }
        public int? percentageOralAssessment { get; set; }

        static int calculateGrade(int percentage)
        {
            return 0;
        }

    }
}
