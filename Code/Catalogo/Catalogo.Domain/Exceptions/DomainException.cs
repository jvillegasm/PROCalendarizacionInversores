namespace Catalogo.Domain.Exceptions;

/// <summary>
/// Clase base para todas las excepciones de dominio del Catálogo Service.
/// Permite al middleware global distinguir errores de negocio de errores técnicos.
/// </summary>
public abstract class DomainException(string message) : Exception(message);
