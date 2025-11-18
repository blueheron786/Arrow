using FluentMigrator;

namespace Arrow.Blazor.Migrations.Initial;

[Migration(20241117001, "Create users table")]
public sealed class Version20241117001_CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("email").AsString(320).NotNullable()
            .WithColumn("password_hash").AsString(500).NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable();

        Create.Index("ix_users_email")
            .OnTable("users")
            .OnColumn("email").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Index("ix_users_email").OnTable("users");
        Delete.Table("users");
    }
}
