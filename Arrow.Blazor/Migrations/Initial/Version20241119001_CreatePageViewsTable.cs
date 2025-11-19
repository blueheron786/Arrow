using FluentMigrator;

namespace Arrow.Blazor.Migrations.Initial;

[Migration(20241119001, "Create page_views table for analytics")]
public sealed class Version20241119001_CreatePageViewsTable : Migration
{
    public override void Up()
    {
        Create.Table("page_views")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("page_path").AsString(500).NotNullable()
            .WithColumn("viewed_at_utc").AsDateTimeOffset().NotNullable();

        Create.Index("ix_page_views_page_path_viewed_at")
            .OnTable("page_views")
            .OnColumn("page_path").Ascending()
            .OnColumn("viewed_at_utc").Ascending();
    }

    public override void Down()
    {
        Delete.Index("ix_page_views_page_path_viewed_at").OnTable("page_views");
        Delete.Table("page_views");
    }
}
