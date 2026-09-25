# @summary: One entry point for humans and the daily agent: `make check` = build + tests + sim + preview + lints.
DOTNET ?= dotnet
MIN ?= 30

.PHONY: check build test sim preview lint snapshot ctx bundle clean optix

check: build test sim preview lint ctx
	@echo "CHECK OK"

build:
	$(DOTNET) build src/Sim.Cli -nologo -v q
	$(DOTNET) build src/Factory.Optix -nologo -v q
	$(DOTNET) build tests/Factory.Tests -nologo -v q

test:
	@mkdir -p design/data
	$(DOTNET) run --no-build --project tests/Factory.Tests | tee design/data/tests.txt

sim:
	$(DOTNET) run --no-build --project src/Sim.Cli -- --minutes $(MIN) --frame 2

preview:
	python3 tools/ctx.py --write
	python3 tools/gen_preview.py
	python3 tools/gen_status.py

lint:
	python3 tools/check_design.py

ctx:
	python3 tools/ctx.py --check

snapshot:
	python3 tools/snapshot.py

# Copy for the Optix project (Plan B, docs/studio-setup.md): dist/optix/ProjectFiles/...
optix:
	python3 tools/export_optix.py

# Backup for sessions without GitHub access: whole history as base64 text inside the Design canvas.
bundle:
	git bundle create /tmp/repo.bundle --all
	base64 -w0 /tmp/repo.bundle > design/canvas/project/data/repo.bundle.b64
	@echo "bundle: $$(du -h design/canvas/project/data/repo.bundle.b64 | cut -f1)"

clean:
	rm -rf src/*/bin src/*/obj tests/*/bin tests/*/obj stubs/*/bin stubs/*/obj design/snapshots
