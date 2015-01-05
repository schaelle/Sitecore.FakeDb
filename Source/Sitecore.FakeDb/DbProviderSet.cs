﻿namespace Sitecore.FakeDb
{
  using Sitecore.Diagnostics;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  public class DbProviderSet : IDisposable
  {
    private readonly IDictionary<Type, Type> providers;

    private readonly IProviderSwitcherFactory switcherFactory;

    private readonly IList<IDisposable> switchers;

    private bool disposed;

    public DbProviderSet(IProviderSwitcherFactory switcherFactory)
    {
      Assert.ArgumentNotNull(switcherFactory, "switcherFactory");

      this.providers = new Dictionary<Type, Type>();
      this.switchers = new Collection<IDisposable>();
      this.switcherFactory = switcherFactory;
    }

    // TODO: Make it readonly.
    public IDictionary<Type, Type> Providers
    {
      get { return providers; }
    }

    protected IProviderSwitcherFactory SwitcherFactory
    {
      get { return this.switcherFactory; }
    }

    protected IList<IDisposable> Switchers
    {
      get { return this.switchers; }
    }

    public virtual void RegisterSwitcher(Type providerType, Type switcherType)
    {
      Assert.ArgumentNotNull(providerType, "providerType");
      Assert.ArgumentNotNull(switcherType, "switcherType");

      this.providers[providerType] = switcherType;
    }

    public virtual DbProviderSet Switch<TProvider>(TProvider provider)
    {
      Assert.ArgumentNotNull(provider, "provider");

      var providerType = typeof(TProvider);
      Assert.IsTrue(
        this.providers.ContainsKey(providerType),
        "Unable to switch the provider of type '{0}'. The switcher has not been registered.",
        providerType);

      var switcherType = this.providers[providerType];

      var switcher = this.switcherFactory.Create(switcherType, provider);
      this.switchers.Add(switcher);

      return this;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
      {
        return;
      }

      if (!disposing)
      {
        return;
      }

      foreach (var switcher in this.switchers)
      {
        switcher.Dispose();
      }

      this.disposed = true;
    }
  }
}