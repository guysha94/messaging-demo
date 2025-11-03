namespace MessagingDemo.Utils;

public static class Generics
{
    public static MethodInfo CreateGenericInstanceMethod(
        Type type,
        string methodName,
        Type genericTargetType,
        params Type[] genericTypeArguments
    )
    {
        MethodInfo? method;
        if (genericTypeArguments.Length == 0)
            method = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 0);
        else
            method = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.IsGenericMethod
                    && m.GetParameters()
                        .Select(p => p.ParameterType)
                        .AreSetEqual(genericTypeArguments));

        if (method is null)
            throw new InvalidOperationException(
                $"Could not find method {methodName} in type {type} with generic arguments {string.Join(", ", genericTypeArguments.Select(t => t.Name))}.");
        return method.MakeGenericMethod(genericTargetType);
    }

    public static ConstructorInfo CreateGenericConstructor(
        Type type,
        Type genericTargetType,
        params Type[] genericTypeArguments
    )
    {
        if (!type.IsGenericType)
            throw new ArgumentException("Type must be a generic type.", nameof(type));

        var genericType = type
            .MakeGenericType(genericTargetType);

        if (genericType is null)
            throw new InvalidOperationException(
                $"Failed to create generic type {type} with target type {genericTargetType}.");
        var constructor = genericType.GetConstructor(genericTypeArguments);
        if (constructor is null)
            throw new InvalidOperationException(
                $"No suitable constructor found for type {genericType} with arguments {string.Join(", ", genericTypeArguments.Select(t => t.Name))}.");
        return constructor;
    }

    public static object CreateGenericInstance(
        Type type,
        Type genericTargetType,
        Type[]? genericTypeArguments = null,
        params object[]? constructorArguments
    )
    {
        var constructor = CreateGenericConstructor(type, genericTargetType, genericTypeArguments ?? []);
        return constructor.Invoke(constructorArguments);
    }

    public static MethodInfo CreateGenericStaticMethod(
        Type type,
        string methodName,
        Type genericTargetType,
        params Type[] genericTypeArguments
    )
    {
        MethodInfo? method;
        if (genericTypeArguments.Length == 0)
            method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.IsGenericMethod &&
                    m.GetParameters().Length == 0);
        else
            method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.IsGenericMethod
                    && m.GetParameters()
                        .Select(p => p.ParameterType)
                        .AreSetEqual(genericTypeArguments));

        if (method is null)
            throw new InvalidOperationException(
                $"Could not find method {methodName} in type {type} with generic arguments {string.Join(", ", genericTypeArguments.Select(t => t.Name))}.");
        return method.MakeGenericMethod(genericTargetType);
    }

    public static Type CreateGenericType(
        Type type,
        params Type[] genericTypeArguments
    )
    {
        if (!type.IsGenericType)
            throw new ArgumentException("Type must be a generic type.", nameof(type));
        if (genericTypeArguments.Length == 0)
            throw new ArgumentException("At least one generic type argument is required.",
                nameof(genericTypeArguments));

        var genericType = type.MakeGenericType(genericTypeArguments);
        if (genericType is null)
            throw new InvalidOperationException(
                $"Failed to create generic type {type} with arguments {string.Join(", ", genericTypeArguments.Select(t => t.Name))}.");
        return genericType;
    }

    public static Type GetGenericTypeArgument(
        Type type,
        Type genericTypeDefinition,
        int index = 0
    )
    {
        return GetGenericTypeArguments(type, genericTypeDefinition)[index];
    }

    public static Type[] GetGenericTypeArguments(
        Type type,
        Type genericTypeDefinition
    )
    {
        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericTypeDefinition)?
            .GetGenericArguments() ?? [];
    }
}