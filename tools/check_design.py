#!/usr/bin/env python3
# @summary: Static lint of the Design canvas: canvas.json <-> artboards, required head line, hole syntax, sizes, data files.
import json, pathlib, re, sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
PROJ = ROOT / "design" / "canvas" / "project"
HOLE = re.compile(r"\{\{(.*?)\}\}")
PATH_OK = re.compile(r"^\s*([A-Za-z_$][\w$]*(\.[\w$]+)*|true|false|\d+(\.\d+)?)\s*$")


def main() -> int:
    errs: list[str] = []
    canvas = json.loads((PROJ / "canvas.json").read_text(encoding="utf-8"))
    boards = canvas["boards"]
    if set(canvas["order"]) != set(boards): errs.append("canvas.json: order != boards")
    for name, b in boards.items():
        f = PROJ / name
        if not f.exists():
            errs.append(f"{name}: missing file"); continue
        html = f.read_text(encoding="utf-8")
        if '<script src="./support.js"></script>' not in html: errs.append(f"{name}: support.js head line missing")
        if "data-dc-script" not in html or "extends DCLogic" not in html: errs.append(f"{name}: no DCLogic script")
        m = re.search(r'"\$preview":\{"width":(\d+),"height":(\d+)\}', html)
        if not m or (int(m.group(1)), int(m.group(2))) != (b["w"], b["h"]): errs.append(f"{name}: $preview != board {b['w']}x{b['h']}")
        root = re.search(r"<x-dc>.*?</helmet>\s*<div style=\"width: (\d+)px; height: (\d+)px", html, flags=re.S)
        if not root or (int(root.group(1)), int(root.group(2))) != (b["w"], b["h"]): errs.append(f"{name}: root size != board")
        markup = html.split('<script type="text/x-dc"')[0]
        for h in HOLE.findall(markup):
            if not PATH_OK.match(h): errs.append(f"{name}: hole is an expression: {{{{{h}}}}}")
        for ref in re.findall(r"fetch\('([^']+)'\)", html):
            if not (PROJ / ref).exists(): errs.append(f"{name}: fetch target {ref} missing")

    pv = json.loads((PROJ / "data" / "preview.json").read_text(encoding="utf-8"))
    for k in ("meta", "layout", "nodes", "frames", "belts", "beltPaths", "events"):
        if k not in pv: errs.append(f"preview.json: key '{k}' missing")
    paths = {n["path"] for n in pv["nodes"]}
    for lid, line in pv["layout"]["lines"].items():
        for s in line["stations"] + line["conveyors"]:
            if f"{lid}/{s['id']}" not in paths: errs.append(f"preview.json: {lid}/{s['id']} has layout but no signals")

    print("design lint: " + ("ok" if not errs else f"{len(errs)} problem(s)\n  " + "\n  ".join(errs)))
    return 1 if errs else 0


if __name__ == "__main__":
    sys.exit(main())
