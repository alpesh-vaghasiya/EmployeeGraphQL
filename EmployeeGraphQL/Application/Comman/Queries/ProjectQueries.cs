public static class ProjectQueries
{
    public const string GetProjects = @"

        SELECT COUNT(*)
        FROM project p
        JOIN template t ON t.template_id = p.template_id
        WHERE EXISTS (
            SELECT 1
            FROM template_department_config td
            WHERE td.template_id = t.template_id
            AND td.department_id = @DepartmentId
        )
        AND (@Search IS NULL OR p.title ILIKE '%' || @Search || '%')
        AND (@Status IS NULL OR p.status = @Status);

        SELECT
            p.project_id AS ProjectId,
            p.title AS ProjectName,
            p.description,
            t.sampark_type_id::text AS SamparkType,

            COALESCE(pk.kcount,0) AS KaryakarCount,
            COALESCE(pf.fcount,0) AS FamilyCount,

            p.project_start_date AS StartDate,
            p.project_end_date AS EndDate,
            p.created_by AS CreatedBy,
            p.status AS Status

        FROM project p
        JOIN template t ON t.template_id = p.template_id

        LEFT JOIN (
            SELECT project_id, COUNT(*) kcount
            FROM project_karyakar
            GROUP BY project_id
        ) pk ON pk.project_id = p.project_id

        LEFT JOIN (
            SELECT project_id, COUNT(*) fcount
            FROM project_family
            GROUP BY project_id
        ) pf ON pf.project_id = p.project_id

        WHERE EXISTS (
            SELECT 1
            FROM template_department_config td
            WHERE td.template_id = t.template_id
            AND td.department_id = @DepartmentId
        )
        AND (@Search IS NULL OR p.title ILIKE '%' || @Search || '%')
        AND (@Status IS NULL OR p.status = @Status)

        ORDER BY {0} {1}

        LIMIT @PageSize OFFSET @Offset;

    ";
}