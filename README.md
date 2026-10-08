# Narula.AI.SystemOne.SDK

Provider-neutral SDK for decision models. Today: Cloudflare Clef. Also a generic SystemOne-compatible HTTP provider (local Clef, Jev, ...).

- `src/Narula.AI.SystemOne.SDK.Clef` - the library
- `test/Narula.AI.SystemOne.SDK.Clef.Test` - builds `Clef.exe` (sample runner) and holds all unit tests

## Settings
All settings live in `Clef.sqlite.config` (SQLite) beside the executable; created with defaults on first run.

    dotnet run --project test/Narula.AI.SystemOne.SDK.Clef.Test -- init
    dotnet run --project test/Narula.AI.SystemOne.SDK.Clef.Test -- config set cloudflare.account_id <ID>
    dotnet run --project test/Narula.AI.SystemOne.SDK.Clef.Test -- config set cloudflare.api_token <TOKEN>

The token is stored in plaintext (file is chmod 600 on Unix). The file is git-ignored.

## Samples (dry run by default; `--live` calls the API)
    ... -- sample text | noul | score | image <file> | video <frames...>

## Use in code
    var client = ClientFactory.Create(new SettingsStore());   // or services.AddClef()
    var m = await client.BestMatchAsync("keep files synced", descriptions);

## Unverified assumptions (see ClefWire.cs)
Image/video encoding on the wire, score `criteria` shape, and the noul answer field name. Run one live call and compare `DecisionResult.RawResponse`.
