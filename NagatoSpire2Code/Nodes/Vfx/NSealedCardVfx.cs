using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

public partial class NSealedCardVfx : Control
{
	public const string ScenePath = "res://NagatoSpire2/scenes/cards/overlays/afflictions/Sealed.tscn";

	private readonly List<Vector2[]> _cords = [];
	private float _age;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		BuildCords();
	}

	public override void _Process(double delta)
	{
		if (!IsVisibleInTree())
			return;
		_age += (float)delta;
		QueueRedraw();
	}

	public override void _Draw()
	{
		float entrance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp(_age / 0.42f, 0f, 1f));
		float pulse = 0.5f + 0.5f * Mathf.Sin(_age * 1.45f);
		float tightening = Mathf.Lerp(1.035f, 1f, entrance);
		Vector2 knot = new(150f, 151f);
		DrawSetTransformMatrix(Transform2D.Identity.Scaled(Vector2.One * tightening)
			.Translated(knot * (1f - tightening)));

		foreach (var cord in _cords)
		{
			DrawPolyline(cord, new Color(0.88f, 0.055f, 0.11f, entrance * (0.06f + pulse * 0.06f)), 18f, true);
			DrawPolyline(cord, new Color(0.11f, 0.018f, 0.03f, entrance * 0.9f), 8.3f, true);
			DrawPolyline(cord, new Color(0.56f, 0.035f, 0.072f, entrance), 6.1f, true);
			DrawPolyline(Offset(cord, -1.35f), new Color(0.94f, 0.16f, 0.19f, entrance), 1.35f, true);
			DrawPolyline(Offset(cord, 1.35f), new Color(0.73f, 0.07f, 0.115f, entrance), 1.5f, true);
			DrawPolyline(Offset(cord, -0.15f), new Color(1f, 0.55f, 0.34f, entrance * (0.5f + pulse * 0.12f)), 0.7f, true);
			DrawTwist(cord, entrance);
		}

		DrawPolyline(Bezier(new(134, 151), new(145, 141), new(157, 139), new(166, 150)),
			new Color(0.15f, 0.016f, 0.027f, entrance), 10f, true);
		DrawPolyline(Bezier(new(134, 151), new(145, 141), new(157, 139), new(166, 150)),
			new Color(0.85f, 0.08f, 0.13f, entrance), 6.3f, true);
		DrawPolyline(Bezier(new(137, 149), new(146, 142), new(155, 141), new(163, 148)),
			new Color(1f, 0.43f, 0.28f, entrance * 0.8f), 1.2f, true);
		DrawTassel(new(128f, 215f), -0.12f, entrance, pulse);
		DrawTassel(new(174f, 215f), 0.12f, entrance, pulse);
		DrawSetTransformMatrix(Transform2D.Identity);
	}

	private void BuildCords()
	{
		_cords.Clear();
		_cords.Add(Bezier(new(-5, 66), new(56, 97), new(230, 175), new(305, 236)));
		_cords.Add(Bezier(new(305, 66), new(244, 97), new(70, 175), new(-5, 236)));
		_cords.Add(Join(
			Bezier(new(149, 161), new(117, 147), new(88, 119), new(104, 103)),
			Bezier(new(104, 103), new(120, 84), new(153, 103), new(156, 132)),
			Bezier(new(156, 132), new(160, 160), new(165, 183), new(194, 179)),
			Bezier(new(194, 179), new(216, 173), new(199, 143), new(151, 143))));
		_cords.Add(Join(
			Bezier(new(151, 162), new(184, 147), new(213, 118), new(197, 103)),
			Bezier(new(197, 103), new(180, 84), new(147, 103), new(144, 133)),
			Bezier(new(144, 133), new(140, 162), new(135, 184), new(107, 179)),
			Bezier(new(107, 179), new(84, 173), new(101, 143), new(151, 143))));
		_cords.Add(Bezier(new(143, 164), new(143, 186), new(128, 194), new(128, 215)));
		_cords.Add(Bezier(new(159, 164), new(159, 186), new(174, 194), new(174, 215)));
	}

	private void DrawTwist(Vector2[] points, float opacity)
	{
		for (int i = 2; i < points.Length - 1; i += 4)
		{
			Vector2 tangent = (points[i + 1] - points[i - 1]).Normalized();
			Vector2 normal = new(-tangent.Y, tangent.X);
			DrawLine(points[i] - normal * 2.4f - tangent * 1.1f,
				points[i] + normal * 2.4f + tangent * 1.1f,
				new Color(0.24f, 0.018f, 0.045f, opacity * 0.45f), 0.8f, true);
		}
	}

	private void DrawTassel(Vector2 origin, float tilt, float opacity, float pulse)
	{
		float sway = Mathf.Sin(_age * 1.25f + origin.X * 0.02f) * 1.6f;
		Vector2 tip = origin + new Vector2(sway + tilt * 20f, 21f);
		DrawLine(origin, tip, new Color(0.18f, 0.018f, 0.028f, opacity), 11f, true);
		for (int i = -2; i <= 2; i++)
			DrawLine(origin + new Vector2(i * 1.4f, 0f), tip + new Vector2(i * 1.8f, 0f),
				new Color(0.78f + i * 0.025f, 0.065f + pulse * 0.02f, 0.12f, opacity), 1.3f, true);
		DrawLine(origin + new Vector2(-4f, 1f), origin + new Vector2(4f, 1f),
			new Color(0.86f, 0.59f, 0.24f, opacity), 3f, true);
	}

	private static Vector2[] Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		Vector2[] points = new Vector2[49];
		for (int i = 0; i < points.Length; i++)
		{
			float t = i / (float)(points.Length - 1);
			float u = 1f - t;
			points[i] = u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
		}
		return points;
	}

	private static Vector2[] Join(params Vector2[][] paths) =>
		paths.SelectMany((path, index) => index == 0 ? path : path.Skip(1)).ToArray();

	private static Vector2[] Offset(Vector2[] path, float distance)
	{
		Vector2[] result = new Vector2[path.Length];
		for (int i = 0; i < path.Length; i++)
		{
			Vector2 tangent = (path[Math.Min(path.Length - 1, i + 1)] - path[Math.Max(0, i - 1)]).Normalized();
			result[i] = path[i] + new Vector2(-tangent.Y, tangent.X) * distance;
		}
		return result;
	}
}
