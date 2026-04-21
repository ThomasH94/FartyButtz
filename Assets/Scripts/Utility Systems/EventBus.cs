using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class EventBus
{
	private class Subscription
	{
		public Delegate Handler;

		public Delegate Predicate;

		public Subscription(Delegate handler, Delegate predicate)
		{
			Handler = handler;
			Predicate = predicate;
		}
	}

	private static readonly Dictionary<Type, List<Subscription>> _subscriptions = new Dictionary<Type, List<Subscription>>();

	private static readonly object _lock = new object();

	public static void Subscribe<T>(Action<T> handler, Func<T, bool> predicate = null)
	{
		lock (_lock)
		{
			Type typeFromHandle = typeof(T);
			if (!_subscriptions.TryGetValue(typeFromHandle, out var value))
			{
				value = new List<Subscription>();
				_subscriptions[typeFromHandle] = value;
			}
			value.Add(new Subscription(handler, predicate));
		}
	}

	public static void Unsubscribe<T>(Action<T> handler)
	{
		lock (_lock)
		{
			Type typeFromHandle = typeof(T);
			if (_subscriptions.TryGetValue(typeFromHandle, out var value))
			{
				value.RemoveAll((Subscription sub) => sub.Handler.Equals(handler));
				if (value.Count == 0)
				{
					_subscriptions.Remove(typeFromHandle);
				}
			}
		}
	}

	public static void Publish<T>(T msg)
	{
		List<Subscription> list = null;
		lock (_lock)
		{
			if (_subscriptions.TryGetValue(typeof(T), out var value))
			{
				list = new List<Subscription>(value);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (Subscription item in list)
		{
			try
			{
				if ((!(item.Predicate is Func<T, bool> func) || func(msg)) && item.Handler is Action<T> action)
				{
					action(msg);
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"EventBus handler threw for {typeof(T).Name}: {arg}");
			}
		}
	}

	public static UniTask<T> NextAsync<T>()
	{
		UniTaskCompletionSource<T> tcs = new UniTaskCompletionSource<T>();
		Action<T> handler = null;
		handler = delegate(T msg)
		{
			Unsubscribe(handler);
			tcs.TrySetResult(msg);
		};
		Subscribe(handler);
		return tcs.Task;
	}

	public static UniTask<T> NextAsync<T>(Func<T, bool> predicate = null)
	{
		UniTaskCompletionSource<T> tcs = new UniTaskCompletionSource<T>();
		Action<T> handler = null;
		handler = delegate(T msg)
		{
			try
			{
				if (predicate == null || predicate(msg))
				{
					Unsubscribe(handler);
					tcs.TrySetResult(msg);
				}
			}
			catch (Exception exception)
			{
				Unsubscribe(handler);
				tcs.TrySetException(exception);
			}
		};
		Subscribe(handler);
		return tcs.Task;
	}
}
