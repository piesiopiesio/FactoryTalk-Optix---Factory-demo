#!/usr/bin/env python3
# @summary: Builds design/canvas/project/data/status.json (checks, tests, KPIs, backlog, journal, context) for the Status artboard.
import json, pathlib, re, subprocess, datetime, sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
DATA = ROOT / "design" / "data"
OUT = ROOT / "design" / "canvas" / "project" / "data" / "status.json"


def git(*args: str) -> str:
    try:
        return subprocess.run(["git", *args], cwd=ROOT, capture_output=True, text=True, check=True).stdout.strip()
    except Exception:
        return "?"


def backlog_next(limit: int = 5) -> list[str]:
    text = (ROOT / "BACKLOG.md").read_text(encoding="utf-8")
    section = text.split("## Next", 1)[-1].split("\n## ", 1)[0]
    items = re.findall(r"^- \[ \] (.+)$", section, flags=re.M)
    return [re.sub(r"`", "", i) for i in items[:limit]]


def journal(limit: int = 3) -> list[dict]:
    text = (ROOT / "JOURNAL.md").read_text(encoding="utf-8")
    entries = re.findall(r"^## (\d{4}-\d{2}-\d{2})\n(.*?)(?=^## |\Z)", text, flags=re.M | re.S)
    return [{"date": d, "text": "\n".join(l.lstrip("- ").strip() for l in body.strip().splitlines() if l.strip())} for d, body in entries[:limit]]


def tests() -> dict:
    f = DATA / "tests.txt"
    m = re.search(r"(\d+)/(\d+) passed", f.read_text()) if f.exists() else None
    return {"passed": int(m.group(1)), "total": int(m.group(2))} if m else {"passed": 0, "total": 0}


def main() -> int:
    summary = json.loads((DATA / "trace-summary.json").read_text(encoding="utf-8"))
    ctx = json.loads((DATA / "context.json").read_text()) if (DATA / "context.json").exists() else {"coreTokens": 0, "ok": False}
    line = summary["lines"][0]
    status = {
        "updated": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%d %H:%M UTC"),
        "branch": git("rev-parse", "--abbrev-ref", "HEAD"),
        "commit": git("rev-parse", "--short", "HEAD"),
        "source": "github" if git("remote") not in ("", "?") else "lokalna kopia",
        "tests": tests(),
        "checks": summary["checks"],
        "kpi": {"oee": line["kpi"]["Oee"], "throughput": line["kpi"]["ThroughputPerMin"]},
        "context": ctx,
        "backlog": backlog_next(),
        "journal": journal(),
    }
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(status, ensure_ascii=False, indent=1), encoding="utf-8")
    print(f"status.json: {status['tests']['passed']}/{status['tests']['total']} tests, {sum(c['ok'] for c in status['checks'])}/{len(status['checks'])} checks")
    return 0


if __name__ == "__main__":
    sys.exit(main())
