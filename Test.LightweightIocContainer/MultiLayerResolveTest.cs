﻿// Author: Gockner, Simon
// Created: 2021-12-09
// Copyright(c) 2021 SimonG. All Rights Reserved.

using JetBrains.Annotations;
using LightweightIocContainer;
using NUnit.Framework;

namespace Test.LightweightIocContainer;

[TestFixture]
public class MultiLayerResolveTest
{
    public interface IA
    {
        IB BProperty { get; }
    }
    
    public interface IB
    {
        C C { get; }
    }
    
    [UsedImplicitly]
    public interface IAFactory
    {
        IA Create();
    }
    
    [UsedImplicitly]
    public interface IBFactory
    {
        IB Create(C c);
    }

    [UsedImplicitly]
    private class A : IA
    {
        public A(IBFactory bFactory) => BProperty = bFactory.Create(new C("from A"));
        public IB BProperty { get; }
    }
    
    private class OtherA : IA
    {
        public OtherA(IB bProperty, IB secondB)
        {
            BProperty = bProperty;
            SecondB = secondB;
        }

        public IB BProperty { get; }
        public IB SecondB { get; }
    }

    [UsedImplicitly]
    private class B : IB
    {
        public B(C c) => C = c;

        public C C { get; }
    }
    
    [UsedImplicitly]
    public class C
    {
        public C(string test)
        {
            
        }
    }

    [Test]
    public void TestResolveFactoryAsCtorParameter()
    {
        IocContainer container = new();
        container.Register(r => r.Add<IA, A>().WithFactory<IAFactory>());
        container.Register(r => r.Add<IB, B>().WithFactory<IBFactory>());

        IA a = container.Resolve<IA>();
        Assert.IsInstanceOf<A>(a);
    }

    [Test]
    public void TestResolveSingleTypeRegistrationAsCtorParameter()
    {
        IocContainer container = new();
        container.Register(r => r.Add<IA, A>());
        container.Register(r => r.Add<IB, B>().WithFactory<IBFactory>());
        container.Register(r => r.Add<C>().WithFactoryMethod(_ => new C("test")));

        IB b = container.Resolve<IB>();
        Assert.IsInstanceOf<B>(b);
    }

    [Test]
    public void TestResolveSingletonTwiceAsCtorParameterInSameCtor()
    {
        IocContainer container = new();
        container.Register(r => r.Add<IA, OtherA>());
        container.Register(r => r.Add<IB, B>());
        container.Register(r => r.Add<C>(Lifestyle.Singleton).WithParameters("test"));

        OtherA a = container.Resolve<OtherA>();
        Assert.AreEqual(a.BProperty.C, a.SecondB.C);
    }
}