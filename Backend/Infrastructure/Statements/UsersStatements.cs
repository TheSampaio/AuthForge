namespace Infrastructure.Statements
{
    public class UsersStatements
    {
        public const string SelectAll = @"
            SELECT
                id,
                first_name,
                last_name,
                email,
                password_hash,
                birthdate,
                is_active,
                is_platform_admin,
                created_at_utc
            FROM
                users
            WHERE
                is_active = true";

        public const string SelectById = @"
            SELECT
                id,
                first_name,
                last_name,
                email,
                password_hash,
                birthdate,
                is_active,
                is_platform_admin,
                created_at_utc
            FROM
                users
            WHERE
                is_active = true
                AND id = @Id";

        public const string SelectByEmail = @"
            SELECT
                id,
                first_name,
                last_name,
                email,
                password_hash,
                birthdate,
                is_active,
                is_platform_admin,
                created_at_utc
            FROM
                users
            WHERE
                is_active = true
                AND email = @Email";

        public const string ExistsPlatformAdmin = @"
            SELECT EXISTS(
                SELECT 1
                FROM users
                WHERE is_platform_admin = true
            )";
    }
}