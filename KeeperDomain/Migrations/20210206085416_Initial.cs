using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KeeperDomain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarAccountId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    IssueYear = table.Column<int>(nullable: false),
                    Vin = table.Column<string>(nullable: true),
                    StateRegNumber = table.Column<string>(nullable: true),
                    PurchaseDate = table.Column<DateTime>(nullable: false),
                    PurchaseMileage = table.Column<int>(nullable: false),
                    SaleDate = table.Column<DateTime>(nullable: false),
                    SaleMileage = table.Column<int>(nullable: false),
                    SupposedSalePrice = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepositCalculationRules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepositOfferConditionsId = table.Column<int>(nullable: false),
                    IsFactDays = table.Column<bool>(nullable: false),
                    EveryStartDay = table.Column<bool>(nullable: false),
                    EveryFirstDayOfMonth = table.Column<bool>(nullable: false),
                    EveryLastDayOfMonth = table.Column<bool>(nullable: false),
                    IsCapitalized = table.Column<bool>(nullable: false),
                    IsRateFixed = table.Column<bool>(nullable: false),
                    HasAdditionalProcent = table.Column<bool>(nullable: false),
                    AdditionalProcent = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositCalculationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepositOffers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BankId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    IsNotRevocable = table.Column<bool>(nullable: false),
                    MainCurrency = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MyAccountId = table.Column<int>(nullable: false),
                    DepositOfferId = table.Column<int>(nullable: false),
                    Serial = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    FinishDate = table.Column<DateTime>(nullable: false),
                    ShortName = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fuellings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<int>(nullable: false),
                    Volume = table.Column<double>(nullable: false),
                    FuelType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fuellings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OneRate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<double>(nullable: false),
                    Unit = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayCards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MyAccountId = table.Column<int>(nullable: false),
                    CardNumber = table.Column<string>(nullable: true),
                    CardHolder = table.Column<string>(nullable: true),
                    PaymentSystem = table.Column<int>(nullable: false),
                    IsPayPass = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagAssociations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalAccount = table.Column<int>(nullable: false),
                    OperationType = table.Column<int>(nullable: false),
                    Tag = table.Column<int>(nullable: false),
                    Destination = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagAssociations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Operation = table.Column<int>(nullable: false),
                    PaymentWay = table.Column<int>(nullable: false),
                    Receipt = table.Column<int>(nullable: false),
                    MyAccount = table.Column<int>(nullable: false),
                    MySecondAccount = table.Column<int>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    AmountInReturn = table.Column<decimal>(nullable: false),
                    Currency = table.Column<int>(nullable: false),
                    CurrencyInReturn = table.Column<int>(nullable: true),
                    Tags = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YearMileages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarId = table.Column<int>(nullable: false),
                    YearNumber = table.Column<int>(nullable: false),
                    Mileage = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearMileages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearMileages_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositConditions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepositOfferId = table.Column<int>(nullable: false),
                    DateFrom = table.Column<DateTime>(nullable: false),
                    CalculationRulesId = table.Column<int>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositConditions_DepositCalculationRules_CalculationRulesId",
                        column: x => x.CalculationRulesId,
                        principalTable: "DepositCalculationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Header = table.Column<string>(nullable: true),
                    OwnerId = table.Column<int>(nullable: false),
                    IsFolder = table.Column<bool>(nullable: false),
                    DepositId = table.Column<int>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Deposits_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Deposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CbrRate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsdId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CbrRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CbrRate_OneRate_UsdId",
                        column: x => x.UsdId,
                        principalTable: "OneRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NbRbRates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsdId = table.Column<int>(nullable: true),
                    EuroId = table.Column<int>(nullable: true),
                    RurId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NbRbRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NbRbRates_OneRate_EuroId",
                        column: x => x.EuroId,
                        principalTable: "OneRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NbRbRates_OneRate_RurId",
                        column: x => x.RurId,
                        principalTable: "OneRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NbRbRates_OneRate_UsdId",
                        column: x => x.UsdId,
                        principalTable: "OneRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepositRateLines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepositOfferConditionsId = table.Column<int>(nullable: false),
                    DateFrom = table.Column<DateTime>(nullable: false),
                    AmountFrom = table.Column<decimal>(nullable: false),
                    AmountTo = table.Column<decimal>(nullable: false),
                    Rate = table.Column<decimal>(nullable: false),
                    DepositConditionsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositRateLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositRateLines_DepositConditions_DepositConditionsId",
                        column: x => x.DepositConditionsId,
                        principalTable: "DepositConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    NbRatesId = table.Column<int>(nullable: true),
                    CbrRateId = table.Column<int>(nullable: true),
                    MyEurUsdRateId = table.Column<int>(nullable: true),
                    MyUsdRateId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rates_CbrRate_CbrRateId",
                        column: x => x.CbrRateId,
                        principalTable: "CbrRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rates_OneRate_MyEurUsdRateId",
                        column: x => x.MyEurUsdRateId,
                        principalTable: "OneRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rates_OneRate_MyUsdRateId",
                        column: x => x.MyUsdRateId,
                        principalTable: "OneRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rates_NbRbRates_NbRatesId",
                        column: x => x.NbRatesId,
                        principalTable: "NbRbRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_DepositId",
                table: "Accounts",
                column: "DepositId");

            migrationBuilder.CreateIndex(
                name: "IX_CbrRate_UsdId",
                table: "CbrRate",
                column: "UsdId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositConditions_CalculationRulesId",
                table: "DepositConditions",
                column: "CalculationRulesId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositRateLines_DepositConditionsId",
                table: "DepositRateLines",
                column: "DepositConditionsId");

            migrationBuilder.CreateIndex(
                name: "IX_NbRbRates_EuroId",
                table: "NbRbRates",
                column: "EuroId");

            migrationBuilder.CreateIndex(
                name: "IX_NbRbRates_RurId",
                table: "NbRbRates",
                column: "RurId");

            migrationBuilder.CreateIndex(
                name: "IX_NbRbRates_UsdId",
                table: "NbRbRates",
                column: "UsdId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_CbrRateId",
                table: "Rates",
                column: "CbrRateId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_MyEurUsdRateId",
                table: "Rates",
                column: "MyEurUsdRateId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_MyUsdRateId",
                table: "Rates",
                column: "MyUsdRateId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_NbRatesId",
                table: "Rates",
                column: "NbRatesId");

            migrationBuilder.CreateIndex(
                name: "IX_YearMileages_CarId",
                table: "YearMileages",
                column: "CarId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "DepositOffers");

            migrationBuilder.DropTable(
                name: "DepositRateLines");

            migrationBuilder.DropTable(
                name: "Fuellings");

            migrationBuilder.DropTable(
                name: "PayCards");

            migrationBuilder.DropTable(
                name: "Rates");

            migrationBuilder.DropTable(
                name: "TagAssociations");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "YearMileages");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "DepositConditions");

            migrationBuilder.DropTable(
                name: "CbrRate");

            migrationBuilder.DropTable(
                name: "NbRbRates");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "DepositCalculationRules");

            migrationBuilder.DropTable(
                name: "OneRate");
        }
    }
}
