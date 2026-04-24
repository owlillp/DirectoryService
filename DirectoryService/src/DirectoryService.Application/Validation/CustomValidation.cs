using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentValidation;
using Shared.Failures;

namespace DirectoryService.Application.Validation;

public static class CustomValidation
{
    extension<T, TElement>(IRuleBuilder<T, TElement> ruleBuilder)
    {
        public IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<TValueObject>(Func<TElement, Result<TValueObject, Errors>> factoryMethod)
        {
            return ruleBuilder.Custom((value, context) =>
            {
                Result<TValueObject, Errors> result = factoryMethod.Invoke(value);
                if (result.IsSuccess)
                    return;

                context.AddFailure(JsonSerializer.Serialize(result.Error));
            });
        }

        public IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<TValueObject>(Func<TElement, Result<TValueObject, Error>> factoryMethod)
        {
            return ruleBuilder.Custom((value, context) =>
            {
                Result<TValueObject, Error> result = factoryMethod.Invoke(value);
                if (result.IsSuccess)
                    return;

                context.AddFailure(JsonSerializer.Serialize(result.Error));
            });
        }
    }

    extension<T, TElement>(IRuleBuilder<T, TElement?> ruleBuilder)
    {
        public IRuleBuilderOptionsConditions<T, TElement?> MustBeNullableValueObject<TValueObject>(Func<TElement, Result<TValueObject, Errors>> factoryMethod)
        {
            return ruleBuilder.Custom((value, context) =>
            {
                if (value is null)
                    return;

                Result<TValueObject, Errors> result = factoryMethod.Invoke(value);
                if (result.IsSuccess)
                    return;

                context.AddFailure(JsonSerializer.Serialize(result.Error));
            });
        }

        public IRuleBuilderOptionsConditions<T, TElement?> MustBeNullableValueObject<TValueObject>(Func<TElement, Result<TValueObject, Error>> factoryMethod)
        {
            return ruleBuilder.Custom((value, context) =>
            {
                if (value is null)
                    return;

                Result<TValueObject, Error> result = factoryMethod.Invoke(value);
                if (result.IsSuccess)
                    return;

                context.AddFailure(JsonSerializer.Serialize(result.Error));
            });
        }
    }

    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> ruleBuilder, Error error)
        => ruleBuilder.WithMessage(JsonSerializer.Serialize(error));
}