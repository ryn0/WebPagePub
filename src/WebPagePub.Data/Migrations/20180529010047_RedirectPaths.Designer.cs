﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;

namespace WebPagePub.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180529010047_RedirectPaths")]
    partial class RedirectPaths
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasMaxLength(36);

                    b.Property<string>("RoleId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.HasKey("UserId", "RoleId");

                    b.HasAlternateKey("RoleId", "UserId");

                    b.ToTable("AspNetUserRoles");

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityUserRole<string>");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36);

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("AuthorName")
                        .HasMaxLength(50);

                    b.Property<string>("AuthorUrl")
                        .HasMaxLength(150);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.BlockedIP", b =>
                {
                    b.Property<int>("BlockedIPId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(75);

                    b.HasKey("BlockedIPId");

                    b.HasIndex("IpAddress")
                        .IsUnique()
                        .HasFilter("[IpAddress] IS NOT NULL");

                    b.ToTable("BlockedIP");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.ClickLog", b =>
                {
                    b.Property<int>("ClickLogId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Headers");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(75);

                    b.Property<string>("Url");

                    b.HasKey("ClickLogId");

                    b.ToTable("ClickLog");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.ContentSnippet", b =>
                {
                    b.Property<int>("ContentSnippetId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("SnippetType");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ContentSnippetId");

                    b.HasIndex("SnippetType")
                        .IsUnique();

                    b.ToTable("ContentSnippet");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.EmailSubscription", b =>
                {
                    b.Property<int>("EmailSubscriptionId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(100);

                    b.Property<bool>("IsSubscribed");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("EmailSubscriptionId");

                    b.ToTable("EmailSubscription");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.LinkRedirection", b =>
                {
                    b.Property<int>("LinkRedirectionId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LinkKey")
                        .HasMaxLength(75);

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("UrlDestination");

                    b.HasKey("LinkRedirectionId");

                    b.ToTable("LinkRedirection");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.RedirectPath", b =>
                {
                    b.Property<int>("RedirectPathId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Path");

                    b.Property<string>("PathDestination");

                    b.HasKey("RedirectPathId");

                    b.HasIndex("Path")
                        .IsUnique()
                        .HasFilter("[Path] IS NOT NULL");

                    b.ToTable("RedirectPath");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.SitePageComment", b =>
                {
                    b.Property<int>("SitePageCommentId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comment");

                    b.Property<byte>("CommentStatus");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(75);

                    b.Property<string>("IpAddress")
                        .HasMaxLength(75);

                    b.Property<string>("Name")
                        .HasMaxLength(75);

                    b.Property<Guid>("RequestId");

                    b.Property<int>("SitePageId");

                    b.Property<string>("WebSite")
                        .HasMaxLength(255);

                    b.HasKey("SitePageCommentId");

                    b.HasIndex("SitePageId");

                    b.ToTable("SitePageComment");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.SitePageSection", b =>
                {
                    b.Property<int>("SitePageSectionId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BreadcrumbName")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("SitePageSectionId");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("SitePageSection");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.SitePage", b =>
                {
                    b.Property<int>("SitePageId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AllowsComments");

                    b.Property<string>("BreadcrumbName")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Content");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedByUserId")
                        .HasMaxLength(36);

                    b.Property<bool>("ExcludePageFromSiteMapXml");

                    b.Property<bool>("IsLive");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("MetaDescription")
                        .HasMaxLength(160);

                    b.Property<string>("MetaKeywords")
                        .HasMaxLength(255);

                    b.Property<string>("PageHeader")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("PageType");

                    b.Property<DateTime>("PublishDateTimeUtc");

                    b.Property<double>("ReviewBestValue");

                    b.Property<string>("ReviewItemName")
                        .HasMaxLength(160);

                    b.Property<double>("ReviewRatingValue");

                    b.Property<double>("ReviewWorstValue");

                    b.Property<int>("SitePageSectionId");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(80);

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("UpdatedByUserId")
                        .HasMaxLength(36);

                    b.HasKey("SitePageId");

                    b.HasIndex("SitePageSectionId", "Key")
                        .IsUnique();

                    b.ToTable("SitePage");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.SitePagePhoto", b =>
                {
                    b.Property<int>("SitePagePhotoId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description");

                    b.Property<bool>("IsDefault");

                    b.Property<string>("PhotoFullScreenUrl")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("PhotoFullScreenUrlHeight");

                    b.Property<int>("PhotoFullScreenUrlWidth");

                    b.Property<string>("PhotoPreviewUrl")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("PhotoThumbUrl")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("PhotoUrl")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("Rank");

                    b.Property<int>("SitePageId");

                    b.Property<string>("Title")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("SitePagePhotoId");

                    b.HasIndex("SitePageId");

                    b.ToTable("SitePagePhoto");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.SitePageTag", b =>
                {
                    b.Property<int>("SitePageId");

                    b.Property<int>("TagId");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("SitePageId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("SitePageTag");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Tag", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Key")
                        .HasMaxLength(75);

                    b.Property<string>("Name")
                        .HasMaxLength(75);

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("TagId");

                    b.HasIndex("Key")
                        .IsUnique()
                        .HasFilter("[Key] IS NOT NULL");

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("WebPagePub.Data.Models.ApplicationUserRole", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityUserRole<string>");


                    b.ToTable("ApplicationUserRole");

                    b.HasDiscriminator().HasValue("ApplicationUserRole");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("WebPagePub.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("WebPagePub.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebPagePub.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("WebPagePub.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebPagePub.Data.Models.Db.SitePageComment", b =>
                {
                    b.HasOne("WebPagePub.Data.Models.SitePage", "SitePage")
                        .WithMany("Comments")
                        .HasForeignKey("SitePageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebPagePub.Data.Models.SitePage", b =>
                {
                    b.HasOne("WebPagePub.Data.Models.Db.SitePageSection", "SitePageSection")
                        .WithMany()
                        .HasForeignKey("SitePageSectionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebPagePub.Data.Models.SitePagePhoto", b =>
                {
                    b.HasOne("WebPagePub.Data.Models.SitePage", "SitePage")
                        .WithMany("Photos")
                        .HasForeignKey("SitePageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebPagePub.Data.Models.SitePageTag", b =>
                {
                    b.HasOne("WebPagePub.Data.Models.SitePage", "SitePage")
                        .WithMany("SitePageTags")
                        .HasForeignKey("SitePageId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebPagePub.Data.Models.Tag", "Tag")
                        .WithMany("SitePageTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
