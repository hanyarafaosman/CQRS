# Validation Guide

## Overview
The CQRS.Mediator library includes built-in validation support through pipeline behaviors. Validators are automatically executed before request handlers, ensuring data integrity.

## Quick Start

### 1. Enable Validation

Add validation support to your service collection:

```csharp
services.AddMediator(typeof(Program).Assembly);
services.AddMediatorValidation(typeof(Program).Assembly);
```

### 2. Create a Validator

Implement `IValidator<TRequest>` for your request:

```csharp
using CQRS.Mediator.Validation;

public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public ValidationResult Validate(CreateUserCommand request)
    {
        return ValidateAsync(request).GetAwaiter().GetResult();
    }

    public Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationError(nameof(request.Name), "Name is required"));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(new ValidationError(nameof(request.Email), "Email is required"));
        }
        else if (!request.Email.Contains("@"))
        {
            errors.Add(new ValidationError(nameof(request.Email), "Email must be valid"));
        }

        return Task.FromResult(errors.Count > 0 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success());
    }
}
```

### 3. Handle Validation Errors

Validation errors are thrown as `ValidationException`:

```csharp
try
{
    var userId = await mediator.Send(new CreateUserCommand("", "invalid"));
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

## Validation Components

### IValidator<TRequest>
The interface all validators must implement:

```csharp
public interface IValidator<in TRequest>
{
    ValidationResult Validate(TRequest request);
    Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}
```

### ValidationResult
Represents the result of validation:

```csharp
public class ValidationResult
{
    public bool IsValid { get; }
    public List<ValidationError> Errors { get; }
    
    public static ValidationResult Success();
    public static ValidationResult Failure(params ValidationError[] errors);
    public static ValidationResult Failure(string propertyName, string errorMessage);
}
```

### ValidationError
Represents a single validation error:

```csharp
public class ValidationError
{
    public string PropertyName { get; }
    public string ErrorMessage { get; }
}
```

### ValidationException
Exception thrown when validation fails:

```csharp
public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }
}
```

## Multiple Validators

You can register multiple validators for the same request type. All validators will be executed:

```csharp
public class CreateUserBusinessRulesValidator : IValidator<CreateUserCommand>
{
    public async Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Business logic validation
        var userExists = await _repository.UserExistsAsync(request.Email);
        
        if (userExists)
        {
            return ValidationResult.Failure("Email", "User with this email already exists");
        }
        
        return ValidationResult.Success();
    }
}
```

Both validators will run and all errors will be collected.

## Validation Behavior

The `ValidationBehavior<TRequest, TResponse>` is a pipeline behavior that:

1. Resolves all validators for the request type
2. Executes all validators in parallel
3. Collects all validation errors
4. Throws `ValidationException` if any errors exist
5. Continues to the next behavior/handler if validation passes

## Best Practices

### 1. Separate Concerns
- **Input Validation**: Check for required fields, format, length
- **Business Rules**: Check against database, business logic

```csharp
// Input validator - fast, synchronous
public class CreateUserInputValidator : IValidator<CreateUserCommand>
{
    public Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken ct)
    {
        // Validate input format
    }
}

// Business rules validator - may be async, access database
public class CreateUserBusinessValidator : IValidator<CreateUserCommand>
{
    private readonly IUserRepository _repository;
    
    public async Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken ct)
    {
        // Check business rules
    }
}
```

### 2. Validation Helpers

Create reusable validation methods:

```csharp
public static class ValidationHelpers
{
    public static ValidationError RequiredField(string fieldName, string value)
    {
        return string.IsNullOrWhiteSpace(value) 
            ? new ValidationError(fieldName, $"{fieldName} is required")
            : null;
    }
    
    public static ValidationError EmailFormat(string email)
    {
        return !email.Contains("@") 
            ? new ValidationError("Email", "Email must be a valid email address")
            : null;
    }
}
```

### 3. Async Validators

Prefer async validation when you need to:
- Query databases
- Call external services
- Perform I/O operations

```csharp
public async Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken ct)
{
    var emailExists = await _repository.EmailExistsAsync(request.Email, ct);
    
    if (emailExists)
    {
        return ValidationResult.Failure("Email", "Email already registered");
    }
    
    return ValidationResult.Success();
}
```

## ASP.NET Core Integration

In a web API, handle validation exceptions globally:

```csharp
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                errors = validationEx.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
    });
});
```

## Testing Validators

Validators are easy to test in isolation:

```csharp
[Fact]
public async Task ValidateAsync_EmptyName_ReturnsError()
{
    // Arrange
    var validator = new CreateUserCommandValidator();
    var command = new CreateUserCommand("", "test@example.com");
    
    // Act
    var result = await validator.ValidateAsync(command);
    
    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "Name");
}
```

## Disabling Validation

To disable validation for specific requests, don't register validators for those request types, or use a custom pipeline:

```csharp
// Only add validation for specific assemblies
services.AddMediatorValidation(typeof(YourValidators).Assembly);
```

## Performance Considerations

- Validators run in parallel for better performance
- Input validation is typically fast (microseconds)
- Business rule validation may be slower (database queries)
- Consider caching for frequently validated business rules

## Example: Complex Validation

```csharp
public class CreateOrderValidator : IValidator<CreateOrderCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    
    public async Task<ValidationResult> ValidateAsync(CreateOrderCommand request, CancellationToken ct)
    {
        var errors = new List<ValidationError>();
        
        // Input validation
        if (request.Items.Count == 0)
        {
            errors.Add(new ValidationError("Items", "Order must have at least one item"));
        }
        
        // Business rules validation
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, ct);
        if (customer == null)
        {
            errors.Add(new ValidationError("CustomerId", "Customer not found"));
        }
        else if (!customer.IsActive)
        {
            errors.Add(new ValidationError("CustomerId", "Customer account is not active"));
        }
        
        // Validate each item
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
            if (product == null)
            {
                errors.Add(new ValidationError($"Items[{item.ProductId}]", "Product not found"));
            }
            else if (product.Stock < item.Quantity)
            {
                errors.Add(new ValidationError($"Items[{item.ProductId}]", "Insufficient stock"));
            }
        }
        
        return errors.Count > 0 
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }
}
```
