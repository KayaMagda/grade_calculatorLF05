
namespace GradeCalculatorDesktop
{
    public class Specialty
    {
        public int specialtyId { get; set; }
        public string specialtyName { get; set; }
        public string[] specialtyVariableAssesments { get; set; }

        public Specialty(int id, string name, string[] assessments)
        {
            specialtyId = id;
            specialtyName = name;
            specialtyVariableAssesments = assessments;
        }      
        

    }
}