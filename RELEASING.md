# Releasing the Teak SDK suite

How to cut a coordinated release across the three SDK repos (`teak-ios`,
`teak-android`, `teak-unity`). The natives ship first, Unity consumes them, and a
human approves the final "latest" promotion.

Every release is cut against its **stable branch** — `X.Y-stable` (e.g.
`4.3-stable`, `4.4-stable`). A new minor/major starts by cutting that `X.Y-stable`
branch on all repos; a patch reuses the existing one. Below, `<stable>` is that
branch and `<ver>` is the version you're shipping.

> **Per-repo mechanics** (tag orb, CI workflows, S3 paths) live in each repo's
> own `CLAUDE.md` "Release Flow" / "Version Management" section. This doc is the
> *cross-repo orchestration* — it does not duplicate those.

## Who does what

- **APM** owns the mechanical execution of this runbook end-to-end.
- **Lead** owns the judgment calls this runbook marks **ESCALATE** — chiefly
  changelog curation / cross-platform parity.
- Whoever pushes **acks before every immutable push** (a promote commit creates
  a permanent tag), same as the APM acks before a merge.
- **Human** owns the `deploy_latest` approval gate (and the iOS CocoaPods
  publish). That is the only step a human touches.

## The shape

```
teak-ios    ─ promote ─► CI tags + deploy_versioned ─► CDN ─┐
teak-android─ promote ─► CI tags + deploy_versioned ─► CDN ─┤
                                                            ├─► teak-unity promote
                                             (unity pulls natives by version)
```

Natives **must** be on the CDN before Unity promotes — the Unity build
downloads `Teak-<ver>.xcframework.zip` / `teak-<ver>.aar` by the versions in
`native.config.yml`. Promote Unity too early and its build 404s on the natives.

## Preconditions

- Every fix intended for this release is merged to `<stable>` on all three repos.
- The beta/rc line has been validated (integration-tested).
- You know the next version. **Versions are immutable** — once a `Promote to:`
  commit is pushed and CI tags it, that version is permanently consumed. Check
  existing tags first: `git ls-remote --tags origin '<version>'`.
- **teak-unity-cleanroom builds via both consumption paths**: `.unitypackage`
  and UPM (Package Manager resolve of `#<major>.<minor>`, no `Teak.unitypackage`
  present). The two paths are independent — a packaging change to one can
  silently break only the other — so neither build alone clears this gate.

  ⚠️ **Both paths build the wrong SDK by default, and neither fails when they
  do.** A green build is not evidence until you confirm *which* SDK it built.

  - `.unitypackage` — runs **pre-promote**, against the cut worktree.
    `rake package:copy` **ignores `FL_TEAK_SDK_SOURCE`**: the Rakefile hardcodes
    it (`fastlane 'sdk', env: { FL_TEAK_SDK_SOURCE: ".../../teak-unity/" }`), so
    it copies whatever stale `Teak.unitypackage` is sitting in the live checkout
    — which may be months old. Bypass the hardcoded env:

    ```bash
    cd teak-unity-cleanroom
    bundle exec rake clean
    FL_TEAK_SDK_SOURCE=<cut-worktree>/ bundle exec fastlane sdk   # not rake package:copy
    USE_FACEBOOK=false bundle exec rake package:import config:all build:android:local
    ```

  - UPM — resolves `upm-package-teak.git#<major>.<minor>`, a **separate repo**
    written only by `upm:deploy_versioned`. It therefore **cannot** test an
    unpromoted version: run it *after* the unity promote's `deploy_versioned`,
    *before* approving `deploy_latest` (Step 5). Run pre-promote, it silently
    resolves the *previous* release and passes.

  Confirm the SDK under test on each run — `TEAK_VERSION`, the bundled AAR's
  `BuildConfig` version, and the built APK's dex should all name the version you
  are shipping.

## Work in isolated worktrees, never the live checkout

`<stable>` is checked out in the human's primary checkout (may have WIP / a
running Unity). Cut from a fresh **detached** worktree so nothing collides:

```bash
git -C <repo> fetch origin
git -C <repo> worktree add --detach <repo>-cut-<ver> origin/<stable>
# ...build commits...
git -C <repo>-cut-<ver> push origin HEAD:<stable>
```

Promote commits go **directly** to `<stable>` (not via PR) — a promote is a
mechanical version bump, not reviewable work. Changelog finalization is also
lead-owned, so it commits directly too.

## Step 1 — Changelog finalization (per repo)   ⚠️ ESCALATE curation to lead

Each repo keeps `docs/modules/changelog/unreleased.yaml` (working entries) and
`versions/<ver>.yaml` (that release's notes; created at the first beta).

- Roll each `unreleased.yaml` entry into `versions/<ver>.yaml`, then leave a
  **fresh empty** `unreleased.yaml`. (Never put `unreleased.yaml` under
  `versions/` — `doxygen2adoc` requires semver filenames and will break.)
- **ESCALATE to lead**: whether an entry belongs in this release at all, and
  cross-platform parity — if iOS and Android shipped the *same* fix, they should
  read consistently (e.g. in 4.3.14, iOS carried no separate line for a shared
  LogListener-throws guard, so the identical Android line was dropped, not rolled in).
- Docs-only commits since the last beta do **not** need changelog entries.
- Commit the roll on its own (clear subject explaining it). Editorial rules:
  reserve "crash" for genuine uncaught exceptions (see changelog-editorial learning).

## Step 2 — Promote the natives (iOS + Android)   ⚠️ ACK before push

Native version comes from git tags, so the promote commit is **empty**:

```bash
git -C <native>-cut-<ver> commit --allow-empty -m "Promote to: <ver>"
git -C <native>-cut-<ver> push origin HEAD:<stable>   # immutable — ack first
```

(If a repo also has a changelog-finalization commit, it sits *before* the
promote so the promote stays HEAD — CI's orb parses the HEAD message.)

The push triggers: tag-promote orb → tag `<ver>` → tagged-build →
`deploy_versioned` uploads to `s3://teak-build-artifacts/{ios,android}/`, served
at `sdks.teakcdn.com`.

## Step 3 — Wait for the natives on the CDN

Poll until all four native artifacts return 200 (public CDN, no creds):

```
https://sdks.teakcdn.com/ios/Teak-<ver>.xcframework.zip
https://sdks.teakcdn.com/ios/TeakExtension-<ver>.xcframework.zip
https://sdks.teakcdn.com/ios/TeakResources-<ver>.bundle.zip
https://sdks.teakcdn.com/android/teak-<ver>.aar
```

**If a promote build fails on a transient infra step** (e.g. CircleCI's
"checkout code" step), it's a **CI re-run, not a tree fix** — the promote commit
is fine. Someone with CircleCI access re-runs the failed workflow; no new commit,
no version burned (the tag only gets cut once the build passes). Confirm the tree
is actually inert to the failed step before re-running — a changelog-only diff
can't break `assemble`/tests, so a build failure there is transient by
elimination. Check status via `gh api repos/GoCarrot/<repo>/commits/<sha>/status`.

## Step 4 — Promote teak-unity   ⚠️ ACK before push

Unity carries its version in files, not tags. In the unity worktree:

1. `VERSION`: `<ver>` (drop the beta/rc suffix).
2. `native.config.yml`: bump `ios` + `android` to `<ver>` (the finalized natives).
3. Changelog roll (Step 1) if any unreleased entries remain.
4. Commit `Promote to: <ver>` (VERSION + native.config; changelog roll is a
   separate preceding commit). Push to `<stable>`.

CI tags `<ver>` → tagged-build downloads the now-live natives, builds the
`.unitypackage` + UPM package, `deploy_versioned` to S3.

## Step 5 — Deploy as latest   👤 HUMAN gate

Each repo's `tagged-build` holds at a manual approval gate after
`deploy_versioned`. The human approves `deploy_latest` (and, for iOS, the
CocoaPods trunk publish). This is the only human touchpoint.

That hold is also the window for the **UPM half of the cleanroom gate**
(Preconditions): `deploy_versioned` has published the new version to
`upm-package-teak`, so a UPM cleanroom build now resolves what you are actually
shipping. Run it before approving.

## After the cut

- Close the release's tracking issue (Linear auto-closes if the promote branch
  name carries the id; a direct-push promote has no such branch, so close it
  manually).
- Postmortem + any learnings as usual.
