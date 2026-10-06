namespace BikeShop.Api.Domain;

public sealed class BusinessRuleException(string message) : Exception(message);
