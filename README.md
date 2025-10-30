# Math ARENA 🧮⚔️
수학을 **경쟁**과 **보상**으로 즐기는 에듀테인먼트. 체험장→훈련장→아레나 플로우로 실력을 전투력과 티어로 증명.

<p align="left">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-2022%2B-black?logo=unity">
  <img alt="Flask" src="https://img.shields.io/badge/Flask-API-blue?logo=flask">
  <img alt="MySQL" src="https://img.shields.io/badge/MySQL-8%2B-4479A1?logo=mysql&logoColor=white">
  <img alt="Redis" src="https://img.shields.io/badge/Redis-6%2B-CB2029?logo=redis&logoColor=white">
  <img alt="License" src="https://img.shields.io/badge/License-TBD-lightgrey">
</p>

---

## 목차
- [프로젝트 개요](#프로젝트-개요)
- [팀원 명단 및 역할](#팀원-명단-및-역할)
- [실행 방법/환경](#실행-방법환경)
  - [사전 요구사항](#사전-요구사항)
  - [백엔드(API) 실행](#백엔드api-실행)
  - [클라이언트(Unity) 실행](#클라이언트unity-실행)
  - [OCR(Tesseract) 설치](#ocrtesseract-설치)
- [주요 폴더 구조 예시](#주요-폴더-구조-예시)
- [문서](#문서)
- [로드맵](#로드맵)
- [라이선스](#라이선스)

---

## 프로젝트 개요
- 에듀테크 + 게이미피케이션. 학습 지속성과 몰입도에 집중.
- 핵심 구조: **체험장**(기록 미반영 연습) → **훈련장**(전투력 갱신·난이도 해금) → **아레나**(실력 매칭 PvP, 레이팅·티어 반영).
- 보상 루프: 전투력/레이팅/골드/코스튬/티켓/경험치/퀘스트.
- 기술 스택: Unity(C#) · Flask(Python) · MySQL · Redis · WebSocket(Flask-SocketIO) · Tesseract OCR.

---

## 팀원 명단 및 역할
> 이름은 저장소 협업자 기준. 필요시 수정.

| 역할 | 이름 | 책임 |
| --- | --- | --- |
| PM · 백엔드 리드 | **최서영** | 일정/범위, API/DB 설계, 배포 파이프라인 |
| 클라이언트(Unity) | 미정 | 게임 루프, UI/UX 이식, 이펙트, 입력/타이머 |
| 서버(Flask/Socket) | 미정 | REST, 실시간 배틀 동기화, 어뷰징 방어 |
| 데이터/문제 검수 | 미정 | 종목별 문제 가공·난이도 태깅·품질관리 |
| OCR/ML | 미정 | Tesseract 연동, 사전/후처리, 인식률 개선 |
| UX/UI 디자인 | 미정 | Figma, 컴포넌트 가이드, 접근성 |

---

## 실행 방법/환경

### 사전 요구사항
- **OS**: Windows 10/11, macOS 13+, Ubuntu 22.04+
- **Unity**: 2022 LTS+
- **Python**: 3.10+
- **DB/Cache**: MySQL 8+, Redis 6+
- **모바일 빌드**: Android Studio / Xcode

### 백엔드(API) 실행
```bash
# 1) 가상환경
python -m venv .venv
# Windows
.venv\Scripts\activate
# macOS/Linux
source .venv/bin/activate

# 2) 의존성
pip install -r src/server/requirements.txt

# 3) 환경변수
cp src/server/.env.example src/server/.env
# .env에 DB_URL, REDIS_URL, JWT_SECRET 등 채우기

# 4) 스키마 초기화
mysql -u root -p < src/server/db/schema.sql
# 선택: 샘플 데이터
python src/server/scripts/seed.py

# 5) 서버
export FLASK_APP=src/server/app.py  # Windows: set FLASK_APP=src/server/app.py
flask run  # http://127.0.0.1:5000
