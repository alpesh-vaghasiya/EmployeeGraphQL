using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using EmployeeGraphQL.Infrastructure.Data;
using EmployeeGraphQL.Domain.Entities;
using HotChocolate.Authorization;

namespace Api.GraphQL;

[Authorize]
public class Query
{
    // Employees
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Employee> GetEmployees(
        [Service] AppDbContext context)
        => context.Employees;

    [UseProjection]
    public IQueryable<Employee> GetEmployeeById(
        int id,
        [Service] AppDbContext context)
        => context.Employees.Where(e => e.Id == id);


    // Departments
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [AllowAnonymous]
    public IQueryable<Department> GetDepartments(
        [Service] AppDbContext context)
        => context.Departments;

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [AllowAnonymous]
    public IQueryable<Department> GetDepartmentsNoTracking(
        [Service] AppDbContext context)
        => context.Departments.AsNoTracking();

    [UseProjection]
    public IQueryable<Department> GetDepartmentById(
        int id,
        [Service] AppDbContext context)
        => context.Departments.Where(d => d.Id == id);
}