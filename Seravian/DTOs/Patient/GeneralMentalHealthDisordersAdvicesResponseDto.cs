namespace Seravian.DTOs.Patient
{
    public class GeneralMentalHealthDisordersAdvicesResponseDto
    {
        public int Id { get; set; }
        public string Disorder { get; set; }
        public List<string> Advices { get; set; }
    }
}
