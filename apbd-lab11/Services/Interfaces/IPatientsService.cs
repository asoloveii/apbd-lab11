using apbd_lab11.DTOs;

namespace apbd_lab11.Services.Interfaces;

public interface IPatientsService
{
    Task<PatientDetailsResponse> GetPatientDetailsAsync(int id);
}