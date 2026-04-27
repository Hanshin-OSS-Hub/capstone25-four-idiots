# Math ARENA 백엔드 실행 가이드

이 문서는 처음 프로젝트를 받은 사람도 그대로 따라 하면 로컬에서 백엔드 서버를 실행하고, MySQL을 준비하고, 다른 기기 테스트와 Render 배포까지 진행할 수 있도록 작성된 가이드입니다.

## 1. 백엔드 위치

백엔드 관련 작업은 모두 `backend/` 폴더 안에서 진행합니다.

```bash
cd backend
```

## 2. 준비 프로그램

먼저 아래 프로그램이 설치되어 있어야 합니다.

- Git
- Python 3.11 권장
- MySQL 8.x 권장
- 선택: ngrok
- 선택: Render 계정
- 선택: MySQL Workbench / DBeaver / TablePlus

## 3. 저장소 받기

```bash
git clone <repository-url>
cd capstone25-four-idiots
cd backend
```

## 4. 가상환경 생성

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

정상적으로 활성화되면 보통 프롬프트 앞에 `(.venv)`가 표시됩니다.

## 5. Python 패키지 설치

```bash
pip install -r requirements.txt
```

## 6. `.env` 파일 생성

`backend/` 폴더 안에 `.env` 파일을 생성합니다.

예시:

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

## 7. 환경변수 설명

### 인증 관련

- `JWT_SECRET`: JWT 서명용 비밀키
- `AUTH_OFF`: 인증을 잠시 우회할 때만 `true` 사용
- `DEV_TOKEN`: 개발용 기본 토큰 값

### 데이터베이스 관련

- `MYSQL_HOST`: MySQL 서버 주소
- `MYSQL_PORT`: MySQL 포트
- `MYSQL_USER`: MySQL 계정
- `MYSQL_PASSWORD`: MySQL 비밀번호
- `MYSQL_DATABASE`: 사용할 DB 이름
- `DB_URL`: 서버가 실제로 사용하는 최종 SQLAlchemy 연결 문자열

`DB_URL` 형식:

```text
mysql+pymysql://USER:PASSWORD@HOST:PORT/DATABASE?charset=utf8mb4
```

### Flask 관련

- `FLASK_ENV`: `development` 또는 `production`
- `FLASK_DEBUG`: 로컬 디버그 여부
- `APP_PORT`: 로컬 서버 포트

## 8. MySQL 준비

먼저 MySQL 서버를 실행합니다.

권장 데이터베이스 이름:

```sql
CREATE DATABASE math_arena DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

필요하면 사용자도 생성합니다.

```sql
CREATE USER 'arena_user'@'%' IDENTIFIED BY 'arena_pw';
GRANT ALL PRIVILEGES ON math_arena.* TO 'arena_user'@'%';
FLUSH PRIVILEGES;
```

중요:

- 제공된 SQL 파일은 `math_arena`를 기준으로 작성되어 있습니다
- `.env`의 DB 이름도 `math_arena`로 맞추는 것을 권장합니다
- DB 이름이 다르면 API가 정상 동작하지 않을 수 있습니다

## 9. 프로젝트 SQL 적용

아래 스키마 파일을 사용합니다.

```text
SQL/4. Math ARENA_MySQL.sql
```

이 파일에는 다음이 포함됩니다.

- 데이터베이스 및 핵심 테이블
- USER / PROFILE / TIER / DIFFICULTY 관련 테이블
- 문제 테이블
- 아레나 / 훈련장 관련 기록 테이블

### 방법 A. GUI 툴로 적용

MySQL Workbench, DBeaver 등으로:

1. MySQL 접속
2. `4. Math ARENA_MySQL.sql` 파일 열기
3. 전체 실행

### 방법 B. CLI로 적용

```bash
mysql -u arena_user -p math_arena < "4. Math ARENA_MySQL.sql"
```

## 10. 백엔드 서버 실행

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

로컬 접속 주소 예시:

```text
http://127.0.0.1:8000
```

또는

```text
http://localhost:8000
```

## 11. 헬스체크 확인

먼저 아래 엔드포인트로 서버 상태를 확인합니다.

```text
GET /healthz
```

예시:

```text
http://127.0.0.1:8000/healthz
```

DB까지 정상 연결되면 정상 응답이 와야 합니다.

## 12. 처음 확인하기 좋은 API

초기 연동 시 아래 API부터 확인하는 것을 권장합니다.

- `GET /healthz`
- `POST /v1/auth/login`
- `GET /v1/user/profile`
- `POST /v1/experience/start`
- `POST /v1/training/start`
- `GET /v1/match/recommendations`

## 13. 다른 기기에서 ngrok으로 테스트

프론트가 다른 네트워크 또는 다른 기기에서 접속해야 하면 로컬 `localhost`만으로는 부족합니다. 이때 `ngrok`을 사용합니다.

### 1단계. 로컬 서버 실행

```bash
python app.py
```

### 2단계. ngrok 실행

```bash
ngrok http 8000
```

예시 공개 주소:

```text
https://xxxx.ngrok-free.app
```

### 3단계. 프론트에서 ngrok 주소 사용

프론트의 API Base URL을 ngrok 주소로 바꿉니다.

예시:

```text
https://xxxx.ngrok-free.app/v1/auth/login
```

주의:

- ngrok 창을 닫으면 주소가 만료됩니다
- 무료 플랜은 주소가 바뀔 수 있습니다

## 14. Render 배포 가이드

Render에서는 로컬 MySQL을 사용할 수 없습니다. 반드시 Render MySQL 또는 외부에서 접근 가능한 MySQL을 사용해야 합니다.

### Web Service 설정

권장 Render 설정:

- Repository: 이 GitHub 저장소
- Branch: `main` 또는 배포 브랜치
- Root Directory: `backend`
- Runtime: `Docker`

### Render 환경변수 예시

```env
JWT_SECRET=some-secret
AUTH_OFF=false
FLASK_ENV=production
PORT=10000
DB_URL=mysql+pymysql://arena_user:password@mysql-service-host:3306/math_arena?charset=utf8mb4
REDIS_URL=redis://...
```

중요:

- Render에서는 `localhost`를 DB 주소로 쓰면 안 됩니다
- Render MySQL 내부 호스트명 또는 외부 접속 가능한 DB 주소를 써야 합니다

## 15. Render MySQL 설정

Render에 MySQL 서비스를 별도로 생성합니다.

권장 값:

- 데이터베이스 이름: `math_arena`
- 사용자: `arena_user`
- 비밀번호: 직접 지정
- 디스크 마운트 경로: `/var/lib/mysql`

MySQL 서비스 생성 후 SQL 스키마도 반드시 적용해야 합니다.

## 16. Render용 DB_URL 예시

```text
mysql+pymysql://arena_user:password@mysql-service-name:3306/math_arena?charset=utf8mb4
```

이 값을 Render 백엔드 서비스의 `DB_URL` 환경변수에 넣습니다.

## 17. 자주 발생하는 문제와 해결법

### 문제 1. `/healthz`가 `db_error`를 반환함

가능한 원인:

- MySQL이 꺼져 있음
- `DB_URL`이 잘못됨
- DB 이름이 맞지 않음
- Render에서 아직 `localhost`를 바라보고 있음

해결:

- MySQL 실행 여부 확인
- `.env` 확인
- `math_arena` DB 존재 여부 확인
- Render가 실제 MySQL 호스트를 보도록 수정

### 문제 2. `user not found`

가능한 원인:

- 테스트 계정이 없음
- USER / PROFILE 데이터가 들어가지 않음

해결:

- `USER`, `PROFILE` 테이블 확인
- 필요하면 테스트 계정 수동 추가

### 문제 3. 테이블이 없다고 나옴

원인:

- SQL 파일이 적용되지 않음

해결:

- `SQL/4. Math ARENA_MySQL.sql` 전체 실행

### 문제 4. 다른 기기에서 서버 접속이 안 됨

원인:

- 로컬 `localhost`는 외부 접근이 안 됨

해결:

- ngrok 사용
- 또는 Render 배포 주소 사용

### 문제 5. README나 저장소 구조가 꼬임

원인:

- 백엔드 파일을 저장소 루트에서 수정함
- 다른 브랜치에서 작업해야 할 내용을 잘못 반영함

해결:

- 백엔드 작업은 `backend/` 기준으로 유지
- push 전 현재 브랜치 확인
- PR 전에 변경된 파일 경로 확인

## 18. 처음 세팅할 때 권장 순서

1. 저장소 clone
2. `backend/` 진입
3. 가상환경 생성
4. `pip install -r requirements.txt`
5. `.env` 작성
6. MySQL 실행
7. `math_arena` DB 생성
8. SQL 파일 적용
9. `python app.py`
10. `/healthz` 확인
11. 로그인 / 프로필 API 테스트
12. 다른 기기 테스트가 필요하면 ngrok 사용

## 19. 최종 체크리스트

도움 요청 전 아래를 먼저 확인합니다.

- `backend/` 폴더 안에서 실행 중인가
- 가상환경이 활성화되었는가
- `pip install -r requirements.txt`가 완료되었는가
- `.env` 파일이 존재하는가
- `DB_URL`이 올바른가
- MySQL이 실행 중인가
- `math_arena` DB가 존재하는가
- SQL 파일이 적용되었는가
- `/healthz`가 정상 응답하는가
- 다른 기기 테스트라면 ngrok 또는 Render 주소를 쓰고 있는가
