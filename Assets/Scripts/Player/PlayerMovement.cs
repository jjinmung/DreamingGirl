using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 15f;
    public float dashForce = 20f;
    private float dashCooldown => Managers.Player.data.dashCooldown.TotalValue;
    private float movementSpeed =>Managers.Player.data.moveSpeed.TotalValue;

    private Rigidbody _rb;
    private float _lastDashTime = -999f;
    public bool CanMove { get; set; } = true;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void Move(Vector2 input, Vector3 direction)
    {
        if (!CanMove) return;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 targetVelocity = direction * movementSpeed;
            targetVelocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = targetVelocity;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
        }
    }

    public void ExecuteDash(Vector3 direction, System.Action onDashStart)
    {
        if (Time.time < _lastDashTime + dashCooldown) return;
        _lastDashTime = Time.time;
        onDashStart?.Invoke();

        Vector3 dashDir = direction.sqrMagnitude > 0.01f ? direction : transform.forward;
        transform.forward = dashDir;
        _rb.linearVelocity = dashDir * dashForce;
    }

    
    public void StopVelocity() => _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
}