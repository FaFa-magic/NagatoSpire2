using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using NagatoSpire2.NagatoSpire2Code.Characters;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

public static class NagatoChoicePointingImage
{
    public static IEnumerable<string> AssetPaths =>
    [
        "res://NagatoSpire2/images/characters/hands/multiplayer_hand_nagato_point.png",
        "res://NagatoSpire2/images/characters/feet/multiplayer_foot_nagato_point.png"
    ];

    public static void Configure(NHandImage hand)
    {
        if (hand.Player.Character is not NagatoCharacter)
            return;

        var texture = hand.GetNode<TextureRect>("TextureRect");
        Vector2 pivot = texture.Texture?.ResourcePath switch
        {
            "res://NagatoSpire2/images/characters/hands/multiplayer_hand_nagato_point.png" => new(130.25f, 19f),
            "res://NagatoSpire2/images/characters/feet/multiplayer_foot_nagato_point.png" => new(123.25f, 7.5f),
            _ => Vector2.Zero
        };
        if (pivot == Vector2.Zero)
            return;

        texture.Size = new Vector2(211f, 600f);
        texture.Position = -pivot;
        texture.PivotOffset = pivot;
    }
}
