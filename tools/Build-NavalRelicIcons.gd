extends SceneTree
## Mechanical size/alpha exports only; artwork is generated separately.

const PROJECT = "D:/GitClone/NagatoSpire2/"
const SOURCE = PROJECT + "art_sources/icons/naval_batch_20261018/"
const RELICS = PROJECT + "NagatoSpire2/images/relics/"
const NAMES = ["CrossfireDirector", "ZFlag"]
const DEFAULT_MASTERS = {
	"CrossfireDirector": PROJECT + "art_sources/icons/naval_refit_20261018/CrossfireDirector_master.png",
	"ZFlag": SOURCE + "ZFlag_master.png",
}
const OUTLINE_RADIUS = 2.25 # Visible pink ring at 88px, measured against both torii relics.

func sample_outline_color() -> Color:
	# Sample the actual opaque pink ring, not a guessed white/magenta UI outline.
	var reference = Image.load_from_file(RELICS + "outline/BigShipsBigGuns.png")
	assert(reference != null and reference.get_size() == Vector2i(88, 88))
	var counts: Dictionary = {}
	for y in 88:
		for x in 88:
			var pixel = reference.get_pixel(x, y)
			if pixel.a > 0.95 and pixel.r > 0.9 and pixel.g > 0.3 and pixel.g < 0.55 and pixel.b > 0.6:
				var rgb = pixel.to_rgba32()
				counts[rgb] = counts.get(rgb, 0) + 1
	assert(not counts.is_empty(), "Reference relic has no pink outline")
	var best: int = 0
	for candidate in counts:
		if best == 0 or counts[candidate] > counts[best]:
			best = candidate
	var result = Color.hex(best)
	result.a = 1.0
	return result

func fit_artwork(source: Image, pixels: int, content_pixels: int) -> Image:
	var used = source.get_used_rect()
	assert(used.has_area())
	var artwork = source.get_region(used)
	artwork.convert(Image.FORMAT_RGBA8)
	artwork.fix_alpha_edges()
	var factor = float(content_pixels) / max(used.size.x, used.size.y)
	artwork.resize(maxi(1, roundi(used.size.x * factor)), maxi(1, roundi(used.size.y * factor)), Image.INTERPOLATE_LANCZOS)
	var result = Image.create_empty(pixels, pixels, false, Image.FORMAT_RGBA8)
	var offset = Vector2i((pixels - artwork.get_width()) / 2, (pixels - artwork.get_height()) / 2)
	result.blit_rect(artwork, Rect2i(Vector2i.ZERO, artwork.get_size()), offset)
	return result

func with_outline(packed: Image, color: Color) -> Image:
	var result = Image.create_empty(88, 88, false, Image.FORMAT_RGBA8)
	# Circular alpha dilation with a one-pixel antialiased outer edge. The colored
	# icon is composited back unchanged, as in the existing Nagato outline assets.
	var offsets: Array[Vector3] = []
	for dy in range(-6, 7):
		for dx in range(-6, 7):
			var coverage = clampf(OUTLINE_RADIUS + 0.6 - Vector2(dx, dy).length(), 0.0, 1.0)
			if coverage > 0.0:
				offsets.append(Vector3(dx, dy, coverage))
	for y in 88:
		for x in 88:
			var alpha = 0.0
			for offset in offsets:
				var sx = x + int(offset.x)
				var sy = y + int(offset.y)
				if sx >= 0 and sx < 88 and sy >= 0 and sy < 88:
					alpha = maxf(alpha, packed.get_pixel(sx, sy).a * offset.z)
			result.set_pixel(x, y, Color(color.r, color.g, color.b, alpha))
	result.blend_rect(packed, Rect2i(Vector2i.ZERO, packed.get_size()), Vector2i.ZERO)
	return result

func _initialize() -> void:
	var source_directory: String = ""
	var names: Array = NAMES
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--source="):
			source_directory = argument.trim_prefix("--source=").trim_suffix("/") + "/"
		elif argument.begins_with("--names="):
			names = Array(argument.trim_prefix("--names=").split(",", false))
	assert(not names.is_empty(), "No relic names specified")
	var color = sample_outline_color()
	print("Outline reference: BigShipsBigGuns.png; color #%s; radius %.2f native pixels" % [color.to_html(false), OUTLINE_RADIUS])
	for name in names:
		assert(String(name).is_valid_identifier(), "Invalid relic name")
		var source_path: String = source_directory + name + "_master.png" if not source_directory.is_empty() else DEFAULT_MASTERS.get(name, SOURCE + name + "_master.png")
		var source = Image.load_from_file(source_path)
		assert(source != null and source.detect_alpha() != Image.ALPHA_NONE, "Missing transparent master " + name)
		var big = fit_artwork(source, 256, 244)
		var packed = fit_artwork(source, 88, 80)
		var outline = with_outline(packed, color)
		for folder in ["big", "packed", "outline"]:
			var artwork = big if folder == "big" else (packed if folder == "packed" else outline)
			var path = RELICS + folder + "/" + name + ".png"
			assert(artwork.save_png(path) == OK)
			var reloaded = Image.load_from_file(path)
			assert(reloaded != null and reloaded.get_size() == artwork.get_size())
			assert(reloaded.detect_alpha() != Image.ALPHA_NONE)
			print("VERIFIED %s: %dx%d transparent RGBA" % [path, reloaded.get_width(), reloaded.get_height()])
	quit()
