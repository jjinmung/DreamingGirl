using TMPro;
using UnityEngine;
using DG.Tweening; // 페이드 아웃 연출을 위해 추가

public class TutorialWASD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI w, a, s, d;

    private Vector2 direction;
    private Color32 inActiveColor = new Color32(255, 255, 255, 60);
    // 각 키를 한 번이라도 눌렀는지 체크
    private bool pressedW, pressedA, pressedS, pressedD;
    private bool isFinished = false;

    private void Update()
    {
        if (isFinished) return; // 이미 완료되었다면 로직 중단

        direction = Managers.Input.GetMoveInput();
        UpdateKeyColors();
        CheckAllPressed();
    }

    private void UpdateKeyColors()
    {
        // 색상 변경 및 입력 체크
        if (direction.y > 0) { w.color = inActiveColor; pressedW = true; }

        if (direction.y < 0) { s.color = inActiveColor; pressedS = true; }

        if (direction.x < 0) { a.color = inActiveColor; pressedA = true; }

        if (direction.x > 0) { d.color = inActiveColor; pressedD = true; }
    }

    private void CheckAllPressed()
    {
        // 4개 키가 모두 한 번씩 눌렸다면
        if (pressedW && pressedA && pressedS && pressedD)
        {
            isFinished = true;
            FinishTutorial();
        }
    }

    private void FinishTutorial()
    {
        transform.DOScale(0f, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                gameObject.SetActive(false);
                Managers.Player.Control.StopDashPhysics();
                Managers.Player.Anim.SetFloat("MOVE", 0);;
                Managers.Dialogue.StartDialogue("2");
            });
    }
}