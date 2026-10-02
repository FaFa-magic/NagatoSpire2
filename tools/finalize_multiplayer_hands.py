"""Normalize painted Nagato hand sprites to the game's 422x1200 canvas.

Only pixel dimensions and the lower sleeve's alpha fade are changed; the
gesture, anatomy, costume, and painted details remain from the supplied art.
"""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
from PIL import Image


SIZE = (422, 1200)
GESTURES = ("paper", "rock", "scissors", "point")


def finalize(source: Path, target: Path) -> None:
    with Image.open(source) as original:
        image = original.convert("RGBA").resize(SIZE, Image.Resampling.LANCZOS)
    pixels = np.array(image)

    # Let every costume sleeve dissolve to transparent over its final 15%.
    # Smoothstep also avoids a perceptible straight alpha edge at the bottom.
    y = np.arange(SIZE[1], dtype=np.float32)
    t = np.clip((y - 1020.0) / 180.0, 0.0, 1.0)
    fade = 1.0 - t * t * (3.0 - 2.0 * t)
    pixels[:, :, 3] = np.rint(pixels[:, :, 3].astype(np.float32) * fade[:, None]).astype(np.uint8)
    pixels[:, :, 3][pixels[:, :, 3] < 5] = 0

    target.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(pixels, "RGBA").save(target, optimize=True)


def main() -> None:
    parser = argparse.ArgumentParser()
    for gesture in GESTURES:
        parser.add_argument(f"--{gesture}", type=Path, required=True)
    parser.add_argument("--out-dir", type=Path, required=True)
    args = parser.parse_args()
    for gesture in GESTURES:
        target = args.out_dir / f"multiplayer_hand_nagato_{gesture}.png"
        finalize(getattr(args, gesture), target)
        print(target)


if __name__ == "__main__":
    main()
