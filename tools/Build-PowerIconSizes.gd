extends SceneTree

const PROJECT = "D:/GitClone/NagatoSpire2/"
const SOURCE = PROJECT + "art_sources/powers/power_icons_20261016/"
const POWERS = PROJECT + "NagatoSpire2/images/powers/"
const NAMES = ["LastWhiteLightEchoPower", "LastWhiteLightPower", "NavyHolidayPower", "SoulWelcomingPower"]

func _initialize() -> void:
	for folder in ["big", "packed"]:
		var directory_error = DirAccess.make_dir_recursive_absolute(POWERS + folder)
		assert(directory_error == OK)
	for name in NAMES:
		var source = Image.load_from_file(SOURCE + name + "_master.png")
		assert(source != null and not source.is_empty(), "Missing master: " + name)
		assert(source.get_width() == source.get_height(), "Master must be square: " + name)
		assert(source.detect_alpha() != Image.ALPHA_NONE, "Master needs transparency: " + name)
		for pixels in [256, 64]:
			var result = source.duplicate()
			result.convert(Image.FORMAT_RGBA8)
			result.fix_alpha_edges()
			result.resize(pixels, pixels, Image.INTERPOLATE_LANCZOS)
			var folder = "big" if pixels == 256 else "packed"
			var path = POWERS + folder + "/" + name + ".png"
			assert(result.save_png(path) == OK, "Cannot save " + path)
			var reloaded = Image.load_from_file(path)
			assert(reloaded.get_size() == Vector2i(pixels, pixels))
			assert(reloaded.detect_alpha() != Image.ALPHA_NONE)
			assert(reloaded.get_pixel(0, 0).a < 0.01, "Background is not transparent: " + path)
			print("VERIFIED %s: %dx%d, transparent RGBA" % [path, pixels, pixels])
	quit()
