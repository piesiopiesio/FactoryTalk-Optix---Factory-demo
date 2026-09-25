#!/usr/bin/env python3
# @summary: Plan B deploy: copies src/Factory.Core + src/Factory.Optix + factory.json into an Optix project layout (dist/optix).
"""Usage: python3 tools/export_optix.py [--out dist/optix]

Result (relative to an Optix project folder, e.g. Factory_demo/):
  ProjectFiles/factory.json
  ProjectFiles/NetSolution/Factory/Core/**.cs     (pure C#)
  ProjectFiles/NetSolution/Factory/Optix/*.cs     (NetLogic layer helpers)
  ProjectFiles/NetSolution/<NetLogic>.cs          (classes bound to NetLogic nodes: Studio expects <NodeName>.cs
                                                   at the NetSolution root and creates a template there otherwise)
  ProjectFiles/NetSolution/Factory/README.txt
The SDK-style NetSolution .csproj compiles every .cs under NetSolution, so no .csproj edit is needed.
Files in NetSolution/Factory are generated: edit src/ in the repo, never the copies.
"""
import argparse, pathlib, shutil

ROOT = pathlib.Path(__file__).resolve().parent.parent
# Classes bound by name to NetLogic nodes. Studio looks for NetSolution/<NodeName>.cs and writes a template
# (duplicate class -> build error) when it is missing, so these go to the NetSolution root.
NETLOGIC_ROOT = {"FactoryBuilder.cs", "SimulationLogic.cs"}
HEADER = "// GENERATED COPY of {src} (tools/export_optix.py). Edit the repo, not this file.\n"


def copy_tree(src: pathlib.Path, dst: pathlib.Path, rel_root: pathlib.Path, root: pathlib.Path | None = None) -> int:
    n = 0
    for f in sorted(src.rglob("*.cs")):
        if any(p in ("bin", "obj") for p in f.relative_to(src).parts):
            continue
        out = (root / f.name) if root is not None and f.name in NETLOGIC_ROOT else dst / f.relative_to(src)
        out.parent.mkdir(parents=True, exist_ok=True)
        out.write_text(HEADER.format(src=f.relative_to(rel_root).as_posix()) + f.read_text(encoding="utf-8"), encoding="utf-8")
        n += 1
    return n


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default=str(ROOT / "dist" / "optix"))
    out = pathlib.Path(ap.parse_args().out)
    if out.exists():
        shutil.rmtree(out)
    net = out / "ProjectFiles" / "NetSolution" / "Factory"
    n = copy_tree(ROOT / "src" / "Factory.Core", net / "Core", ROOT)
    n += copy_tree(ROOT / "src" / "Factory.Optix", net / "Optix", ROOT, root=net.parent)
    (net / "README.txt").write_text(
        "Generated from the GitHub repo (src/Factory.Core, src/Factory.Optix) by tools/export_optix.py.\n"
        "Do not edit here - changes are overwritten on the next export.\n", encoding="utf-8")
    shutil.copy2(ROOT / "factory.json", out / "ProjectFiles" / "factory.json")
    print(f"{n} .cs files + factory.json -> {out}")


if __name__ == "__main__":
    main()
