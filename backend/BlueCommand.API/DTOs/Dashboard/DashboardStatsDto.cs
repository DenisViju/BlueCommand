namespace BlueCommand.API.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalDosare { get; init; }
    public int DosareDeschise { get; init; }
    public int DosareInLucru { get; init; }
    public int DosareInchise { get; init; }
    public int TotalAgenti { get; init; }
    public int TotalSectii { get; init; }
    public int IncidentePeUltimele30Zile { get; init; }
}

