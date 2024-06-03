using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private GameObject goalFX;

    [Header("Attributes")]
    [SerializeField] private float maxPower = 125000f; // Greatly increased max power
    [SerializeField] private float power = 50000f; // Greatly increased power
    [SerializeField] private float maxGoalSpeed = 50000f; // Greatly increased max goal speed
    [SerializeField] private float airResistance = 0.00000001f; // Significantly reduced air resistance
    [SerializeField] private float friction = 0.00000001f; // Significantly reduced friction

    private bool isDragging;
    private bool inHole;

    private void Update() {
        PlayerInput();

        if (LevelManager.main.outOfStrokes && rb.velocity.magnitude <= 0.2f && !LevelManager.main.levelCompleted) {
            LevelManager.main.GameOver();
        }
    }

    private bool IsReady() {
        return rb.velocity.magnitude <= 0.2f;
    }

    private void PlayerInput() {
        if (!IsReady()) return;

        Vector2 inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(transform.position, inputPos);

        if (Input.GetMouseButtonDown(0) && distance <= 0.5f) DragStart();
        if (Input.GetMouseButton(0) && isDragging) DragChange(inputPos);
        if (Input.GetMouseButtonUp(0) && isDragging) DragRelease(inputPos);
    }

    private void DragStart() {
        isDragging = true;
        lr.positionCount = 2;
    }

    private void DragChange(Vector2 pos) {
        Vector2 dir = (Vector2)transform.position - pos;

        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, (Vector2)transform.position + Vector2.ClampMagnitude((dir * power) / 2, maxPower / 2));
    }

    private void DragRelease(Vector2 pos) {
        float distance = Vector2.Distance((Vector2)transform.position, pos);
        isDragging = false;
        lr.positionCount = 0;

        if (distance < 1f) {
            return;
        }

        LevelManager.main.IncreaseStroke();

        Vector2 dir = (Vector2)transform.position - pos;

        rb.velocity = Vector2.ClampMagnitude(dir * power, maxPower);
    }

    private void CheckWinState() {
        if (inHole) return;

        if (rb.velocity.magnitude <= maxGoalSpeed) {
            inHole = true;

            rb.velocity = Vector2.zero;
            gameObject.SetActive(false);

            GameObject fx = Instantiate(goalFX, transform.position, Quaternion.identity);
            Destroy(fx, 2f);

            LevelManager.main.ShowlevelCompleteUI();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Goal") CheckWinState();
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.tag == "Goal") CheckWinState();
    }

    private void FixedUpdate() {
        ApplyPhysics();
    }

    private void ApplyPhysics() {
        // 공기 저항 계산
        Vector2 velocity = rb.velocity;
        Vector2 airResistanceForce = -airResistance * velocity * velocity.magnitude;

        // 마찰력 계산
        Vector2 frictionForce = -friction * velocity;

        // 총 힘 계산
        Vector2 totalForce = airResistanceForce + frictionForce;

        // 가속도 계산
        Vector2 acceleration = totalForce / rb.mass;

        // 오일러 방법을 사용한 속도 업데이트
        Vector2 newVelocity = rb.velocity + acceleration * Time.fixedDeltaTime;

        // 속도와 위치 적용
        rb.velocity = newVelocity;
    }
}