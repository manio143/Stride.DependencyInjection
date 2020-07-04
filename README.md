# Stride.DependencyInjection

This is an unofficial extension to Stride that allows for extended use of Inversion of Control with the Microsoft's DependencyInjection library.

License: MIT.

## Usage

First create dependencies

```csharp
public interface IDependency { }
public class MyDependency : IDependency { }
public class MyOtherDependency { }
```

Next create a static method that configures the IoC container (see [ASP.NET docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1) for more)

```csharp
public static class ContainerInitializer
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IDependency, MyDependency>();
        services.AddSingleton<MyOtherDependency>();
    }
}
```

In the per platform project, before running your game, configure the DependencyService

```csharp
static void Main(string[] args)
{
    using (var game = new Game())
    {
        game.ConfigureDependencyInjection(ContainerInitializer.ConfigureServices);
        game.Run();
    }
}
```

Now you can use `[Inject]` and `[InjectComponent]` attributes on fields and properties in your scripts.

```csharp
public class MyScript : SyncScrypt
{
    public override void Start()
    {
        this.InjectComponents();
        this.InjectDependencies();
    }

    [Inject]
    private IDependency dependency = null;

    [InjectComponent]
    private TransformComponent transform = null;

    public override void Update
    {
        // use dependecies
    }
}
```

_Note: the null assignment is to suppress a compiler warning which can't understand we're updating the field via Reflection._

If you have a serializable class that wants to join in on the injection you need to add a `[DependencyInjectable]` attribute on it. The injection will only happen if it is a member of an instance that calls `InjectDependecies()`.

```csharp
[DataContract]
[DependencyInjectable]
public class MyData
{
    [Inject]
    private MyOtherDependency deps = null;

    public string Data;

    public void ModifyMe()
    {
        deps.DoSomething();
    }
}

public class MyScript : StartupScript
{
    public MyData Data { get; set; }

    public override void Start()
    {
        this.InjectDependencies();
    }
}
```
