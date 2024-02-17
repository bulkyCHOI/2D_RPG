﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.DB;

#nullable disable

namespace Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Server.DB.AccountDb", b =>
                {
                    b.Property<int>("AccountDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AccountDbId"));

                    b.Property<string>("AccountName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AccountDbId");

                    b.HasIndex("AccountName")
                        .IsUnique();

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.Property<int>("PlayerDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlayerDbId"));

                    b.Property<int>("AccountDbId")
                        .HasColumnType("int");

                    b.Property<int>("DEX")
                        .HasColumnType("int");

                    b.Property<int>("INT")
                        .HasColumnType("int");

                    b.Property<string>("PlayerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("STR")
                        .HasColumnType("int");

                    b.Property<int>("hp")
                        .HasColumnType("int");

                    b.Property<int>("level")
                        .HasColumnType("int");

                    b.Property<int>("maxHp")
                        .HasColumnType("int");

                    b.Property<int>("maxMp")
                        .HasColumnType("int");

                    b.Property<int>("mp")
                        .HasColumnType("int");

                    b.Property<float>("speed")
                        .HasColumnType("real");

                    b.Property<int>("totalExp")
                        .HasColumnType("int");

                    b.HasKey("PlayerDbId");

                    b.HasIndex("AccountDbId");

                    b.HasIndex("PlayerName")
                        .IsUnique();

                    b.ToTable("Player");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.HasOne("Server.DB.AccountDb", "Account")
                        .WithMany("Players")
                        .HasForeignKey("AccountDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Server.DB.AccountDb", b =>
                {
                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
