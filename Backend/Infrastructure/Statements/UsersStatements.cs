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
                created_at_utc
            FROM
                users
            WHERE
                is_active = true
                AND email = @Email";
    }
}