namespace ECommerce.Application.Common.Exceptions;

public class UnauthorizedException(string message = "Invalid credentials.") : Exception(message);
