namespace shared.Model;

public class OrdinationVaegtDTO
{
    public List<PNVaegtDTO> PNOrdinationer  { get; set; }
    public List<DagligFastVaegtDTO> DagligFastOrdinationer { get; set; }
    public List<DagligSkævVaegtDTO> DagligSkævOrdinationer { get; set; }
}

public class PNVaegtDTO
{
    public PN PNOrdination {get; set;}
    public double Vaegt { get; set; }
}

public class DagligSkævVaegtDTO
{
    public DagligSkæv DagligSkævOrdination {get; set;}
    public double Vaegt { get; set; }
}

public class DagligFastVaegtDTO
{
    public DagligFast DagligFastOrdination {get; set;}
    public double Vaegt { get; set; }
}