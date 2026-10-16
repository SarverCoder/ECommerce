namespace ECommerce.Application.Common.Exceptions;

public class ForbiddenException(string message = "You do not have access to this resource.") : Exception(message);
