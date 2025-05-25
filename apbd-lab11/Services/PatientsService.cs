using apbd_lab11.DTOs;
using apbd_lab11.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace apbd_lab11.Services;

public class PatientsService : IPatientsService
{
    private readonly string _connectionString;

    public PatientsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");;
    }
    
    public async Task<PatientDetailsResponse> GetPatientDetailsAsync(int patientId)
    {
        var response = new PatientDetailsResponse
        {
            Prescriptions = new List<PrescriptionDto>()
        };

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // get patient
        var cmdPatient = new SqlCommand(
            @"SELECT FirstName, LastName FROM Patient WHERE IdPatient = @Id",
            connection);
        cmdPatient.Parameters.AddWithValue("@Id", patientId);
        using var reader = await cmdPatient.ExecuteReaderAsync();

        if (!reader.Read()) return null;

        response.IdPatient = patientId;
        response.FirstName = reader.GetString(0);
        response.LastName = reader.GetString(1);
        await reader.CloseAsync();

        // get prescriptions
        var cmdPres = new SqlCommand(@"
            SELECT p.IdPrescription, p.Date, p.DueDate, d.IdDoctor, d.FirstName AS DoctorFirstName
            FROM Prescription p
            JOIN Doctor d ON d.IdDoctor = p.IdDoctor
            WHERE p.IdPatient = @Id
            ORDER BY p.DueDate", connection);
        cmdPres.Parameters.AddWithValue("@Id", patientId);
        using var presReader = await cmdPres.ExecuteReaderAsync();

        var prescriptions = new List<PrescriptionDto>();
        while (await presReader.ReadAsync())
        {
            prescriptions.Add(new PrescriptionDto
            {
                IdPrescription = presReader.GetInt32(0),
                Date = presReader.GetDateTime(1),
                DueDate = presReader.GetDateTime(2),
                Doctor = new DoctorDto
                {
                    IdDoctor = presReader.GetInt32(3),
                    FirstName = presReader.GetString(4)
                },
                Medicaments = new List<MedicamentDto>()
            });
        }
        await presReader.CloseAsync();

        foreach (var pres in prescriptions)
        {
            var cmdMed = new SqlCommand(@"
                SELECT m.IdMedicament, m.Name, pm.Dose, pm.Details
                FROM Prescription_Medicament pm
                JOIN Medicament m ON m.IdMedicament = pm.IdMedicament
                WHERE pm.IdPrescription = @Id", connection);
            cmdMed.Parameters.AddWithValue("@Id", pres.IdPrescription);

            using var medReader = await cmdMed.ExecuteReaderAsync();
            while (await medReader.ReadAsync())
            {
                pres.Medicaments.Add(new MedicamentDto
                {
                    IdMedicament = medReader.GetInt32(0),
                    Name = medReader.GetString(1),
                    Dose = medReader.GetInt32(2),
                    Description = medReader.GetString(3)
                });
            }
            await medReader.CloseAsync();
        }

        response.Prescriptions = prescriptions;
        return response;
    }
}