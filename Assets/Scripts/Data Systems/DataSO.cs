using Sirenix.OdinInspector;
using UnityEngine;

// TBD - Add Excel Data Importing?

public abstract class DataSO : SerializedScriptableObject//, IDataImportListener
{
	[Tooltip("Unique integer ID for this data entry.")]
	public int ID;

	[Tooltip("Human-readable name for this data entry.")]
	public string displayName;

	private void OnEnable()
	{
		//EventBus.Subscribe<PreImportPayload>(OnPreImport);
		//EventBus.Subscribe<PostImportPayload>(OnPostImport);
	}

	private void OnDisable()
	{
		//EventBus.Unsubscribe<PreImportPayload>(OnPreImport);
		//EventBus.Unsubscribe<PostImportPayload>(OnPostImport);
	}

	/*
	public virtual void OnPreImport(PreImportPayload payload)
	{
	}

	public virtual void OnPostImport(PostImportPayload payload)
	{
	}
	*/
	
	public override string ToString()
	{
		return displayName;
	}
}
