using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogapi.Migrations
{
    /// <inheritdoc />
    public partial class ChangePostContentToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
    ALTER TABLE "Posts"
    ALTER COLUMN "Content" TYPE jsonb
    USING jsonb_build_object(
        'type', 'doc',
        'content',
        jsonb_build_array(
            jsonb_build_object(
                'type', 'paragraph',
                'content',
                CASE
                    WHEN "Content" IS NULL OR btrim("Content") = ''
                    THEN '[]'::jsonb
                    ELSE jsonb_build_array(
                        jsonb_build_object(
                            'type', 'text',
                            'text', "Content"
                        )
                    )
                END
            )
        )
    );
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(JsonElement),
                oldType: "jsonb");
        }
    }
}
