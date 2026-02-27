# Pre-push security checklist (Unity)

## 1) Verify ignored files
- Confirm `.gitignore` exists in project root.
- Confirm `Library/`, `Temp/`, `Logs/`, `Obj/`, `Build/`, `UserSettings/` are NOT staged.

## 2) Secret scan
- Run a local secret scan before first push.
- Example tools: `gitleaks`, `trufflehog`.

## 3) Review staged files
- Check for accidental credentials in:
  - `ProjectSettings/`
  - `Assets/Resources/`
  - `*.json`, `*.asset`, `*.txt`

## 4) Third-party assets / licenses
- Verify redistribution rights for asset packs.
- If license forbids source redistribution, remove those assets from public repo.

## 5) Clean commit history if needed
- If a secret was committed before, rotate it and rewrite history (BFG or `git filter-repo`).

## 6) Final commands
```bash
git status
git add .
git status
git commit -m "Initial public-safe commit"
```
