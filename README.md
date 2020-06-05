# AutoWire 

![AutoWire](https://github.com/travelgateX/dotnet-extensions-dependencyinjection-autowire/workflows/AutoWire/badge.svg)

Library to auto register dependencies and to auto bind option classes for Microsoft.Extensions.DependencyInjection

# Getting Started

Just call `AutoWire` on your service collection, like:
 
```csharp
serviceCollection.AutoWire(config);
``` 

Where `config` is of type `IConfiguration`.

Usually that's all you need. AutoWire will auto register types as services and options if the following applies:

#### For services

- They are classes, including generics with open type (`SomeType<>`).
- They belong to the entry assembly as obtained by `Assembly.GetEntryAsembly()`.
- They are not marked with or inherit `[IgnoreAutoWire]` attribute.
- They are not abstract or nested.
- They don't end with "Options". 

#### For options

- They are classes.
- They have a parameter-less constructor.
- Their name match a section from the provided `IConfiguration` object.

# Additional features

#### Use IncludePrefixed option

Using this option only classes under to the `IEnumerable<string>` namespace prefixes are added.

```csharp
var sc = new ServiceCollection();
sc.AutoWire(config, options => options.IncludePrefixed = new[] { typeof(SomeClass).Namespace });
```

For example, assuming that someClass belongs to `ProjectName.Folder`, all classes located in the
same namespace will be registered.

#### Use RegisterByAttribute option

```csharp
var sc = new ServiceCollection();
sc.AutoWire(config, options => options.RegisterByAttribute = true);
```

Using this option only classes that are marked with or inherit `[AutoWire]` are registered. For example:

```csharp
[AutoWire]
public class SomeClass {}
```

__NOTE: if both `IncludePrefixed` and `RegisterByAttribute`__, they are both applied and in this order. 

#### Ignoring classes

If you wish to ignore a class or derived classes from a base class or interface,
mark the class, base class or interface with `[IgnoreAutoWire]`, for example:

```csharp
[IgnoreAutoWire]
public class SomeClass {}
```

# Contribute
Yes please!
