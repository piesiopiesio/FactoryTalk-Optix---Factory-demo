#!/usr/bin/env python3
# @summary: Context budget guard + file index: `--check` enforces token/line limits, `--write` refreshes the index in CONTEXT.md.
"""Token estimate = characters / 3.5 (good enough for PL text + C#)."""
import json, pathlib, re, sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
CORE_FILES = ["CONTEXT.md", "factory.json", "BACKLOG.md", "JOURNAL.md"]
SOURCE_GLOBS = ["src/**/*.cs", "src/**/*.csproj", "tests/**/*.cs", "stubs/**/*.cs", "tools/*.py", "tools/*.sh", "design/canvas/project/*.dc.html",
                "AGENT_DAILY.md", "Makefile", "docs/*.md"]
CORE_BUDGET, SOURCE_BUDGET, MAX_CS_LINES = 12_000, 60_000, 250
SKIP = ("/obj/", "/bin/")


def tokens(p: pathlib.Path) -> int:
    return int(len(p.read_text(encoding="utf-8", errors="ignore")) / 3.5)


def source_files() -> list[pathlib.Path]:
    out: list[pathlib.Path] = []
    for g in SOURCE_GLOBS:
        out += [p for p in ROOT.glob(g) if not any(s in p.as_posix() for s in SKIP)]
    return sorted(set(out))


def summary(p: pathlib.Path) -> str:
    text = p.read_text(encoding="utf-8", errors="ignore")
    if p.suffix == ".html":
        m = re.search(r"<title>(.*?)</title>", text)
        return f"artboard: {m.group(1)}" if m else ""
    head = text.splitlines()[:3]
    for line in head:
        m = re.search(r"@summary:\s*(.+?)(\s*-->)?$", line)
        if m:
            return m.group(1).strip()
    return ""


def index_md() -> str:
    rows = []
    for p in source_files():
        if p.suffix in (".cs", ".py", ".sh", ".csproj", ".html") and not p.name.endswith("GlobalUsings.cs"):
            rows.append(f"- `{p.relative_to(ROOT).as_posix()}` — {summary(p) or 'MISSING @summary'}")
    return "\n".join(rows)


def main() -> int:
    args = set(sys.argv[1:])
    problems: list[str] = []
    core = sum(tokens(ROOT / f) for f in CORE_FILES if (ROOT / f).exists())
    src = sum(tokens(p) for p in source_files())
    if core > CORE_BUDGET: problems.append(f"core context {core} > {CORE_BUDGET} tokens")
    if src > SOURCE_BUDGET: problems.append(f"source {src} > {SOURCE_BUDGET} tokens")
    for p in source_files():
        rel = p.relative_to(ROOT).as_posix()
        if p.suffix == ".cs":
            n = len(p.read_text(encoding="utf-8").splitlines())
            if n > MAX_CS_LINES: problems.append(f"{rel}: {n} lines > {MAX_CS_LINES}")
        if p.suffix in (".cs", ".py", ".sh") and not summary(p): problems.append(f"{rel}: missing @summary header")

    if "--write" in args:
        ctx = ROOT / "CONTEXT.md"
        text = ctx.read_text(encoding="utf-8")
        new = re.sub(r"(<!-- index:start -->\n).*?(<!-- index:end -->)", lambda m: m.group(1) + index_md() + "\n" + m.group(2), text, flags=re.S)
        ctx.write_text(new, encoding="utf-8")

    report = {"coreTokens": core, "sourceTokens": src, "ok": not problems, "problems": problems}
    out = ROOT / "design" / "data" / "context.json"
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(report, indent=1), encoding="utf-8")
    print(f"context: core {core}/{CORE_BUDGET}, source {src}/{SOURCE_BUDGET} tokens" + ("" if not problems else "\n  " + "\n  ".join(problems)))
    return 1 if ("--check" in args and problems) else 0


if __name__ == "__main__":
    sys.exit(main())
