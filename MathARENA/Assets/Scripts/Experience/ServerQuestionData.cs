using System;

namespace MathArena.Network
{
    [Serializable]
    public class ServerQuestionData
    {
        public string q_id; // 문제 식별자
        public string content; // 문제 내용
        public string opt1,
            opt2,
            opt3,
            opt4; // 보기 (객관식/설계용)
        public string answer; // 객관식 정답 (Concept, Idea)
        public string ocr_answer; // OCR 정답 (Calc, Practical)
        public string order_answer; // 순서 정답 (Design, 예: "0-2-3-1")
        public string diff_name; // 난이도 명칭
    }
}
