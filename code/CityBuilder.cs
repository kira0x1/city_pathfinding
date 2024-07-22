namespace Kira;

[Category("Kira")]
public sealed class CityBuilder : Component
{
    [Property, Range(1, 10)]
    public int GridCols { get; set; } = 6;

    [Property, Range(1, 10)]
    public int GridRows { get; set; } = 6;

    [Property, Range(0, 20)]
    public float Offset { get; set; } = 0f;

    [Property, Range(1, 100)]
    public float GridScale { get; set; } = 20f;

    [Property, Range(0, 100), Category("Cell")]
    public float CellHeight { get; set; } = 1.0f;

    [Property, Category("Cell")]
    public Color CellColor { get; set; } = Color.White;

    [Property, Category("Cell")]
    public Color CellTextColor { get; set; } = Color.White;

    [Property, Category("Cell")]
    public Color HoverColor { get; set; } = Color.Green;

    private Vector2 MousePos { get; set; }
    public bool IsOnGridSlot { get; set; }

    public Vector2Int CellHovering { get; set; }
    public Vector2Int CellSelected { get; set; }

    private enum PivotModes
    {
        CENTER,
        TOPLEFT
    }

    [Property]
    private PivotModes PivotMode { get; set; }

    protected override void OnUpdate()
    {
        UpdateMousePos();
        DrawGrid();
        HandleGridHovering();
    }

    private void UpdateMousePos()
    {
        var cam = Scene.Components.GetAll<CameraComponent>().FirstOrDefault();

        if (cam == null)
        {
            Log.Info("city builder not found");
            return;
        }

        var ray = cam.ScreenPixelToRay(Mouse.Position);
        MousePos = ray.Position;
    }

    private void DrawGrid()
    {
        using (Gizmo.Scope("Grid"))
        {
            Gizmo.Draw.Color = CellColor;
            DrawCells();
        }
    }

    private void DrawCells()
    {
        for (int x = 0; x < GridRows; x++)
        {
            for (int y = 0; y < GridCols; y++)
            {
                DrawCell(x, y);
            }
        }
    }

    private void DrawCell(int x, int y)
    {
        // Set cell color on hover
        Color cellColor = IsOnGridSlot && CellHovering.x == x && CellHovering.y == y ? HoverColor : CellColor;
        var pos = Transform.LocalPosition;

        using (Gizmo.Scope("Grid"))
        {
            Gizmo.Draw.Color = cellColor;


            if (PivotMode == PivotModes.CENTER)
            {
                pos.x -= (GridScale + Offset) * GridRows / 2f;
                pos.y -= (GridScale + Offset) * GridCols / 2f;
            }

            pos.x += x * (GridScale + Offset);
            pos.y += y * (GridScale + Offset);

            var min = pos;
            min.z = pos.z;

            var max = pos + GridScale;
            max.z = pos.z + CellHeight;

            var box = new BBox(min, max);
            Gizmo.Draw.SolidBox(box);

            DrawGridLengths(box);
        }

        using (Gizmo.Scope("CellText"))
        {
            Gizmo.Draw.Color = CellTextColor;
            DrawGridText(x, y, pos);
        }
    }

    private void DrawGridLengths(BBox box)
    {
        Gizmo.Draw.Color = Color.Green;
        var linePos = Transform.Position;
        linePos.x -= 5f;
        linePos.y -= 5f;

        if (PivotMode == PivotModes.CENTER)
        {
            linePos.x -= (GridScale + Offset) * GridRows / 2f;
            linePos.y -= (GridScale + Offset) * GridCols / 2f;
        }

        Gizmo.Draw.Line(linePos, linePos.WithX(box.Maxs.x));
        Gizmo.Draw.Line(linePos, linePos.WithY(box.Maxs.y));
    }

    public void HandleGridHovering()
    {
        var mult = GridScale + Offset;

        // Calculate X
        var maxX = GridRows * mult;

        var mpos = MousePos;
        mpos -= new Vector2(Transform.Position.x, Transform.Position.y);

        var curX = mpos.x / maxX * GridRows;

        // Calculate Y
        var maxY = GridCols * mult;
        var curY = mpos.y / maxY * GridCols;

        // True if cursor is out of bounds i.e outside of the grid area
        var isOutX = curX > GridRows || curX < 0;
        var isOutY = curY > GridCols || curY < 0;

        // Cursor is inside the grid area
        IsOnGridSlot = !(isOutX || isOutY);

        if (!IsOnGridSlot) return;

        CellHovering = new Vector2Int(curX.FloorToInt(), curY.FloorToInt());
    }

    private void DrawGridText(int x, int y, Vector3 position)
    {
        var t = GameObject.Transform.World;
        float halfScale = GridScale / 2;
        var mult = GridScale + Offset;

        t.Position.x += x * mult + halfScale;
        t.Position.y += y * mult + halfScale;
        if (PivotMode == PivotModes.CENTER)
        {
            t.Position.x -= (GridScale + Offset) * GridRows / 2f;
            t.Position.y -= (GridScale + Offset) * GridCols / 2f;
        }


        var angles = t.Rotation.Angles();
        angles.yaw = 90;
        angles.pitch = 180;
        t.Rotation = angles.ToRotation();

        Gizmo.Draw.Text($"{x},{y}", t, size: 26);
    }
}
