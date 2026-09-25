#!/usr/bin/env python3
# @summary: Packs design/data/{layout,trace}.json into design/canvas/project/data/preview.json (columnar, compact) for the Design canvas.
"""Usage: python3 tools/gen_preview.py [--frames N]

The canvas artboards never compute simulation logic: they only replay this file.
Keep keys stable: Main/Line1/Status .dc.html read them by name.
"""
import json, sys, pathlib, datetime

ROOT = pathlib.Path(__file__).resolve().parents[1]
DATA = ROOT / "design" / "data"
OUT = ROOT / "design" / "canvas" / "project" / "data" / "preview.json"


def main() -> int:
    layout = json.loads((DATA / "layout.json").read_text(encoding="utf-8"))
    trace = json.loads((DATA / "trace.json").read_text(encoding="utf-8"))
    schema = trace["schema"]
    paths = list(schema.keys())

    nodes = [{"path": p, "type": schema[p]["type"], "name": schema[p]["name"],
              "signals": schema[p]["signals"], "units": schema[p]["units"]} for p in paths]

    belt_paths = [f"{lid}/{c['id']}" for lid, line in layout["lines"].items() for c in line["conveyors"]]

    def compact(v):
        if isinstance(v, bool):
            return 1 if v else 0
        return v

    frames, belts = [], []
    for f in trace["frames"]:
        frames.append([f["t"], [[compact(x) for x in f["v"][p]] for p in paths]])
        belts.append([f["belts"].get(p, []) for p in belt_paths])

    preview = {
        "meta": {
            "seed": trace["meta"]["seed"],
            "frameS": trace["meta"]["frameS"],
            "minutes": trace["meta"]["minutes"],
            "palette": trace["meta"]["palette"],
            "stateNames": ["Stopped", "Running", "Starved", "Blocked", "Faulted", "Maintenance"],
            "generated": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        },
        "layout": layout,
        "nodes": nodes,
        "beltPaths": belt_paths,
        "frames": frames,
        "belts": belts,
        "events": trace["events"],
    }
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(preview, ensure_ascii=False, separators=(",", ":")), encoding="utf-8")
    print(f"preview.json: {len(frames)} frames, {len(nodes)} nodes, {OUT.stat().st_size // 1024} KB")
    return 0


if __name__ == "__main__":
    sys.exit(main())
