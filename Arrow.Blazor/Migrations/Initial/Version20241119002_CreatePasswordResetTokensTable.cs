using FluentMigrator;

namespace Arrow.Blazor.Migrations.Initial;

[Migration(20241119002, "Create password_reset_tokens table")]
public sealed class Version20241119002_CreatePasswordResetTokensTable : Migration
{
    public override void Up()
    {
        Create.Table("password_reset_tokens")
            .WithColumn("token").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("expires_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("is_used").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Index("ix_password_reset_tokens_user_id")
            .OnTable("password_reset_tokens")
            .OnColumn("user_id").Ascending();

        Create.Index("ix_password_reset_tokens_expires_at")
            .OnTable("password_reset_tokens")
            .OnColumn("expires_at_utc").Ascending();

        Create.ForeignKey("fk_password_reset_tokens_user_id")
            .FromTable("password_reset_tokens").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_password_reset_tokens_user_id").OnTable("password_reset_tokens");
        Delete.Index("ix_password_reset_tokens_expires_at").OnTable("password_reset_tokens");
        Delete.Index("ix_password_reset_tokens_user_id").OnTable("password_reset_tokens");
        Delete.Table("password_reset_tokens");
    }
}
