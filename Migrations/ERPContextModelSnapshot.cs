﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkerService1.Contexts;

#nullable disable

namespace WorkerService1.Migrations
{
    [DbContext(typeof(ERPContext))]
    partial class ERPContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("WorkerService1.Models.Platform", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Platforms");
                });

            modelBuilder.Entity("WorkerService1.Models.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Approval")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("BigPack")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Expiry")
                        .HasColumnType("date");

                    b.Property<int?>("MidPack")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("PlatformId")
                        .HasColumnType("int");

                    b.Property<decimal?>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProducerName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("ProductDate")
                        .HasColumnType("date");

                    b.Property<string>("SaleTip")
                        .HasColumnType("longtext");

                    b.Property<string>("SellTip")
                        .HasColumnType("longtext");

                    b.Property<string>("Specs")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("StockAmount")
                        .HasColumnType("int");

                    b.Property<string>("Unit")
                        .HasColumnType("longtext");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("PlatformId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("WorkerService1.Models.Product", b =>
                {
                    b.HasOne("WorkerService1.Models.Platform", "Platform")
                        .WithMany("Products")
                        .HasForeignKey("PlatformId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Platform");
                });

            modelBuilder.Entity("WorkerService1.Models.Platform", b =>
                {
                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}
