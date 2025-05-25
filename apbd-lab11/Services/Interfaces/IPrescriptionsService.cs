using apbd_lab11.DTOs;

namespace apbd_lab11.Services.Interfaces;

public interface IPrescriptionsService
{
    Task AddPrescriptionAsync(CreatePrescriptionRequest prescription);
}