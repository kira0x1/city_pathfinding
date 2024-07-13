namespace Kira;

[Category("Kira")]
public sealed class CityBuilder : Component
{
    [Property, Range(1, 10)]
    public int GridCols { get; set; } = 6;

    [Property, Range(1, 10)]
    public int GridRows { get; set; } = 6;

    [Property]
    public float Offset { get; set; } = 0f;

    [Property, Range(1, 100)]
    public float GridScale { get; set; } = 20f;

    [Property, Category("Lines")]
    public Color LineColor { get; set; } = Color.White;

    [Property, Category("Lines"), Range(0.1f, 10f)]
    public float LineThickness { get; set; } = 1f;

    private Vector2 MousePos { get; set; }

    protected override void OnUpdate()
    {
        UpdateMousePos();


        DrawGrid();
    }

    protected override void DrawGizmos()
    {
        DrawGrid();
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
        List<Line> gridLines = CreateGridLines();

        using (Gizmo.Scope("Grid"))
        {
            Gizmo.Draw.LineThickness = LineThickness;
            Gizmo.Draw.Color = LineColor;
            Gizmo.Draw.Lines(gridLines);
        }
    }

    private List<Line> CreateGridLines()
    {
        List<Line> lines = new List<Line>();

        for (int x = 0; x < GridRows; x++)
        {
            for (int y = 0; y < GridCols; y++)
            {
                DrawGridText(x, y);
                lines.Add(CreateLine(x, y, Vector3.Forward));
                lines.Add(CreateLine(x, y, Vector3.Left));
            }
        }

        // Left Boundry
        for (int i = 0; i < GridRows; i++)
        {
            lines.Add(CreateLine(i, GridCols, Vector3.Forward));
        }

        // Top Boundry
        for (int i = 0; i < GridCols; i++)
        {
            lines.Add(CreateLine(GridRows, i, Vector3.Left));
        }


        return lines;
    }

    public void GetGridNode(Vector2 pos)
    {
        // Check min and max boundry first
    }

    private void DrawGridText(int x, int y)
    {
        var t = GameObject.Transform.World;
        t.Position.x += x * (GridScale + Offset) + GridScale / 2;
        t.Position.y += y * (GridScale + Offset) + GridScale / 2;
        var angles = t.Rotation.Angles();
        angles.yaw = 90;
        angles.pitch = 180;
        t.Rotation = angles.ToRotation();

        Gizmo.Draw.WorldText($"{x},{y}", t, size: 22);
    }

    private Line CreateLine(int x, int y, Vector3 direction)
    {
        var pos = Transform.Position;
        pos.x += x * (GridScale + Offset);
        pos.y += y * (GridScale + Offset);
        var yline = new Line(pos, direction, GridScale);
        return yline;
    }
}