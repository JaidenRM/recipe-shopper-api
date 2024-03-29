﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RecipeShopper.Data;

#nullable disable

namespace RecipeShopper.Migrations
{
    [DbContext(typeof(RecipeShopperContext))]
    [Migration("20220424024414_MinorRecipeRenaming")]
    partial class MinorRecipeRenaming
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RecipeShopper.Entities.Ingredient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int?>("ProductId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("numeric");

                    b.Property<int>("RecipeId")
                        .HasColumnType("integer");

                    b.Property<int?>("SupermarketId")
                        .HasColumnType("integer");

                    b.Property<int>("Unit")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.HasIndex("SupermarketId");

                    b.HasIndex("ProductId", "SupermarketId")
                        .IsUnique();

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Instruction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<int>("RecipeId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId", "Order")
                        .IsUnique();

                    b.ToTable("Instructions");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<int>("SupermarketId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id", "SupermarketId");

                    b.HasIndex("SupermarketId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Recipe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOnUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("DurationMinutes")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastModifiedUTC")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("Servings")
                        .HasColumnType("integer");

                    b.Property<string>("Tags")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Supermarket", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Supermarket");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Ingredient", b =>
                {
                    b.HasOne("RecipeShopper.Entities.Recipe", "Recipe")
                        .WithMany("Ingredients")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RecipeShopper.Entities.Supermarket", "Supermarket")
                        .WithMany()
                        .HasForeignKey("SupermarketId");

                    b.HasOne("RecipeShopper.Entities.Product", "Product")
                        .WithOne()
                        .HasForeignKey("RecipeShopper.Entities.Ingredient", "ProductId", "SupermarketId");

                    b.Navigation("Product");

                    b.Navigation("Recipe");

                    b.Navigation("Supermarket");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Instruction", b =>
                {
                    b.HasOne("RecipeShopper.Entities.Recipe", "Recipe")
                        .WithMany("Instructions")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Product", b =>
                {
                    b.HasOne("RecipeShopper.Entities.Supermarket", "Supermarket")
                        .WithMany("Products")
                        .HasForeignKey("SupermarketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Supermarket");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Recipe", b =>
                {
                    b.Navigation("Ingredients");

                    b.Navigation("Instructions");
                });

            modelBuilder.Entity("RecipeShopper.Entities.Supermarket", b =>
                {
                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}
