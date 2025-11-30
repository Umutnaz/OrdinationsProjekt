using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using shared.Model;
using static shared.Util;
using Data;

namespace Service;

public class DataService
{
    private OrdinationContext db { get; }

    public DataService(OrdinationContext db) {
        this.db = db;
    }

    /// <summary>
    /// Seeder noget nyt data i databasen, hvis det er nødvendigt.
    /// </summary>
    public void SeedData() {

        // Patients
        Patient[] patients = new Patient[5];
        patients[0] = db.Patienter.FirstOrDefault()!;

        if (patients[0] == null)
        {
            patients[0] = new Patient("121256-0512", "Jane Jensen", 63.4);
            patients[1] = new Patient("070985-1153", "Finn Madsen", 83.2);
            patients[2] = new Patient("050972-1233", "Hans Jørgensen", 89.4);
            patients[3] = new Patient("011064-1522", "Ulla Nielsen", 59.9);
            patients[4] = new Patient("123456-1234", "Ib Hansen", 87.7);

            db.Patienter.Add(patients[0]);
            db.Patienter.Add(patients[1]);
            db.Patienter.Add(patients[2]);
            db.Patienter.Add(patients[3]);
            db.Patienter.Add(patients[4]);
            db.SaveChanges();
        }

        Laegemiddel[] laegemiddler = new Laegemiddel[5];
        laegemiddler[0] = db.Laegemiddler.FirstOrDefault()!;
        if (laegemiddler[0] == null)
        {
            laegemiddler[0] = new Laegemiddel("Acetylsalicylsyre", 0.1, 0.15, 0.16, "Styk");
            laegemiddler[1] = new Laegemiddel("Paracetamol", 1, 1.5, 2, "Ml");
            laegemiddler[2] = new Laegemiddel("Fucidin", 0.025, 0.025, 0.025, "Styk");
            laegemiddler[3] = new Laegemiddel("Methotrexat", 0.01, 0.015, 0.02, "Styk");
            laegemiddler[4] = new Laegemiddel("Prednisolon", 0.1, 0.15, 0.2, "Styk");

            db.Laegemiddler.Add(laegemiddler[0]);
            db.Laegemiddler.Add(laegemiddler[1]);
            db.Laegemiddler.Add(laegemiddler[2]);
            db.Laegemiddler.Add(laegemiddler[3]);
            db.Laegemiddler.Add(laegemiddler[4]);

            db.SaveChanges();
        }

        Ordination[] ordinationer = new Ordination[6];
        ordinationer[0] = db.Ordinationer.FirstOrDefault()!;
        if (ordinationer[0] == null) {
            Laegemiddel[] lm = db.Laegemiddler.ToArray();
            Patient[] p = db.Patienter.ToArray();

            ordinationer[0] = new PN(new DateTime(2025, 1, 1), new DateTime(2025, 1, 12), 123, lm[1]);    
            ordinationer[1] = new PN(new DateTime(2025, 2, 12), new DateTime(2025, 2, 14), 3, lm[0]);    
            ordinationer[2] = new PN(new DateTime(2025, 1, 20), new DateTime(2025, 1, 25), 5, lm[2]);    
            ordinationer[3] = new PN(new DateTime(2025, 1, 1), new DateTime(2025, 1, 12), 123, lm[1]);
            ordinationer[4] = new DagligFast(new DateTime(2025, 1, 10), new DateTime(2025, 1, 12), lm[1], 2, 0, 1, 0);
            ordinationer[5] = new DagligSkæv(new DateTime(2025, 1, 23), new DateTime(2025, 1, 24), lm[2]);
            
            ((DagligSkæv) ordinationer[5]).doser = new Dosis[] { 
                new Dosis(CreateTimeOnly(12, 0, 0), 0.5),
                new Dosis(CreateTimeOnly(12, 40, 0), 1),
                new Dosis(CreateTimeOnly(16, 0, 0), 2.5),
                new Dosis(CreateTimeOnly(18, 45, 0), 3)        
            }.ToList();
            

            db.Ordinationer.Add(ordinationer[0]);
            db.Ordinationer.Add(ordinationer[1]);
            db.Ordinationer.Add(ordinationer[2]);
            db.Ordinationer.Add(ordinationer[3]);
            db.Ordinationer.Add(ordinationer[4]);
            db.Ordinationer.Add(ordinationer[5]);

            db.SaveChanges();

            p[0].ordinationer.Add(ordinationer[0]);
            p[0].ordinationer.Add(ordinationer[1]);
            p[2].ordinationer.Add(ordinationer[2]);
            p[3].ordinationer.Add(ordinationer[3]);
            p[1].ordinationer.Add(ordinationer[4]);
            p[1].ordinationer.Add(ordinationer[5]);

            db.SaveChanges();
        }
    }

    
    public List<PN> GetPNs() {
        return db.PNs.Include(o => o.laegemiddel).Include(o => o.dates).ToList();
    }

    public List<DagligFast> GetDagligFaste() {
        return db.DagligFaste
            .Include(o => o.laegemiddel)
            .Include(o => o.MorgenDosis)
            .Include(o => o.MiddagDosis)
            .Include(o => o.AftenDosis)            
            .Include(o => o.NatDosis)            
            .ToList();
    }

    public List<DagligSkæv> GetDagligSkæve() {
        return db.DagligSkæve
            .Include(o => o.laegemiddel)
            .Include(o => o.doser)
            .ToList();
    }

    public List<Patient> GetPatienter() {
        return db.Patienter.Include(p => p.ordinationer).ToList();
    }

    public List<Laegemiddel> GetLaegemidler() {
        return db.Laegemiddler.ToList();
    }

    public PN OpretPN(int patientId, int laegemiddelId, double antal, DateTime startDato, DateTime slutDato) {
        
            if (patientId < 0 || laegemiddelId < 0)
                throw new ArgumentOutOfRangeException("fejl");

            if (antal <= 0)
                throw new ArgumentOutOfRangeException("skal være over 0");

            if (startDato < DateTime.Now.AddDays(-1) || slutDato < DateTime.Now
                                                     || startDato > slutDato)
                throw new ArgumentOutOfRangeException("startdate må ikke være slutdate");
            
            Patient? patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);
            if (patient == null)
                throw new ArgumentException($"patientId {patientId} findes ikke");

            Laegemiddel? laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);
            if (laegemiddel == null)
                throw new ArgumentException($"LægemiddelId {laegemiddelId} findes ikke");
            
            PN pn = new PN(startDato, slutDato, antal, laegemiddel);
            
            db.PNs.Add(pn);
            db.SaveChanges();
        
            patient.ordinationer.Add(pn);
            
            db.SaveChanges();
            return pn;

        return null!;
    }

    public DagligFast OpretDagligFast(int patientId, int laegemiddelId, 
        double antalMorgen, double antalMiddag, double antalAften, double antalNat, 
        DateTime startDato, DateTime slutDato) {

        // Parameter check til OpretDagligFast
        // Er id'erne korrekt? Er doserne korrekt? Er tidspunkt korrekt?
        if (patientId < 1 || laegemiddelId < 1)
        {
            Console.WriteLine("patientId or laegemiddelId is invalid");
            throw new ArgumentOutOfRangeException();
        }

        if (antalMorgen < 0 || antalMiddag < 0 || antalAften < 0 || antalNat < 0)
        {
            Console.WriteLine("Dosis range is negative");
            throw new ArgumentOutOfRangeException();
        }

        if (startDato < DateTime.Now.AddDays(-1) || slutDato < DateTime.Now
                                     || startDato > slutDato)
        {
            Console.WriteLine("Date ranges are invalid");
            throw new ArgumentOutOfRangeException();
        }
        
        // Checker om patient og lægemiddel eksisterer
        Patient? patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);
        if (patient == null)
        {
            throw new NullReferenceException($"Patient with id {patientId} not found");
        }
        Laegemiddel? laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);
        if (laegemiddel == null)
        {
            throw new NullReferenceException($"Laegemiddel with id {laegemiddelId} not found");
        }
        DagligFast dagligFast = new DagligFast(startDato, slutDato, laegemiddel, antalMorgen, antalMiddag, antalAften, antalNat);
        
        patient.ordinationer.Add(dagligFast);
        db.SaveChanges();
        
        return dagligFast;
    }

    public DagligSkæv OpretDagligSkaev(int patientId, int laegemiddelId, Dosis[] doser, DateTime startDato, DateTime slutDato) {
        if (patientId < 0)
        {
            Console.WriteLine("patientId is invalid");
            throw new ArgumentOutOfRangeException();
        }
        Laegemiddel? laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);
        if (laegemiddel == null)
        {
            throw new ArgumentException($"Laegemiddel with id {laegemiddelId} not found");
        }
        if (laegemiddelId < 0)
        {
            Console.WriteLine("laegemiddelId is invalid");
            throw new ArgumentOutOfRangeException();
        }
        if (startDato < DateTime.Now.AddDays(-1) || slutDato < DateTime.Now
                                                 || startDato > slutDato)
        {
            Console.WriteLine("Date ranges are invalid");
            throw new ArgumentOutOfRangeException();
        }
        Patient? patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);
        if (patient == null)
        {
            throw new ArgumentException($"Patient with id {patientId} not found");
        }
        if (doser == null) //tester om listen er null eller tom
        {
            Console.WriteLine("doser.list is null");
            throw new ArgumentException("Doser-list is null");
            
        }

        if (doser.Length == 0) //tester om listen er tom
        {
            Console.WriteLine("doser.list is empty");
            throw new ArgumentException("Doser-list must contain at least one Dosis");
        }
        if (doser.Any(d => d == null)) //tester om listen indeholder et null element
        {
            Console.WriteLine("doser contains null elements");
            throw new ArgumentException("Each Dosis in the doser-list must be non-null");
        }

        DagligSkæv dagligSkaev = new DagligSkæv(startDato, slutDato, laegemiddel); //opretter den nye skæve dosering
        dagligSkaev.doser = doser.ToList(); //tilføjer doserne til den skæve dosering
        patient.ordinationer.Add(dagligSkaev); //tilføjer den skæve dosering til patienten til dosisen, så når man under linjen her, sætter dagligskæv ind i db så ryger dens forbindelse til patienten med
        db.DagligSkæve.Add(dagligSkaev); //tilføjer den skæve dosering objekt til databasen
        db.SaveChanges();
        return dagligSkaev;
    }

    public string AnvendOrdination(int id, Dato dato)
    {
        
        PN? pn = db.PNs.Include(p => p.dates).FirstOrDefault(p => p.OrdinationId == id);

        if (pn == null)
        {
            throw new NullReferenceException($"PN with id {id} not found");
        }
        
        DateTime pnDato = dato.dato;
        if (pn.startDen > pnDato || pn.slutDen < pnDato)
        {
            throw new ArgumentOutOfRangeException();
        }
        
        pn.dates.Add(dato);
        db.SaveChanges();
        return "tilføjet";
    }

    public OrdinationVaegtDTO? GetOrdinationer(int vaegtFra, int vaegtTil, int laegemiddelId)
    {
        // Filtrer efter vægt
        List<Patient> patienterMedRigtigVægt = db.Patienter.Include(p => p.ordinationer).ThenInclude(p => p.laegemiddel).Where(p => p.vaegt >= vaegtFra && p.vaegt <= vaegtTil).ToList();
        if (patienterMedRigtigVægt.Count <= 0)
        {
            return null;
        }
        
        // Find ordinationer med rigtige lægemiddel-navn, kobl sammen med patient vægt
        List<PNVaegtDTO> pns = new List<PNVaegtDTO>();
        List<DagligFastVaegtDTO> fast = new List<DagligFastVaegtDTO>();
        List<DagligSkævVaegtDTO> skæv = new List<DagligSkævVaegtDTO>();
        
        foreach (Patient patient in patienterMedRigtigVægt)
        {
            foreach (Ordination ordination in patient.ordinationer)
            {
                if (ordination.laegemiddel.LaegemiddelId == laegemiddelId)
                {
                    switch (ordination.getType())
                    {
                        case "PN":
                            PNVaegtDTO pn = new PNVaegtDTO()
                            {
                                PNOrdination = (PN)ordination,
                                Vaegt = patient.vaegt,
                            };
                            pns.Add(pn);
                            break;
                        case "DagligSkæv":
                            DagligSkævVaegtDTO sk = new DagligSkævVaegtDTO()
                            {
                                DagligSkævOrdination = (DagligSkæv)ordination,
                                Vaegt = patient.vaegt,
                            };
                            skæv.Add(sk);
                            break;
                        case "DagligFast":
                            DagligFastVaegtDTO dag = new DagligFastVaegtDTO()
                            {
                                DagligFastOrdination = (DagligFast)ordination,
                                Vaegt = patient.vaegt,
                            };
                            fast.Add(dag);
                            break;
                        default:
                            Console.WriteLine("Unknown ordination type");
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
        if (pns.Count <= 0 &&  fast.Count <= 0 && skæv.Count <= 0)
            return null;
        
        var dto = new OrdinationVaegtDTO()
        {
            PNOrdinationer = pns,
            DagligFastOrdinationer = fast,
            DagligSkævOrdinationer = skæv
        };
        return dto;
    }

    /// <summary>
    /// Den anbefalede dosis for den pågældende patient, per døgn, hvor der skal tages hensyn til
	/// patientens vægt. Enheden afhænger af lægemidlet. Patient og lægemiddel må ikke være null.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="laegemiddel"></param>
    /// <returns></returns>
	public double GetAnbefaletDosisPerDøgn(int patientId, int laegemiddelId) {
        if (patientId < 0)
        {
            Console.WriteLine("patientId is invalid");
            throw new ArgumentOutOfRangeException();
        }
        Laegemiddel? laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);
        if (laegemiddel == null)
        {
            throw new ArgumentException($"Laegemiddel with id {laegemiddelId} not found");
        }
        Patient? patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);
        if (patient == null)
        {
            throw new ArgumentException($"Patient with id {patientId} not found");
        }

        if (patient.vaegt < 25)
        {
            return laegemiddel.enhedPrKgPrDoegnLet * patient.vaegt;
        }

        if (25 <= patient.vaegt && patient.vaegt <= 120)
        {
            return laegemiddel.enhedPrKgPrDoegnNormal * patient.vaegt;
        }

        if (patient.vaegt > 120)
        {
            return laegemiddel.enhedPrKgPrDoegnTung * patient.vaegt;
        }

        return -1;
    }
    
}