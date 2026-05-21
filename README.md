# Math ARENA 🧮⚔️
수학을 **경쟁**과 **보상**으로 즐기는 에듀테인먼트 게임

---

## 📘 소개 (About)

Math ARENA는 중등 수학을 기반으로  
**체험장 → 훈련장 → 아레나**로 이어지는 경쟁 구조를 가진 교육형 게임이다.

- 에듀테크 + 게이미피케이션 결합  
- 실력 기반 PvP(아레나), 전투력 시스템  
- 5개 종목  
  - 개념이해 · 연산 · 발상 · 설계 · 실전(OCR)  
- Tesseract OCR로 손글씨 인식(연산/실전 모드)

> 전체 기획·요구사항 문서는 `/docs` 폴더에서 확인 가능

---

## 🧱 기술 스택

### Client
- Unity 6  
- C#  
- TextMeshPro  
- Tesseract OCR (Unity Wrapper)

### Server
- Flask (REST API)  
- Flask-SocketIO (Real-time)  
- MySQL  
- Redis

### Tools
- Figma  
- GitHub

---

## 🎮 핵심 기능

- **체험장**: 기록 미반영, 자유 연습  
- **훈련장**: 전투력 증가, 난이도 해금  
- **아레나(PvP)**: 실력 기반 매칭 · 티어 시스템  
- **프로필**: 레벨 · 티어 · 전투력 · 코스튬 정보  
- **인벤토리/코스튬 시스템**  
- **상점(IAP 구조 준비)**  
- **퀘스트/보상 시스템**

### OCR 적용 콘텐츠
- 연산, 실전 종목 → 손글씨 입력 → OCR → 판정

---

## 📱 UI 개발 현황

### 01_Login 씬
- 아이디/비밀번호 입력  
- Google/Apple 로그인 버튼  
- 회원가입/계정찾기 버튼  
- Anchor 기반 모바일 대응  
- Scene 전환 기능 구현

사용 스크립트:  
- LoginUI.cs  
- SceneChangeButton.cs

---

### 02_Lobby 씬
- 프로필 요약바  
- 체험장/훈련장/아레나 메뉴  
- 햄버거 메뉴 슬라이드 구현

사용 스크립트:  
- LobbySideMenuController.cs

---

### 04_Inventory 씬
- 카테고리별 아이템 필터링  
- Grid 자동 생성  
- 장착 버튼 기능 포함

사용 스크립트:  
- InventoryUI.cs  
- InventoryItemCardView.cs  
- InventoryTypes.cs

---

## 🚀 실행 방법

### 1. 앱 실행 방법

이 프로젝트는 Android 환경에서 플레이할 수 있습니다.

아래 버튼을 눌러 최신 버전의 APK 파일을 다운로드하세요.

[![Download APK](https://img.shields.io/badge/Download-APK-green?style=for-the-badge&logo=android)](https://github.com/Hanshin-OSS-Hub/capstone25-four-idiots/releases/download/v1.0.0/MathArena.apk)

1. 최신 `.apk` 파일을 다운로드합니다.
2. 안드로이드 기기에서 파일을 실행하여 설치합니다.
   출처를 알 수 없는 앱 설치 허용이 필요할 수 있습니다.
3. 앱을 실행하고 로그인합니다.

### 2. 백엔드 서버 실행 방법

백엔드 서버는 저장소의 `backend/` 폴더 기준으로 실행합니다.

#### 준비물

- Python 3.11 권장
- MySQL 8.x 권장
- Git

#### 실행 순서

1. 저장소를 clone 받은 뒤 `backend/` 폴더로 이동합니다.

```bash
git clone <repository-url>
cd capstone25-four-idiots
cd backend
```

2. 가상환경을 생성하고 활성화합니다.

Windows PowerShell:

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
```

macOS / Linux:

```bash
python3 -m venv .venv
source .venv/bin/activate
```

3. 패키지를 설치합니다.

```bash
pip install -r requirements.txt
```

4. `backend/.env` 파일을 생성하고 DB 접속 정보를 설정합니다.

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
APP_PORT=8001
```

5. MySQL을 실행한 뒤 아래 SQL 파일을 적용합니다.

```text
SQL/4. Math ARENA_MySQL.sql
```

중요:

- 데이터베이스 이름은 `math_arena`로 맞추는 것을 권장합니다.
- `.env`의 `MYSQL_DATABASE`, `DB_URL`도 같은 이름으로 맞춰야 합니다.

6. 서버를 실행합니다.

Windows PowerShell:

```powershell
cd backend
.\.venv\Scripts\Activate.ps1
python app.py
```

macOS / Linux:

```bash
cd backend
source .venv/bin/activate
python app.py
```

7. 서버가 실행되면 아래 주소로 상태를 확인합니다.

```text
http://127.0.0.1:8001/healthz
```

#### 다른 기기에서 테스트할 때

프론트가 다른 기기 또는 다른 네트워크에서 접속해야 한다면 `ngrok`을 사용할 수 있습니다.

```bash
ngrok http 8001
```

생성된 주소를 프론트의 API Base URL로 사용하면 됩니다.

#### Render 배포 시 주의사항

- Render에서는 로컬 MySQL(`localhost`)을 사용할 수 없습니다.
- Render MySQL 또는 외부에서 접근 가능한 MySQL 주소를 `DB_URL`에 넣어야 합니다.
- Render 배포 시 `Root Directory`는 `backend`로 설정하는 것을 권장합니다.

상세한 서버 실행 및 배포 가이드는 아래 문서를 참고하세요.

- [backend/BACKEND_SETUP.md](./backend/BACKEND_SETUP.md)

---

## 🔌 서버 연동 준비 상태

- 로그인/회원가입 API 연동 지점 준비됨  
- 아레나 실시간 통신(WebSocket) 구조 설계 완료  
- 실제 서버 연결 시 확장할 요소  
  - 로그인 인증  
  - 프로필/전투력 동기화  
  - 인벤토리/코스튬 DB 연동  
  - 결제(IAP) 검증 로직

전체 서버 구조 문서: `/docs` 폴더 참고

---

## 👥 팀 구성

| 이름 | 역할 |
|------|------|
| 유상혁 | 팀장 · DB 개발 |
| 김형준 | 서버 개발 |
| 최호건 | Unity 클라이언트 개발 |
| 허준하 | UI/UX 디자인 |

---

## 📌 개발 로드맵

- [ ] 서버 로그인 API 연동  
- [ ] 체험장/훈련장 문제 데이터 연동  
- [ ] 실시간 아레나 배틀 테스트  
- [ ] 코스튬/인벤토리 서버 바인딩  
- [ ] 상점(IAP) 구현  
- [ ] 퀘스트/보상 시스템 개발  

---

## 📄 라이선스
TBD
