# Emit C# Proxy Class at Runtime with ILGenerator

## Problem Description

In C#, we often run into objects or services that provide dynmaic method invocation by a single method like:

```cs
public abstract class ProxyBase
{
  protected abstract object Invoke(object someMethodRelatedInfo, object[] arguments);
}
```

It is good practice to create a proxy class that wraps all remote methods and call the ```Invoke``` method internally to provide strong typed interface.

Say we have a remote service. The first thing to do is declare its methods with interface:

```cs
public interface IFooService
{
  void MethodWithNoReturn();
  int MethodTakeParameterAndReturn(int a, int b);
}
```

Then we implement invocation method:

```cs
public class FooProxyBase : ProxyBase 
{
  protected override object Invoke(object someMethodRelatedInfo, object[] arguments)
  {
    // Pack to JSON and send via http
    // Or adapte and call other classes
    // Or whatever
  }
}
```

Finally we create a proxy class for IFooService:

```cs
public class FooService : FooProxyBase, IFooService
{
  #region Implement IFooService
  public void MethodWithNoReturn() 
  {
    Invoke("MethodWithNoReturn", new object[0]);
  }
  
  public int MethodTakeParameterAndReturn(int a, int b)
  {
    return Invoke("MethodTakeParameterAndReturn", new object[] { a, b });
  }
  #endregion
}
```

As you can see, the implementation of proxy class is quite trival but will take many work if methods are too many.

The point of interest here is to automatically generate ```FooService``` class at runtime. This goal can be achieved by various methods. This project in particular uses C#'s ```ILGenerator``` to emit IL code at runtime.

Instead manually writing ```FooService```, all you need is one line of code:
```cs

IFooService proxy = ProxyEmitter.CreateProxy<FooProxyBase, IFooService>(/*Constructor parameters are supported*/);

```

## How To Use

0. Include project in your solution, or add reference to compiled ```ProxyEmitter.dll```.
1. Make an base class that derives from ```ProxyBase``` and implement all abstract methods (just slightly differenct from above). You can create whatever constructors you want. ```ProxyEmitter``` will make sure the generated class have the same ones.
2. Declare an service interface for the service. You can provide additional namespace information by tagging it with ```ProxyNamespace``` attribute.
3. Call ```ProxyEmitter.CreateProxy<YourProxyBase, YourServiceInterface(/*constructor arguments*/)``` to get an instance of the generated proxy class.
4. That's all. Just do your stuff with the service.
