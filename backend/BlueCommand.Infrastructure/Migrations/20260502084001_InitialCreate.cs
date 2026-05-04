using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BlueCommand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roluri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    denumire = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roluri", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sectii",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nume = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    adresa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    zona = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    latitudine = table.Column<double>(type: "double precision", nullable: true),
                    longitudine = table.Column<double>(type: "double precision", nullable: true),
                    creat_la = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sectii", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "utilizatori",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    sectie_id = table.Column<int>(type: "integer", nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parola_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nume = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    prenume = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    grad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_creare = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    este_activ = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_utilizatori", x => x.id);
                    table.ForeignKey(
                        name: "fk_utilizatori_roluri_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roluri",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_utilizatori_sectii_sectie_id",
                        column: x => x.sectie_id,
                        principalTable: "sectii",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    utilizator_id = table.Column<int>(type: "integer", nullable: true),
                    actiune = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    detalii = table.Column<string>(type: "text", nullable: true),
                    ip_adresa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creat_la = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_log_utilizator_utilizator_id",
                        column: x => x.utilizator_id,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "dosare",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numar_dosar = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titlu = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    descriere = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DESCHIS"),
                    tip_incident = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_incident = table.Column<DateOnly>(type: "date", nullable: true),
                    sectie_id = table.Column<int>(type: "integer", nullable: false),
                    creat_de = table.Column<int>(type: "integer", nullable: false),
                    creat_la = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    actualizat_la = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dosare", x => x.id);
                    table.ForeignKey(
                        name: "fk_dosare_sectii_sectie_id",
                        column: x => x.sectie_id,
                        principalTable: "sectii",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dosare_utilizatori_creat_de",
                        column: x => x.creat_de,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "istoric_sectii",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sectie_id = table.Column<int>(type: "integer", nullable: false),
                    camp_modificat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valoare_veche = table.Column<string>(type: "text", nullable: true),
                    valoare_noua = table.Column<string>(type: "text", nullable: true),
                    modificat_de = table.Column<int>(type: "integer", nullable: false),
                    modificat_la = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_istoric_sectii", x => x.id);
                    table.ForeignKey(
                        name: "fk_istoric_sectii_sectii_sectie_id",
                        column: x => x.sectie_id,
                        principalTable: "sectii",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_istoric_sectii_utilizatori_modificat_de",
                        column: x => x.modificat_de,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "istoric_utilizatori",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    utilizator_id = table.Column<int>(type: "integer", nullable: false),
                    camp_modificat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valoare_veche = table.Column<string>(type: "text", nullable: true),
                    valoare_noua = table.Column<string>(type: "text", nullable: true),
                    modificat_de = table.Column<int>(type: "integer", nullable: false),
                    modificat_la = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_istoric_utilizatori", x => x.id);
                    table.ForeignKey(
                        name: "fk_istoric_utilizatori_utilizatori_modificat_de",
                        column: x => x.modificat_de,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_istoric_utilizatori_utilizatori_utilizator_id",
                        column: x => x.utilizator_id,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rapoarte",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    utilizator_id = table.Column<int>(type: "integer", nullable: false),
                    tip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    filtru_perioada = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_generare = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    cale_fisier = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rapoarte", x => x.id);
                    table.ForeignKey(
                        name: "fk_rapoarte_utilizatori_utilizator_id",
                        column: x => x.utilizator_id,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "documente_dosar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dosar_id = table.Column<int>(type: "integer", nullable: false),
                    nume_fisier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cale_fisier = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    marime_bytes = table.Column<long>(type: "bigint", nullable: true),
                    incarcat_de = table.Column<int>(type: "integer", nullable: false),
                    data_incarcare = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documente_dosar", x => x.id);
                    table.ForeignKey(
                        name: "fk_documente_dosar_dosare_dosar_id",
                        column: x => x.dosar_id,
                        principalTable: "dosare",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_documente_dosar_utilizatori_incarcat_de",
                        column: x => x.incarcat_de,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dosar_agenti",
                columns: table => new
                {
                    dosar_id = table.Column<int>(type: "integer", nullable: false),
                    utilizator_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dosar_agenti", x => new { x.dosar_id, x.utilizator_id });
                    table.ForeignKey(
                        name: "fk_dosar_agenti_dosare_dosar_id",
                        column: x => x.dosar_id,
                        principalTable: "dosare",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dosar_agenti_utilizatori_utilizator_id",
                        column: x => x.utilizator_id,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "istoric_dosare",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dosar_id = table.Column<int>(type: "integer", nullable: false),
                    camp_modificat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valoare_veche = table.Column<string>(type: "text", nullable: true),
                    valoare_noua = table.Column<string>(type: "text", nullable: true),
                    modificat_de = table.Column<int>(type: "integer", nullable: false),
                    modificat_la = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_istoric_dosare", x => x.id);
                    table.ForeignKey(
                        name: "fk_istoric_dosare_dosare_dosar_id",
                        column: x => x.dosar_id,
                        principalTable: "dosare",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_istoric_dosare_utilizatori_modificat_de",
                        column: x => x.modificat_de,
                        principalTable: "utilizatori",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_utilizator_id",
                table: "audit_log",
                column: "utilizator_id");

            migrationBuilder.CreateIndex(
                name: "ix_documente_dosar_dosar_id",
                table: "documente_dosar",
                column: "dosar_id");

            migrationBuilder.CreateIndex(
                name: "ix_documente_dosar_incarcat_de",
                table: "documente_dosar",
                column: "incarcat_de");

            migrationBuilder.CreateIndex(
                name: "ix_dosar_agenti_utilizator_id",
                table: "dosar_agenti",
                column: "utilizator_id");

            migrationBuilder.CreateIndex(
                name: "ix_dosare_creat_de",
                table: "dosare",
                column: "creat_de");

            migrationBuilder.CreateIndex(
                name: "ix_dosare_numar_dosar",
                table: "dosare",
                column: "numar_dosar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dosare_sectie_id",
                table: "dosare",
                column: "sectie_id");

            migrationBuilder.CreateIndex(
                name: "ix_istoric_dosare_dosar_id",
                table: "istoric_dosare",
                column: "dosar_id");

            migrationBuilder.CreateIndex(
                name: "ix_istoric_dosare_modificat_de",
                table: "istoric_dosare",
                column: "modificat_de");

            migrationBuilder.CreateIndex(
                name: "ix_istoric_sectii_modificat_de",
                table: "istoric_sectii",
                column: "modificat_de");

            migrationBuilder.CreateIndex(
                name: "ix_istoric_sectii_sectie_id",
                table: "istoric_sectii",
                column: "sectie_id");

            migrationBuilder.CreateIndex(
                name: "ix_istoric_utilizatori_modificat_de",
                table: "istoric_utilizatori",
                column: "modificat_de");

            migrationBuilder.CreateIndex(
                name: "ix_istoric_utilizatori_utilizator_id",
                table: "istoric_utilizatori",
                column: "utilizator_id");

            migrationBuilder.CreateIndex(
                name: "ix_rapoarte_utilizator_id",
                table: "rapoarte",
                column: "utilizator_id");

            migrationBuilder.CreateIndex(
                name: "ix_utilizatori_rol_id",
                table: "utilizatori",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "ix_utilizatori_sectie_id",
                table: "utilizatori",
                column: "sectie_id");

            migrationBuilder.CreateIndex(
                name: "ix_utilizatori_username",
                table: "utilizatori",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "documente_dosar");

            migrationBuilder.DropTable(
                name: "dosar_agenti");

            migrationBuilder.DropTable(
                name: "istoric_dosare");

            migrationBuilder.DropTable(
                name: "istoric_sectii");

            migrationBuilder.DropTable(
                name: "istoric_utilizatori");

            migrationBuilder.DropTable(
                name: "rapoarte");

            migrationBuilder.DropTable(
                name: "dosare");

            migrationBuilder.DropTable(
                name: "utilizatori");

            migrationBuilder.DropTable(
                name: "roluri");

            migrationBuilder.DropTable(
                name: "sectii");
        }
    }
}
