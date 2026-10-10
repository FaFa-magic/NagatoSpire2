using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

/// <summary>Deployment-table presentation, independent of card selection and network state.</summary>
public partial class NagatoOrderTable : Control
{
    private static AtlasTexture? _commandPlate;
    public Control Design { get; private set; } = null!;
    public Control UndoVisual { get; private set; } = null!;
    public Control ResetVisual { get; private set; } = null!;
    public Texture2D CommandPlateTexture { get; private set; } = null!;
    public float LayoutScale => Math.Min(Size.X / 1920f, Size.Y / 1080f);

    public override void _Ready()
    {
        Design = GetNode<Control>("Design");
        UndoVisual = GetNode<Control>("Design/UndoVisual");
        ResetVisual = GetNode<Control>("Design/ResetVisual");
        // Trim generator padding at display time; the original alpha asset stays untouched.
        var texture = GetNode<TextureRect>("Design/UndoVisual/Plate").Texture;
        var atlas = _commandPlate ??= CreateCommandPlate(texture);
        CommandPlateTexture = atlas;
        GetNode<TextureRect>("Design/UndoVisual/Plate").Texture = atlas;
        GetNode<TextureRect>("Design/ResetVisual/Plate").Texture = atlas;
        Resized += UpdateLayout;
        UpdateLayout();
    }

    public void Configure(string guide, string hint, string undo, string reset)
    {
        GetNode<Label>("Design/Guide").Text = guide;
        GetNode<Label>("Design/Hint").Text = hint;
        UndoVisual.GetNode<Label>("Text").Text = undo;
        ResetVisual.GetNode<Label>("Text").Text = reset;
    }

    public void SetStatus(string status) => GetNode<Label>("Design/Status").Text = status;

    private static AtlasTexture CreateCommandPlate(Texture2D texture)
    {
        using var image = texture.GetImage();
        if (image.IsCompressed()) image.Decompress();
        image.Convert(Image.Format.Rgba8);
        byte[] pixels = image.GetData();
        int width = image.GetWidth(), height = image.GetHeight();
        int left = width, top = height, right = -1, bottom = -1;
        // Ignore imperceptible generator alpha specks, which GetUsedRect treats as padding content.
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (pixels[(y * width + x) * 4 + 3] < 16) continue;
            left = Math.Min(left, x); top = Math.Min(top, y);
            right = Math.Max(right, x); bottom = Math.Max(bottom, y);
        }
        return new AtlasTexture
        {
            Atlas = texture, FilterClip = true,
            Region = right < 0 ? new Rect2(0, 0, width, height) : new Rect2(left, top, right - left + 1, bottom - top + 1)
        };
    }

    private void UpdateLayout()
    {
        Design.Scale = Vector2.One * LayoutScale;
        Design.Position = (Size - new Vector2(1920f, 1080f) * LayoutScale) * 0.5f;
    }
}
