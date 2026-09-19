namespace NagatoSpire2.NagatoSpire2Code.Characters;

public enum NagatoSkin
{
	Default,
	VariantTwo,
	VariantThree,
	VariantFour,
	VariantFive,
	VariantSix,
	VariantH
}

public sealed record NagatoSkinDefinition(
	NagatoSkin Id,
	string SpineSkeletonDataPath,
	string MerchantAnimPath,
	string RestSiteAnimPath);

public static class NagatoSkinManager
{
	private const string AnimationRoot = "res://NagatoSpire2/animations/characters/Nagato";
	private const string SceneRoot = "res://NagatoSpire2/scenes/characters";

	private static readonly IReadOnlyDictionary<NagatoSkin, NagatoSkinDefinition> Skins =
		new Dictionary<NagatoSkin, NagatoSkinDefinition>
		{
			[NagatoSkin.Default] = Create(NagatoSkin.Default, "changmen", ""),
			[NagatoSkin.VariantTwo] = Create(NagatoSkin.VariantTwo, "changmen_2", "_2"),
			[NagatoSkin.VariantThree] = Create(NagatoSkin.VariantThree, "changmen_3", "_3"),
			[NagatoSkin.VariantFour] = Create(NagatoSkin.VariantFour, "changmen_4", "_4"),
			[NagatoSkin.VariantFive] = Create(NagatoSkin.VariantFive, "changmen_5", "_5"),
			[NagatoSkin.VariantSix] = Create(NagatoSkin.VariantSix, "changmen_6", "_6"),
			[NagatoSkin.VariantH] = Create(NagatoSkin.VariantH, "changmen_h", "_h")
		};

	public static NagatoSkinDefinition GetDefinition(NagatoSkin skin) => Skins[skin];

	private static NagatoSkinDefinition Create(NagatoSkin id, string resourceName, string sceneSuffix)
	{
		return new(
			id,
			$"{AnimationRoot}/{resourceName}_spine.tres",
			$"{SceneRoot}/Nagato_merchant{sceneSuffix}.tscn",
			$"{SceneRoot}/Nagato_rest_site{sceneSuffix}.tscn");
	}
}
