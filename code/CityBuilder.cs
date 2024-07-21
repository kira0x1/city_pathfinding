namespace Kira;

using System;

[Category("Kira")]
public sealed class CityBuilder : Component
{
    [Property, Range(1, 10)]
    public int GridCols { get; set; } = 6;

    [Property, Range(1, 10)]
    public int GridRows { get; set; } = 6;

    [Property, Range(0, 40)]
    public float Offset { get; set; } = 0f;

    [Property, Range(1, 500)]
    public float GridScale { get; set; } = 20f;

    [Property]
    public Color CellColor { get; set; } = Color.White;

    [Property]
    public Color CellTextColor { get; set; } = Color.White;

    [Property]
    public Color HoverColor { get; set; } = Color.Green;

    private Vector2 MousePos { get; set; }

    public bool IsOnGridSlot { get; set; }

    public Vector2Int CellHovering { get; set; }
    public Vector2Int CellSelected { get; set; }

    [Property, Range(0.1f, 5f)]
    public float TextScale { get; set; } = 1f;

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
                int cellX = x;
                int cellY = y;

                if (PivotMode == PivotModes.CENTER)
                {
                    cellX = x - GridRows;
                    cellY = y - GridCols;
                }

                DrawCell(cellX, cellY);
            }
        }
    }

    private void DrawCell(int x, int y)
    {
        // Set cell color on hover
        Color cellColor = IsOnGridSlot && CellHovering.x == x && CellHovering.y == y ? HoverColor : CellColor;

        // Cell
        using (Gizmo.Scope("Grid"))
        {
            Gizmo.Draw.Color = cellColor;

            var startpos = Transform.LocalPosition;
            var pos = startpos;

            if (PivotMode == PivotModes.CENTER)
            {
                pos.x += x * (GridScale + Offset) + (GridScale / 2f) * (GridCols / 2f);
                pos.y += y * (GridScale + Offset) + (GridScale / 2f) * (GridRows / 2f);
            }
            else
            {
                pos.x += x * (GridScale + Offset);
                pos.y += y * (GridScale + Offset);
            }


            var maxpos = pos + GridScale;
            maxpos.z = pos.z - 90f;
            pos.z -= 100f;
            var box = new BBox(pos, maxpos);
            // Gizmo.Draw.IgnoreDepth = true;
            // Gizmo.Draw.IgnoreDepth = true;
            Gizmo.Draw.SolidBox(box);
        }

        // Cell Text
        using (Gizmo.Scope("CellText"))
        {
            // Gizmo.Draw.IgnoreDepth = true;
            Gizmo.Draw.Color = CellTextColor;
            DrawGridText(x, y);
        }
    }

    public Vector3 GetPos(int x, int y)
    {
        var startpos = Transform.LocalPosition;

        if (PivotMode == PivotModes.CENTER)
        {
            startpos.x -= GridScale + Offset;
        }

        var pos = startpos;
        pos.x += x * (GridScale + Offset);
        pos.y += y * (GridScale + Offset);

        return pos;
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

    private void DrawGridText(int x, int y)
    {
        var t = GameObject.Transform.World;

        float halfScale = GridScale / 2;
        var mult = GridScale + Offset;

        Vector3 startPos = t.Position;

        if (PivotMode == PivotModes.CENTER)
        {
            startPos.x += x * mult + (GridScale + Offset + halfScale);
            startPos.y += y * mult + (GridScale + Offset + halfScale);
        }
        else
        {
            startPos.x += x * mult + halfScale;
            startPos.y += y * mult + halfScale;
        }

        t.Position = startPos;

        var angles = t.Rotation.Angles();
        angles.yaw = 90;
        angles.pitch = 180;
        t.Rotation = angles.ToRotation();

        Gizmo.Draw.WorldText($"{x},{y}", t.WithPosition(t.Position.WithZ(t.Position.z + 100f)), size: (GridScale / 2) / TextScale);
    }
}