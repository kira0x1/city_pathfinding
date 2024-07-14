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
    public Vector2Int GridSelecting { get; set; }

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
                using (Gizmo.Scope("Grid"))
                {
                    if (IsOnGridSlot && (GridSelecting.x == x && GridSelecting.y == y))
                    {
                        Gizmo.Draw.Color = HoverColor;
                    }
                    else
                    {
                        Gizmo.Draw.Color = CellColor;
                    }

                    var pos = Transform.LocalPosition;
                    pos.x += x * (GridScale + Offset);
                    pos.y += y * (GridScale + Offset);

                    var box = new BBox(pos, pos + GridScale);
                    Gizmo.Draw.SolidBox(box);
                }

                using (Gizmo.Scope("CellText"))
                {
                    Gizmo.Draw.Color = CellTextColor;
                    DrawGridText(x, y);
                }
            }
        }
    }

    private List<Line> CreateGridLines()
    {
        List<Line> lines = new List<Line>();

        for (int x = 0; x < GridRows; x++)
        {
            for (int y = 0; y < GridCols; y++)
            {
                if (IsOnGridSlot && (GridSelecting.x == x && GridSelecting.y == y))
                {
                    Gizmo.Draw.Color = Color.Green;
                }
                else
                {
                    Gizmo.Draw.Color = CellColor;
                }

                using (Gizmo.Scope("Grid"))
                {
                    DrawGridText(x, y);
                }

                if (y > 0) lines.Add(CreateLine(x, y, Vector3.Forward));
                if (x > 0) lines.Add(CreateLine(x, y, Vector3.Left));

                // lines.Add(CreateLine(x, y, Vector3.Forward));
                // lines.Add(CreateLine(x, y + 1, Vector3.Forward));
                // lines.Add(CreateLine(x, y, Vector3.Left));
                // lines.Add(CreateLine(x + 1, y, Vector3.Left));
            }
        }

        return lines;
    }

    private Line CreateLine(int x, int y, Vector3 direction)
    {
        var pos = Transform.LocalPosition;
        pos.x += x * (GridScale + Offset);
        pos.y += y * (GridScale + Offset);
        var yline = new Line(pos, direction, GridScale);
        return yline;
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

        GridSelecting = new Vector2Int(curX.FloorToInt(), curY.FloorToInt());
    }

    private void DrawGridLengths(float maxX, float maxY)
    {
        Gizmo.Draw.Color = Color.Green;
        var linePos = Transform.Position;
        linePos.x -= 20f;
        linePos.y -= 20f;

        Gizmo.Draw.Line(linePos, linePos.WithX(Transform.Position.x + maxX));
        Gizmo.Draw.Line(linePos, linePos.WithY(Transform.Position.y + maxY));
    }

    private void DrawGridText(int x, int y)
    {
        var t = GameObject.Transform.World;
        float halfScale = GridScale / 2;
        var mult = GridScale + Offset;

        t.Position.x += x * mult + halfScale;
        t.Position.y += y * mult + halfScale;

        var angles = t.Rotation.Angles();
        angles.yaw = 90;
        angles.pitch = 180;
        t.Rotation = angles.ToRotation();

        Gizmo.Draw.Text($"{x},{y}", t, size: 26);
    }
}