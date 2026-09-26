from pathlib import Path
import struct

SIZE = 64
OUT = Path(__file__).resolve().parents[1] / "installer" / "RankOn-Setup.ico"

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


def point_in_polygon(x, y, pts):
    inside = False
    j = len(pts) - 1
    for i, (xi, yi) in enumerate(pts):
        xj, yj = pts[j]
        if ((yi > y) != (yj > y)) and (x < (xj - xi) * (y - yi) / (yj - yi) + xi):
            inside = not inside
        j = i
    return inside


pixels = [[TRANSPARENT for _ in range(SIZE)] for _ in range(SIZE)]

for y in range(SIZE):
    for x in range(SIZE):
        if inside_round_rect(x, y, 2, 2, 62, 62, 12):
            pixels[y][x] = BG

for x0, y0, x1, y1 in [(14, 43, 23, 53), (27, 35, 36, 53), (40, 27, 49, 53)]:
    for y in range(y0, y1):
        for x in range(x0, x1):
            pixels[y][x] = BAR

arrow = [
    (12, 40), (20, 37), (28, 32), (35, 25), (31, 22),
    (50, 16), (47, 35), (43, 30), (38, 35), (31, 40), (22, 45)
]
for y in range(12, 48):
    for x in range(8, 54):
        if point_in_polygon(x + 0.5, y + 0.5, arrow):
            pixels[y][x] = ACCENT

header = struct.pack(
    "<IIIHHIIIIII",
    40, SIZE, SIZE * 2, 1, 32, 0, SIZE * SIZE * 4, 0, 0, 0, 0
)
raw = bytearray()
for y in range(SIZE - 1, -1, -1):
    for x in range(SIZE):
        r, g, b, a = pixels[y][x]
        raw += bytes((b, g, r, a))

mask_row = ((SIZE + 31) // 32) * 4
mask = bytes(mask_row * SIZE)
frame = header + raw + mask
ico_header = struct.pack("<HHH", 0, 1, 1)
dir_entry = struct.pack("<BBBBHHII", SIZE, SIZE, 0, 0, 1, 32, len(frame), 22)

OUT.write_bytes(ico_header + dir_entry + frame)
print(f"Generated {OUT} ({OUT.stat().st_size} bytes)")
