# capstone1_server

상세 실행 및 배포 가이드: [BACKEND_SETUP.md](./BACKEND_SETUP.md)

Math Arena backend server built with Flask and MySQL.

## Current Structure

```text
capstone1_server/
|-- app.py
|-- config.py
|-- database.py
|-- README.md
|-- .env
|-- api/
|   |-- __init__.py
|   |-- auth.py
|   |-- user.py
|   |-- sessions.py
|   |-- experience.py
|   |-- training.py
|   `-- match.py
|-- services/
|   |-- __init__.py
|   |-- auth.py
|   |-- runtime_state.py
|   |-- question_service.py
|   |-- training_service.py
|   `-- arena_service.py
|-- models/
|   |-- __init__.py
|   |-- base.py
|   |-- user.py
|   `-- profile.py
|-- scripts/
|   `-- import_concept_csv.py
|-- common/
|   |-- __init__.py
|   |-- errors.py
|   `-- responses.py
|-- migrations/
`-- legacy/
    |-- api/
    |-- models/
    |-- services/
    `-- scripts/
```

## Directory Overview

### app.py
- Flask application entry point
- Loads configuration
- Registers blueprints
- Sets logging behavior

### config.py
- Loads environment values from `.env`
- Manages DB URL, JWT secret, port, and flags

### database.py
- Creates SQLAlchemy engine and session factory
- Provides `get_db()` for request-scoped DB access

### api/
HTTP request/response layer.

- `auth.py`
  - register
  - login / JWT
  - phone auth request / verify
  - find user id
  - reset password
  - delete account
- `user.py`
  - profile read API
- `sessions.py`
  - active session info
  - logout response
- `experience.py`
  - experience mode start / question / submit / finish
- `training.py`
  - training mode start / question / submit / finish
- `match.py`
  - arena find / start / submit / finish

### services/
Business logic layer.

- `auth.py`
  - account rules and auth workflows
- `runtime_state.py`
  - persistent runtime state for training / experience / arena
- `question_service.py`
  - category normalization
  - question lookup
  - duplicate prevention
  - choice shuffle
  - design answer remapping
- `training_service.py`
  - training difficulty progression
  - power update rules
  - record set save logic
- `arena_service.py`
  - round resolution
  - AR delta logic
  - tier progression logic
  - arena progress save logic

### models/
Only active ORM models remain here.

- `user.py` -> USER table model
- `profile.py` -> PROFILE table model
- `base.py` -> shared SQLAlchemy Base

### scripts/
Operational helper scripts.

- `import_concept_csv.py`
  - imports concept questions from CSV into `Q_CONCEPT`

### common/
Shared error and response helpers.

- `errors.py` -> app error definitions
- `responses.py` -> `ok()` / `fail()` response helpers

### legacy/
Old code moved out of the active backend path.
These files are kept only for reference and backward compatibility.

Examples:
- old generic problem model
- old session model
- old store API
- old dummy game logic
- old seed script

## Active Domains

### 1. Auth and Account
- register
- login / JWT
- phone verification
- find user id
- reset password
- delete account

### 2. Question System
- category-specific question loading
- difficulty score mapping
- duplicate prevention
- recycle when exhausted
- choice order randomization
- design answer order remapping

### 3. Profile
- nickname
- per-category power
- average power
- tier and arena rating
- icon URL fields

### 4. Experience
- start / question / submit / finish
- max question count based on current power
- correct answer count summary

### 5. Training
- 4 lives
- 60 second time limit per question
- accumulated power scoring
- update PROFILE only on new high score
- save `RECORD_BATTLE_MATCH` and `TRAINING_Q_SET_RECORD` only on new high score

### 6. Arena
- opponent recommendation
- sorted by closest power gap
- question replay from opponent training record set
- resume progress support
- win/lose resolution
- AR calculation
- tier promotion / demotion
- immediate PROFILE update

## Run Server

```powershell
cd D:\capstone1_server
.\.venv\Scripts\Activate.ps1
python app.py
```

## Deploy To Render

Render reference:
- Web services should bind to `0.0.0.0` and use the `PORT` environment variable.
- Render supports Docker-based web services.
- Render can also run MySQL as a separate private service.

This repo now includes:
- `Dockerfile` for Render Docker deploys
- `render.yaml` for a basic Render web service definition

Recommended Render setup:
1. Push this repo to GitHub.
2. In Render, create a new Web Service from the repo.
3. Use the `Dockerfile` in this repo.
4. Set `DB_URL` to your MySQL connection string.
5. Set `REDIS_URL` if you use Redis outside the default local fallback.
6. Keep `PORT=10000` or let Render provide its default web port.

Example DB URL:

```text
mysql+pymysql://USER:PASSWORD@HOST:3306/DATABASE?charset=utf8mb4
```

If you want MySQL on Render too:
1. Create a separate private MySQL service on Render.
2. Use that internal host and port in `DB_URL`.
3. Deploy this Flask web service in the same region/workspace.

## Syntax Check Example

```powershell
D:\capstone1_server\.venv\Scripts\python.exe -m py_compile D:/capstone1_server/api/training.py D:/capstone1_server/api/match.py
```

## Notes
- The active database is MySQL `math_arena`.
- Old SQLite test flow is no longer the main runtime path.
- Legacy files were moved into `legacy/` instead of being deleted.

