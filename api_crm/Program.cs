using api_crm.Services;
using api_crm.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using api_crm.Attributes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<DbHandler>();

var app = builder.Build();

ValidationContextProvider.Initialize(app.Services);

app.MapGet("/api/crm/customer/balance/{customerCode}", async (string customerCode, DbHandler dbHandler) =>
{
    if (customerCode == null)
    {
        return Results.BadRequest(new ApiResponse(400, "Customer code is required", DBNull.Value).ToString());
    }

    if (customerCode.Length != 6 || !customerCode.All(char.IsDigit))
    {
        return Results.BadRequest(new ApiResponse(400, "Invalid customer code", DBNull.Value).ToString());
    }

    try
    {
        var balance = await dbHandler.GetCustomerBalanceSingleAsync(customerCode);
        return Results.Ok(new ApiResponse(200, "OK", balance));
    }
    catch
    {
        return Results.Problem(new ApiResponse(500, "An error occurred while getting the customer's balance", DBNull.Value).ToString());
    }
});

app.MapGet("/api/crm/document/data", async (string documentTypeNumber, bool useTestEnvironment, DbHandler dbHandler) =>
{
    if (documentTypeNumber == null)
    {
        return Results.BadRequest(new ApiResponse(400, "Document type & number is required", DBNull.Value).ToString());
    }

    try
    {
        var documentData = await dbHandler.GetDocumentDataAsync(documentTypeNumber, useTestEnvironment);
        return Results.Ok(new ApiResponse(200, "OK", documentData));
    }
    catch
    {
        return Results.Problem(new ApiResponse(500, "An error occurred while retrieving document data", DBNull.Value).ToString());
    }
});

app.MapGet("/api/crm/report/aging", async (DbHandler dbHandler) =>
{
    try
    {
        var agingReports = await dbHandler.GetAllAgingReportsAsync();
        return Results.Ok(new ApiResponse(200, "OK", agingReports));
    }
    catch
    {
        return Results.Problem(new ApiResponse(500, "An error occurred while retrieving aging report", DBNull.Value).ToString());
    }
});

app.MapPost("/api/crm/customer/create", async ([FromBody] CreateCustomerRequest request, DbHandler dbHandler) =>
{
    var validationResults = new List<ValidationResult>();

    if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
    {
        var errors = validationResults.Select(r => r.ErrorMessage).ToList();
        return Results.BadRequest(new ApiResponse(400, "Validation failed", errors));
    }

    try
    {
        var createdCustomerCode = await dbHandler.CreateCustomerAsync(request);
        return Results.Ok(new ApiResponse(201, "Customer created successfully", createdCustomerCode));
    }
    catch (Exception ex)
    {
        return Results.Problem(new ApiResponse(500, "An error occurred while creating the customer", ex.Message).ToString());
    }
});

app.MapPatch("/api/crm/customer/update", async ([FromBody] UpdateCustomerRequest request, DbHandler dbHandler) =>
{
    var validationResults = new List<ValidationResult>();

    if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
    {
        var errors = validationResults.Select(r => r.ErrorMessage).ToList();
        return Results.BadRequest(new ApiResponse(400, "Validation failed", errors));
    }

    try
    {
        await dbHandler.UpdateCustomerAsync(request);
        return Results.Ok(new ApiResponse(200, "Customer updated successfully", request.CustomerCode));
    }
    catch (Exception ex)
    {
        return Results.Problem(new ApiResponse(500, "An error occurred while updating the customer", ex.Message).ToString());
    }
});

app.Run();
