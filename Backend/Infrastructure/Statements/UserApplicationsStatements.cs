namespace Infrastructure.Statements
{
    public class UserApplicationsStatements
    {
        public const string SelectGrant = @"
            SELECT
                id,
                user_id,
                application_id,
                roles,
                is_active,
                created_at_utc
            FROM
                user_applications
            WHERE
                user_id = @UserId
                AND application_id = @ApplicationId";
    }
}