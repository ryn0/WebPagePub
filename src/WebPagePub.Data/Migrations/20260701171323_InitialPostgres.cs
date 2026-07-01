using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebPagePub.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AuthorUrl = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Author",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    LastName = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    AuthorBio = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PhotoOriginalUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhotoThumbUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhotoPreviewUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhotoFullScreenUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Author", x => x.AuthorId);
                });

            migrationBuilder.CreateTable(
                name: "BlockedIP",
                columns: table => new
                {
                    BlockedIPId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedIP", x => x.BlockedIPId);
                });

            migrationBuilder.CreateTable(
                name: "ClickLog",
                columns: table => new
                {
                    ClickLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    RefererUrl = table.Column<string>(type: "text", nullable: true),
                    IsBot = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClickLog", x => x.ClickLogId);
                });

            migrationBuilder.CreateTable(
                name: "ContentSnippet",
                columns: table => new
                {
                    ContentSnippetId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SnippetType = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentSnippet", x => x.ContentSnippetId);
                });

            migrationBuilder.CreateTable(
                name: "EmailSubscription",
                columns: table => new
                {
                    EmailSubscriptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsSubscribed = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSubscription", x => x.EmailSubscriptionId);
                });

            migrationBuilder.CreateTable(
                name: "LinkRedirection",
                columns: table => new
                {
                    LinkRedirectionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LinkKey = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    UrlDestination = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkRedirection", x => x.LinkRedirectionId);
                });

            migrationBuilder.CreateTable(
                name: "RedirectPath",
                columns: table => new
                {
                    RedirectPathId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Path = table.Column<string>(type: "text", nullable: true),
                    PathDestination = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectPath", x => x.RedirectPathId);
                });

            migrationBuilder.CreateTable(
                name: "SitePageAudit",
                columns: table => new
                {
                    SitePageAuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SitePageId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PageHeader = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BreadcrumbName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MetaKeywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExcludePageFromSiteMapXml = table.Column<bool>(type: "boolean", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    IsLive = table.Column<bool>(type: "boolean", nullable: false),
                    PublishDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MetaDescription = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    AllowsComments = table.Column<bool>(type: "boolean", nullable: false),
                    SitePageSectionId = table.Column<int>(type: "integer", nullable: false),
                    PageType = table.Column<int>(type: "integer", nullable: false),
                    ReviewItemName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ReviewRatingValue = table.Column<double>(type: "double precision", nullable: false),
                    ReviewWorstValue = table.Column<double>(type: "double precision", nullable: false),
                    ReviewBestValue = table.Column<double>(type: "double precision", nullable: false),
                    AuthorId = table.Column<int>(type: "integer", nullable: true),
                    IsSectionHomePage = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePageAudit", x => x.SitePageAuditId);
                });

            migrationBuilder.CreateTable(
                name: "SitePageSection",
                columns: table => new
                {
                    SitePageSectionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsHomePageSection = table.Column<bool>(type: "boolean", nullable: false),
                    BreadcrumbName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePageSection", x => x.SitePageSectionId);
                });

            migrationBuilder.CreateTable(
                name: "SiteSearchLogs",
                columns: table => new
                {
                    SiteSearchLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Term = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    PageNumber = table.Column<int>(type: "integer", nullable: false),
                    PageSize = table.Column<int>(type: "integer", nullable: false),
                    ResultsCount = table.Column<int>(type: "integer", nullable: false),
                    ClientIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Referer = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSearchLogs", x => x.SiteSearchLogId);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    Key = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(36)", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "character varying(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(36)", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(36)", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SitePage",
                columns: table => new
                {
                    SitePageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PageHeader = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BreadcrumbName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MetaKeywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExcludePageFromSiteMapXml = table.Column<bool>(type: "boolean", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    IsLive = table.Column<bool>(type: "boolean", nullable: false),
                    PublishDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MetaDescription = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    AllowsComments = table.Column<bool>(type: "boolean", nullable: false),
                    SitePageSectionId = table.Column<int>(type: "integer", nullable: false),
                    PageType = table.Column<int>(type: "integer", nullable: false),
                    ReviewItemName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ReviewRatingValue = table.Column<double>(type: "double precision", nullable: false),
                    ReviewWorstValue = table.Column<double>(type: "double precision", nullable: false),
                    ReviewBestValue = table.Column<double>(type: "double precision", nullable: false),
                    AuthorId = table.Column<int>(type: "integer", nullable: true),
                    IsSectionHomePage = table.Column<bool>(type: "boolean", nullable: false),
                    WordCount = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePage", x => x.SitePageId);
                    table.ForeignKey(
                        name: "FK_SitePage_Author_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Author",
                        principalColumn: "AuthorId");
                    table.ForeignKey(
                        name: "FK_SitePage_SitePageSection_SitePageSectionId",
                        column: x => x.SitePageSectionId,
                        principalTable: "SitePageSection",
                        principalColumn: "SitePageSectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SitePageComment",
                columns: table => new
                {
                    SitePageCommentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SitePageId = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    WebSite = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    Name = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CommentStatus = table.Column<byte>(type: "smallint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePageComment", x => x.SitePageCommentId);
                    table.ForeignKey(
                        name: "FK_SitePageComment_SitePage_SitePageId",
                        column: x => x.SitePageId,
                        principalTable: "SitePage",
                        principalColumn: "SitePageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SitePagePhoto",
                columns: table => new
                {
                    SitePagePhotoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PhotoOriginalUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhotoThumbUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhotoPreviewUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhotoFullScreenUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhotoFullScreenUrlHeight = table.Column<int>(type: "integer", nullable: false),
                    PhotoFullScreenUrlWidth = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SitePageId = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePagePhoto", x => x.SitePagePhotoId);
                    table.ForeignKey(
                        name: "FK_SitePagePhoto_SitePage_SitePageId",
                        column: x => x.SitePageId,
                        principalTable: "SitePage",
                        principalColumn: "SitePageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SitePageTag",
                columns: table => new
                {
                    SitePageId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePageTag", x => new { x.SitePageId, x.TagId });
                    table.ForeignKey(
                        name: "FK_SitePageTag_SitePage_SitePageId",
                        column: x => x.SitePageId,
                        principalTable: "SitePage",
                        principalColumn: "SitePageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SitePageTag_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Author_AuthorId",
                table: "Author",
                column: "AuthorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlockedIP_IpAddress",
                table: "BlockedIP",
                column: "IpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClickLog_CreateDate",
                table: "ClickLog",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContentSnippet_SnippetType",
                table: "ContentSnippet",
                column: "SnippetType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedirectPath_Path",
                table: "RedirectPath",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePage_AuthorId",
                table: "SitePage",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePage_SitePageSectionId_Key",
                table: "SitePage",
                columns: new[] { "SitePageSectionId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePageComment_SitePageId",
                table: "SitePageComment",
                column: "SitePageId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePagePhoto_SitePageId",
                table: "SitePagePhoto",
                column: "SitePageId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePageSection_Key",
                table: "SitePageSection",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePageTag_TagId",
                table: "SitePageTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSearchLogs_CreateDate",
                table: "SiteSearchLogs",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSearchLogs_Term",
                table: "SiteSearchLogs",
                column: "Term");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Key",
                table: "Tag",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BlockedIP");

            migrationBuilder.DropTable(
                name: "ClickLog");

            migrationBuilder.DropTable(
                name: "ContentSnippet");

            migrationBuilder.DropTable(
                name: "EmailSubscription");

            migrationBuilder.DropTable(
                name: "LinkRedirection");

            migrationBuilder.DropTable(
                name: "RedirectPath");

            migrationBuilder.DropTable(
                name: "SitePageAudit");

            migrationBuilder.DropTable(
                name: "SitePageComment");

            migrationBuilder.DropTable(
                name: "SitePagePhoto");

            migrationBuilder.DropTable(
                name: "SitePageTag");

            migrationBuilder.DropTable(
                name: "SiteSearchLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SitePage");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Author");

            migrationBuilder.DropTable(
                name: "SitePageSection");
        }
    }
}
