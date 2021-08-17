﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Migrations
{
    [DbContext(typeof(TenantDbContext))]
    [Migration("20210628161021_AddUpgradedFromIdToAssignment")]
    partial class AddUpgradedFromIdToAssignment
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("WB.Services.Export.Assignment.Assignment", b =>
                {
                    b.Property<Guid>("PublicKey")
                        .HasColumnName("public_key")
                        .HasColumnType("uuid");

                    b.Property<bool>("AudioRecording")
                        .HasColumnName("audio_recording")
                        .HasColumnType("boolean");

                    b.Property<string>("Comment")
                        .HasColumnName("comment")
                        .HasColumnType("text");

                    b.Property<int>("Id")
                        .HasColumnName("id")
                        .HasColumnType("integer");

                    b.Property<int?>("Quantity")
                        .HasColumnName("quantity")
                        .HasColumnType("integer");

                    b.Property<string>("QuestionnaireId")
                        .HasColumnName("questionnaire_id")
                        .HasColumnType("text");

                    b.Property<Guid>("ResponsibleId")
                        .HasColumnName("responsible_id")
                        .HasColumnType("uuid");

                    b.Property<int?>("UpgradedFromId")
                        .HasColumnName("upgraded_from_id")
                        .HasColumnType("integer");

                    b.Property<bool?>("WebMode")
                        .HasColumnName("web_mode")
                        .HasColumnType("boolean");

                    b.HasKey("PublicKey");

                    b.HasAlternateKey("Id");

                    b.ToTable("__assignment");
                });

            modelBuilder.Entity("WB.Services.Export.Assignment.AssignmentAction", b =>
                {
                    b.Property<long>("GlobalSequence")
                        .HasColumnName("global_sequence")
                        .HasColumnType("bigint");

                    b.Property<int>("Position")
                        .HasColumnName("position")
                        .HasColumnType("integer");

                    b.Property<int>("AssignmentId")
                        .HasColumnName("assignment_id")
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .HasColumnName("comment")
                        .HasColumnType("text");

                    b.Property<string>("NewValue")
                        .HasColumnName("new_value")
                        .HasColumnType("text");

                    b.Property<string>("OldValue")
                        .HasColumnName("old_value")
                        .HasColumnType("text");

                    b.Property<Guid>("OriginatorId")
                        .HasColumnName("originator_id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ResponsibleId")
                        .HasColumnName("responsible_id")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnName("status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("TimestampUtc")
                        .HasColumnName("timestamp_utc")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("GlobalSequence", "Position");

                    b.HasIndex("AssignmentId");

                    b.ToTable("__assignment__action");
                });

            modelBuilder.Entity("WB.Services.Export.InterviewDataStorage.GeneratedQuestionnaireReference", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnName("id")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnName("deleted_at")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id")
                        .HasName("pk_generated_questionnaires");

                    b.ToTable("__generated_questionnaire_reference");
                });

            modelBuilder.Entity("WB.Services.Export.InterviewDataStorage.InterviewReference", b =>
                {
                    b.Property<Guid>("InterviewId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("interview_id")
                        .HasColumnType("uuid");

                    b.Property<int?>("AssignmentId")
                        .HasColumnName("assignment_id")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("DeletedAtUtc")
                        .HasColumnName("deleted_at_utc")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Key")
                        .HasColumnName("key")
                        .HasColumnType("text");

                    b.Property<string>("QuestionnaireId")
                        .HasColumnName("questionnaire_id")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnName("status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdateDateUtc")
                        .HasColumnName("update_date_utc")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("InterviewId");

                    b.ToTable("interview__references");
                });

            modelBuilder.Entity("WB.Services.Export.InterviewDataStorage.Metadata", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnName("id")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnName("value")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_metadata");

                    b.ToTable("metadata");
                });
#pragma warning restore 612, 618
        }
    }
}