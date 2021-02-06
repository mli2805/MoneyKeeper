﻿// <auto-generated />
using System;
using KeeperDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KeeperDomain.Migrations
{
    [DbContext(typeof(KeeperContext))]
    [Migration("20210206160004_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.11");

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

                    b.Property<int>("CarAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<int>("IssueYear")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("PurchaseDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("PurchaseMileage")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("SaleDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("SaleMileage")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StateRegNumber")
                        .HasColumnType("TEXT");

                    b.Property<int>("SupposedSalePrice")
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

                    b.ToTable("Deposits");
                });

            modelBuilder.Entity("KeeperDomain.DepositCalculationRules", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("AdditionalProcent")
                        .HasColumnType("REAL");

                    b.Property<int>("DepositOfferConditionsId")
                        .HasColumnType("INTEGER");

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

            modelBuilder.Entity("KeeperDomain.DepositConditions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CalculationRulesId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateFrom")
                        .HasColumnType("TEXT");

                    b.Property<int>("DepositOfferId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CalculationRulesId");

                    b.ToTable("DepositConditions");
                });

            modelBuilder.Entity("KeeperDomain.DepositOffer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BankId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsNotRevocable")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MainCurrency")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DepositOffers");
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

                    b.Property<int?>("DepositConditionsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DepositOfferConditionsId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Rate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DepositConditionsId");

                    b.ToTable("DepositRateLines");
                });

            modelBuilder.Entity("KeeperDomain.Fuelling", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("FuelType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TransactionId")
                        .HasColumnType("INTEGER");

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

            modelBuilder.Entity("KeeperDomain.PayCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CardHolder")
                        .HasColumnType("TEXT");

                    b.Property<string>("CardNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsPayPass")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MyAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PaymentSystem")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PayCards");
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

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("AmountInReturn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<int>("Currency")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CurrencyInReturn")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MyAccount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MySecondAccount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Operation")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PaymentWay")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Receipt")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Tags")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

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

                    b.ToTable("YearMileages");
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

            modelBuilder.Entity("KeeperDomain.DepositConditions", b =>
                {
                    b.HasOne("KeeperDomain.DepositCalculationRules", "CalculationRules")
                        .WithMany()
                        .HasForeignKey("CalculationRulesId");
                });

            modelBuilder.Entity("KeeperDomain.DepositRateLine", b =>
                {
                    b.HasOne("KeeperDomain.DepositConditions", null)
                        .WithMany("RateLines")
                        .HasForeignKey("DepositConditionsId");
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
#pragma warning restore 612, 618
        }
    }
}
