namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
    }

    [TestMethod]
    public void InvalidDagligFastParametre()
    {
        int patientId = -1;
        int laegemiddelId = 1;
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.OpretDagligFast(patientId, laegemiddelId, 1, 1,
            1, 1, DateTime.Today.AddDays(1),DateTime.Now.AddDays(2)));
        
        patientId = 1;
        laegemiddelId = -1;
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.OpretDagligFast(patientId, laegemiddelId, 1, 1,
            1, 1, DateTime.Today.AddDays(1),DateTime.Now.AddDays(2)));
        
        DateTime invalidStart = DateTime.Now.AddDays(-1);
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.OpretDagligFast(patientId, laegemiddelId, 1, 1,
            1, 1, invalidStart,DateTime.Now.AddDays(2)));
        
        DateTime validStart = DateTime.Now.AddDays(5);
        DateTime invalidEnd = DateTime.Now.AddDays(4);
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.OpretDagligFast(patientId, laegemiddelId, 1, 1,
            1, 1, validStart,invalidEnd));

        double invalidDosis = -1;
        
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.OpretDagligFast(patientId, laegemiddelId, 1, invalidDosis,
            1, 1, DateTime.Today.AddDays(1),DateTime.Now.AddDays(2)));

    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    {
        // Herunder skal man så kalde noget kode,
        // der smider en exception.

        // Hvis koden _ikke_ smider en exception,
        // så fejler testen.

        Console.WriteLine("Her kommer der ikke en exception. Testen fejler.");
    }
}