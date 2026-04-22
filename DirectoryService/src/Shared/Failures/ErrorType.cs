namespace Shared.Failures;

public enum ErrorType
{
    /// <summary>
    /// Ошибка валидации
    /// </summary>
    VALIDATION,

    /// <summary>
    /// Запись не найдена
    /// </summary>
    NOT_FOUND,

    /// <summary>
    /// Базовая ошибка
    /// </summary>
    FAILURE,

    /// <summary>
    /// Конфликт записей
    /// </summary>
    CONFLICT,

    /// <summary>
    /// Операция отменена
    /// </summary>
    CANCELED,
}