using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MvcMoviesCore.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonType",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecordCarrier",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordCarrier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sex",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sex", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true),
                    SexId = table.Column<Guid>(),
                    Birthday = table.Column<DateTime>(nullable: true),
                    Obit = table.Column<DateTime>(nullable: true),
                    Nationality = table.Column<string>(nullable: true),
                    Größe = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Gewicht = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PersonTypesId = table.Column<Guid>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Person_PersonType_PersonTypesId",
                        column: x => x.PersonTypesId,
                        principalTable: "PersonType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Person_Sex_SexId",
                        column: x => x.SexId,
                        principalTable: "Sex",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true),
                    YearOfPublication = table.Column<DateTime>(nullable: true),
                    GenreId = table.Column<Guid>(),
                    RecordCarrierId = table.Column<Guid>(nullable: true),
                    InStock = table.Column<bool>(),
                    StorageLocationId = table.Column<Guid>(nullable: true),
                    Added = table.Column<DateTime>(nullable: true),
                    OnWatch = table.Column<string>(nullable: true),
                    Remark = table.Column<string>(nullable: true),
                    Adult = table.Column<bool>(),
                    ThreeD = table.Column<bool>(),
                    Owner = table.Column<string>(nullable: true),
                    IMDB = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ranking = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastView = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movies_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Movies_RecordCarrier_RecordCarrierId",
                        column: x => x.RecordCarrierId,
                        principalTable: "RecordCarrier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movies_StorageLocation_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MoviesPerson",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    MoviesId = table.Column<Guid>(),
                    PersonId = table.Column<Guid>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviesPerson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoviesPerson_Movies_MoviesId",
                        column: x => x.MoviesId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoviesPerson_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_GenreId",
                table: "Movies",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_RecordCarrierId",
                table: "Movies",
                column: "RecordCarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_StorageLocationId",
                table: "Movies",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MoviesPerson_MoviesId",
                table: "MoviesPerson",
                column: "MoviesId");

            migrationBuilder.CreateIndex(
                name: "IX_MoviesPerson_PersonId",
                table: "MoviesPerson",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PersonTypesId",
                table: "Person",
                column: "PersonTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_SexId",
                table: "Person",
                column: "SexId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoviesPerson");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "RecordCarrier");

            migrationBuilder.DropTable(
                name: "StorageLocation");

            migrationBuilder.DropTable(
                name: "PersonType");

            migrationBuilder.DropTable(
                name: "Sex");
        }
    }
}