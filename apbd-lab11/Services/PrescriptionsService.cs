using apbd_lab11.DTOs;
using apbd_lab11.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace apbd_lab11.Services;

public class PrescriptionsService : IPrescriptionsService
{
    private readonly string _connectionString;

    public PrescriptionsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task AddPrescriptionAsync(CreatePrescriptionRequest request)
    {
        if (request.Medicaments.Count > 10)
            throw new ArgumentException("Max 10 medications allowed.");

        if (request.DueDate < request.Date)
            throw new ArgumentException("DueDate must be >= Date.");
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            // check doctor
            var cmdDoctor = new SqlCommand("SELECT COUNT(*) FROM Doctor WHERE IdDoctor = @Id", connection, transaction);
            cmdDoctor.Parameters.AddWithValue("@Id", request.DoctorId);
            var doctorExists = (int)await cmdDoctor.ExecuteScalarAsync() > 0;
            if (!doctorExists) throw new ArgumentException("Doctor not found");

            // insert patient
            int patientId;
            var cmdFindPatient = new SqlCommand(
                @"SELECT IdPatient FROM Patient WHERE FirstName = @FirstName AND LastName = @LastName AND Birthdate = @BirthDate",
                connection, transaction);
            cmdFindPatient.Parameters.AddWithValue("@FirstName", request.Patient.FirstName);
            cmdFindPatient.Parameters.AddWithValue("@LastName", request.Patient.LastName);
            cmdFindPatient.Parameters.AddWithValue("@BirthDate", request.Patient.BirthDate);

            var result = await cmdFindPatient.ExecuteScalarAsync();
            if (result == null)
            {
                var cmdInsertPatient = new SqlCommand(
                    @"INSERT INTO Patient (FirstName, LastName, BirthDate) 
                      OUTPUT INSERTED.IdPatient
                      VALUES (@FirstName, @LastName, @BirthDate)",
                    connection, transaction);
                cmdInsertPatient.Parameters.AddWithValue("@FirstName", request.Patient.FirstName);
                cmdInsertPatient.Parameters.AddWithValue("@LastName", request.Patient.LastName);
                cmdInsertPatient.Parameters.AddWithValue("@BirthDate", request.Patient.BirthDate);
                patientId = (int)await cmdInsertPatient.ExecuteScalarAsync();
            }
            else
            {
                patientId = (int)result;
            }

            // check medications exist
            foreach (var med in request.Medicaments)
            {
                var cmdMed = new SqlCommand("SELECT COUNT(*) FROM Medicament WHERE IdMedicament = @Id", connection, transaction);
                cmdMed.Parameters.AddWithValue("@Id", med.MedicamentId);
                var exists = (int)await cmdMed.ExecuteScalarAsync() > 0;
                if (!exists)
                    throw new ArgumentException($"Medicament ID {med.MedicamentId} not found.");
            }

            // insert prescription
            var cmdInsertPrescription = new SqlCommand(
                @"INSERT INTO Prescription (Date, DueDate, IdPatient, IdDoctor)
                  OUTPUT INSERTED.IdPrescription
                  VALUES (@Date, @DueDate, @IdPatient, @IdDoctor)",
                connection, transaction);
            cmdInsertPrescription.Parameters.AddWithValue("@Date", request.Date);
            cmdInsertPrescription.Parameters.AddWithValue("@DueDate", request.DueDate);
            cmdInsertPrescription.Parameters.AddWithValue("@IdPatient", patientId);
            cmdInsertPrescription.Parameters.AddWithValue("@IdDoctor", request.DoctorId);

            var prescriptionId = (int)await cmdInsertPrescription.ExecuteScalarAsync();

            // insert prescription-medications
            foreach (var med in request.Medicaments)
            {
                var cmdInsertMed = new SqlCommand(
                    @"INSERT INTO Prescription_Medicament (IdMedicament, IdPrescription, Dose, Details)
                      VALUES (@IdMedicament, @IdPrescription, @Dose, @Details)",
                    connection, transaction);
                cmdInsertMed.Parameters.AddWithValue("@IdMedicament", med.MedicamentId);
                cmdInsertMed.Parameters.AddWithValue("@IdPrescription", prescriptionId);
                cmdInsertMed.Parameters.AddWithValue("@Dose", med.Dose);
                cmdInsertMed.Parameters.AddWithValue("@Details", med.Description);
                await cmdInsertMed.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}