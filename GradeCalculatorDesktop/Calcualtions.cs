namespace GradeCalculatorDesktop
{
    public class Calculations
    {
        //calculate Grade from given percentage
        //100-92 = 1, 91-81 = 2, 80 - 67 = 3, 66 - 50 = 4, 49 - 30 = 5, 29 - 0 = 6
        public static int calculateGrade(int points)
        {
            if (points >= 0 && points <= 29)
            {
                return 6;
            } else if (points >= 30 && points <= 49)
            {
                return 5;
            } else if (points >= 50 && points <= 66)
            {
                return 4;
            } else if (points >= 67 && points <= 80)
            {
                return 3;
            } else if (points >= 81 && points <= 91)
            {
                return 2;
            } else
            {
                return 1;
            }
        }
        
}