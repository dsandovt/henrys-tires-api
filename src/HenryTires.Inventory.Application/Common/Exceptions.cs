namespace HenryTires.Inventory.Application.Common;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
