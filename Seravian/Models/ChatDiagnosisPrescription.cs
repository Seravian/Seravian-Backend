public class ChatDiagnosisPrescription
{
    public long Id { get; set; }
    public long ChatDiagnosisId { get; set; }
    public int OrderIndex { get; set; }
    public string Content { get; set; }
    public virtual ChatDiagnosis ChatDiagnosis { get; set; }
}
