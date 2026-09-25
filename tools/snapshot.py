#!/usr/bin/env python3
# @summary: Renders PNG snapshots (hall + line) from preview.json via headless Chromium so the agent can look at its own UI.
"""Usage: python3 tools/snapshot.py [--frame N]  -> design/snapshots/{hall,line1}.png (gitignored)
Simplified SVG of the same geometry and colors as the canvas artboards (not a pixel copy)."""
import json, pathlib, sys, html

ROOT = pathlib.Path(__file__).resolve().parents[1]
PV = ROOT / "design" / "canvas" / "project" / "data" / "preview.json"
OUT = ROOT / "design" / "snapshots"


def svg_line(d: dict, i: int) -> str:
    f = d["frames"][i]
    idx = {n["path"]: k for k, n in enumerate(d["nodes"])}
    val = lambda p, s: f[1][idx[p]][d["nodes"][idx[p]]["signals"].index(s)]
    pal, names = d["meta"]["palette"], d["meta"]["stateNames"]
    L = d["layout"]["lines"]["L1"]
    out = [f'<rect width="1600" height="900" fill="#E0E0E0"/>',
           f'<text x="40" y="60" fill="#3F3F3F" font-size="28">{html.escape(L["name"])} · t={f[0]:.0f}s · OEE {val("L1", "Oee"):.1f}% · '
           f'{val("L1", "ThroughputPerMin"):.0f}/min · faults {val("L1", "ActiveFaults")}</text>']
    for c in L["conveyors"]:
        x, y, w, h = c["rect"]
        out.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="#E0E0E0" stroke-width="3" stroke="{"#EC8629" if val("L1/" + c["id"], "State") == 4 else "#A0A0A4"}"/>')
        for q in d["belts"][i][d["beltPaths"].index("L1/" + c["id"])]:
            cx = x + (q if c["dir"] == "right" else 1 - q if c["dir"] == "left" else 0.5) * w
            cy = y + (q if c["dir"] == "down" else 1 - q if c["dir"] == "up" else 0.5) * h
            out.append(f'<circle cx="{cx:.0f}" cy="{cy:.0f}" r="4" fill="#3F3F3F"/>')
    for s in L["stations"]:
        x, y, w, h = s["rect"]
        st = val("L1/" + s["id"], "State")
        out.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="#E0E0E0" stroke="{"#EC8629" if st == 4 else "#A0A0A4"}" stroke-width="{3 if st == 4 else 1}"/>'
                   f'<rect x="{x + w - 28}" y="{y + 8}" width="18" height="18" fill="{pal[names[st]]}" stroke="#A0A0A4"/>'
                   f'<text x="{x + 10}" y="{y + 32}" fill="#3F3F3F" font-size="14">{html.escape(s["name"])}</text>'
                   f'<text x="{x + 10}" y="{y + 52}" fill="#767676" font-size="12">{s["id"]} · {names[st]}</text>'
                   f'<text x="{x + 10}" y="{y + 84}" fill="#475CA7" font-weight="bold" font-size="16">{val("L1/" + s["id"], "Good")}</text>')
    return "".join(out)


def svg_hall(d: dict, i: int) -> str:
    f = d["frames"][i]
    idx = {n["path"]: k for k, n in enumerate(d["nodes"])}
    val = lambda p, s: f[1][idx[p]][d["nodes"][idx[p]]["signals"].index(s)]
    pal, names = d["meta"]["palette"], d["meta"]["stateNames"]
    out = ['<rect width="1600" height="900" fill="#E0E0E0"/>']
    for z in d["layout"]["hall"]["zones"]:
        x, y, w, h = z["rect"]
        dash = ' stroke-dasharray="8 6"' if z["planned"] else ""
        out.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{"#E0E0E0" if z["planned"] else "#E8E8E8"}" stroke="#A0A0A4"{dash}/>'
                   f'<text x="{x + 14}" y="{y + 26}" fill="#767676" font-size="14">{html.escape(z["name"])}</text>')
    for l in d["layout"]["hall"]["lines"]:
        x, y, w, h = l["rect"]
        out.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="#E0E0E0" stroke="#A0A0A4"/>'
                   f'<rect x="{x + 20}" y="{y + 18}" width="14" height="14" fill="{pal[names[val(l["id"], "State")]]}" stroke="#A0A0A4"/>'
                   f'<text x="{x + 44}" y="{y + 31}" fill="#3F3F3F" font-size="20">{html.escape(l["name"])} · OEE {val(l["id"], "Oee"):.1f}%</text>')
    return "".join(out)


def main() -> int:
    d = json.loads(PV.read_text(encoding="utf-8"))
    i = int(sys.argv[sys.argv.index("--frame") + 1]) if "--frame" in sys.argv else len(d["frames"]) - 1
    OUT.mkdir(parents=True, exist_ok=True)
    from playwright.sync_api import sync_playwright
    with sync_playwright() as p:
        b = p.chromium.launch()
        page = b.new_page(viewport={"width": 1600, "height": 900})
        for name, body in (("hall", svg_hall(d, i)), ("line1", svg_line(d, i))):
            page.set_content(f'<body style="margin:0;font-family:Arial,Helvetica,sans-serif"><svg xmlns="http://www.w3.org/2000/svg" width="1600" height="900">{body}</svg></body>')
            page.screenshot(path=str(OUT / f"{name}.png"))
        b.close()
    print(f"snapshots: {OUT.relative_to(ROOT)}/hall.png, line1.png (frame {i})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
