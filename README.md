# Math ARENA 🧮⚔️
수학을 **경쟁**과 **보상**으로 즐기는 에듀테인먼트.  
체험장 → 훈련장 → 아레나 플로우로 실력을 전투력과 티어로 증명한다.

---

## 📘 프로젝트 개요
- **에듀테크 + 게이미피케이션**을 결합한 학습형 게임.  
- 학습 지속성과 몰입도를 높이기 위해 **경쟁(매칭, 티어, 랭킹)** 과 **보상(전투력, 코스튬, 티켓, 경험치)** 을 적용.  
- **핵심 구조**  
  1. **체험장**: 기록이 반영되지 않는 자유 연습 공간.  
  2. **훈련장**: 전투력 상승 및 난이도 해금.  
  3. **아레나**: 실력 기반 PvP 매칭, 레이팅·티어 시스템 반영.  
- **기술 스택 계획**  
  Unity(C#) · Flask(Python) · MySQL · Redis · WebSocket(Flask-SocketIO) · Tesseract OCR  
- **범위**  
  중학교 수학 중심으로 시작, 추후 고등과정까지 확장 예정.

---

## 👥 팀원 명단 및 역할

| 이름 | 역할 | 주요 업무 |
|------|------|-----------|
| **유상혁 (팀장)** | 프로젝트 총괄 | 프로젝트 기획·관리, MySQL·Redis 기반 DB 개발 |
| **김형준 (서버 개발)** | 백엔드 | Flask 서버 구축, 실시간 통신 구현, 디자인 보조 |
| **최호건 (클라이언트 개발)** | Unity 개발 | Unity·C#을 통한 게임 로직 및 시각적 요소 구현, 프로젝트 관리 |
| **허준하 (UI/UX 디자인)** | 디자인 | Figma 기반 게임 화면 설계, UX 연구, 프론트 디자인 담당 |

---

## 🧩 시스템 및 서버 아키텍처  
*(제안서 내용 통합 섹션)*

### 🏗 시스템 구성도
<img width="492" height="159" alt="image" src="https://github.com/user-attachments/assets/146a7ef8-9b7b-4190-881c-e4f9fbfe6e58" />


### 🖥 서버 아키텍처 설계
<img width="490" height="214" alt="image" src="https://github.com/user-attachments/assets/8e669468-2672-4825-9c49-3f667d989787" />


**요약**
- Unity 클라이언트 ↔ Flask API/SocketIO 서버
- MySQL: 사용자·전투력·코스튬 등 영구 저장
- Redis: 세션·매칭·랭킹 캐시
- Tesseract OCR: 필기 인식(연산/실전 모드)
- API 구성: 로그인·회원가입·아레나·훈련장·상점·퀘스트  
- 실시간 매칭: Flask-SocketIO 기반 HP/공격 동기화

---

## 🎨 UI Development Progress (Login · Lobby)

Unity 6 기반으로 구현 중인 **01_Login** 및 **02_Lobby** 화면의 개발 현황이다.  
UI는 Figma 디자인 시안을 기준으로 Anchor(Shift/Alt 조합) 및 SafeArea 대응을 적용하여 모바일 환경에서 안정적으로 표시되도록 구성했다.

---

### 1. 로그인 화면 (01_Login)

> *(로그인 화면 이미지 삽입 예정)*

**구성 요소**
- 아이디/비밀번호 입력 필드  
- 일반 로그인 버튼  
- Google / Apple 로그인  
- 회원가입 / 계정찾기 버튼  

**구현 사항**
- 자동 로그인 구조 준비  
- TextMeshPro 입력 필드 구성  
- Anchor 조정 기준  
  - **Shift + 드래그**: 균등 확장  
  - **Alt + 드래그**: Pivot 기준 크기 조절  
- SafeArea 적용  
- 모바일 종횡비 대응 완료  

---

### 2. 로비 화면 (02_Lobby)

> *(로비 메인 화면 이미지 삽입 예정)*

**구성 요소**
- 햄버거 메뉴 버튼  
- 프로필 요약 카드(레벨, 티어, 골드, 아레나 티켓)  
- 체험장 / 훈련장 / 아레나 메뉴 카드  

**구현 사항**
- LayoutGroup 최소 사용 → Anchor 기반 정적 배치  
- 프로필 더미 데이터 바인딩  
- 이후 실제 API 연동을 위한 구조 사전 준비  
- 씬 전환 구조 설계 진행  

---

### 3. 사이드 메뉴 (Lobby Side Menu)

> *(사이드 메뉴 이미지 삽입 예정)*

**메뉴 구성**
- 코스튬  
- 상점  
- 설정  
- 로그아웃  

**구현 사항**
- `Panel_SideMenuRoot` 슬라이드 인/아웃 애니메이션 구현  
- Dimmed Panel을 활용한 UI 입력 차단 처리  
- `LobbySideMenuController`에서 열림/닫힘 전체 제어  
- 스크립트를 잘못된 오브젝트에 붙여 메뉴가 동작하지 않던 문제 해결  
  (Canvas_Lobby에 스크립트 재배치 후 정상 작동)  

---

### 4. 현재 UI 오브젝트 구조

**01_Login**
- Panel_LoginRoot  
- Panel_SNS  
- Panel_Register  
- Panel_FindAccount  

**02_Lobby**
- Panel_TopBar  
- Panel_ProfileSummary  
- Panel_MainMenu  
- Panel_SideMenuRoot  
- Panel_Dimmed  

---

### 5. UI 최적화

**반응형 대응**
- Anchor 기반 레이아웃  
- SafeArea 지원  
- Canvas Scaler: `Scale With Screen Size`  

**성능 고려**
- 정적 UI로 Instantiate 최소화  
- Sprite Atlas 계획  
- 텍스트/아이콘 수 최소화  

---

## 📂 실행방법 및 환경(임시)
