"""Render three Nagato orb geometry guides from caliber-matched lathe meshes.

The model is an original, stylized 41 cm-class projectile. It intentionally has
no cartridge case: a flat base, narrow copper driving band, constant-caliber
body, and rounded ogive are all part of each projectile. Run with the bundled
Python runtime (numpy and Pillow required).
"""

from __future__ import annotations

import argparse
import math
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter


RADIUS = 0.50
SPECS = {
    # Same diameter, intentionally different bodies, shoulders, and nose lengths.
    "high_explosive_shell_orb": (2.60, 1.98, 0.80),
    "armor_piercing_shell_orb": (2.98, 2.18, 1.15),
    "type_three_shell_orb": (2.78, 2.16, 0.62),
}
SEGMENTS = 72
CANVAS = 1024
TEXTURE_SIZE = 1024
CAMERA = np.array([0.66, -1.0, -0.60], dtype=np.float64)
CAMERA /= np.linalg.norm(CAMERA)
LIGHT = np.array([-0.25, -0.8, 1.4], dtype=np.float64)
LIGHT /= np.linalg.norm(LIGHT)


def mesh(kind: str) -> tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray]:
    # profile coordinates are axial length and radius in calibers.
    # Copper driving band is the only raised part near the base.
    length, shoulder, roundness = SPECS[kind]
    base_knots = np.array([
        [0.00, 0.455], [0.055, 0.455], [0.105, 0.475],
        [0.125, 0.475], [0.138, 0.500], [0.245, 0.500],
        [0.258, 0.480], [0.340, 0.480], [0.405, RADIUS],
        [0.700, RADIUS], [1.200, RADIUS], [shoulder, RADIUS],
    ], dtype=np.float64)
    zs = np.unique(np.concatenate((np.linspace(0, length, 146),
                                   base_knots[:, 0], [shoulder])))
    rs = np.interp(zs, base_knots[:, 0], base_knots[:, 1])
    nose = zs > shoulder
    t = (zs[nose] - shoulder) / (length - shoulder)
    # A half-ellipsoid nose ends in a visibly rounded dome, not a needle.
    rs[nose] = RADIUS * np.maximum(0, 1 - t ** roundness) ** 0.5
    dr = np.gradient(rs, zs)
    angles = np.linspace(0, 2 * math.pi, SEGMENTS + 1)
    vertices = []
    normals = []
    uvs = []
    for z, radius, slope in zip(zs, rs, dr):
        for j, angle in enumerate(angles):
            c, s = math.cos(angle), math.sin(angle)
            vertices.append((radius * c, radius * s, z))
            normal = np.array((c, s, -slope))
            normal /= np.linalg.norm(normal)
            normals.append(normal)
            uvs.append((j / SEGMENTS, z / length))
    faces = []
    stride = SEGMENTS + 1
    for i in range(len(zs) - 1):
        for j in range(SEGMENTS):
            a = i * stride + j
            b = a + stride
            faces.extend(((a, a + 1, b), (a + 1, b + 1, b)))
    base_center = len(vertices)
    vertices.append((0.0, 0.0, 0.0))
    normals.append((0.0, 0.0, -1.0))
    uvs.append((0.0, 0.0))
    for j in range(SEGMENTS):
        faces.append((base_center, j + 1, j))
    return (
        np.asarray(vertices, dtype=np.float64),
        np.asarray(normals, dtype=np.float64),
        np.asarray(uvs, dtype=np.float64),
        np.asarray(faces, dtype=np.int32),
        zs,
    )


def save_obj(path: Path, vertices: np.ndarray, normals: np.ndarray,
             uvs: np.ndarray, faces: np.ndarray) -> None:
    with path.open("w", encoding="utf-8", newline="\n") as output:
        output.write("# Original stylized projectile; common 1.0-caliber diameter.\n")
        for x, y, z in vertices:
            output.write(f"v {x:.7f} {y:.7f} {z:.7f}\n")
        for u, v in uvs:
            output.write(f"vt {u:.7f} {v:.7f}\n")
        for x, y, z in normals:
            output.write(f"vn {x:.7f} {y:.7f} {z:.7f}\n")
        for a, b, c in faces:
            output.write(
                f"f {a+1}/{a+1}/{a+1} {b+1}/{b+1}/{b+1} "
                f"{c+1}/{c+1}/{c+1}\n"
            )


def y_at(z: float, length: float) -> int:
    return round((1 - z / length) * (TEXTURE_SIZE - 1))


def rectangle_at_z(draw: ImageDraw.ImageDraw, z0: float, z1: float,
                   color: str, length: float) -> None:
    y0, y1 = sorted((y_at(z0, length), y_at(z1, length)))
    draw.rectangle((0, y0, TEXTURE_SIZE - 1, y1), fill=color)


def texture(kind: str) -> Image.Image:
    length, shoulder, _ = SPECS[kind]
    ground = {
        "high_explosive_shell_orb": "#b93636",
        "armor_piercing_shell_orb": "#263b61",
        "type_three_shell_orb": "#e4d9c5",
    }[kind]
    img = Image.new("RGB", (TEXTURE_SIZE, TEXTURE_SIZE), ground)
    draw = ImageDraw.Draw(img)

    # Shared physical components: steel heel, copper driving band, no case.
    def band(z0: float, z1: float, color: str) -> None:
        rectangle_at_z(draw, z0, z1, color, length)
    band(0.00, 0.105, "#495461")
    band(0.105, 0.138, "#7c5b38")
    band(0.138, 0.245, "#d08b46")
    band(0.245, 0.300, "#8b5a32")
    band(0.300, 0.390, "#58606a")
    band(0.390, 0.410, "#d2a85f")

    if kind == "high_explosive_shell_orb":
        band(0.410, shoulder, "#bb3335")
        band(0.86, 1.48, "#dc6740")
        band(shoulder, length, "#a23932")
        ink = "#ffe5a3"
    elif kind == "armor_piercing_shell_orb":
        band(0.410, shoulder, "#273d62")
        band(0.86, 1.48, "#285e9e")
        band(shoulder, length, "#354557")
        ink = "#e8c575"
    else:
        band(0.410, shoulder, "#e6deca")
        band(0.86, 1.48, "#ad3142")
        band(shoulder, length, "#d9cdb6")
        ink = "#f4db98"

    # Color seams are painted; they do not change the mesh or caliber.
    for z in (0.84, 1.49, 2.10):
        band(z - 0.028, z + 0.028, "#d8ae68")
    if kind == "type_three_shell_orb":
        for z in (1.75, 1.84, 1.93):
            band(z, z + 0.018, "#bd8c48")
    elif kind == "armor_piercing_shell_orb":
        band(1.79, 1.84, "#b9c9d7")
    else:
        band(1.79, 1.84, "#eb9a58")

    # Paint emblems in cylindrical UV space on the camera-facing side.
    face_u = (math.atan2(CAMERA[1], CAMERA[0]) % (2 * math.pi)) / (2 * math.pi)
    cx = round(face_u * (TEXTURE_SIZE - 1))
    cy = y_at(1.18, length)
    if kind == "high_explosive_shell_orb":
        draw.ellipse((cx - 42, cy - 42, cx + 42, cy + 42), outline=ink, width=13)
        for i in range(8):
            a = i * math.pi / 4
            r0, r1 = 53, 82
            draw.line((cx + r0 * math.cos(a), cy + r0 * math.sin(a),
                       cx + r1 * math.cos(a), cy + r1 * math.sin(a)),
                      fill=ink, width=12)
        draw.polygon(((cx, cy - 29), (cx + 21, cy + 15),
                      (cx, cy + 37), (cx - 21, cy + 15)), fill=ink)
    elif kind == "armor_piercing_shell_orb":
        draw.polygon(((cx, cy - 92), (cx + 43, cy + 7),
                      (cx + 15, cy - 5), (cx + 15, cy + 82),
                      (cx - 15, cy + 82), (cx - 15, cy - 5),
                      (cx - 43, cy + 7)), fill=ink)
    else:
        draw.ellipse((cx - 9, cy + 29, cx + 9, cy + 47), fill=ink)
        for dx, dy in ((-62, -52), (0, -86), (62, -52)):
            draw.line((cx, cy + 30, cx + dx, cy + dy), fill=ink, width=13)
            draw.ellipse((cx + dx - 10, cy + dy - 10,
                          cx + dx + 10, cy + dy + 10), fill=ink)
    return img


def projected(vertices: np.ndarray, scale: float | None = None) -> tuple[np.ndarray, np.ndarray, float]:
    right = np.cross((0.0, 0.0, 1.0), CAMERA)
    right /= np.linalg.norm(right)
    up = np.cross(CAMERA, right)
    x = vertices @ right
    y = vertices @ up
    angle = math.radians(43)
    rotated = np.column_stack((x * math.cos(angle) + y * math.sin(angle),
                               -x * math.sin(angle) + y * math.cos(angle)))
    # World up becomes screen up; raster image coordinates increase downward.
    rotated[:, 1] *= -1
    extent = np.ptp(rotated, axis=0)
    if scale is None:
        scale = (CANVAS - 125) / max(extent)
    center = (rotated.max(axis=0) + rotated.min(axis=0)) / 2
    screen = (rotated - center) * scale + CANVAS / 2
    depth = vertices @ CAMERA
    return screen, depth, scale


def render(vertices: np.ndarray, normals: np.ndarray, uvs: np.ndarray,
           faces: np.ndarray, tex: Image.Image, scale: float) -> Image.Image:
    screen, depth, _ = projected(vertices, scale)
    texels = np.asarray(tex, dtype=np.uint8)
    rgb = np.zeros((CANVAS, CANVAS, 3), dtype=np.uint8)
    alpha = np.zeros((CANVAS, CANVAS), dtype=np.uint8)
    z_buffer = np.full((CANVAS, CANVAS), -np.inf)
    for triangle in faces:
        p = screen[triangle]
        xmin = max(0, math.floor(p[:, 0].min()))
        xmax = min(CANVAS - 1, math.ceil(p[:, 0].max()))
        ymin = max(0, math.floor(p[:, 1].min()))
        ymax = min(CANVAS - 1, math.ceil(p[:, 1].max()))
        if xmax < xmin or ymax < ymin:
            continue
        denominator = ((p[1, 1] - p[2, 1]) * (p[0, 0] - p[2, 0])
                       + (p[2, 0] - p[1, 0]) * (p[0, 1] - p[2, 1]))
        if abs(denominator) < 1e-10:
            continue
        yy, xx = np.mgrid[ymin:ymax + 1, xmin:xmax + 1]
        xx = xx + 0.5
        yy = yy + 0.5
        a = ((p[1, 1] - p[2, 1]) * (xx - p[2, 0])
             + (p[2, 0] - p[1, 0]) * (yy - p[2, 1])) / denominator
        b = ((p[2, 1] - p[0, 1]) * (xx - p[2, 0])
             + (p[0, 0] - p[2, 0]) * (yy - p[2, 1])) / denominator
        c = 1.0 - a - b
        depths = a * depth[triangle[0]] + b * depth[triangle[1]] + c * depth[triangle[2]]
        sub_z = z_buffer[ymin:ymax + 1, xmin:xmax + 1]
        visible = (a >= -1e-7) & (b >= -1e-7) & (c >= -1e-7) & (depths > sub_z)
        if not np.any(visible):
            continue
        normal = (a[..., None] * normals[triangle[0]]
                  + b[..., None] * normals[triangle[1]]
                  + c[..., None] * normals[triangle[2]])
        normal /= np.maximum(1e-8, np.linalg.norm(normal, axis=2, keepdims=True))
        diffuse = np.clip(normal @ LIGHT, 0, 1)
        facing = np.clip(normal @ CAMERA, 0, 1)
        # Bright lacquer and broad cel-like highlights survive the 256px import.
        shade = 0.67 + 0.25 * diffuse + 0.10 * (diffuse > 0.42)
        shade += 0.12 * diffuse ** 9
        shade += 0.08 * (np.clip(facing, 0, 1) ** 12)
        uv = a[..., None] * uvs[triangle[0]] + b[..., None] * uvs[triangle[1]] + c[..., None] * uvs[triangle[2]]
        tx = np.clip((uv[..., 0] * (TEXTURE_SIZE - 1)).astype(np.int32), 0, TEXTURE_SIZE - 1)
        ty = np.clip(((1 - uv[..., 1]) * (TEXTURE_SIZE - 1)).astype(np.int32), 0, TEXTURE_SIZE - 1)
        color = np.clip(texels[ty, tx].astype(np.float64) * shade[..., None], 0, 255).astype(np.uint8)
        sub_rgb = rgb[ymin:ymax + 1, xmin:xmax + 1]
        sub_alpha = alpha[ymin:ymax + 1, xmin:xmax + 1]
        sub_rgb[visible] = color[visible]
        sub_alpha[visible] = 255
        sub_z[visible] = depths[visible]

    sprite = Image.fromarray(np.dstack((rgb, alpha)), "RGBA")
    silhouette = Image.fromarray(alpha, "L")
    outline = silhouette.filter(ImageFilter.MaxFilter(7))
    outlined = Image.new("RGBA", (CANVAS, CANVAS), (25, 28, 39, 0))
    outlined.putalpha(outline)
    outlined.alpha_composite(sprite)
    return outlined


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--out-dir", type=Path, required=True)
    args = parser.parse_args()
    args.out_dir.mkdir(parents=True, exist_ok=True)
    kinds = tuple(SPECS)
    models = {kind: mesh(kind) for kind in kinds}
    _, _, shared_scale = projected(models["armor_piercing_shell_orb"][0])
    for kind in kinds:
        vertices, normals, uvs, faces, _ = models[kind]
        save_obj(args.out_dir / f"{kind}.obj", vertices, normals, uvs, faces)
        tex = texture(kind)
        tex.save(args.out_dir / f"{kind}_uv.png")
        projected_icon = render(vertices, normals, uvs, faces, tex, shared_scale)
        projected_icon.save(args.out_dir / f"{kind}_geometry.png")
        projected_icon.resize((256, 256), Image.Resampling.LANCZOS).save(
            args.out_dir / f"{kind}_geometry_preview_256.png"
        )
        print(f"{kind}: {projected_icon.size}")
    comparison = Image.new("RGBA", (3 * 256, 256), (0, 0, 0, 0))
    for column, kind in enumerate(kinds):
        with Image.open(args.out_dir / f"{kind}_geometry_preview_256.png") as icon:
            comparison.alpha_composite(icon, (column * 256, 0))
    comparison.save(args.out_dir / "geometry_comparison_256.png")


if __name__ == "__main__":
    main()
