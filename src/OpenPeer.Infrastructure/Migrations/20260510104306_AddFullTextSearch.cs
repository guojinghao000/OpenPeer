using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenPeer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Papers_SearchVector"" ON ""Papers"" USING GIN (to_tsvector('english', ""Title"" || ' ' || ""Abstract""));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Papers_SearchVector"";");
        }
    }
}
