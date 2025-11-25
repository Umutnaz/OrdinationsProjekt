namespace shared.Model;

public class PN : Ordination {
	public double antalEnheder { get; set; }
    public List<Dato> dates { get; set; } = new List<Dato>(); //Logger alle dage der er blevet taget, også de samme dage.

    public PN (DateTime startDen, DateTime slutDen, double antalEnheder, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
		this.antalEnheder = antalEnheder;
	}

    public PN() : base(null!, new DateTime(), new DateTime()) {
    }

    /// <summary>
    /// Registrerer at der er givet en dosis på dagen givesDen
    /// Returnerer true hvis givesDen er inden for ordinationens gyldighedsperiode og datoen huskes
    /// Returner false ellers og datoen givesDen ignoreres
    /// </summary>
    public bool givDosis(Dato givesDen) {
        // TODO: Implement!
        return false;
    }

    public override double doegnDosis() {
	    if (dates.Count == 0) return 0;

	    var firstDate = dates.Min(d => d.dato).Date;
	    var lastDate  = dates.Max(d => d.dato).Date;

	    int antalDage = (lastDate - firstDate).Days + 1; // begge dage med

	    return (dates.Count * antalEnheder) / antalDage;
    } 



    public override double samletDosis() {
        return dates.Count() * antalEnheder;
    }

    public int getAntalGangeGivet() {
        return dates.Count();
    }

	public override String getType() {
		return "PN";
	}
}
