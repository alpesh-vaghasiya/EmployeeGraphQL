using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Api.GraphQL.Inputs;
using EmployeeGraphQL.Infrastructure.Data;
using EmployeeGraphQL.Domain.Entities;
using System.Linq;
using HotChocolate.Authorization;
using FluentValidation;

namespace Api.GraphQL;

[Authorize]
public class Mutation
{
    // ---------------------------------------------------
    // EMPLOYEE MUTATIONS
    // ---------------------------------------------------

    // [UseDbContext(typeof(AppDbContext))]
    // public async Task<Employee> AddEmployee(AddEmployeeInput input, [Service] AppDbContext context)
    // {
    //     if (await context.Employees.AnyAsync(x => x.Name.Trim() == input.Name.Trim() && x.Email == input.Email.Trim()))
    //         throw new GraphQLException("Employee already exists.");

    //     // Validate project
    //     if (!await context.Projects.AnyAsync(x => input.ProjectIds != null && input.ProjectIds.Contains((int)x.ProjectId)))
    //         throw new GraphQLException($"Project with ID {input.ProjectIds} not found.");

    //     var emp = new Employee
    //     {
    //         Name = input.Name.Trim(),
    //         Email = input.Email.Trim(),
    //         Salary = input.Salary,
    //         DepartmentId = input.DepartmentId
    //     };

    //     context.Employees.Add(emp);
    //     await context.SaveChangesAsync();

    //     // Assign multiple projects
    //     if (input.ProjectIds != null)
    //     {
    //         var employeeProjects = await context.EmployeeProjects.Where(x => input.ProjectIds.Contains((int)x.ProjectId)).ToListAsync();

    //         foreach (var project in employeeProjects)
    //         {
    //             emp.EmployeeProjects.Add(project);
    //         }

    //         await context.SaveChangesAsync();
    //     }


    //     return emp;
    // }

    // // [UseDbContext(typeof(AppDbContext))]
    // public async Task<Employee> UpdateEmployee(int id, UpdateEmployeeInput input, [Service] AppDbContext context)
    // {
    //     var emp = await context.Employees.FindAsync(id);
    //     if (emp == null)
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Employee not found.").SetCode("VALIDATION_ERROR").Build());

    //     if (input.DepartmentId.HasValue)
    //     {
    //         if (!await context.Departments.AnyAsync(x => x.Id == input.DepartmentId.Value))
    //             throw new GraphQLException(ErrorBuilder.New().SetMessage("Department not found.").SetCode("VALIDATION_ERROR").Build());

    //     }

    //     if (input.ProjectIds != null && input.ProjectIds.Count > 0)
    //     {
    //         var dbProjects = await context.Projects.Where(x => input.ProjectIds.Contains((int)x.ProjectId)).Select(x => x.ProjectId).ToListAsync();
    //         var missing = input.ProjectIds.Except(dbProjects).ToList();

    //         if (missing.Any())
    //             throw new GraphQLException(ErrorBuilder.New().SetMessage($"Projects not found: {string.Join(", ", missing)}").SetCode("VALIDATION_ERROR").Build());
    //     }

    //     if (!string.IsNullOrWhiteSpace(input.Name)) emp.Name = input.Name.Trim();
    //     if (!string.IsNullOrWhiteSpace(input.Email)) emp.Email = input.Email.Trim();
    //     if (input.Salary.HasValue) emp.Salary = input.Salary.Value;
    //     if (input.DepartmentId.HasValue) emp.DepartmentId = input.DepartmentId.Value;

    //     // Project UPDATE logic (Add / Remove / Keep)
    //     if (input.ProjectIds != null)
    //     {
    //         // Load current projects
    //         await context.Entry(emp)
    //             .Collection(e => e.EmployeeProjects)
    //             .LoadAsync();

    //         var existingProjectIds = emp.EmployeeProjects.Select(p => p.ProjectId).ToList();

    //         // 1️⃣ Remove deleted projects
    //         var projectsToRemove = emp.EmployeeProjects
    //             .Where(p => !input.ProjectIds.Contains((int)p.ProjectId))
    //             .ToList();

    //         foreach (var project in projectsToRemove)
    //         {
    //             emp.EmployeeProjects.Remove(project);
    //         }

    //         // 2️⃣ Add new projects
    //         var projectsToAddIds = input.ProjectIds
    //             .Where(id => !existingProjectIds.Contains((int)id))
    //             .ToList();

    //         if (projectsToAddIds.Any())
    //         {
    //             var projectsToAdd = await context.EmployeeProjects
    //                 .Where(p => projectsToAddIds.Contains((int)p.ProjectId))
    //                 .ToListAsync();

    //             foreach (var project in projectsToAdd)
    //             {
    //                 emp.EmployeeProjects.Add(project);
    //             }
    //         }
    //     }

    //     await context.SaveChangesAsync();
    //     return emp;
    // }

    // // [UseDbContext(typeof(AppDbContext))]
    // public async Task<bool> DeleteEmployee(int id, [Service] AppDbContext context)
    // {
    //     var emp = await context.Employees.FindAsync(id);
    //     if (emp == null)
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Employee not found.").SetCode("VALIDATION_ERROR").Build());

    //     context.Employees.Remove(emp);
    //     await context.SaveChangesAsync();
    //     return true;
    // }

    // // ---------------------------------------------------
    // // MANY-TO-MANY: EMPLOYEE ↔ PROJECT
    // // ---------------------------------------------------

    // // [UseDbContext(typeof(AppDbContext))]
    // public async Task<bool> AssignEmployeeToProject(
    // int employeeId,
    // int projectId,
    // [Service] AppDbContext context)
    // {
    //     var employee = await context.Employees
    //         .Include(e => e.EmployeeProjects)
    //         .FirstOrDefaultAsync(e => e.Id == employeeId);

    //     if (employee == null)
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Employee not found.").SetCode("VALIDATION_ERROR").Build());

    //     var project = await context.EmployeeProjects
    //         .FirstOrDefaultAsync(p => p.ProjectId == projectId);

    //     if (project == null)
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Project not found.").SetCode("VALIDATION_ERROR").Build());

    //     // Check if already assigned
    //     if (!employee.EmployeeProjects.Any(p => p.ProjectId == projectId))
    //     {
    //         employee.EmployeeProjects.Add(project);
    //         await context.SaveChangesAsync();
    //     }

    //     return true;
    // }

    // // ---------------------------------------------------
    // // DEPARTMENT MUTATIONS
    // // ---------------------------------------------------

    // public async Task<Department> AddDepartment(
    // DepartmentInput input,
    // [Service] AppDbContext db,
    // [Service] IValidator<DepartmentInput> validator,
    //  CancellationToken cancellationToken
    //  )
    // {
    //     // FluentValidation
    //     var result = await validator.ValidateAsync(input, cancellationToken);

    //     if (!result.IsValid)
    //         throw new ValidationException(result.Errors);

    //     var name = input.Name.Trim();

    //     if (await db.Departments.AnyAsync(x => x.Name == name))
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Department name already exists.").SetCode("VALIDATION_ERROR").Build());

    //     var dept = new Department { Name = name };
    //     db.Departments.Add(dept);
    //     await db.SaveChangesAsync();
    //     return dept;
    // }

    // public async Task<Department> UpdateDepartment(UpdateDepartmentInput input, [Service] AppDbContext context)
    // {
    //     var dept = await context.Departments.FindAsync(input.Id);
    //     if (dept == null)
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Department not found.").SetCode("VALIDATION_ERROR").Build());

    //     var name = input.Name.Trim();

    //     if (await context.Departments.AnyAsync(x => x.Name == name && x.Id != input.Id))
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Department name already exists.").SetCode("VALIDATION_ERROR").Build());

    //     dept.Name = name;
    //     await context.SaveChangesAsync();
    //     return dept;
    // }

    // public async Task<bool> DeleteDepartment(int id, [Service] AppDbContext context)
    // {
    //     var dept = await context.Departments.FindAsync(id);
    //     if (dept == null)
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Department not found.").SetCode("VALIDATION_ERROR").Build());

    //     if (await context.Employees.AnyAsync(x => x.DepartmentId == id))
    //         throw new GraphQLException(ErrorBuilder.New().SetMessage("Cannot delete department. Employees are assigned.").SetCode("VALIDATION_ERROR").Build());

    //     context.Departments.Remove(dept);
    //     await context.SaveChangesAsync();
    //     return true;
    // }

    // ---------------------------------------------------
    // PROJECT MUTATIONS
    // ---------------------------------------------------

    public async Task<Project> AddProject(AddProjectInput input, [Service] AppDbContext context)
    {
        var name = input.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Project name cannot be empty.").SetCode("VALIDATION_ERROR").Build());

        var project = new Project
        {
            Title = name,
            Description = input.Description?.Trim()
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateProject(UpdateProjectInput input, [Service] AppDbContext context)
    {
        var project = await context.Projects.FindAsync(input.Id);
        if (project == null)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Project not found.").SetCode("VALIDATION_ERROR").Build());

        if (!string.IsNullOrWhiteSpace(input.Name))
            project.Title = input.Name.Trim();

        if (!string.IsNullOrWhiteSpace(input.Description))
            project.Description = input.Description.Trim();

        await context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteProject(int id, [Service] AppDbContext context)
    {
        var project = await context.Projects.FindAsync(id);
        if (project == null)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Project not found.").SetCode("VALIDATION_ERROR").Build());

        // if (await context.Employees.AnyAsync(x => x.ProjectId == id))
        //     throw new GraphQLException("Cannot delete project. Employees are assigned.");

        context.Projects.Remove(project);
        await context.SaveChangesAsync();
        return true;
    }

    [GraphQLName("uploadFileAsync")]
    public async Task<string> UploadFileAsync(IFile file)
    {
        if (file == null)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("File is required.").SetCode("VALIDATION_ERROR").Build());


        // ✅ Extension Validation
        if (System.IO.Path.GetExtension(file.Name).ToLower() != ".csv")
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Only CSV files are allowed.").SetCode("VALIDATION_ERROR").Build());


        // ✅ Size Validation (5MB)
        const long maxSize = 5 * 1024 * 1024; // 5MB

        if (file.Length > maxSize)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("File size must be less than 5MB.").SetCode("VALIDATION_ERROR").Build());

        int recordCount = 0;

        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            // Skip header (if exists)
            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                await reader.ReadLineAsync();
                recordCount++;
            }
        }

        // ✅ Record Limit Validation
        if (recordCount >= 200)
            throw new GraphQLException(ErrorBuilder.New().SetMessage("File contains too many records. Maximum allowed is 200.").SetCode("VALIDATION_ERROR").Build());

        // ✅ Save File
        var folder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(folder);

        var filePath = System.IO.Path.Combine(folder, file.Name);

        using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        return $"File uploaded successfully. Total records: {recordCount}";
    }
}