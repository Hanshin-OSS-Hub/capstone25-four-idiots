# Math ARENA Backend Setup Guide

This document explains how to run the backend server locally, prepare MySQL, test from other devices, and deploy to Render.

## 1. Backend Location

All backend work should be done inside the `backend/` folder.

```bash
cd backend
```

## 2. Required Software

Install the following first:

- Git
- Python 3.11 recommended
- MySQL 8.x recommended
- Optional: ngrok
- Optional: Render account
- Optional: MySQL Workbench / DBeaver / TablePlus

## 3. Clone the Repository

```bash
git clone <repository-url>
cd capstone25-four-idiots
cd backend
```

## 4. Create a Virtual Environment

### Windows PowerShell

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
```

### macOS / Linux

```bash
python3 -m venv .venv
source .venv/bin/activate
```

If the environment is activated correctly, your shell prompt usually shows `(.venv)`.

## 5. Install Python Packages

```bash
pip install -r requirements.txt
```

## 6. Create the `.env` File

Create a file named `.env` inside the `backend/` folder.

Example:

```env
JWT_SECRET=change-this-secret
AUTH_OFF=false
DEV_TOKEN=dev

MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_USER=arena_user
MYSQL_PASSWORD=arena_pw
MYSQL_DATABASE=math_arena

DB_URL=mysql+pymysql://arena_user:arena_pw@localhost:3306/math_arena?charset=utf8mb4

REDIS_URL=redis://localhost:6379/0

FLASK_ENV=development
FLASK_DEBUG=true
APP_PORT=8000
```

## 7. Environment Variable Notes

### Auth

- `JWT_SECRET`: JWT signing secret
- `AUTH_OFF`: set to `true` only for temporary local test bypassing auth
- `DEV_TOKEN`: local development token fallback

### Database

- `MYSQL_HOST`: MySQL server host
- `MYSQL_PORT`: MySQL server port
- `MYSQL_USER`: MySQL user
- `MYSQL_PASSWORD`: MySQL password
- `MYSQL_DATABASE`: database name
- `DB_URL`: final SQLAlchemy connection string used by the app

`DB_URL` format:

```text
mysql+pymysql://USER:PASSWORD@HOST:PORT/DATABASE?charset=utf8mb4
```

### Flask

- `FLASK_ENV`: `development` or `production`
- `FLASK_DEBUG`: local debug flag
- `APP_PORT`: local server port

## 8. Prepare MySQL

Start MySQL first.

Recommended database name:

```sql
CREATE DATABASE math_arena DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Optional user setup:

```sql
CREATE USER 'arena_user'@'%' IDENTIFIED BY 'arena_pw';
GRANT ALL PRIVILEGES ON math_arena.* TO 'arena_user'@'%';
FLUSH PRIVILEGES;
```

Important:

- The provided SQL file uses `math_arena`
- Your `.env` file should also use `math_arena`
- If the DB name does not match, many APIs will fail

## 9. Import the Project SQL

Use the schema file below:

```text
SQL/4. Math ARENA_MySQL.sql
```

This file creates:

- database and core tables
- user/profile/tier/difficulty tables
- question tables
- arena and training-related records

### Option A. Import with GUI Tool

Use MySQL Workbench, DBeaver, or another SQL client:

1. Connect to MySQL
2. Open `4. Math ARENA_MySQL.sql`
3. Execute the full file

### Option B. Import with CLI

```bash
mysql -u arena_user -p math_arena < "4. Math ARENA_MySQL.sql"
```

## 10. Run the Backend Server

### Windows PowerShell

```powershell
cd backend
.\.venv\Scripts\Activate.ps1
python app.py
```

### macOS / Linux

```bash
cd backend
source .venv/bin/activate
python app.py
```

Expected local address:

```text
http://127.0.0.1:8000
```

or

```text
http://localhost:8000
```

## 11. Health Check

Test the server first with:

```text
GET /healthz
```

Example:

```text
http://127.0.0.1:8000/healthz
```

If DB is connected correctly, this endpoint should return a normal healthy response.

## 12. Commonly Tested API Endpoints

Recommended initial API checks:

- `GET /healthz`
- `POST /v1/auth/login`
- `GET /v1/user/profile`
- `POST /v1/experience/start`
- `POST /v1/training/start`
- `GET /v1/match/recommendations`

## 13. Test from Another Device with ngrok

If the frontend is running on a different network or device, local `localhost` is not enough.
Use ngrok.

### Step 1. Run the backend locally

```bash
python app.py
```

### Step 2. Start ngrok

```bash
ngrok http 8000
```

Example public URL:

```text
https://xxxx.ngrok-free.app
```

### Step 3. Use the ngrok URL in the frontend

Set the frontend API base URL to the ngrok address.

Example:

```text
https://xxxx.ngrok-free.app/v1/auth/login
```

Notes:

- If ngrok is closed, the URL stops working
- On the free plan, the URL may change

## 14. Render Deployment Guide

Local MySQL cannot be used from Render.
You must use Render MySQL or another externally reachable MySQL server.

### Web Service Settings

Recommended Render web service setup:

- Repository: this GitHub repo
- Branch: `main` or the deployment branch
- Root Directory: `backend`
- Runtime: `Docker`

### Render Environment Variables

Example:

```env
JWT_SECRET=some-secret
AUTH_OFF=false
FLASK_ENV=production
PORT=10000
DB_URL=mysql+pymysql://arena_user:password@mysql-service-host:3306/math_arena?charset=utf8mb4
REDIS_URL=redis://...
```

Important:

- Do not use `localhost` for Render database connections
- Use the internal Render MySQL hostname or another reachable DB host

## 15. Render MySQL Setup

Create a separate MySQL service in Render.

Recommended values:

- database name: `math_arena`
- user: `arena_user`
- password: set explicitly
- disk mount path: `/var/lib/mysql`

After MySQL is created, import the SQL schema into that DB.

## 16. Recommended Render DB URL Example

```text
mysql+pymysql://arena_user:password@mysql-service-name:3306/math_arena?charset=utf8mb4
```

Put this value into the Render backend environment variable `DB_URL`.

## 17. Typical Problems and Fixes

### Problem 1. `/healthz` returns `db_error`

Possible causes:

- MySQL is not running
- `DB_URL` is wrong
- DB name mismatch
- Render is still pointing to `localhost`

Fix:

- verify MySQL is running
- verify `.env`
- verify `math_arena` exists
- verify Render uses the actual MySQL host

### Problem 2. `user not found`

Possible causes:

- test account does not exist
- user/profile rows were not inserted

Fix:

- inspect `USER` and `PROFILE` tables
- create test accounts manually if needed

### Problem 3. table not found errors

Cause:

- SQL file was not imported

Fix:

- import `SQL/4. Math ARENA_MySQL.sql`

### Problem 4. another device cannot reach the server

Cause:

- local `localhost` is not externally reachable

Fix:

- use ngrok
- or use a deployed Render URL

### Problem 5. README or repo structure becomes mixed up

Cause:

- backend files edited at repo root instead of `backend/`
- work done on the wrong branch

Fix:

- keep backend work inside `backend/`
- check branch before pushing
- review changed paths before merging

## 18. Recommended First-Time Setup Order

1. Clone the repository
2. Enter `backend/`
3. Create the virtual environment
4. Run `pip install -r requirements.txt`
5. Create `.env`
6. Start MySQL
7. Create `math_arena`
8. Import the SQL file
9. Run `python app.py`
10. Check `/healthz`
11. Test login/profile APIs
12. Use ngrok if another device must connect

## 19. Final Checklist

Before asking for help, verify the following:

- Are you inside the `backend/` folder?
- Is the virtual environment activated?
- Did `pip install -r requirements.txt` finish?
- Does `.env` exist?
- Is `DB_URL` correct?
- Is MySQL running?
- Does the `math_arena` DB exist?
- Was the SQL file imported?
- Does `/healthz` respond normally?
- If testing from another device, are you using ngrok or Render?
