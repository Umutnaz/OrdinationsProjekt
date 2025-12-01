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
    public void InvalidIdAnvendOrdination()
    {
        int id = -1;
        Dato dato = new Dato { dato = DateTime.Now };

        Assert.ThrowsException<NullReferenceException>(() => service.AnvendOrdination(id, dato));
    }

    [TestMethod]
    public void InvalIdDatoAnvendOrdination()
    {
        PN pn = new PN
        {
            OrdinationId = 1,
            startDen = new DateTime(2025, 11, 25),
            slutDen = new DateTime(2025, 11, 28),
            dates = new List<Dato>()
        };
        Dato dato = new Dato { dato = DateTime.Now };

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.AnvendOrdination(pn.OrdinationId, dato));
    }
    
    [TestMethod]
    public void PatientEllerLaegemiddelEksistererIkkeDagligFast()
    {
        int patientId = int.MaxValue;
        int laegemiddelId = int.MaxValue;
        DateTime validStart = DateTime.Now.AddDays(1);
        DateTime validEnd = DateTime.Now.AddDays(2);
        
        // PatientId
        Assert.ThrowsException<NullReferenceException>(()=> service.OpretDagligFast(patientId, 1, 2, 2, 2, 2, validStart, validEnd));
        // laegemiddelId
        Assert.ThrowsException<NullReferenceException>(()=> service.OpretDagligFast(1, laegemiddelId, 2, 2, 2, 2, validStart, validEnd));
    }
    
    [TestMethod]
    public void StartDatoLigMedSlutDatoDagligFast()
    {
        int patientId = 1;
        int laegemiddelId = 1;
        DateTime startDato = DateTime.Now.AddDays(1);
        DateTime endDato = DateTime.Now.AddDays(1);
        
        int count = service.GetDagligFaste().Count();
        
        service.OpretDagligFast(patientId, laegemiddelId, 1, 1, 1, 1,  startDato, endDato);
        
        Assert.AreEqual(count + 1, service.GetDagligFaste().Count());
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void TestAtKodenSmiderEnException()
    {
        int patientId = int.MaxValue;
        int laegemiddelId = int.MaxValue;
        DateTime validStart = DateTime.Now.AddDays(1);
        DateTime validEnd = DateTime.Now.AddDays(2);
        
        // PatientId
        service.OpretDagligFast(patientId, 1, 2, 2, 2, 2, validStart, validEnd);
    }
    [TestMethod]
public void PatientIdUnderNulDagligSkaev() //fejl for patientId < 0
{
    
    int patientId = -1;
    Laegemiddel lm = service.GetLaegemidler().First();
    Dosis[] doser = new Dosis[] { new Dosis() };
    DateTime startDato = DateTime.Now.AddDays(1);
    DateTime slutDato = DateTime.Now.AddDays(2);

    
    Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        service.OpretDagligSkaev(patientId, lm.LaegemiddelId, doser, startDato, slutDato));
}

[TestMethod]
public void LaegemiddelEksistererIkkeDagligSkaev() //fejl for laegemiddel findes ikke
{
    
    Patient patient = service.GetPatienter().First();
    int laegemiddelId = int.MaxValue; // findes ikke
    Dosis[] doser = new Dosis[] { new Dosis() };
    DateTime startDato = DateTime.Now.AddDays(1);
    DateTime slutDato = DateTime.Now.AddDays(2);

    
    Assert.ThrowsException<ArgumentException>(() =>
        service.OpretDagligSkaev(patient.PatientId, laegemiddelId, doser, startDato, slutDato));
}

[TestMethod]
public void LaegemiddelIdUnderNulDagligSkaev() //fejl for laegemiddelId < 0
{
    
    Patient patient = service.GetPatienter().First();
    int laegemiddelId = -1;
    Dosis[] doser = new Dosis[] { new Dosis() };
    DateTime startDato = DateTime.Now.AddDays(1);
    DateTime slutDato = DateTime.Now.AddDays(2);

    
    Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        service.OpretDagligSkaev(patient.PatientId, laegemiddelId, doser, startDato, slutDato));
}

[TestMethod]
public void UgyldigtDatoIntervalDagligSkaev() //fejl for startDato > slutDato
{
    
    Patient patient = service.GetPatienter().First();
    Laegemiddel lm = service.GetLaegemidler().First();
    Dosis[] doser = new Dosis[] { new Dosis() };
    DateTime startDato = DateTime.Now.AddDays(5);
    DateTime slutDato = DateTime.Now.AddDays(3); // startDato > slutDato

    
    Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, startDato, slutDato));
}

[TestMethod]
public void PatientEksistererIkkeDagligSkaev() //fejl for patient findes ikke
{
    
    int patientId = int.MaxValue; // findes ikke
    Laegemiddel lm = service.GetLaegemidler().First();
    Dosis[] doser = new Dosis[] { new Dosis() };
    DateTime startDato = DateTime.Now.AddDays(1);
    DateTime slutDato = DateTime.Now.AddDays(2);

    
    Assert.ThrowsException<ArgumentException>(() =>
        service.OpretDagligSkaev(patientId, lm.LaegemiddelId, doser, startDato, slutDato));
}

[TestMethod]
public void DoserErNullDagligSkaev() //fejl for doser er null
{
    
    Patient patient = service.GetPatienter().First();
    Laegemiddel lm = service.GetLaegemidler().First();
    Dosis[] doser = null;
    DateTime startDato = DateTime.Now.AddDays(1);
    DateTime slutDato = DateTime.Now.AddDays(2);

    
    Assert.ThrowsException<ArgumentException>(() =>
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, startDato, slutDato));
}

}
