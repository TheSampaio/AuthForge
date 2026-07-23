namespace Infrastructure.Statements
{
    public class ApplicationsStatements
    {
        public const string SelectById = @"
            SELECT
                id,
                name,
                client_id,
                client_secret,
                is_active,
                created_at_utc
            FROM
                applications
            WHERE
                is_active = true
                AND id = @Id";

        public const string SelectByClientId = @"
            SELECT
                id,
                name,
                client_id,
                client_secret,
                is_active,
                created_at_utc
            FROM
                applications
            WHERE
                is_active = true
                AND client_id = @ClientId";

        public const string SelectByUserId = @"
            SELECT
                a.id,
                a.name,
                a.client_id,
                a.is_active
            FROM
                applications a
                INNER JOIN user_applications ua ON a.id = ua.application_id
            WHERE
                a.is_active = true
                AND ua.is_active = true
                AND ua.user_id = @UserId
                AND ua.roles LIKE '%Admin%'";
    }
}