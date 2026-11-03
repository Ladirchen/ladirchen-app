using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebPushNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebPushSubscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EndpointHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    P256dh = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Auth = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebPushSubscriptions", x => x.SubscriptionId);
                });

            migrationBuilder.CreateTable(
                name: "WebPushNotifications",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TitleDe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyDe = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Tag = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebPushNotifications", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_WebPushNotifications_WebPushSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "WebPushSubscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebPushNotifications_PublishedAt_NextAttemptAt",
                table: "WebPushNotifications",
                columns: new[] { "PublishedAt", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WebPushNotifications_SubscriptionId",
                table: "WebPushNotifications",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebPushSubscriptions_EndpointHash",
                table: "WebPushSubscriptions",
                column: "EndpointHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebPushSubscriptions_FamilyId_MemberId",
                table: "WebPushSubscriptions",
                columns: new[] { "FamilyId", "MemberId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebPushNotifications");

            migrationBuilder.DropTable(
                name: "WebPushSubscriptions");
        }
    }
}
