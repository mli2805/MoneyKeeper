﻿// <auto-generated />
using System;
using KeeperSqliteDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KeeperSqliteDb.Migrations
{
    [DbContext(typeof(KeeperContext))]
    [Migration("20200404134938_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("KeeperDomain.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DepositId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Header")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsFolder")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DepositId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("KeeperDomain.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Finish")
                        .HasColumnType("TEXT");

                    b.Property<int>("IssueYear")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MileageFinish")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MileageStart")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.Property<string>("StateRegNumber")
                        .HasColumnType("TEXT");

                    b.Property<int>("SupposedSale")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("Vin")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("KeeperDomain.CbrRate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UsdId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UsdId");

                    b.ToTable("CbrRate");
                });

            modelBuilder.Entity("KeeperDomain.CurrencyRates", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CbrRateId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MyEurUsdRateId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MyUsdRateId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("NbRatesId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CbrRateId");

                    b.HasIndex("MyEurUsdRateId");

                    b.HasIndex("MyUsdRateId");

                    b.HasIndex("NbRatesId");

                    b.ToTable("Rates");
                });

            modelBuilder.Entity("KeeperDomain.Deposit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<int>("DepositOfferId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("FinishDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("MyAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Serial")
                        .HasColumnType("TEXT");

                    b.Property<string>("ShortName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Deposit");
                });

            modelBuilder.Entity("KeeperDomain.DepositCalculationRules", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("AdditionalProcent")
                        .HasColumnType("REAL");

                    b.Property<bool>("EveryFirstDayOfMonth")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EveryLastDayOfMonth")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EveryStartDay")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasAdditionalProcent")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsCapitalized")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFactDays")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsRateFixed")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("DepositCalculationRules");
                });

            modelBuilder.Entity("KeeperDomain.DepositEssential", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CalculationRulesId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<int>("DepositOfferId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CalculationRulesId");

                    b.ToTable("DepositEssentials");
                });

            modelBuilder.Entity("KeeperDomain.DepositRateLine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("AmountFrom")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("AmountTo")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateFrom")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DepositEssentialId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Rate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DepositEssentialId");

                    b.ToTable("DepositRateLines");
                });

            modelBuilder.Entity("KeeperDomain.Fuelling", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT");

                    b.Property<int>("CarAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<int>("Currency")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FuelType")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("OneLitreInUsd")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("OneLitrePrice")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<double>("Volume")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("Fuellings");
                });

            modelBuilder.Entity("KeeperDomain.NbRbRates", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EuroId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RurId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UsdId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("EuroId");

                    b.HasIndex("RurId");

                    b.HasIndex("UsdId");

                    b.ToTable("NbRbRates");
                });

            modelBuilder.Entity("KeeperDomain.OneRate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Unit")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Value")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("OneRate");
                });

            modelBuilder.Entity("KeeperDomain.TagAssociation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Destination")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExternalAccount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OperationType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Tag")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TagAssociations");
                });

            modelBuilder.Entity("KeeperDomain.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("KeeperDomain.YearMileage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CarId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Mileage")
                        .HasColumnType("INTEGER");

                    b.Property<int>("YearNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.ToTable("YearMileage");
                });

            modelBuilder.Entity("KeeperDomain.Account", b =>
                {
                    b.HasOne("KeeperDomain.Deposit", "Deposit")
                        .WithMany()
                        .HasForeignKey("DepositId");
                });

            modelBuilder.Entity("KeeperDomain.CbrRate", b =>
                {
                    b.HasOne("KeeperDomain.OneRate", "Usd")
                        .WithMany()
                        .HasForeignKey("UsdId");
                });

            modelBuilder.Entity("KeeperDomain.CurrencyRates", b =>
                {
                    b.HasOne("KeeperDomain.CbrRate", "CbrRate")
                        .WithMany()
                        .HasForeignKey("CbrRateId");

                    b.HasOne("KeeperDomain.OneRate", "MyEurUsdRate")
                        .WithMany()
                        .HasForeignKey("MyEurUsdRateId");

                    b.HasOne("KeeperDomain.OneRate", "MyUsdRate")
                        .WithMany()
                        .HasForeignKey("MyUsdRateId");

                    b.HasOne("KeeperDomain.NbRbRates", "NbRates")
                        .WithMany()
                        .HasForeignKey("NbRatesId");
                });

            modelBuilder.Entity("KeeperDomain.DepositEssential", b =>
                {
                    b.HasOne("KeeperDomain.DepositCalculationRules", "CalculationRules")
                        .WithMany()
                        .HasForeignKey("CalculationRulesId");
                });

            modelBuilder.Entity("KeeperDomain.DepositRateLine", b =>
                {
                    b.HasOne("KeeperDomain.DepositEssential", null)
                        .WithMany("RateLines")
                        .HasForeignKey("DepositEssentialId");
                });

            modelBuilder.Entity("KeeperDomain.NbRbRates", b =>
                {
                    b.HasOne("KeeperDomain.OneRate", "Euro")
                        .WithMany()
                        .HasForeignKey("EuroId");

                    b.HasOne("KeeperDomain.OneRate", "Rur")
                        .WithMany()
                        .HasForeignKey("RurId");

                    b.HasOne("KeeperDomain.OneRate", "Usd")
                        .WithMany()
                        .HasForeignKey("UsdId");
                });

            modelBuilder.Entity("KeeperDomain.YearMileage", b =>
                {
                    b.HasOne("KeeperDomain.Car", null)
                        .WithMany("YearMileages")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
