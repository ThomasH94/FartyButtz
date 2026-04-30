using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Database<TDB, TEntry> : SerializedScriptableObject where TDB : Database<TDB, TEntry> where TEntry : UnityEngine.Object
{
	[SerializeField]
	protected List<TEntry> entries = new List<TEntry>();

	protected Dictionary<int, TEntry> _byId;

	protected Dictionary<string, TEntry> _byName;

	private static TDB _instance;

	public static TDB Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
			TDB val = Resources.Load<TDB>("DB/" + typeof(TDB).Name);
			if (val != null)
			{
				_instance = val;
				_instance.BuildIndex();
				return _instance;
			}
			TDB[] array = Resources.LoadAll<TDB>("DB");
			if (array != null && array.Length != 0)
			{
				_instance = array[0];
				_instance.BuildIndex();
				if (array.Length > 1)
				{
					Debug.LogWarning("[" + typeof(TDB).Name + "] Multiple DB assets under Resources/DB; using '" + _instance.name + "'.");
				}
				return _instance;
			}
			TDB[] array2 = Resources.LoadAll<TDB>(string.Empty);
			if (array2 != null && array2.Length != 0)
			{
				_instance = array2[0];
				_instance.BuildIndex();
				if (array2.Length > 1)
				{
					Debug.LogWarning("[" + typeof(TDB).Name + "] Multiple DB assets in Resources; using '" + _instance.name + "'.");
				}
				return _instance;
			}
			Debug.LogError("[" + typeof(TDB).Name + "] No DB asset found. Place it at Resources/DB/" + typeof(TDB).Name + ".asset.");
			return null;
		}
		protected set
		{
			_instance = value;
		}
	}

	public IReadOnlyList<TEntry> Entries => entries;

	protected virtual void OnEnable()
	{
		if (_instance == null)
		{
			_instance = (TDB)this;
		}
		BuildIndex();
	}

	public virtual TEntry GetByID(int id)
	{
		if (_byId == null)
		{
			BuildIndex();
		}
		if (_byId == null || !_byId.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public virtual TEntry GetByName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return null;
		}
		if (_byName == null)
		{
			BuildIndex();
		}
		string key = NormalizeKey(name);
		if (_byName == null || !_byName.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public bool TryGetByName(string name, out TEntry entry)
	{
		entry = GetByName(name);
		return entry != null;
	}

	protected virtual void BuildIndex()
	{
		if (entries == null)
		{
			entries = new List<TEntry>();
		}
		_byId = new Dictionary<int, TEntry>();
		_byName = new Dictionary<string, TEntry>(StringComparer.OrdinalIgnoreCase);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (TEntry item in entries.Where((TEntry x) => x != null))
		{
			if (TryGetId(item, out var id))
			{
				if (_byId.ContainsKey(id))
				{
					num2++;
				}
				else
				{
					_byId[id] = item;
				}
			}
			else
			{
				num++;
			}
			if (!TryGetName(item, out var s))
			{
				continue;
			}
			string text = NormalizeKey(s);
			if (!string.IsNullOrEmpty(text))
			{
				if (_byName.ContainsKey(text))
				{
					num3++;
				}
				else
				{
					_byName[text] = item;
				}
			}
		}
		if (num > 0)
		{
			Debug.LogWarning($"[Database<{typeof(TDB).Name},{typeof(TEntry).Name}>] {num} entries missing int ID.");
		}
		if (num2 > 0)
		{
			Debug.LogWarning($"[Database<{typeof(TDB).Name},{typeof(TEntry).Name}>] {num2} duplicate IDs; keeping first.");
		}
		if (num3 > 0)
		{
			Debug.LogWarning($"[Database<{typeof(TDB).Name},{typeof(TEntry).Name}>] {num3} duplicate names (normalized); keeping first.");
		}
	}

	private static bool TryGetId(TEntry entry, out int id)
	{
		id = 0;
		if (entry == null)
		{
			return false;
		}
		Type type = entry.GetType();
		PropertyInfo property = type.GetProperty("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.PropertyType == typeof(int) && property.GetValue(entry, null) is int num)
		{
			id = num;
			return true;
		}
		FieldInfo field = type.GetField("ID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null && field.FieldType == typeof(int) && field.GetValue(entry) is int num2)
		{
			id = num2;
			return true;
		}
		return false;
	}

	private static bool TryGetName(TEntry entry, out string name)
	{
		name = null;
		if (entry == null)
		{
			return false;
		}
		Type type = entry.GetType();
		PropertyInfo property = type.GetProperty("displayName", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.PropertyType == typeof(string))
		{
			name = property.GetValue(entry, null) as string;
			if (!string.IsNullOrWhiteSpace(name))
			{
				return true;
			}
		}
		FieldInfo field = type.GetField("displayName", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null && field.FieldType == typeof(string))
		{
			name = field.GetValue(entry) as string;
			if (!string.IsNullOrWhiteSpace(name))
			{
				return true;
			}
		}
		property = type.GetProperty("Name", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.PropertyType == typeof(string))
		{
			name = property.GetValue(entry, null) as string;
			if (!string.IsNullOrWhiteSpace(name))
			{
				return true;
			}
		}
		field = type.GetField("Name", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null && field.FieldType == typeof(string))
		{
			name = field.GetValue(entry) as string;
			if (!string.IsNullOrWhiteSpace(name))
			{
				return true;
			}
		}
		if ((object)entry != null && !string.IsNullOrWhiteSpace(entry.name))
		{
			name = entry.name;
			return true;
		}
		return false;
	}

	private static string NormalizeKey(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return null;
		}
		return s.Trim().ToLowerInvariant().Replace(" ", "")
			.Replace("_", "")
			.Replace("-", "");
	}
}
