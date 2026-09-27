from pathlib import Path
import struct

SIZES = [16, 24, 32, 48, 64, 128, 256]
ROOT = Path(__file__).resolve().parents[1]
APP_OUT = ROOT / "src" / "RankOn" / "Resources" / "RankOn.ico"
INSTALLER_OUT = ROOT / "installer" / "RankOn-Setup.ico"

BG = (11, 25, 40, 255)
BAR = (33, 177, 230, 255)
ACCENT = (28, 212, 238, 255)
TRANSPARENT = (0, 0, 0, 0)


def inside_round_rect(x, y, left, top, right, bottom, radius):
    if left + radius <= x < right - radius or top + radius <= y < bottom - radius:
        return left <= x < right and top <= y < bottom

    cx = left + radius if x < left + radius else right - radius - 1
    cy = top + radius if y < top + radius else bottom - radius - 1
    return (x - cx) ** 2 + (y - cy) ** 2 <= radius ** 2


def point_in_polygon(x, y, points):
    inside = False
    j = len(points) - 1

    for i, (xi, yi) in enumerate(points):
        xj, yj = points[j]

        if ((yi > y) != (yj > y)) and (
            x < (xj - xi) * (y - yi) / (yj - yi) + xi
        ):
            inside = not inside

        j = i

    return inside


def make_frame(size):
    scale = size / 64.0
    pixels = [
        [TRANSPARENT for _ in range(size)]
        for _ in range(size)
    ]

    left = round(2 * scale)
    top = round(2 * scale)
    right = round(62 * scale)
    bottom = round(62 * scale)
    radius = max(2, round(12 * scale))

    for y in range(size):
        for x in range(size):
            if inside_round_rect(
                x, y, left, top, right, bottom, radius
            ):
                pixels[y][x] = BG

    for x0, y0, x1, y1 in [
        (14, 43, 23, 53),
        (27, 35, 36, 53),
        (40, 27, 49, 53),
    ]:
        sx0 = round(x0 * scale)
        sy0 = round(y0 * scale)
        sx1 = round(x1 * scale)
        sy1 = round(y1 * scale)

        for y in range(max(0, sy0), min(size, sy1)):
            for x in range(max(0, sx0), min(size, sx1)):
                pixels[y][x] = BAR

    arrow = [
        (12, 40), (20, 37), (28, 32), (35, 25), (31, 22),
        (50, 16), (47, 35), (43, 30), (38, 35), (31, 40),
        (22, 45),
    ]
    scaled_arrow = [
        (x * scale, y * scale)
        for x, y in arrow
    ]

    for y in range(size):
        for x in range(size):
            if point_in_polygon(
                x + 0.5,
                y + 0.5,
                scaled_arrow,
            ):
                pixels[y][x] = ACCENT

    dib_header = struct.pack(
        "<IIIHHIIIIII",
        40,
        size,
        size * 2,
        1,
        32,
        0,
        size * size * 4,
        0,
        0,
        0,
        0,
    )

    raw = bytearray()

    for y in range(size - 1, -1, -1):
        for x in range(size):
            red, green, blue, alpha = pixels[y][x]
            raw += bytes((blue, green, red, alpha))

    mask_row = ((size + 31) // 32) * 4
    mask = bytes(mask_row * size)
    return dib_header + raw + mask


frames = [make_frame(size) for size in SIZES]

ico_header = struct.pack("<HHH", 0, 1, len(SIZES))
offset = 6 + (16 * len(SIZES))
entries = []

for size, frame in zip(SIZES, frames):
    dimension = 0 if size == 256 else size

    entries.append(
        struct.pack(
            "<BBBBHHII",
            dimension,
            dimension,
            0,
            0,
            1,
            32,
            len(frame),
            offset,
        )
    )

    offset += len(frame)

icon_bytes = ico_header + b"".join(entries) + b"".join(frames)

APP_OUT.parent.mkdir(parents=True, exist_ok=True)
INSTALLER_OUT.parent.mkdir(parents=True, exist_ok=True)

APP_OUT.write_bytes(icon_bytes)
INSTALLER_OUT.write_bytes(icon_bytes)

print(
    f"Generated multi-size Windows icon: {APP_OUT} "
    f"({', '.join(str(size) for size in SIZES)} px)"
)
