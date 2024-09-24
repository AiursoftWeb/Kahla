﻿// <auto-generated />
using System;
using Aiursoft.Kahla.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Aiursoft.Kahla.Server.Migrations
{
    [DbContext(typeof(KahlaDbContext))]
    partial class KahlaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.At", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TargetUserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.HasIndex("TargetUserId");

                    b.ToTable("Ats");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Conversation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ConversationCreateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Conversations");

                    b.HasDiscriminator().HasValue("Conversation");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Device", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("IpAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("PushAuth")
                        .HasColumnType("TEXT");

                    b.Property<string>("PushEndpoint")
                        .HasColumnType("TEXT");

                    b.Property<string>("PushP256Dh")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.KahlaUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AccountCreateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Bio")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConnectKey")
                        .HasColumnType("TEXT");

                    b.Property<int>("CurrentChannel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EmailReasonInJson")
                        .HasColumnType("TEXT");

                    b.Property<bool>("EnableEmailNotification")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableEnterToSendMessage")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableInvisiable")
                        .HasColumnType("INTEGER");

                    b.Property<string>("IconFilePath")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastEmailHimTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ListInSearchResult")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<bool>("MarkEmailPublic")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NickName")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<string>("PreferedLanguage")
                        .HasColumnType("TEXT");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("Sex")
                        .HasColumnType("TEXT");

                    b.Property<int>("ThemeId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<int>("ConversationId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("GroupWithPrevious")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Read")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("SendTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("SenderId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ConversationId");

                    b.HasIndex("SenderId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Report", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReportTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TargetId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TriggerId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TargetId");

                    b.HasIndex("TriggerId");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Completed")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatorId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TargetId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("TargetId");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.UserGroupRelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("JoinTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Muted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ReadTimeStamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("UserId");

                    b.ToTable("UserGroupRelations");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.GroupConversation", b =>
                {
                    b.HasBaseType("Aiursoft.Kahla.SDK.Models.Conversation");

                    b.Property<string>("GroupImagePath")
                        .HasColumnType("TEXT");

                    b.Property<string>("GroupName")
                        .HasColumnType("TEXT");

                    b.Property<string>("JoinPassword")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ListInSearchResult")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OwnerId")
                        .HasColumnType("TEXT");

                    b.HasIndex("OwnerId");

                    b.HasDiscriminator().HasValue("GroupConversation");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.PrivateConversation", b =>
                {
                    b.HasBaseType("Aiursoft.Kahla.SDK.Models.Conversation");

                    b.Property<string>("RequesterId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TargetId")
                        .HasColumnType("TEXT");

                    b.HasIndex("RequesterId");

                    b.HasIndex("TargetId");

                    b.HasDiscriminator().HasValue("PrivateConversation");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.At", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.Message", "Message")
                        .WithMany()
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "TargetUser")
                        .WithMany("ByAts")
                        .HasForeignKey("TargetUserId");

                    b.Navigation("Message");

                    b.Navigation("TargetUser");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Device", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "KahlaUser")
                        .WithMany("HisDevices")
                        .HasForeignKey("UserId");

                    b.Navigation("KahlaUser");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Message", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.Conversation", "Conversation")
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "Sender")
                        .WithMany("MessagesSent")
                        .HasForeignKey("SenderId");

                    b.Navigation("Conversation");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Report", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "Target")
                        .WithMany("ByReported")
                        .HasForeignKey("TargetId");

                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "Trigger")
                        .WithMany("Reported")
                        .HasForeignKey("TriggerId");

                    b.Navigation("Target");

                    b.Navigation("Trigger");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Request", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId");

                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "Target")
                        .WithMany()
                        .HasForeignKey("TargetId");

                    b.Navigation("Creator");

                    b.Navigation("Target");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.UserGroupRelation", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.GroupConversation", "Group")
                        .WithMany("Users")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "User")
                        .WithMany("GroupsJoined")
                        .HasForeignKey("UserId");

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.GroupConversation", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "Owner")
                        .WithMany("GroupsOwned")
                        .HasForeignKey("OwnerId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.PrivateConversation", b =>
                {
                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "RequestUser")
                        .WithMany("Friends")
                        .HasForeignKey("RequesterId");

                    b.HasOne("Aiursoft.Kahla.SDK.Models.KahlaUser", "TargetUser")
                        .WithMany("OfFriends")
                        .HasForeignKey("TargetId");

                    b.Navigation("RequestUser");

                    b.Navigation("TargetUser");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.Conversation", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.KahlaUser", b =>
                {
                    b.Navigation("ByAts");

                    b.Navigation("ByReported");

                    b.Navigation("Friends");

                    b.Navigation("GroupsJoined");

                    b.Navigation("GroupsOwned");

                    b.Navigation("HisDevices");

                    b.Navigation("MessagesSent");

                    b.Navigation("OfFriends");

                    b.Navigation("Reported");
                });

            modelBuilder.Entity("Aiursoft.Kahla.SDK.Models.GroupConversation", b =>
                {
                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
