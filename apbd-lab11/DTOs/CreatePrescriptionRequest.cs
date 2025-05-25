namespace apbd_lab11.DTOs;

public class CreatePrescriptionRequest
{
    public PatientDto Patient { get; set; }
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public List<PrescriptionMedicamentDto> Medicaments { get; set; }
}

public class PatientDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
}

public class PrescriptionMedicamentDto
{
    public int MedicamentId { get; set; }
    public int Dose { get; set; }
    public string Description { get; set; }
}
