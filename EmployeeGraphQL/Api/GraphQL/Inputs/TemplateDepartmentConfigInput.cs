public record TemplateDepartmentConfigInput(
    long DepartmentId,
    long? OwnerRoleId,
    bool IsPrimary
);