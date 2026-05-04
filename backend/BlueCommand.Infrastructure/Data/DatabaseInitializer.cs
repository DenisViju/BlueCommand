using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Constants;
using BlueCommand.Domain.Entities;
using BlueCommand.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task MigrateAndSeedAsync(BlueCommandDbContext db, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Roluri.AnyAsync(cancellationToken))
            return;

        var rolAdmin = new Rol { Id = 1, Denumire = RoluriDenumiri.Administrator };
        var rolSef = new Rol { Id = 2, Denumire = RoluriDenumiri.SefInspectorat };
        var rolAgent = new Rol { Id = 3, Denumire = RoluriDenumiri.AgentPolitie };
        db.Roluri.AddRange(rolAdmin, rolSef, rolAgent);

        var sectia1 = new Sectie
        {
            Nume = "Sectia 1",
            Adresa = "Strada Amaradia 32, 200157 Craiova",
            Zona = "Centru",
            Latitudine = 44.3279,
            Longitudine = 23.7935,
            CreatLa = DateTime.UtcNow
        };
        var sectia2 = new Sectie
        {
            Nume = "Sectia 2",
            Adresa = "	Strada Nicolae Coculescu 22, 200696 Craiova",
            Zona = "Nord",
            Latitudine = 44.3387,
            Longitudine = 23.7828,
            CreatLa = DateTime.UtcNow
        };
        var sectia3 = new Sectie
        {
            Nume = "Sectia 3",
            Adresa = "Strada Henri Coandă 69 bis, 200514 Craiova",
            Zona = "Sud",
            Latitudine = 44.3071,
            Longitudine = 23.8215,
            CreatLa = DateTime.UtcNow
        };
        db.Sectii.AddRange(sectia1, sectia2, sectia3);

        await db.SaveChangesAsync(cancellationToken);

        var admin = new Utilizator
        {
            Username = "admin",
            ParolaHash = passwordHasher.Hash("Admin@1234"),
            RolId = rolAdmin.Id,
            SectieId = null,
            Nume = "Admin",
            Prenume = "System",
            Grad = null,
            DataCreare = DateTime.UtcNow,
            EsteActiv = true
        };
        var sef01 = new Utilizator
        {
            Username = "sef01",
            ParolaHash = passwordHasher.Hash("Test@1234"),
            RolId = rolSef.Id,
            SectieId = sectia1.Id,
            Nume = "Sef",
            Prenume = "Inspectorat",
            Grad = "Comisar",
            DataCreare = DateTime.UtcNow,
            EsteActiv = true
        };
        var agent01 = new Utilizator
        {
            Username = "agent01",
            ParolaHash = passwordHasher.Hash("Test@1234"),
            RolId = rolAgent.Id,
            SectieId = sectia1.Id,
            Nume = "Agent",
            Prenume = "Unu",
            Grad = "Agent",
            DataCreare = DateTime.UtcNow,
            EsteActiv = true
        };
        var agent02 = new Utilizator
        {
            Username = "agent02",
            ParolaHash = passwordHasher.Hash("Test@1234"),
            RolId = rolAgent.Id,
            SectieId = sectia2.Id,
            Nume = "Agent",
            Prenume = "Doi",
            Grad = "Agent",
            DataCreare = DateTime.UtcNow,
            EsteActiv = true
        };
        db.Utilizatori.AddRange(admin, sef01, agent01, agent02);

        await db.SaveChangesAsync(cancellationToken);

        var dosar1 = new Dosar
        {
            NumarDosar = "2025/001",
            Titlu = "Furt locuinta",
            Descriere = "Sesizare furt locuinta.",
            Status = DosarStatus.DESCHIS,
            TipIncident = "Furt",
            DataIncident = new DateOnly(2025, 1, 10),
            SectieId = sectia1.Id,
            CreatDe = sef01.Id,
            CreatLa = DateTime.UtcNow
        };
        var dosar2 = new Dosar
        {
            NumarDosar = "2025/002",
            Titlu = "Talharie stradala",
            Descriere = "Sesizare talharie.",
            Status = DosarStatus.IN_LUCRU,
            TipIncident = "Talharie",
            DataIncident = new DateOnly(2025, 2, 15),
            SectieId = sectia2.Id,
            CreatDe = sef01.Id,
            CreatLa = DateTime.UtcNow
        };
        var dosar3 = new Dosar
        {
            NumarDosar = "2025/003",
            Titlu = "Vandalism",
            Descriere = "Sesizare vandalism.",
            Status = DosarStatus.INCHIS,
            TipIncident = "Vandalism",
            DataIncident = new DateOnly(2025, 3, 1),
            SectieId = sectia1.Id,
            CreatDe = sef01.Id,
            CreatLa = DateTime.UtcNow
        };

        db.Dosare.AddRange(dosar1, dosar2, dosar3);
        await db.SaveChangesAsync(cancellationToken);

        db.DosarAgenti.AddRange(
            new DosarAgent { DosarId = dosar1.Id, UtilizatorId = agent01.Id },
            new DosarAgent { DosarId = dosar2.Id, UtilizatorId = agent02.Id },
            new DosarAgent { DosarId = dosar3.Id, UtilizatorId = agent01.Id }
        );

        await db.SaveChangesAsync(cancellationToken);
    }
}

