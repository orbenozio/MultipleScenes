using UnityEngine;

public class Shape : PersistableObject
{
    static int COLOR_PROPERTY_ID = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock SHARED_PROPERTY_BLOCK;

    private int _shapeId = int.MinValue;
    private Color _color;
    private MeshRenderer _meshRenderer;

    public int ShapeId
    {
        get { return _shapeId; }
        set
        {
            if (_shapeId == int.MinValue && value != int.MinValue)
            {
                _shapeId = value;                
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId.");
            }
        }
    }
    
    public int MaterialId { get; private set; }

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetMaterial(Material material, int materialId)
    {
        _meshRenderer.material = material;
        MaterialId = materialId;
    }

    public void SetColor(Color color)
    {
        _color = color;

        if (SHARED_PROPERTY_BLOCK == null)
        {
            SHARED_PROPERTY_BLOCK = new MaterialPropertyBlock();
        }
        
        SHARED_PROPERTY_BLOCK.SetColor(COLOR_PROPERTY_ID, color);
        _meshRenderer.SetPropertyBlock(SHARED_PROPERTY_BLOCK);
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(_color);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        SetColor(reader.Version > 0 ? reader.ReadColor(): Color.white);
    }
}
