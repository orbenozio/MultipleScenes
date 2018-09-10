using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class Game : PersistableObject
{
	private const int SAVE_VERSION = 1;

	[SerializeField] private ShapeFactory _shapeFactory;
	
	[SerializeField] private KeyCode _createKey = KeyCode.C;
	[SerializeField] private KeyCode _newGameKey = KeyCode.N;
	[SerializeField] private KeyCode _saveKey = KeyCode.S;
	[SerializeField] private KeyCode _loadKey = KeyCode.L;
	[SerializeField] private KeyCode _destoryKey = KeyCode.X;
	
	[SerializeField] private PersistentStorage _storage;
	
	private List<Shape> _shapes;
	private float _creationProgress;
	private float _destructionProgress;

	public float CreationSpeed { get; set; }
	public float DestructionSpeed { get; set; }

	private void Awake()
	{
		_shapes = new List<Shape>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(_createKey))
		{
			CreateShape();
		}
		else if (Input.GetKeyDown(_newGameKey))
		{
			BeginNewGame();
		}
		else if (Input.GetKeyDown(_saveKey))
		{
			_storage.Save(this, SAVE_VERSION);
		}
		else if (Input.GetKeyDown((_loadKey)))
		{
			BeginNewGame();
			_storage.Load(this);
		}
		else if (Input.GetKeyDown((_destoryKey)))
		{
			DestroyShape();
		}

		_creationProgress += Time.deltaTime * CreationSpeed;

		while (_creationProgress >= 1f)
		{
			_creationProgress -= 1f;
			CreateShape();
		}
		
		_destructionProgress += Time.deltaTime * DestructionSpeed;

		while (_destructionProgress >= 1f)
		{
			_destructionProgress -= 1f;
			DestroyShape();
		}
	}

	private void DestroyShape()
	{
		if (_shapes.Count <= 0)
		{
			return;
		}

		var index = Random.Range(0, _shapes.Count);
		_shapeFactory.Reclaim(_shapes[index]);
		var lastIndex = _shapes.Count - 1;
		_shapes[index] = _shapes[lastIndex];
		_shapes.RemoveAt(lastIndex);
	}

	private void BeginNewGame()
	{
		foreach (var shape in _shapes)
		{
			_shapeFactory.Reclaim(shape);
		}

		_shapes.Clear();
	}

	private void CreateShape()
	{
		var instance = _shapeFactory.GetRandom();
		var t = instance.transform;
		
		t.localPosition = Random.insideUnitSphere * 5f;
		t.rotation = Random.rotation;
		t.localScale = Vector3.one * Random.Range(0.1f, 1f);
		instance.SetColor(Random.ColorHSV(
			hueMin: 0f, 
			hueMax: 1f, 
			saturationMin: 0.5f, 
			saturationMax: 1f, 
			valueMin: 0.25f, 
			valueMax: 1f, 
			alphaMin: 1f, 
			alphaMax: 1f));
		
		_shapes.Add(instance);
	}
	
	public override void Save (GameDataWriter writer) 
	{
		writer.Write(_shapes.Count);
		
		for (var i = 0; i < _shapes.Count; i++) 
		{
			writer.Write(_shapes[i].ShapeId);
			writer.Write(_shapes[i].MaterialId);
			_shapes[i].Save(writer);
		}
	}

	public override void Load(GameDataReader reader)
	{
		var version = reader.Version;	
		
		if (version > SAVE_VERSION) 
		{
			Debug.LogError("Unsupported future save version " + version);
			return;
		}
		
		var count = version <=0 ? -version : reader.ReadInt();
		
		for (var i = 0; i < count; i++)
		{
			var shapeId =  version > 0 ? reader.ReadInt() : 0;
			var materialId =  version > 0 ? reader.ReadInt() : 0;
			var instance = _shapeFactory.Get(shapeId, materialId); 
			instance.Load(reader);
			
			_shapes.Add(instance);
		}
	}
}
